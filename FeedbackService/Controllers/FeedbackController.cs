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

            var result = await _ai.GenerateAsync(dto, Prompt.ProgressFeedback, ct);
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
    public async Task<IActionResult> GenerateFull([FromBody] FeedbackRequestDto req, CancellationToken ct)
    {
        try
        {
            // 🔹 Kiểm tra dữ liệu đầu vào
            if (req.StudentId <= 0)
                return BadRequest("Thiếu StudentId.");
            if (string.IsNullOrWhiteSpace(req.AssignmentTitle))
                return BadRequest("Thiếu AssignmentTitle.");
            if (req.SubmissionId <= 0)
                return BadRequest("Thiếu SubmissionId.");
            if (string.IsNullOrWhiteSpace(req.SourceCode))
                return BadRequest("Thiếu SourceCode.");

            // ✅ Chọn prompt phù hợp
            string prompt = (req.TestResults != null && req.TestResults.Count > 0)
                ? Prompt.ProgressFeedback
                : Prompt.GeneralFeedback;

            // ✅ Gọi AI sinh phản hồi (Gemini)
            var result = await _ai.GenerateAsync(req, prompt, ct);

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
