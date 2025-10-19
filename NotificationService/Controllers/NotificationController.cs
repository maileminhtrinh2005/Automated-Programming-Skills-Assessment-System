using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/Notification")]
public class NotificationController(INotificationAppService app) : ControllerBase
{
    [HttpPost("GetNotification")]
    public async Task<IActionResult> FromFeedback([FromBody] NotificationRequestDto body, CancellationToken ct)
        => Ok(await app.CreateFromFeedbackAsync(body, ct));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(await app.GetAllAsync(take, ct));
}
