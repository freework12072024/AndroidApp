namespace Api.Services;

public interface IPushNotificationService
{
    Task SendChatMessageAsync(
        IReadOnlyCollection<string> tokens,
        int senderId,
        string senderName,
        int messageId,
        string text);
}