using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;


namespace Api.Services
{
    public class EmailService:IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("My App", _config["Email:SmtpUser"]!));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = body
            };

            string? smtpServer = _config["Email:SmtpServer"];
            string? smtpPortStr = _config["Email:SmtpPort"];
            string? smtpUser = _config["Email:SmtpUser"];
            string? smtpPass = _config["Email:SmtpPass"];

            if (string.IsNullOrWhiteSpace(smtpServer))
                throw new InvalidOperationException("SMTP server is not configured.");
            if (string.IsNullOrWhiteSpace(smtpPortStr))
                throw new InvalidOperationException("SMTP port is not configured.");
            if (string.IsNullOrWhiteSpace(smtpUser))
                throw new InvalidOperationException("SMTP user is not configured.");
            if (string.IsNullOrWhiteSpace(smtpPass))
                throw new InvalidOperationException("SMTP password is not configured.");
            try
            {
                int smtpPort = int.Parse(smtpPortStr);

                using var client = new SmtpClient();
                // Set a timeout so the SMTP operations don't hang indefinitely
                client.Timeout = 10_000; // milliseconds

                await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch(Exception ex)
            {
                // Log the exception so it can be diagnosed
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw; // rethrow so callers (or background worker) can handle retries/logging
            }
        }
    }
}