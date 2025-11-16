using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FeedbackService.Application.Dtos;
using FeedbackService.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace FeedbackService.Infrastructure
{
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
            IWebHostEnvironment env)
        {
            _http = http;
            _opts = opts.Value;
            _cfg = cfg;
            _env = env;

            _http.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<FeedbackResponseDto> GenerateAsync(FeedbackRequestDto req,string Prompt ,CancellationToken ct = default)
        {
            // 🔹 Lấy API key
            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                      ?? _cfg["Gemini:ApiKey"]
                      ?? _opts.ApiKey;

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Thiếu GEMINI_API_KEY (chưa set API key cho Gemini).");

            // 🔹 Kiểm tra dữ liệu bắt buộc
            if (req.TestResults == null || req.TestResults.Count == 0)
                throw new ArgumentException("Thiếu testResults để sinh feedback bằng AI.");

            // 🔹 Chuẩn bị nội dung gửi đi
            var testResultsJson = JsonSerializer.Serialize(req.TestResults, new JsonSerializerOptions { WriteIndented = true });
            var model = string.IsNullOrWhiteSpace(_opts.Model) ? "models/gemini-2.0-flash" : _opts.Model;
            var url = $"v1beta/{model}:generateContent";

            var systemInstruction = new
            {
                parts = new[]
                {
                      new { text = Prompt }
                }
            };

            var userContent = new
            {
                role = "user",
                parts = new object[]
                {
                    new { text = $"Student: {req.StudentId}\nAssignment: {req.AssignmentTitle}\nLanguageId: {req.LanguageId}\nRubric: {req.Rubric ?? "(none)"}" },
                    new { text = $"SCORE: {req.Score}" },
                    new { text = "SOURCE CODE:\n```" + (req.SourceCode ?? "") + "```" },
                    new { text = "TEST RESULTS:\n" + testResultsJson },
                    new { text = "Hãy đưa ra nhận xét tổng quan, điểm số và gợi ý cải thiện (JSON format như schema trên)." }
                }
            };

            var body = new
            {
                systemInstruction,
                contents = new[] { userContent },
                generationConfig = new { response_mime_type = "application/json" }
            };

            if (_env.IsDevelopment())
            {
                Console.WriteLine("=== Gemini Request Body ===");
                Console.WriteLine(JsonSerializer.Serialize(body, new JsonSerializerOptions { WriteIndented = true }));
            }

            // 🔹 Gửi request với cơ chế retry
            var delays = new[] { 500, 1000, 2000 };
            HttpResponseMessage? response = null;

            for (int attempt = 0; attempt <= delays.Length; attempt++)
            {
                try
                {
                    using var msg = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                    };
                    msg.Headers.Add("x-goog-api-key", apiKey);

                    response = await _http.SendAsync(msg, ct);
                    if (response.IsSuccessStatusCode) break;

                    // Retry nếu bị quá tải
                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable || (int)response.StatusCode == 429)
                    {
                        if (attempt < delays.Length)
                        {
                            if (_env.IsDevelopment())
                                Console.WriteLine($"[Gemini] Retry {attempt + 1} due to {response.StatusCode}");
                            await Task.Delay(delays[attempt], ct);
                            continue;
                        }
                        return FallbackDto("(AI đang quá tải — dùng phản hồi tối thiểu)");
                    }

                    // Lỗi khác không retry
                    break;
                }
                catch (HttpRequestException ex)
                {
                    if (attempt < delays.Length)
                    {
                        if (_env.IsDevelopment())
                            Console.WriteLine($"[Gemini] Network error, retry {attempt + 1}: {ex.Message}");
                        await Task.Delay(delays[attempt], ct);
                        continue;
                    }
                    return FallbackDto("(Lỗi mạng tới AI — dùng phản hồi tối thiểu)");
                }
            }

            if (response == null)
                return FallbackDto("(Không nhận được phản hồi từ Gemini).");

            var payload = await response.Content.ReadAsStringAsync(ct);

            if (_env.IsDevelopment())
            {
                Console.WriteLine("=== Gemini Raw Response ===");
                Console.WriteLine(payload);
            }

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Gemini error {response.StatusCode}: {payload}");

            // 🔹 Phân tích JSON
            using var doc = JsonDocument.Parse(payload);
            var candidates = doc.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0)
                throw new InvalidOperationException("Gemini không trả candidate nào.");

            var text = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("Gemini không trả text trong response.");

            var feedback = JsonSerializer.Deserialize<FeedbackResponseDto>(text!, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return feedback ?? FallbackDto("(Không parse được phản hồi từ Gemini.)");
        }

        private static FeedbackResponseDto FallbackDto(string message) => new()
        {
            Summary = message,
            Score = 0,
            RubricBreakdown = new(),
            TestCaseFeedback = new(),
            Suggestions = new() { "Thử gửi lại sau ít phút", "Kiểm tra kết nối mạng hoặc limit API" },
            NextSteps = new() { "Hệ thống sẽ thử lại khi AI ổn định" }
        };

       
    }
}
