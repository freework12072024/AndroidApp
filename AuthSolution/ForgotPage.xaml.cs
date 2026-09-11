using AuthSolution.Services;

namespace AuthSolution;

public partial class ForgotPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly ApiClient _api;
    private readonly RealtimeChatService _realtimeChatService;

    public ForgotPage(
        AuthService auth,
        ApiClient api,
        RealtimeChatService realtimeChatService)
    {
        InitializeComponent();

        _auth = auth;
        _api = api;
        _realtimeChatService = realtimeChatService;
    }

    private async void OnForgotClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlertAsync(
                "Error",
                "Please enter your email",
                "OK");

            return;
        }

        var response = await _auth.Forgot(email);

        if (response?.token != null)
        {
            await DisplayAlertAsync(
                "Success",
                "Check your email for the reset token",
                "OK");

            // Open Reset Page
            await Navigation.PushAsync(
                new ResetPage(
                    _auth,
                    _api,
                    _realtimeChatService));
        }
        else
        {
            await DisplayAlertAsync(
                "Error",
                "User not found",
                "OK");
        }
    }
}