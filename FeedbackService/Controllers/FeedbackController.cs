using FeedbackService.Application.Dtos;
using FeedbackService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

    // AI feedback: CHỈ trả về, KHÔNG lưu DB
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] FeedbackRequestDto dto, CancellationToken ct)
    {
        if (dto.TestResults is null || dto.TestResults.Count == 0)
            return BadRequest(new { error = "Vui lòng truyền 'testResults' để test riêng Feedback (AI)." });

        var result = await _ai.GenerateAsync(dto, ct);
        return Ok(result);
    }

    // Manual feedback: LƯU DB
    [HttpPost("manual")]
    public async Task<IActionResult> Manual([FromBody] ManualFeedbackRequestDto dto, CancellationToken ct)
    {
        var saved = await _manual.CreateAsync(dto, ct);
        return Ok(saved);
    }
}
