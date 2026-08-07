using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SubramanyaWebsite.Models;

namespace SubramanyaWebsite.Services;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> options, ILogger<SmtpEmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendCareRequestEmailsAsync(
        CareRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateSettings();

        var from = new MailboxAddress(_settings.FromName, _settings.FromEmail);
        var recipients = _settings.RecipientEmails
            .Select(email => new MailboxAddress("Sri Subramanya Home Healthcare", email))
            .ToArray();

        using var smtp = new SmtpClient();
        var socketOption = _settings.UseSsl
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTlsWhenAvailable;

        try
        {
            await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, socketOption, cancellationToken);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

            var notification = new MimeMessage();
            notification.From.Add(from);
            notification.To.AddRange(recipients);
            notification.ReplyTo.Add(new MailboxAddress(request.Name, request.Email));
            notification.Subject = $"New home care request from {request.Name}";
            notification.Body = new TextPart("plain")
            {
                Text = $"""
                    A new home care request was submitted through the website.

                    Name: {request.Name}
                    Phone: {request.Phone}
                    Email: {request.Email}
                    Patient name & age: {request.PatientName}
                    Location: {request.Location ?? "Not provided"}
                    Required service: {request.RequiredService}
                    """
            };
            await smtp.SendAsync(notification, cancellationToken);

            var confirmation = new MimeMessage();
            confirmation.From.Add(from);
            confirmation.To.Add(new MailboxAddress(request.Name, request.Email));
            confirmation.Subject = "Thank you for contacting Sri Subramanya Home Healthcare";
            confirmation.Body = new TextPart("plain")
            {
                Text = $"""
                    Dear {request.Name},

                    Thank you for submitting your home care request to Sri Subramanya Home Healthcare.
                    We have received your request for {request.RequiredService} for {request.PatientName}.

                    Our care coordinator will contact you at {request.Phone} within 15 minutes.
                    If you need immediate assistance, please call +91 98864 12345.

                    Regards,
                    Sri Subramanya Home Healthcare
                    Belagavi
                    """
            };
            await smtp.SendAsync(confirmation, cancellationToken);

            await smtp.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to send care request emails for {RequestEmail}", request.Email);
            throw;
        }
    }

    private void ValidateSettings()
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(_settings.SmtpHost)) missing.Add("SmtpHost");
        if (_settings.SmtpPort <= 0) missing.Add("SmtpPort");
        if (string.IsNullOrWhiteSpace(_settings.Username)) missing.Add("Username");
        if (string.IsNullOrWhiteSpace(_settings.Password)) missing.Add("Password");
        if (string.IsNullOrWhiteSpace(_settings.FromEmail)) missing.Add("FromEmail");
        if (_settings.RecipientEmails.Length == 0) missing.Add("Emails");

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Email settings are incomplete. Configure: {string.Join(", ", missing)}.");
        }
    }
}