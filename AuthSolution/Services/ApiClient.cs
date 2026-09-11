using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Maui.Storage;

namespace AuthSolution.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public ApiClient()
        {
            // Priority: in-app preference -> environment variable -> default Azure URL
            string? baseUrl = Preferences.Get(key: "ApiBaseUrl", null);
            //if (string.IsNullOrWhiteSpace(baseUrl))
            //{
            //    var env = Environment.GetEnvironmentVariable("API_BASE_URL");
            //    if (!string.IsNullOrWhiteSpace(env))
            //    {
            //        baseUrl = env;
            //    }
            //    else
            //    {
            //        // Default to the provided Azure App Service URL
            //        baseUrl = "https://androidapi-ergpfsg5eze8chg5.centralindia-01.azurewebsites.net/";
            //    }
            //}
#if DEBUG
            baseUrl = "http://localhost:7155/";
#else
            baseUrl = "https://androidapi-ergpfsg5eze08chg5.centralindia-01.azurewebsites.ne/";
#endif

#if ANDROID
            // Only rewrite localhost mapping for emulator; do not alter hosted (https) URLs
            if (baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            {
                baseUrl = baseUrl.Replace("localhost", "10.0.2.2");
            }
#endif

            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            Debug.WriteLine($"ApiClient: using BaseAddress={baseUrl}");
            Console.WriteLine($"ApiClient: using BaseAddress={baseUrl}");
            _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _baseUrl = baseUrl;
        }

        public string BaseUrl => _baseUrl;

        public string? GetStoredJwtToken() => Preferences.Get("JwtToken", string.Empty);

        public async Task<T?> PostAsync<T>(string url, object body)
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(body)
                };

                var res = await _http.SendAsync(req);
                var content = await res.Content.ReadAsStringAsync();

                Debug.WriteLine($"POST {url} => {(int)res.StatusCode} {res.ReasonPhrase}");
                Console.WriteLine($"POST {url} => {(int)res.StatusCode} {res.ReasonPhrase}");
                if (!res.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Response content: {content}");
                    Console.WriteLine($"Response content: {content}");
                    return default;
                }

                if (string.IsNullOrWhiteSpace(content))
                    return default;

                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"POST {url} failed: {ex}");
                Console.WriteLine($"POST {url} failed: {ex}");
                return default;
            }
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            try
            {
                var res = await _http.GetAsync(url);
                var content = await res.Content.ReadAsStringAsync();

                Debug.WriteLine($"GET {url} => {(int)res.StatusCode} {res.ReasonPhrase}");
                Console.WriteLine($"GET {url} => {(int)res.StatusCode} {res.ReasonPhrase}");
                if (!res.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Response content: {content}");
                    Console.WriteLine($"Response content: {content}");
                    return default;
                }

                if (string.IsNullOrWhiteSpace(content)) return default;
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GET {url} failed: {ex}");
                Console.WriteLine($"GET {url} failed: {ex}");
                return default;
            }
        }
    }

    public record State(int Id, string Name);
    public record City(int Id, string Name, int StateId);
}
