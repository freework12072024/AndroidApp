namespace AuthSolution.Services;

public class AppChatState
{
    public int? CurrentChatUserId { get; set; }

    public bool IsChatOpen =>
        CurrentChatUserId.HasValue;
}