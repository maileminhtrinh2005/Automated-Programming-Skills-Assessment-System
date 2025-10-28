using FeedbackService.Application.Dtos;
using FeedbackService.Application.Interfaces;
using FeedbackService.Infrastructure;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class GeminiTestcaseFeedbackGenerator : ITestcaseFeedbackGenerator
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;
    private readonly IWebHostEnvironment _env;
    private readonly string _model;

    public GeminiTestcaseFeedbackGenerator(HttpClient http, IConfiguration cfg, IWebHostEnvironment env, IOptions<GeminiOptions> opts)
    {
        _http = http;
        _cfg = cfg;
        _env = env;
        _model = string.IsNullOrWhiteSpace(opts.Value.Model) ? "models/gemini-2.0-flash" : opts.Value.Model;

        _http.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<FeedbackResponseDto> GenerateAsync(TestcaseFeedbackRequestDto req, string prompt, CancellationToken ct)
    {
        if (req.TestResults is null || req.TestResults.Count == 0)
            throw new ArgumentException("Thiếu TestResults cho chấm chi tiết.");

        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                   ?? _cfg["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Missing GEMINI_API_KEY.");

        var testsJson = JsonSerializer.Serialize(req.TestResults, new JsonSerializerOptions { WriteIndented = true });

        var systemInstruction = new
        {
            parts = new[] { new { text = prompt } }
        };

        var userParts = new object[]
        {
            new { text = $"StudentId: {req.StudentId}\nAssignmentId: {req.AssignmentId}\nSubmissionId: {req.SubmissionId}" },
            new { text = "SOURCE CODE:\n```" + (req.SourceCode ?? "") + "```" },
            new { text = "TEST RESULTS:\n" + testsJson },
            new { text = "Respond JSON ONLY." }
        };

        var bodyObj = new
        {
            systemInstruction,
            contents = new object[] { new { role = "user", parts = userParts } },
            generationConfig = new { response_mime_type = "application/json" }
        };

        using var msg = new HttpRequestMessage(HttpMethod.Post, $"v1beta/{_model}:generateContent")
        {
            Content = new StringContent(JsonSerializer.Serialize(bodyObj), Encoding.UTF8, "application/json")
        };
        msg.Headers.Add("x-goog-api-key", apiKey);

        var res = await _http.SendAsync(msg, ct);
        var payload = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"Gemini error {res.StatusCode}: {payload}");

        using var doc = JsonDocument.Parse(payload);
        var text = doc.RootElement.GetProperty("candidates")[0]
            .GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

        var dto = JsonSerializer.Deserialize<FeedbackResponseDto>(text!,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return dto ?? new FeedbackResponseDto { Summary = "Không parse được phản hồi AI.", Score = 0 };
    }
}
