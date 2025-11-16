
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationAppService _app;
    private readonly AppDbContext _db;

    public NotificationController(INotificationAppService app, AppDbContext db)
    {
        _app = app;
        _db = db;
    }

    [Authorize]
    [HttpPost("GetNotification")]
    public async Task<IActionResult> FromFeedback([FromBody] NotificationRequestDto body, CancellationToken ct)
        => Ok(await _app.CreateFromFeedbackAsync(body, ct));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(await _app.GetAllAsync(take, ct));

    [Authorize]
    [HttpGet("unread")]
    public IActionResult GetUnread()
    {
        var userIdClaim =
            User.FindFirst("userId") ??
            User.FindFirst("nameid") ??
            User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized(new { message = "Token không hợp lệ: không có userId." });

        int userId = int.Parse(userIdClaim.Value);

        var unread = _db.GeneratedNotifications
            .Where(n =>
                (n.StudentId == userId || n.StudentId == 0) &&
                !n.IsRead &&
                //loai bo cac thong bao ko lien quan
                !n.Message.Contains("Assignment:") &&
                !n.Message.Contains("Submission:") &&
                !n.Message.Contains("TestCases:") &&
                !n.Message.Contains("LanguageId")
            )
            .OrderByDescending(n => n.CreatedAtUtc)
            .ToList();

        return Ok(unread);
    }

    // ĐÁNH DẤU ĐÃ ĐỌC
    [Authorize]
    [HttpPost("markasread")]
    public async Task<IActionResult> MarkAsRead([FromQuery] Guid id)
    {
        var noti = await _db.GeneratedNotifications
            .FirstOrDefaultAsync(n => n.Id == id);

        if (noti == null)
            return NotFound($"Không tìm thấy thông báo với ID = {id}");

        noti.IsRead = true;
        _db.GeneratedNotifications.Update(noti);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Đã đánh dấu là đã đọc", id });
    }
}

