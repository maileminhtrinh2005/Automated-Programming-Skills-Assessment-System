using FeedbackService.Application.Constants;
using FeedbackService.Application.Dtos;
using FeedbackService.Application.Interfaces;
using FeedbackService.Domain.Entities;
using FeedbackService.Infrastructure.Persistence;
using ShareLibrary;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FeedbackService.Application.Services
{
    public class FeedbackAppService : IFeedbackAppService
    {
        private readonly IFeedbackGenerator _generator;
        private readonly AppDbContext _db;
        private readonly HttpClient _http;
        private readonly IConfiguration _cfg;
        private readonly FeedbackPushService _pushService;

        private const string MODEL = "models/gemini-2.0-flash";

        public FeedbackAppService(
            IFeedbackGenerator generator,
            AppDbContext db,
            HttpClient http,
            IConfiguration cfg,
            FeedbackPushService pushService)
        {
            _generator = generator;
            _db = db;
            _http = http;
            _cfg = cfg;
            _pushService = pushService;

            _http.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // =====================================
        // 🧠 Sinh nhận xét (AI) + Lưu DB chi tiết
        // =====================================
        public async Task<FeedbackResponseDto> GenerateAsync(
            FeedbackRequestDto req,
            string systemPrompt,
            CancellationToken ct)
        {
            FeedbackResponseDto result;

            // Trường hợp không có test case → nhận xét tổng quát
            if (req.TestResults is null || req.TestResults.Count == 0)
            {
                var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                           ?? _cfg["Gemini:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                    throw new InvalidOperationException("Thiếu GEMINI_API_KEY.");

                var body = new
                {
                    system_instruction = new
                    {
                        parts = new[] { new { text = string.IsNullOrWhiteSpace(systemPrompt) ? Prompt.GeneralFeedback : systemPrompt } }
                    },
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new { text = $"StudentId: {req.StudentId}" },
                                new { text = $"Assignment: {req.AssignmentTitle}" },
                                new { text = $"SourceCode:\n```{req.SourceCode ?? ""}```" },
                                new { text = "Hãy viết nhận xét tổng quan (JSON)." }
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

                result = ai ?? new FeedbackResponseDto { Summary = "Không đọc được phản hồi từ AI." };

                // ✅ đảm bảo Score không null
                result.Score ??= 0.0;
                result.RubricBreakdown ??= new List<RubricItemDto>();
                result.TestCaseFeedback = null;

                // 🔹 Lưu cả GeneratedFeedback và DetailedFeedback
                await SaveFeedbackAsync(req, result, ct);
                await SaveDetailedFeedbackAsync(req, result, ct);

                // 🔹 Gửi thông báo realtime
                await _pushService.PushFeedbackAsync(
                    req.StudentId,
                    req.SubmissionId,
                    req.AssignmentTitle ?? "Bài nộp không rõ",
                    result.Summary ?? "(Không có nội dung phản hồi)",
                    result.Score ?? 0.0   // ✅ ép nullable
                );

                return result;
            }

            // Có test case → dùng generator riêng
            result = await _generator.GenerateAsync(req, ct);

            // ✅ đảm bảo Score không null
            result.Score ??= 0.0;

            // 🔹 Lưu cả hai bảng
            await SaveFeedbackAsync(req, result, ct);
            await SaveDetailedFeedbackAsync(req, result, ct);

            await _pushService.PushFeedbackAsync(
                req.StudentId,
                req.SubmissionId,
                req.AssignmentTitle ?? "Bài nộp không rõ",
                result.Summary ?? "(Không có nội dung phản hồi)",
                result.Score ?? 0.0    // ✅ ép nullable
            );

            return result;
        }

        // =====================================
        // 💾 Lưu feedback tổng quát (GeneratedFeedbacks)
        // =====================================
        private async Task SaveFeedbackAsync(FeedbackRequestDto req, FeedbackResponseDto result, CancellationToken ct)
        {
            var record = new GeneratedFeedbackRecord
            {
                StudentId = req.StudentId,
                AssignmentTitle = req.AssignmentTitle,
                Summary = result.Summary ?? "(no summary)",
                // ✅ Ép kiểu rõ ràng
                Score = (int?)Math.Round(result.Score ?? 0),
                RawJson = JsonSerializer.Serialize(result),
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.GeneratedFeedbacks.Add(record);
            await _db.SaveChangesAsync(ct);

            Console.WriteLine($"💾 [FeedbackAppService] Saved general feedback for '{req.AssignmentTitle}'.");
        }

        // =====================================
        // 💾 Lưu feedback chi tiết (DetailedFeedbacks)
        // =====================================
        private async Task SaveDetailedFeedbackAsync(FeedbackRequestDto req, FeedbackResponseDto result, CancellationToken ct)
        {
            var detail = new DetailedFeedback
            {
                StudentId = req.StudentId,
                SubmissionId = req.SubmissionId,
                AssignmentTitle = req.AssignmentTitle ?? "(Không rõ bài tập)",
                Summary = result.Summary,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.DetailedFeedbacks.Add(detail);
            await _db.SaveChangesAsync(ct);

            Console.WriteLine($"💾 [FeedbackAppService] Saved detailed feedback Id={detail.Id} for student {req.StudentId}.");
        }

        // =====================================
        // 📊 Nhận xét tổng quát (Bulk)
        // =====================================
        public async Task<object> GenerateBulkFeedbackAsync(BulkFeedbackRequestDto dto, CancellationToken ct)
        {
            if (dto == null || dto.Submissions == null || dto.Submissions.Count == 0)
                return new { summary = "Không có submission nào để nhận xét.", overallProgress = "Không xác định" };

            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? _cfg["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Thiếu GEMINI_API_KEY.");

            var studentSummaries = dto.Submissions.Select(s => new
            {
                s.AssignmentTitle,
                s.Score,
                s.Status,
                s.SubmissionId
            }).ToList();

            var body = new
            {
                system_instruction = new { parts = new[] { new { text = Prompt.ProgressFeedback } } },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = "Dưới đây là danh sách bài nộp của sinh viên, mỗi bài có tiêu đề và điểm số:" },
                            new { text = JsonSerializer.Serialize(studentSummaries) },
                            new { text = "Hãy nhận xét tổng quát tiến trình học tập của sinh viên theo định dạng JSON." }
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
                .GetProperty("content").GetProperty("parts")[0]
                .GetProperty("text").GetString();

            var result = JsonSerializer.Deserialize<object>(text!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            string summary = "(Không có tóm tắt)";
            try
            {
                using var jd = JsonDocument.Parse(text!);
                if (jd.RootElement.TryGetProperty("summary", out var s)) summary = s.GetString() ?? summary;
            }
            catch { }

            var record = new GeneratedFeedbackRecord
            {
                StudentId = dto.StudentId,
                AssignmentTitle = "[Bulk] Tổng quát tiến trình học tập",
                Summary = summary,
                Score = 0,
                RawJson = text!,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.GeneratedFeedbacks.Add(record);
            await _db.SaveChangesAsync(ct);

            await _pushService.PushFeedbackAsync(dto.StudentId, 0, "[Bulk] Tổng quát tiến trình học tập", summary, 0.0);

            return result!;
        }

        public Task<GeneratedFeedbackDto> GenerateForStudentAssignmentAsync(int studentId, int assignmentId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
