using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


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

    // 🟢 Lấy thông báo chưa đọc
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

    // 🟢 Đánh dấu là đã đọc
    [Authorize]
    [HttpPost("markasread")]
    public async Task<IActionResult> MarkAsRead([FromQuery] Guid id)
    {
        // ✅ Kiểm tra có tồn tại không
        var noti = await _db.GeneratedNotifications
            .FirstOrDefaultAsync(n => n.Id == id);

        if (noti == null)
            return NotFound($"Không tìm thấy thông báo với ID = {id}");

        // ✅ Đánh dấu là đã đọc
        noti.IsRead = true;

        // ✅ Ghi lại thay đổi
        _db.GeneratedNotifications.Update(noti);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Đã đánh dấu là đã đọc", id });
    }
}
