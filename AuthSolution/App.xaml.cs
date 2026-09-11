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
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var loginPage = _services.GetService<LoginPage>();

        Page page;

        if (loginPage != null)
        {
            page = new NavigationPage(loginPage);
        }
        else
        {
            page = new NavigationPage(new ContentPage
            {
                Content = new Label
                {
                    Text = "Login page could not be loaded.",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            });
        }

        return new Window(page);
    }

    public void SetCurrentUser(int userId)
    {
        CurrentUserId = userId;
    }
}