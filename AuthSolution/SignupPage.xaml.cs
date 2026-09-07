using AuthSolution.Services;

namespace AuthSolution;

public partial class SignupPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly ApiClient _api;
    private List<State>? _states;
    private List<City>? _cities;

    public SignupPage(AuthService auth, ApiClient api)
    {
        InitializeComponent();
        _auth = auth;
        _api = api;

        StatePicker.SelectedIndexChanged += StatePicker_SelectedIndexChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // fetch states
        _states = await _api.GetAsync<List<State>>("api/auth/states");
        StatePicker.ItemsSource = _states?.Select(s => s.Name).ToList();
    }

    private async void StatePicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var idx = StatePicker.SelectedIndex;
        if (_states != null && idx >= 0 && idx < _states.Count)
        {
            var stateId = _states[idx].Id;
            _cities = await _api.GetAsync<List<City>>($"api/auth/cities?stateId={stateId}");
            CityPicker.ItemsSource = _cities?.Select(c => c.Name).ToList();
        }
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(FirstNameEntry.Text) || string.IsNullOrWhiteSpace(LastNameEntry.Text) ||
            string.IsNullOrWhiteSpace(AddressEntry.Text) || StatePicker.SelectedIndex < 0 || CityPicker.SelectedIndex < 0 ||
            string.IsNullOrWhiteSpace(PincodeEntry.Text) || string.IsNullOrWhiteSpace(MobileEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            return false;
        }
        // basic email check
        if (!System.Text.RegularExpressions.Regex.IsMatch(EmailEntry.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) return false;
        // basic phone check - digits length 7-15
        if (!System.Text.RegularExpressions.Regex.IsMatch(MobileEntry.Text, @"^[0-9]{7,15}$")) return false;
        return true;
    }

    private async void OnSignupClicked(object sender, EventArgs e)
    {
        if (!ValidateInputs())
        {
            await DisplayAlert("Error", "Please fill all fields correctly", "OK");
            return;
        }

        var selectedState = _states![StatePicker.SelectedIndex];
        var selectedCity = _cities![CityPicker.SelectedIndex];

        var response = await _auth.Signup(
            EmailEntry.Text,
            PasswordEntry.Text,
            FirstNameEntry.Text,
            LastNameEntry.Text,
            AddressEntry.Text,
            selectedCity.Name,
            PincodeEntry.Text,
            selectedState.Name,
            MobileEntry.Text
        );
        if (response != null)
        {
            await DisplayAlert("Success", "Account created. Check email for activation code.", "OK");
            // navigate to activation page to enter OTP
            await Navigation.PushAsync(new ActivationPage(_auth, EmailEntry.Text));
        }
        else
        {
            await DisplayAlert("Error", "Signup failed", "OK");
        }
    }
}