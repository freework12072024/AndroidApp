using AuthSolution.Models;
using AuthSolution.Services;
using System.Collections.ObjectModel;

namespace AuthSolution;

public partial class ChatPage : ContentPage
{
    private readonly ChatService _chatService;
    private UserDto? _chatUser;

    private  int _currentUserId;

    private CancellationTokenSource? _refreshCancellation;

    private readonly RealtimeChatService _realtimeService;
    private readonly AppChatState _chatState;
    public ObservableCollection<MessageDto> Messages { get; set; } = new();


    // DI se ChatService inject hoga
    public ChatPage(
    ChatService chatService,
    RealtimeChatService realtimeService,
    AppChatState chatState)
    {
        InitializeComponent();

        _chatService = chatService;
        _realtimeService = realtimeService;
        _chatState = chatState;

        MessagesList.ItemsSource = Messages;
    }

    // Runtime parameters set karne ke liye Init method
    public async Task InitAsync(UserDto chatUser, int currentUserId)
    {
        _chatUser = chatUser;
        _currentUserId = currentUserId;
        Title = $"Chat with {chatUser.Name}";
        //await LoadMessages();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_chatUser == null)
            return;

        _chatState.CurrentChatUserId =
            _chatUser.Id;

        _realtimeService.MessageReceived -= OnRealtimeMessageReceived;
        _realtimeService.MessageReceived += OnRealtimeMessageReceived;

        await LoadMessages();

        //StartAutoRefresh();
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _chatState.CurrentChatUserId = null;

        _realtimeService.MessageReceived -=
            OnRealtimeMessageReceived;

        //StopAutoRefresh();
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        var text = MessageEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(text))
            return;

        if (_chatUser == null || _currentUserId <= 0)
            return;

        var request = new MessageDto
        {
            SenderId = _currentUserId,
            ReceiverId = _chatUser.Id,
            Text = text
        };

        try
        {
            var result = await _chatService.PostChatAsync(request);

            if (result != null)
            {
                // IMPORTANT:
                // Yahan Messages.Add(request) MAT karo.
                // API save hone ke baad LoadMessages() message
                // ko database se lekar aayega.

                MessageEntry.Text = string.Empty;

                await LoadMessages();

                if (Messages.Count > 0)
                {
                    MessagesList.ScrollTo(
                        Messages.Count - 1,
                        position: ScrollToPosition.End,
                        animate: true);
                }
            }
            else
            {
                await DisplayAlert(
                    "Error",
                    "Message could not be sent.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                ex.Message,
                "OK");
        }
    }

    private async Task LoadMessages()
    {
        try
        {
            if (_chatUser == null || _currentUserId <= 0)
                return;

            var newMessages = await _chatService.GetChatAsync(
                _currentUserId,
                _chatUser.Id);

            if (newMessages == null)
                return;

            bool newAdded = false;

            foreach (var message in newMessages)
            {
                message.IsMine =
                    message.SenderId == _currentUserId;

                if (!Messages.Any(m => m.Id == message.Id))
                {
                    Messages.Add(message);
                    newAdded = true;
                }
            }

            if (newAdded && Messages.Count > 0)
            {
                MessagesList.ScrollTo(
                    Messages.Count - 1,
                    position: ScrollToPosition.End,
                    animate: false);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"LoadMessages error: {ex}");
        }
    }
 
    private void StartAutoRefresh()
    {
        StopAutoRefresh();

        _refreshCancellation =
            new CancellationTokenSource();

        _ = RefreshLoop(
            _refreshCancellation.Token);
    }


    private void StopAutoRefresh()
    {
        _refreshCancellation?.Cancel();
        _refreshCancellation?.Dispose();
        _refreshCancellation = null;
    }


    private async Task RefreshLoop(
        CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(2),
                    cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;

                await MainThread.InvokeOnMainThreadAsync(
                    async () =>
                    {
                        await LoadMessages();
                    });
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when leaving ChatPage
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"RefreshLoop error: {ex}");
        }
    }

    private void OnRealtimeMessageReceived(
    MessageDto message)
    {
        if (_chatUser == null)
            return;

        if (message.SenderId != _chatUser.Id)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Messages.Any(x => x.Id == message.Id))
                return;

            message.IsMine =
                message.SenderId == _currentUserId;

            Messages.Add(message);

            MessagesList.ScrollTo(
                Messages.Count - 1,
                position: ScrollToPosition.End,
                animate: true);
        });
    }
}
