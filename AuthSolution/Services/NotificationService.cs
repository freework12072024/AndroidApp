using System;

namespace AuthSolution.Services
{
    public class NotificationService
    {
        private readonly ApiClient _apiClient;
        private int _currentUserId;
        private System.Threading.Timer? _notificationTimer;

        public event EventHandler<NotificationEventArgs>? NewMessageReceived;

        public NotificationService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public void Initialize(int currentUserId)
        {
            _currentUserId = currentUserId;
        }

        public void StartPolling()
        {
            if (_notificationTimer != null)
                return;

            _notificationTimer = new System.Threading.Timer(
                async _ => await CheckForNewMessages(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2)
            );
        }

        public void StopPolling()
        {
            _notificationTimer?.Dispose();
            _notificationTimer = null;
        }

        private async Task CheckForNewMessages()
        {
            try
            {
                if (_currentUserId <= 0) return;

                var result = await _apiClient.GetAsync<dynamic>(
                    $"api/chat/unread-count/{_currentUserId}");

                if (result != null)
                {
                    var totalUnread = (int)(result.GetType()
                        .GetProperty("TotalUnread")?.GetValue(result) ?? 0);

                    if (totalUnread > 0)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            NewMessageReceived?.Invoke(this, new NotificationEventArgs
                            {
                                TotalUnread = totalUnread,
                                Message = $"You have {totalUnread} unread messages"
                            });
                        });
                    }
                }
            }
            catch { }
        }
    }

    public class NotificationEventArgs : EventArgs
    {
        public int TotalUnread { get; set; }
        public string Message { get; set; } = "";
    }
}