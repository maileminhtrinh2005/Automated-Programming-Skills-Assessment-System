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


            var testResultsJson = JsonSerializer.Serialize(req.TestResults,
                new JsonSerializerOptions { WriteIndented = true });

   
            var body = new
            {
                system_instruction = new
                {
                    parts = new[] {
                        new {
                            text = prompt ??
                            $"{Prompt.PerTestcaseFeedback}\n" +
                            "Hãy sinh phản hồi JSON có dạng:\n" +
                            "{\n" +
                            "  \"summary\": string,\n" +
                            "  \"score\": number,\n" +
                            "  \"rubricBreakdown\": [{\"criterion\": string, \"score\": number, \"max\": number}],\n" +
                            "  \"testCaseFeedback\": [{\"name\": string, \"comment\": string, \"score\": number, \"status\": string}]\n" +
                            "}\n" +
                            "⚠️ Bắt buộc phải có 'testCaseFeedback' chứa nhận xét cho từng test case, và sử dụng tiếng Việt."
                        }
                    }
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
                            new { text = "Hãy trả lời đúng theo cấu trúc JSON ở trên, và mô tả bằng tiếng Việt." }
                        }
                    }
                },
                generationConfig = new { response_mime_type = "application/json" }
            };

            using var msg = new HttpRequestMessage(HttpMethod.Post, $"v1beta/{MODEL}:generateContent")
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            msg.Headers.Add("x-goog-api-key", apiKey);

            var res = await _http.SendAsync(msg, ct);
            var payload = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"Gemini error {res.StatusCode}: {payload}");


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

 
            if (ai.TestCaseFeedback == null || ai.TestCaseFeedback.Count == 0)
            {
                ai.TestCaseFeedback = req.TestResults.Select((t, i) => new TestCaseFeedbackDto
                {
                    Name = $"Test case {i + 1}",
                    Comment = "(Không có nhận xét)",
                    Status = t.Status ?? "Chưa chạy",
                    Input = t.Input ?? "Không có",
                    ExpectedOutput = t.ExpectedOutput ?? "—"
                }).ToList();
            }

            Console.WriteLine("✅ [Gemini] Sinh nhận xét chi tiết hoàn tất!");
            Console.WriteLine(text);

            for (int i = 0; i < ai.TestCaseFeedback.Count; i++)
            {
                var src = req.TestResults.ElementAtOrDefault(i);
                var dst = ai.TestCaseFeedback[i];

                if (src != null)
                {
                    dst.Input = src.Input ?? "(Không có)";
                    dst.ExpectedOutput = src.ExpectedOutput ?? "(Không có)";
                }

                var comment = dst.Comment?.ToLower() ?? "";

                if (comment.Contains("pass") || comment.Contains("thành công") || comment.Contains("đúng"))
                    dst.Status = "Đúng";
                else if (comment.Contains("fail") || comment.Contains("lỗi") || comment.Contains("sai"))
                    dst.Status = "Sai";
                else
                    dst.Status ??= src?.Status ?? "Chưa chạy";
            }

            
            for (int i = 0; i < ai.TestCaseFeedback.Count; i++)
            {
                var src = req.TestResults.ElementAtOrDefault(i);
                var dst = ai.TestCaseFeedback[i];
                if (dst != null && string.IsNullOrWhiteSpace(dst.Status))
                {
                    dst.Status = src?.Status ?? "Chưa chạy";
                }
            }

            return ai;
        }
    }
}
