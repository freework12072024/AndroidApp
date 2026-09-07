using AuthSolution.Services;
using System.Text.Json;

#if ANDROID
using AuthSolution.Platforms.Android;
#endif

namespace AuthSolution;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly ApiClient _api;
    private readonly RealtimeChatService _realtimeChatService;
    private readonly PushTokenService _pushTokenService;

    public LoginPage(
        AuthService auth,
        ApiClient api,
        RealtimeChatService realtimeChatService,
        PushTokenService pushTokenService)
    {
        InitializeComponent();

        _auth = auth;
        _api = api;
        _realtimeChatService = realtimeChatService;
        _pushTokenService = pushTokenService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

#if ANDROID
        var status = await Permissions.CheckStatusAsync<NotificationPermission>();

        if (status != PermissionStatus.Granted)
        {
            await Permissions.RequestAsync<NotificationPermission>();
        }
#endif
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var res = await _auth.Login(
            EmailEntry.Text,
            PasswordEntry.Text);

        if (res != null)
        {
            // Save token
            Preferences.Set("JwtToken", res.token);

            var userJson = (JsonElement)res.user;

            int userId = userJson
                .GetProperty("id")
                .GetInt32();

            // Save current user
            Preferences.Set("CurrentUserId", userId);

            try
            {
                await _pushTokenService.RegisterStoredTokenAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"FCM registration failed: {ex}");
            }

            // Connect SignalR with token provider
            await _realtimeChatService.ConnectAsync(
                _api.BaseUrl,
                userId,
                () => Task.FromResult<string?>(Preferences.Get("JwtToken", null)));

            // Save current user in App
            if (Application.Current is App app)
            {
                app.SetCurrentUser(userId);
            }

            // Open HomePage
            var userService = new UserService(_api);

            await Navigation.PushAsync(
                new HomePage(userService));
        }
        else
        {
            await DisplayAlert(
                "Error",
                "Invalid credentials",
                "OK");
        }
    }

    private async void OnSignupClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(
            new SignupPage(_auth, _api));
    }

    private async void OnForgotClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(
            new ForgotPage(
                _auth,
                _api,
                _realtimeChatService));
    }
}