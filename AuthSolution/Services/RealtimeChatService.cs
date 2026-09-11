using AuthSolution.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace AuthSolution.Services;

public class RealtimeChatService
{
    private HubConnection? _connection;

    public event Action<MessageDto>? MessageReceived;

    public async Task ConnectAsync(
        string baseUrl,
        int userId,
        Func<Task<string?>> getToken)
    {
        if (_connection != null &&
            _connection.State == HubConnectionState.Connected)
            return;

        var hubUrl =
            $"{baseUrl.TrimEnd('/')}/chatHub";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = async () => await getToken();
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<MessageDto>(
            "ReceiveMessage",
            message =>
            {
                MessageReceived?.Invoke(message);
            });

        await _connection.StartAsync();

        await _connection.InvokeAsync(
            "RegisterUser",
            userId);
    }

    public bool IsConnected =>
        _connection?.State ==
        HubConnectionState.Connected;

    public async Task DisconnectAsync()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}