using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/device")]
public sealed class DeviceController : ControllerBase
{
    private readonly AppDbContext _db;

    public DeviceController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        DeviceRegistrationRequest request)
    {
        var rawUserId = User.FindFirst(
            JwtRegisteredClaimNames.Sub)?.Value;

        if (!int.TryParse(rawUserId, out var userId) ||
            string.IsNullOrWhiteSpace(request.Token))
            return BadRequest();

        var device = await _db.UserDevices.SingleOrDefaultAsync(x =>
            x.UserId == userId &&
            x.PushIdentifier == request.Token);

        if (device == null)
        {
            _db.UserDevices.Add(new UserDevice
            {
                UserId = userId,
                DeviceId = request.Token,
                PushIdentifier = request.Token,
                Platform = "android",
                IsActive = true,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            device.IsActive = true;
            device.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}