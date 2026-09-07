using Android.App;
using Android.Content;
using AndroidX.Core.App;
using AuthSolution.Services;
using Firebase.Messaging;

namespace AuthSolution.Platforms.Android;

[Service(Exported = false)]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public sealed class ChatFirebaseMessagingService : FirebaseMessagingService
{
    public override void OnNewToken(string token)
    {
        base.OnNewToken(token);

        Preferences.Set(PushTokenService.FcmTokenKey, token);

        _ = Task.Run(async () =>
        {
            try
            {
                await new PushTokenService(new ApiClient())
                    .RegisterStoredTokenAsync();
            }
            catch
            {
                // Login flow will retry registration.
            }
        });
    }

    public override void OnMessageReceived(RemoteMessage message)
    {
        base.OnMessageReceived(message);

        var data = message.Data;

        var senderName = data.TryGetValue("senderName", out var name)
            ? name
            : "New message";

        var text = data.TryGetValue("text", out var body)
            ? body
            : "You received a message";

        var chatUserId = data.TryGetValue("chatUserId", out var rawChatUserId) &&
                         int.TryParse(rawChatUserId, out var parsedChatUserId)
            ? parsedChatUserId
            : 0;

        var messageId = data.TryGetValue("messageId", out var rawMessageId) &&
                        int.TryParse(rawMessageId, out var parsedMessageId)
            ? parsedMessageId
            : 0;

        ShowNotification(senderName, text, chatUserId, messageId);
    }

    private void ShowNotification(
        string title,
        string text,
        int chatUserId,
        int messageId)
    {
        var intent = new Intent(this, typeof(MainActivity));
        intent.PutExtra(MainActivity.ChatUserIdExtra, chatUserId);
        intent.SetAction($"chat-{chatUserId}-{messageId}");
        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

        var pendingIntent = PendingIntent.GetActivity(
            this,
            messageId,
            intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var notification = new NotificationCompat.Builder(
                this,
                MainActivity.ChatChannelId)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetContentTitle(title)
            .SetContentText(text)
            .SetStyle(new NotificationCompat.BigTextStyle().BigText(text))
            .SetAutoCancel(true)
            .SetCategory(NotificationCompat.CategoryMessage)
            .SetPriority((int)NotificationPriority.High)
            .SetContentIntent(pendingIntent)
            .Build();

        NotificationManagerCompat.From(this)
            .Notify(messageId == 0 ? chatUserId : messageId, notification);
    }
}