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
#pragma warning disable CS0618
#pragma warning disable CS0672
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

        intent.PutExtra(
            MainActivity.ChatUserIdExtra,
            chatUserId);

        intent.SetAction(
            $"chat-{chatUserId}-{messageId}");

        intent.AddFlags(
            ActivityFlags.ClearTop |
            ActivityFlags.SingleTop);

        // Android 23+ ke liye Immutable
        var pendingIntentFlags =
            PendingIntentFlags.UpdateCurrent;

        if (OperatingSystem.IsAndroidVersionAtLeast(23))
        {
            pendingIntentFlags |=
                PendingIntentFlags.Immutable;
        }

        var pendingIntent = PendingIntent.GetActivity(
            this,
            messageId,
            intent,
            pendingIntentFlags);
#pragma warning disable CS8602
        var builder = new NotificationCompat.Builder(
            this,
            MainActivity.ChatChannelId);

        builder
            .SetSmallIcon(Resource.Drawable.notification_icon)
            .SetContentTitle(title)
            .SetContentText(text)
            .SetStyle(
                new NotificationCompat.BigTextStyle()
                    .BigText(text))
            .SetAutoCancel(true)
            .SetCategory(
                NotificationCompat.CategoryMessage)
            .SetPriority(
                (int)NotificationPriority.High)
            .SetContentIntent(pendingIntent);

        var notification = builder.Build();

        var notificationManager =
            NotificationManagerCompat.From(this);

        notificationManager.Notify(
            messageId == 0
                ? chatUserId
                : messageId,
            notification);
    }
}