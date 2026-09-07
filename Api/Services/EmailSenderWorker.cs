using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Services
{
    public class EmailSenderWorker : BackgroundService
    {
        private readonly EmailQueue _queue;
        private readonly IServiceProvider _provider;
        private readonly ILogger<EmailSenderWorker> _logger;

        public EmailSenderWorker(EmailQueue queue, IServiceProvider provider, ILogger<EmailSenderWorker> logger)
        {
            _queue = queue;
            _provider = provider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmailSenderWorker started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var item = await _queue.DequeueAsync(stoppingToken);
                    if (item == null) continue;

                    try
                    {
                        // Create a scope to resolve the scoped IEmailService
                        using var scope = _provider.CreateScope();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        await emailService.SendEmailAsync(item.To, item.Subject, item.Body);
                        _logger.LogInformation("Sent email to {Email}", item.To);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email to {Email}", item.To);
                        // Optionally re-enqueue or persist for retry
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // shutting down
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in email worker loop");
                    await Task.Delay(1000, stoppingToken);
                }
            }
            _logger.LogInformation("EmailSenderWorker stopping");
        }
    }
}
