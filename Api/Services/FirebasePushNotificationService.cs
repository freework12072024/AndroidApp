using FirebaseAdmin.Messaging;

namespace Api.Services;

public sealed class FirebasePushNotificationService
    : IPushNotificationService
{
    public async Task SendChatMessageAsync(
        IReadOnlyCollection<string> tokens,
        int senderId,
        string senderName,
        int messageId,
        string text)
    {
        if (tokens.Count == 0)
            return;

        var body = text.Length > 160 ? text[..160] : text;

        var message = new MulticastMessage
        {
            Tokens = tokens,

            // Background / screen-off state mein Android system
            // isi notification payload ko show karega.
            Notification = new Notification
            {
                Title = senderName,
                Body = body
            },

            Data = new Dictionary<string, string>
            {
                ["type"] = "chat",
                ["chatUserId"] = senderId.ToString(),
                ["messageId"] = messageId.ToString(),
                ["senderName"] = senderName,
                ["text"] = body
            },

            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    ChannelId = "chat_messages",
                    Sound = "default"
                }
            }
        };

        await FirebaseMessaging.DefaultInstance
            .SendEachForMulticastAsync(message);
    }
}