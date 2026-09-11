using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Api.Services;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IPushNotificationService _pushNotifications;

        public ChatController(
            AppDbContext db,
            IHubContext<ChatHub> hubContext,
            IPushNotificationService pushNotifications)
        {
            _db = db;
            _hubContext = hubContext;
            _pushNotifications = pushNotifications;
        }

        // Send message
        [HttpPost("send")]
        public async Task<ActionResult<ChatMessageDto>> SendMessage(
            [FromBody] ChatMessageDto request)
        {
            if (request.SenderId <= 0)
                return BadRequest("Invalid sender.");

            if (request.ReceiverId <= 0)
                return BadRequest("Invalid receiver.");

            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Message cannot be empty.");

            var senderExists = await _db.Users
                .AnyAsync(x => x.Id == request.SenderId);

            var receiverExists = await _db.Users
                .AnyAsync(x => x.Id == request.ReceiverId);

            if (!senderExists)
                return BadRequest("Sender not found.");

            if (!receiverExists)
                return BadRequest("Receiver not found.");

            var message = new ChatMessage
            {
                SenderId = request.SenderId,
                ReceiverId = request.ReceiverId,
                Text = request.Text.Trim(),
                SentAt = DateTime.UtcNow
            };

            _db.ChatMessages.Add(message);

            await _db.SaveChangesAsync();

            var result = new ChatMessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Text = message.Text,
                SentAt = message.SentAt
            };

            // Send to receiver group
            await _hubContext.Clients
                .Group($"user-{request.ReceiverId}")
                .SendAsync("ReceiveMessage", result);

            var senderName = await _db.Users
    .Where(x => x.Id == request.SenderId)
    .Select(x => (x.FirstName + " " + x.LastName).Trim())
    .SingleAsync();

            var receiverTokens = await _db.UserDevices
                .Where(x =>
                    x.UserId == request.ReceiverId &&
                    x.IsActive &&
                    x.Platform == "android")
                .Select(x => x.PushIdentifier)
                .ToListAsync();

            await _pushNotifications.SendChatMessageAsync(
                receiverTokens,
                request.SenderId,
                string.IsNullOrWhiteSpace(senderName)
                    ? "New message"
                    : senderName,
                result.Id,
                result.Text);

    //        // Optionally notify sender as well (for echo)
    //        await _hubContext.Clients
    //            .Group($"user-{request.SenderId}")
    //            .SendAsync("ReceiveMessage", result);

    //        var senderName = await _db.Users
    //.Where(x => x.Id == request.SenderId)
    //.Select(x => (x.FirstName + " " + x.LastName).Trim())
    //.SingleAsync();

    //        var receiverTokens = await _db.UserDevices
    //            .Where(x =>
    //                x.UserId == request.ReceiverId &&
    //                x.IsActive &&
    //                x.Platform == "android")
    //            .Select(x => x.PushIdentifier)
    //            .ToListAsync();

    //        await _pushNotifications.SendChatMessageAsync(
    //            receiverTokens,
    //            request.SenderId,
    //            string.IsNullOrWhiteSpace(senderName)
    //                ? "New message"
    //                : senderName,
    //            result.Id,
    //            result.Text);

            return Ok(result);
        }


        // Get conversation between current user and selected user
        [HttpGet("conversation/{currentUserId}/{otherUserId}")]
        public async Task<ActionResult<List<ChatMessageDto>>> GetConversation(
            int currentUserId,
            int otherUserId)
        {
            var messages = await _db.ChatMessages
                .Where(x =>
                    (x.SenderId == currentUserId &&
                     x.ReceiverId == otherUserId)
                    ||
                    (x.SenderId == otherUserId &&
                     x.ReceiverId == currentUserId)
                )
                .OrderBy(x => x.SentAt)
                .Select(x => new ChatMessageDto
                {
                    Id = x.Id,
                    SenderId = x.SenderId,
                    ReceiverId = x.ReceiverId,
                    Text = x.Text,
                    SentAt = x.SentAt
                })
                .ToListAsync();

            return Ok(messages);
        }
    }
}