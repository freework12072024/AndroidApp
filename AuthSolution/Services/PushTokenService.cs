using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AuthSolution.Services;

public sealed class PushTokenService
{
    public const string FcmTokenKey = "FcmRegistrationToken";

    private readonly ApiClient _api;

    public PushTokenService(ApiClient api)
    {
        _api = api;
    }

    public async Task RegisterStoredTokenAsync()
    {
        var fcmToken = Preferences.Get(FcmTokenKey, string.Empty);
        var jwt = Preferences.Get("JwtToken", string.Empty);

        if (string.IsNullOrWhiteSpace(fcmToken) ||
            string.IsNullOrWhiteSpace(jwt))
            return;

        using var http = new HttpClient
        {
            BaseAddress = new Uri(_api.BaseUrl)
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/device/register")
        {
            Content = JsonContent.Create(new
            {
                token = fcmToken,
                platform = "android"
            })
        };

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", jwt);

        using var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}