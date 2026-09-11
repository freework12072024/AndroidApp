using AuthSolution.Models;
using AuthSolution.Services;

namespace AuthSolution;

public partial class HomePage : ContentPage
{
    private readonly UserService _userService;

    private bool _isOpeningChat = false;

    public HomePage(UserService userService)
    {
        InitializeComponent();

        _userService = userService;

        LoadUsers();
    }

    private async void LoadUsers()
    {
        try
        {
            var users = await _userService.GetUsersAsync();

            UsersList.ItemsSource = users;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Users load failed:\n{ex.Message}",
                "OK");
        }
    }

    private async void OnUserTapped(
    object sender,
    TappedEventArgs e)
    {
        if (_isOpeningChat)
            return;

        if (sender is not Border border)
            return;

        var selectedUser =
            border.BindingContext as UserDto;

        if (selectedUser == null)
            return;

        try
        {
            // CLICK EFFECT
            border.BackgroundColor = Color.FromArgb("#202C33");

            await Task.Delay(100);

            var currentUserId =
                Preferences.Get("CurrentUserId", 0);

            if (currentUserId <= 0)
                return;

            _isOpeningChat = true;

            var chatPage =
                App.Current?
                    .Handler?
                    .MauiContext?
                    .Services
                    .GetRequiredService<ChatPage>();

            if (chatPage == null)
                return;

            await chatPage.InitAsync(
                selectedUser,
                currentUserId);

            await Navigation.PushAsync(chatPage);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Chat Error",
                ex.ToString(),
                "OK");
        }
        finally
        {
            _isOpeningChat = false;
        }
    }

    private async void OnLogoutClicked(
        object sender,
        EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}