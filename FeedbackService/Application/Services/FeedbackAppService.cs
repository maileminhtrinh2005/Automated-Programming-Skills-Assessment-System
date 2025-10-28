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
        private readonly FeedbackPushService _pushService;  // ✅ thêm push service

        private const string MODEL = "models/gemini-2.0-flash";

        public FeedbackAppService(
            IFeedbackGenerator generator,
            AppDbContext db,
            HttpClient http,
            IConfiguration cfg,
            FeedbackPushService pushService) // ✅ inject push service
        {
            _generator = generator;
            _db = db;
            _http = http;
            _cfg = cfg;
            _pushService = pushService;

            _http.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<FeedbackResponseDto> GenerateAsync(
            FeedbackRequestDto req,
            string systemPrompt,
            CancellationToken ct)
        {
            FeedbackResponseDto result;

            // 🧠 Không có test case -> gọi Gemini sinh NHẬN XÉT TỔNG QUÁT (không chấm điểm)
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

                // Chuẩn hoá output: không chấm điểm, không rubric, không testcase
                result = ai ?? new FeedbackResponseDto { Summary = "Không đọc được phản hồi từ AI." };
                result.Score = 0;
                result.RubricBreakdown = new List<RubricItemDto>();
                result.TestCaseFeedback = null;

                // ✅ Lưu DB
                await SaveFeedbackAsync(req, result, ct);

                // ✅ Push event qua RabbitMQ
                await _pushService.PushFeedbackAsync(
                    req.StudentId,
                    req.SubmissionId,
                    req.AssignmentTitle ?? "Bài nộp không rõ",
                    result.Summary ?? "(Không có nội dung phản hồi)",
                    result.Score
                );

                return result;
            }

            // ✅ Có test case -> dùng generator AI chi tiết
            result = await _generator.GenerateAsync(req, ct);

            // ✅ Lưu DB
            await SaveFeedbackAsync(req, result, ct);

            // ✅ Push qua RabbitMQ
            await _pushService.PushFeedbackAsync(
                req.StudentId,
                req.SubmissionId,
                req.AssignmentTitle ?? "Bài nộp không rõ",
                result.Summary ?? "(Không có nội dung phản hồi)",
                result.Score
            );

            return result;
        }

        private async Task SaveFeedbackAsync(FeedbackRequestDto req, FeedbackResponseDto result, CancellationToken ct)
        {
            var record = new GeneratedFeedbackRecord
            {
                StudentId = req.StudentId,
                AssignmentTitle = req.AssignmentTitle,
                Summary = result.Summary ?? "(no summary)",
                Score = result.Score,
                RawJson = JsonSerializer.Serialize(result),
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.GeneratedFeedbacks.Add(record);
            await _db.SaveChangesAsync(ct);

            Console.WriteLine($"[FeedbackAppService] 💾 Feedback đã được lưu vào DB cho Assignment '{req.AssignmentTitle}'.");
        }

        public Task<GeneratedFeedbackDto> GenerateForStudentAssignmentAsync(
            int studentId, int assignmentId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<object> GenerateBulkFeedbackAsync(BulkFeedbackRequestDto dto, CancellationToken ct)
        {
            if (dto == null || dto.Submissions == null || dto.Submissions.Count == 0)
            {
                var empty = new
                {
                    summary = "Không có submission nào để nhận xét.",
                    overallProgress = "Không xác định",
                    perSubmissionFeedback = new object[0]
                };
                return Task.FromResult<object>(empty);
            }

            // 🔹 Tính điểm trung bình
            double avgScore = dto.Submissions
                .Where(s => s.Score.HasValue)
                .Select(s => s.Score.Value)
                .DefaultIfEmpty(0)
                .Average();

            // 🔹 Sinh nhận xét tổng quát
            string summary;
            if (avgScore >= 4)
                summary = "Sinh viên có sự tiến bộ rõ rệt, các bài sau đạt điểm cao và ổn định hơn.";
            else if (avgScore >= 3)
                summary = "Sinh viên đang cải thiện, cần chú ý hơn về tối ưu thuật toán và cách trình bày mã.";
            else if (avgScore >= 2)
                summary = "Kết quả chưa ổn định, cần xem lại logic và luyện thêm bài tập cơ bản.";
            else
                summary = "Cần cố gắng hơn trong việc hoàn thiện bài và cải thiện chất lượng code.";

            // 🔹 Nhận xét tiến bộ tổng quát
            string overallProgress = avgScore switch
            {
                >= 4 => "Xuất sắc",
                >= 3 => "Khá tốt",
                >= 2 => "Đang tiến bộ",
                _ => "Cần cải thiện"
            };

            // 🔹 Nhận xét từng submission
            var perSubmissionFeedback = dto.Submissions.Select(s => new
            {
                s.SubmissionId,
                s.AssignmentTitle,
                s.Score,
                s.Status,
                comment = s.Score switch
                {
                    >= 4 => "Hoàn thành rất tốt, logic đúng và rõ ràng.",
                    >= 3 => "Đạt yêu cầu, nhưng có thể tối ưu hơn.",
                    >= 2 => "Bài còn một số lỗi nhỏ hoặc chưa xử lý hết các case.",
                    _ => "Bài làm chưa đạt yêu cầu, cần luyện thêm."
                }
            }).ToList();

            var result = new
            {
                summary,
                overallProgress,
                perSubmissionFeedback
            };

            return Task.FromResult<object>(result);
        }
    }
}
