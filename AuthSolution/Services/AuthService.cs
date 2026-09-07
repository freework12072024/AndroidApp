using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace AuthSolution.Services
{
    public class AuthService
    {
        private readonly ApiClient _api;


        public AuthService(ApiClient api) => _api = api;


        public Task<LoginResponse?> Login(string email, string password) =>
        _api.PostAsync<LoginResponse>("api/auth/login", new { Email = email, Password = password });


        // Basic signup (legacy)
        public Task<SignupResponse?> Signup(string email, string password) =>
            _api.PostAsync<SignupResponse>("api/auth/signup", new { Email = email, Password = password });

        // Extended signup with profile fields
        public Task<SignupResponse?> Signup(string email, string password, string? firstName, string? lastName, string? address, string? city, string? pincode, string? state, string? mobile) =>
            _api.PostAsync<SignupResponse>("api/auth/signup", new
            {
                Email = email,
                Password = password,
                FirstName = firstName,
                LastName = lastName,
                Address = address,
                City = city,
                Pincode = pincode,
                State = state,
                Mobile = mobile
            });


        public Task<ForgotResponse?> Forgot(string email) =>
        _api.PostAsync<ForgotResponse>("api/auth/forgot", new { Email = email });

        public Task<ResetResponse?> Reset(string token, string newPassword) =>
        _api.PostAsync<ResetResponse>("api/auth/reset", new { Token = token, NewPassword = newPassword });

        public Task<ActivateResponse?> Activate(string email, string token) =>
            _api.PostAsync<ActivateResponse>("api/auth/activate", new { Email = email, Token = token });
    }


    public record LoginResponse(string token, object user);
    public record ForgotResponse(string message, string token);
    public record ResetResponse(string message);
    public record SignupResponse(string message, string? email);
    public record ActivateResponse(string message);
}
