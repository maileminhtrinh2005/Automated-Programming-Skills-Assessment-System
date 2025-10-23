using System.Net;
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

        // ✅ v1beta: dùng systemInstruction thay vì role=system trong contents
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
            systemInstruction,
            contents = new object[]
            {
                new { role = "user", parts = userParts }
            },
            generationConfig = new
            {
                response_mime_type = "application/json"
            }
        };

        var model = string.IsNullOrWhiteSpace(_opts.Model) ? "models/gemini-2.0-flash" : _opts.Model;

        // [CHANGED] Không để API key trong query string nữa
        var url = $"v1beta/{model}:generateContent";

        if (_env.IsDevelopment())
        {
            Console.WriteLine("=== Gemini request body ===");
            Console.WriteLine(JsonSerializer.Serialize(bodyObj, new JsonSerializerOptions { WriteIndented = true }));
        }

        // [ADDED] — gắn API key bằng header để tránh lộ key trong log/URL
        using var reqMsg = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(bodyObj), Encoding.UTF8, "application/json")
        };
        reqMsg.Headers.Add("x-goog-api-key", apiKey); // [ADDED]

        // [ADDED] — retry 3 lần khi 503/429 với backoff 0.5s/1s/2s + fallback
        HttpResponseMessage res;
        var delays = new[] { 500, 1000, 2000 };
        int attempt = 0;

        while (true)
        {
            attempt++;
            try
            {
                var sent = await _http.SendAsync(reqMsg, ct);
                res = sent;
            }
            catch (HttpRequestException ex)
            {
                if (attempt <= delays.Length)
                {
                    if (_env.IsDevelopment()) Console.WriteLine($"[Gemini] network error, retry {attempt}: {ex.Message}");
                    await Task.Delay(delays[attempt - 1], ct);
                    continue;
                }
                // Fallback cuối cùng
                return FallbackDto("(Lỗi mạng tới AI — dùng phản hồi tối thiểu)");
            }

            if (res.StatusCode == HttpStatusCode.ServiceUnavailable || (int)res.StatusCode == 429)
            {
                if (attempt <= delays.Length)
                {
                    if (_env.IsDevelopment()) Console.WriteLine($"[Gemini] {res.StatusCode}, retry {attempt}");
                    await Task.Delay(delays[attempt - 1], ct);
                    continue;
                }
                // Fallback khi quá tải kéo dài
                var raw = await res.Content.ReadAsStringAsync(ct);
                if (_env.IsDevelopment()) Console.WriteLine("=== Gemini raw response (fallback) ===\n" + raw);
                return FallbackDto("(AI đang quá tải — dùng phản hồi tối thiểu)");
            }

            // thành công hoặc lỗi khác
            break;
        }

        var payload = await res.Content.ReadAsStringAsync(ct);

        if (_env.IsDevelopment())
        {
            Console.WriteLine("=== Gemini raw response ===");
            Console.WriteLine(payload);
        }

        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"Gemini error {res.StatusCode}: {payload}");

        // 5) Parse dữ liệu JSON trả về (v1beta: candidates[0].content.parts[0].text)
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

        var feedback = JsonSerializer.Deserialize<FeedbackResponseDto>(text!, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (feedback is null)
            throw new InvalidOperationException("Không parse được JSON feedback từ Gemini.");

        return feedback;
    }

    // [ADDED] — DTO fallback khi AI unavailable để không ném exception
    private static FeedbackResponseDto FallbackDto(string message) => new()
    {
        Summary = message,
        Score = 0,
        RubricBreakdown = new(),
        TestCaseFeedback = new(),
        Suggestions = new() { "Thử gửi lại sau ít phút", "Kiểm tra kết nối mạng/limit API" },
        NextSteps = new() { "Hệ thống sẽ thử lại khi AI ổn định" }
    };
}
