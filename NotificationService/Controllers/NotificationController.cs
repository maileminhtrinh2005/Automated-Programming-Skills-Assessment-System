using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/Notification")]
public class NotificationController(INotificationAppService app) : ControllerBase
{
    [Authorize]
    [HttpPost("GetNotification")]
    public async Task<IActionResult> FromFeedback([FromBody] NotificationRequestDto body, CancellationToken ct)
        => Ok(await app.CreateFromFeedbackAsync(body, ct));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(await app.GetAllAsync(take, ct));


    // lay thong bao cho student la doc hay chua doc
    [Authorize]
    [HttpGet("unread")]
    public IActionResult GetUnread(int studentId)
    {
        var unread = _db.GeneratedNotifications
            .Where(n => n.StudentId == studentId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAtUtc)
            .ToList();
        return Ok(unread);
    }

    // ✅ Đánh dấu là đã đọc
    [Authorize]
    [HttpPost("markasread")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var noti = await _db.GeneratedNotifications.FindAsync(id);
        if (noti == null) return NotFound();

        noti.IsRead = true;
        await _db.SaveChangesAsync();
        return Ok();
    }
}