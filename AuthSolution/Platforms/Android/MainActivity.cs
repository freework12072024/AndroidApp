using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace AuthSolution;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize |
                           ConfigChanges.Orientation |
                           ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize |
                           ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public const string ChatChannelId = "chat_messages";
    public const string ChatUserIdExtra = "chatUserId";

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        CreateChatChannel();
        HandleNotificationIntent(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

        if (intent != null)
            Intent = intent;

        HandleNotificationIntent(intent);
    }

    private void CreateChatChannel()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
            return;

        var channel = new NotificationChannel(
            ChatChannelId,
            "Chat messages",
            NotificationImportance.High)
        {
            Description = "New message alerts",
            LockscreenVisibility = NotificationVisibility.Private
        };

        channel.EnableVibration(true);

        var manager =
            GetSystemService(NotificationService) as NotificationManager;

        manager?.CreateNotificationChannel(channel);
    }

    private static void HandleNotificationIntent(Intent? intent)
    {
        var chatUserId = intent?.GetIntExtra(ChatUserIdExtra, 0) ?? 0;

        if (chatUserId > 0)
            Preferences.Set("PendingChatUserId", chatUserId);
    }
}