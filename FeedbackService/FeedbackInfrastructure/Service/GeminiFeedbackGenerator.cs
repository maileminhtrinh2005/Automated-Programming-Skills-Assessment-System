using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FeedbackService.Application.Dtos;
using FeedbackService.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace FeedbackService.Infrastructure;

public class GeminiOptions
{
    public string Model { get; set; } = "models/gemini-2.0-flash";
    public string? ApiKey { get; set; }
}

public class GeminiFeedbackGenerator : IFeedbackGenerator
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _opts;
    private readonly IConfiguration _cfg;
    private readonly IWebHostEnvironment _env;

    public GeminiFeedbackGenerator(
        HttpClient http,
        IOptions<GeminiOptions> opts,
        IConfiguration cfg,
        IWebHostEnvironment env
    )
    {
        _http = http;
        _opts = opts.Value;
        _cfg = cfg;
        _env = env;
        _http.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<FeedbackResponseDto> GenerateAsync(FeedbackRequestDto req, CancellationToken ct = default)
    {
        // 1) Lấy API key
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                   ?? _cfg["Gemini:ApiKey"] ?? _opts.ApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Missing GEMINI_API_KEY (chưa set API key cho Gemini).");

        // 2) Yêu cầu tối thiểu: phải có testResults để test riêng feedback (không gọi judge0)
        if (req.TestResults is null || req.TestResults.Count == 0)
            throw new ArgumentException("Vui lòng cung cấp testResults để sinh feedback bằng AI.");

        // 3) Dựng prompt
        var testResultsText = JsonSerializer.Serialize(req.TestResults, new JsonSerializerOptions { WriteIndented = true });

        // ✅ LƯU Ý: v1beta KHÔNG cho role=system trong contents
        // → dùng field 'systemInstruction' hoặc prepend vào user
        var systemInstruction = new
        {
            parts = new[] {
                new { text =
                    "You are an APSAS grading assistant. Return JSON ONLY that matches the schema. " +
                    "Give concise pedagogy-focused feedback mapped to test cases and rubric. " +
                    "Language: Vietnamese."
                }
            }
        };

        var userParts = new object[]
        {
            new { text = $"Student: {req.StudentId}\nAssignment: {req.AssignmentTitle}\nLanguageId: {req.LanguageId}\nRubric: {req.Rubric ?? "(none)"}" },
            new { text = "SOURCE CODE:\n```" + (req.SourceCode ?? "") + "```" },
            new { text = "TEST RESULTS (JSON):\n" + testResultsText },
            new { text = "Hãy trả về JSON đúng schema:\n{ \"summary\": string, \"score\": number, \"rubricBreakdown\":[{\"criterion\": string, \"score\": number, \"max\": number}], \"testCaseFeedback\":[{\"name\": string, \"comment\": string}], \"suggestions\": [string], \"nextSteps\": [string] }" }
        };

        var bodyObj = new
        {
            systemInstruction, // ✅ đúng chỗ cho v1beta
            contents = new object[]
            {
                new { role = "user", parts = userParts }
            },
            generationConfig = new
            {
                response_mime_type = "application/json" // ✅ ép trả JSON text
            }
            // Có thể thêm safetySettings nếu cần nới lỏng chặn nội dung
            // safetySettings = new [] { new { category="HARM_CATEGORY_DANGEROUS_CONTENT", threshold="BLOCK_NONE" } }
        };

        var model = string.IsNullOrWhiteSpace(_opts.Model) ? "models/gemini-2.0-flash" : _opts.Model;
        var url = $"v1beta/{model}:generateContent?key={apiKey}";

        // (opt) Log request khi chạy Development để debug
        if (_env.IsDevelopment())
        {
            Console.WriteLine("=== Gemini request body ===");
            Console.WriteLine(JsonSerializer.Serialize(bodyObj, new JsonSerializerOptions { WriteIndented = true }));
        }

        // 4) Gọi API
        using var reqMsg = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(bodyObj), Encoding.UTF8, "application/json")
        };
        var res = await _http.SendAsync(reqMsg, ct);
        var payload = await res.Content.ReadAsStringAsync(ct);

        if (_env.IsDevelopment())
        {
            Console.WriteLine("=== Gemini raw response ===");
            Console.WriteLine(payload);
        }

        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"Gemini error {res.StatusCode}: {payload}");

        // 5) Parse dữ liệu JSON trả về
        // v1beta: candidates[0].content.parts[0].text là chuỗi JSON bạn yêu cầu
        using var doc = JsonDocument.Parse(payload);
        var candidates = doc.RootElement.GetProperty("candidates");
        if (candidates.GetArrayLength() == 0)
            throw new InvalidOperationException("Gemini không trả candidate nào.");

        var text = candidates[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Gemini không trả text trong phần response.");

        // 6) Map vào DTO
        var feedback = JsonSerializer.Deserialize<FeedbackResponseDto>(text!, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (feedback is null)
            throw new InvalidOperationException("Không parse được JSON feedback từ Gemini.");

        return feedback;
    }
}
