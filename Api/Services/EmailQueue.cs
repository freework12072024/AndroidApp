using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Services
{
    public class EmailItem
    {
        public string To { get; init; } = string.Empty;
        public string Subject { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
    }

    public class EmailQueue
    {
        private readonly ConcurrentQueue<EmailItem> _queue = new();
        private readonly SemaphoreSlim _signal = new(0);

        public void Enqueue(EmailItem item)
        {
            _queue.Enqueue(item);
            _signal.Release();
        }

        public async Task<EmailItem?> DequeueAsync(CancellationToken ct)
        {
            await _signal.WaitAsync(ct);
            _queue.TryDequeue(out var item);
            return item;
        }
    }
}
