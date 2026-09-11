using AuthSolution.Services;

namespace AuthSolution;

public partial class ActivationPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly string _email;

    public ActivationPage(AuthService auth, string email)
    {
        InitializeComponent();
        _auth = auth;
        _email = email;
        EmailEntry.Text = email;
    }

    private async void OnActivateClicked(object sender, EventArgs e)
    {
        var code = CodeEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            await DisplayAlertAsync("Error", "Please enter the activation code", "OK");
            return;
        }

        var res = await _auth.Activate(_email, code);
        if (res != null && res.message.Contains("Account activated"))
        {
            await DisplayAlertAsync("Success", "Account activated", "OK");
            await Navigation.PopToRootAsync();
        }
        else
        {
            await DisplayAlertAsync("Error", res?.message ?? "Activation failed", "OK");
        }
    }

    private async void OnResendClicked(object sender, EventArgs e)
    {
        var resend = await _auth.Forgot(_email);
        if (resend != null)
            await DisplayAlertAsync("Info", "Activation code resent (if email exists)", "OK");
        else
            await DisplayAlertAsync("Error", "Could not resend code", "OK");
    }
}