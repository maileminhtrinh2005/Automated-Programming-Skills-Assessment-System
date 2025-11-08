using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using FeedbackService.Application.Constants;
using FeedbackService.Application.Dtos;
using FeedbackService.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FeedbackService.Application.Services
{
    public class TestcaseFeedbackGenerator : ITestcaseFeedbackGenerator
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _cfg;

        private const string MODEL = "models/gemini-2.0-flash";

        public TestcaseFeedbackGenerator(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _cfg = cfg;

            _http.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<FeedbackResponseDto> GenerateAsync(
            TestcaseFeedbackRequestDto req,
            string prompt,
            CancellationToken ct)
        {
            if (req.TestResults == null || req.TestResults.Count == 0)
                throw new ArgumentException("Thiếu TestResults để chấm chi tiết.");

            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                       ?? _cfg["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Thiếu GEMINI_API_KEY.");

            // 🧩 Chuẩn hóa dữ liệu test case (để Gemini dễ hiểu)
            var testResultsJson = JsonSerializer.Serialize(req.TestResults,
                new JsonSerializerOptions { WriteIndented = true });

            // 🧠 Prompt gửi sang Gemini
            var body = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = prompt ?? Prompt.PerTestcaseFeedback } }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = $"Assignment: {req.AssignmentTitle}" },
                            new { text = $"SourceCode:\n```{req.SourceCode ?? ""}```" },
                            new { text = $"Dưới đây là danh sách test case và kết quả:\n{testResultsJson}" },
                            new { text = "Hãy viết nhận xét JSON theo hướng dẫn trong prompt." }
                        }
                    }
                },
                generationConfig = new { response_mime_type = "application/json" }
            };

            // 📤 Gọi Gemini API
            using var msg = new HttpRequestMessage(HttpMethod.Post, $"v1beta/{MODEL}:generateContent")
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            msg.Headers.Add("x-goog-api-key", apiKey);

            var res = await _http.SendAsync(msg, ct);
            var payload = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"Gemini error {res.StatusCode}: {payload}");

            // 🧩 Phân tích phản hồi từ Gemini
            using var doc = JsonDocument.Parse(payload);
            var text = doc.RootElement.GetProperty("candidates")[0]
                .GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

            var ai = JsonSerializer.Deserialize<FeedbackResponseDto>(
                text!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (ai == null)
            {
                return new FeedbackResponseDto
                {
                    Summary = "Không đọc được phản hồi từ AI.",
                    TestCaseFeedback = new List<TestCaseFeedbackDto>()
                };
            }

            Console.WriteLine("✅ [Gemini] Sinh nhận xét chi tiết hoàn tất!");
            Console.WriteLine(text);
            if (req.TestResults != null && ai.TestCaseFeedback != null)
            {
                for (int i = 0; i < ai.TestCaseFeedback.Count; i++)
                {
                    var src = req.TestResults.ElementAtOrDefault(i);
                    var dst = ai.TestCaseFeedback[i];

                    if (src != null)
                    {
                        dst.Input = src.Input ?? "(Không có)";
                        dst.ExpectedOutput = src.ExpectedOutput ?? "(Không có)";
                        dst.Status ??= src.Status ?? "Chưa chạy";
                    }
                }
            }

            return ai;
        }
    }
}
