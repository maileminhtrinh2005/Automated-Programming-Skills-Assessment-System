using FeedbackService.Application.Constants;
using FeedbackService.Application.Dtos;
using FeedbackService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace FeedbackService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackAppService _ai;
    private readonly IManualFeedbackService _manual;

    public FeedbackController(IFeedbackAppService ai, IManualFeedbackService manual)
    {
        _ai = ai;
        _manual = manual;
    }


    [Authorize(Roles = "Lecturer, Admin")]
    [HttpPost("feedbacksubmit")]
    public async Task<IActionResult> GenerateGeneral([FromBody] FeedbackRequestDto dto, CancellationToken ct)
    {
        try
        {
            // ❗ Không có test case -> FeedbackAppService sẽ tự hiểu là chấm tổng quát
            dto.TestResults = null;

            var result = await _ai.GenerateAsync(dto, Prompt.GeneralFeedback, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ✍️ [2] FEEDBACK THỦ CÔNG — giảng viên nhập tay
    [Authorize(Roles = "Lecturer, Admin")]
    [HttpPost("manual")]// luu db
    public async Task<IActionResult> Manual([FromBody] ManualFeedbackRequestDto dto, CancellationToken ct)
    {
        try
        {
            var saved = await _manual.CreateAsync(dto, ct);
            return Ok(saved);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
    [Authorize(Roles = "Lecturer, Admin")]
    [HttpPost("manual/sendreviewed")]// gui feedback da duoc review
    public async Task<IActionResult> SendReviewedFeedback([FromBody] ManualFeedbackDto dto)
    {
        try
        {
            await _manual.SendReviewedFeedbackAsync(dto);
            return Ok(new { message = "✅ Reviewed feedback sent successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [Authorize(Roles = "Lecturer, Admin")]
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateFull([FromBody] FeedbackAutoRequestDto req, CancellationToken ct)
    {
        try
        {
            if (req.StudentId <= 0 || req.AssignmentId <= 0)
                return BadRequest("Thiếu StudentId hoặc AssignmentId.");

            var http = new HttpClient();

            // 🔹 Gọi qua AssignmentService (5267)
            var assignmentUrl = $"http://localhost:5267/api/Assignment/GetAssignmentById/{req.AssignmentId}";
            var assignment = await http.GetFromJsonAsync<AssignmentDto>(assignmentUrl, ct);
            if (assignment is null)
                return NotFound($"Không tìm thấy bài tập {req.AssignmentId}");

            // 🔹 Gọi qua SubmissionService (5090)
            var subUrl = $"http://localhost:5090/api/Submission/GetYourSubmission/{req.StudentId}";
            var submissions = await http.GetFromJsonAsync<List<SubmissionDto>>(subUrl, ct);
            if (submissions is null || submissions.Count == 0)
                return NotFound("Không tìm thấy submission của học viên.");

            var submission = submissions.FirstOrDefault(s => s.AssignmentId == req.AssignmentId);
            if (submission is null)
                return NotFound("Sinh viên chưa nộp bài này.");

            // 🔹 Gọi Gemini để sinh nhận xét tổng quát
            var aiRequest = new FeedbackRequestDto
            {
                StudentId = req.StudentId,
                AssignmentTitle = assignment.Title,
                Rubric = "Đúng 60, Hiệu năng 20, Style 20",
                SourceCode = submission.SourceCode,
                LanguageId = submission.LanguageId,
                TestResults = null
            };

            var result = await _ai.GenerateAsync(aiRequest, Prompt.GeneralFeedback, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
    [HttpPost("generate/bulk")]
    public async Task<IActionResult> GenerateBulk([FromBody] BulkFeedbackRequestDto request, CancellationToken ct)
    {
        if (request == null || request.Submissions == null || request.Submissions.Count == 0)
            return BadRequest("Không có submission nào để nhận xét.");

        var result = await _ai.GenerateBulkFeedbackAsync(request, ct); //
        return Ok(result);
    }
}
