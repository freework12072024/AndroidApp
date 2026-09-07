namespace Api.Models;

public sealed class DeviceRegistrationRequest
{
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = "android";
}