using AuthSolution.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AuthSolution;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public int CurrentUserId { get; private set; }

    public App(IServiceProvider services)
    {
        InitializeComponent();

        _services = services;

        var loginPage = _services.GetService<LoginPage>();

        if (loginPage != null)
        {
            MainPage = new NavigationPage(loginPage);
        }
        else
        {
            MainPage = new NavigationPage(new ContentPage
            {
                Content = new Label
                {
                    Text = "Login page could not be loaded.",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            });
        }
    }

    public void SetCurrentUser(int userId)
    {
        CurrentUserId = userId;
    }
}