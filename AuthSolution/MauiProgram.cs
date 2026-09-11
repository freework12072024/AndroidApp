using AuthSolution.Services;
using Microsoft.Extensions.Logging;

namespace AuthSolution
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>();
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<ChatService>();
            builder.Services.AddSingleton<RealtimeChatService>();
            builder.Services.AddSingleton<AppChatState>();
            builder.Services.AddSingleton<PushTokenService>();


            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SignupPage>();
            builder.Services.AddTransient<ForgotPage>();
            builder.Services.AddTransient<ResetPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<ChatPage>();

            return builder.Build();
        }
    }
}
