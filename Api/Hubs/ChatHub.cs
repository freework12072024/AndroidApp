using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    public class ChatHub : Hub
    {
        public async Task RegisterUser(int userId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"user-{userId}");
        }
    }
}