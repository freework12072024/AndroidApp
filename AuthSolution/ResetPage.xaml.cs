using AuthSolution.Services;

namespace AuthSolution;

public partial class ResetPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly ApiClient _api;
    private readonly RealtimeChatService _realtimeChatService;

    public ResetPage(
        AuthService auth,
        ApiClient api,
        RealtimeChatService realtimeChatService)
    {
        InitializeComponent();

        _auth = auth;
        _api = api;
        _realtimeChatService = realtimeChatService;
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        var token = TokenEntry.Text?.Trim();
        var newPassword = PasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(token) ||
            string.IsNullOrWhiteSpace(newPassword))
        {
            await DisplayAlert(
                "Error",
                "Please enter both token and new password",
                "OK");

            return;
        }

        var res = await _auth.Reset(token, newPassword);

        if (res != null)
        {
            if (res.message.Contains("Password updated"))
            {
                await DisplayAlert(
                    "Success",
                    "Password reset",
                    "OK");

                // Go back to Login page
                await Navigation.PushAsync(
    new LoginPage(
        _auth,
        _api,
        _realtimeChatService,
        new PushTokenService(_api)));
            }
            else
            {
                await DisplayAlert(
                    "Failure",
                    res.message,
                    "OK");
            }
        }
        else
        {
            await DisplayAlert(
                "Failure",
                "Something went wrong",
                "OK");
        }
    }
}