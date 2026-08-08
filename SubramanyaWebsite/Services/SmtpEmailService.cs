using System.Net;
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

        var sender = ParseMailbox(_settings.Username, "username");

        using var smtp = new SmtpClient();
        var socketOption = GetSocketOption();

        try
        {
            await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, socketOption, cancellationToken);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

            var notification = new MimeMessage();
            notification.From.Add(new MailboxAddress(_settings.FromName, sender.Address));
            notification.To.Add(sender);
            notification.ReplyTo.Add(new MailboxAddress(request.Name, request.Email));
            notification.Subject = $"New home care request from {request.Name}";
            notification.Body = CreateBody(
                BuildNotificationText(request),
                BuildNotificationHtml(request));
            await smtp.SendAsync(notification, cancellationToken);
            _logger.LogInformation(
                "Care request notification accepted by SMTP server for {RequestEmail}.",
                request.Email);

            var confirmation = new MimeMessage();
            confirmation.From.Add(new MailboxAddress(_settings.FromName, sender.Address));
            confirmation.To.Add(ParseMailbox(request.Email, "request"));
            confirmation.Subject = "Thank you for contacting Sri Subramanya Home Healthcare";
            confirmation.Body = CreateBody(
                BuildConfirmationText(request),
                BuildConfirmationHtml(request));
            await smtp.SendAsync(confirmation, cancellationToken);
            _logger.LogInformation(
                "Care request confirmation accepted by SMTP server for {RequestEmail}.",
                request.Email);

            await smtp.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to send care request emails for {RequestEmail}", request.Email);
            throw;
        }
    }

    private static MimeEntity CreateBody(string text, string html)
    {
        var builder = new BodyBuilder
        {
            TextBody = text,
            HtmlBody = html
        };

        return builder.ToMessageBody();
    }

    private static string BuildNotificationText(CareRequest request) =>
        $"""
        A new home care request was submitted through the website.

        Name: {request.Name}
        Phone: {request.Phone}
        Email: {request.Email}
        Patient name & age: {request.PatientName}
        Location: {request.Location ?? "Not provided"}
        Required service: {request.RequiredService}
        """;

    private static string BuildConfirmationText(CareRequest request) =>
        $"""
        Dear {request.Name},

        Thank you for submitting your home care request to Sri Subramanya Home Healthcare.
        We have received your request for {request.RequiredService} for {request.PatientName}.

        Our care coordinator will contact you at {request.Phone} within 15 minutes.
        If you need immediate assistance, please call +91 96325 54468 or +91 97392 113907.

        Regards,
        Sri Subramanya Home Healthcare
        Belagavi
        """;

    private static string BuildNotificationHtml(CareRequest request)
    {
        var name = Encode(request.Name);
        var phone = Encode(request.Phone);
        var email = Encode(request.Email);
        var patient = Encode(request.PatientName);
        var location = Encode(request.Location ?? "Not provided");
        var service = Encode(request.RequiredService);

        return $"""
        <!DOCTYPE html>
        <html lang="en">
        <body style="margin:0;padding:0;background:#f1f5f9;font-family:Arial,Helvetica,sans-serif;color:#172033;">
          <div style="padding:32px 16px;">
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:640px;margin:0 auto;background:#ffffff;border-radius:18px;overflow:hidden;">
              <tr>
                <td style="padding:28px 32px;background:#0f766e;color:#ffffff;">
                  <div style="font-size:12px;font-weight:bold;letter-spacing:1.5px;text-transform:uppercase;opacity:.85;">Sri Subramanya Home Healthcare</div>
                  <h1 style="margin:10px 0 0;font-size:25px;line-height:1.25;">New care request received</h1>
                  <p style="margin:8px 0 0;font-size:14px;line-height:1.5;color:#d5fbf4;">A visitor submitted a home care request through the website.</p>
                </td>
              </tr>
              <tr>
                <td style="padding:28px 32px;">
                  <h2 style="margin:0 0 18px;font-size:17px;color:#0f766e;">Request details</h2>
                  <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="font-size:14px;border-collapse:collapse;">
                    {DetailRow("Name", name)}
                    {DetailRow("Phone", phone)}
                    {DetailRow("Email", email)}
                    {DetailRow("Patient name & age", patient)}
                    {DetailRow("Location", location)}
                    {DetailRow("Required service", service)}
                  </table>
                  <div style="margin-top:24px;padding:16px 18px;background:#ecfdf5;border-left:4px solid #14b8a6;border-radius:8px;color:#115e59;font-size:13px;line-height:1.5;">
                    Please contact the visitor as soon as possible to confirm availability and next steps.
                  </div>
                </td>
              </tr>
              <tr>
                <td style="padding:18px 32px;background:#f8fafc;color:#64748b;font-size:12px;line-height:1.5;">
                  Submitted from the Sri Subramanya Home Healthcare website · Belagavi
                </td>
              </tr>
            </table>
          </div>
        </body>
        </html>
        """;
    }

    private static string BuildConfirmationHtml(CareRequest request)
    {
        var name = Encode(request.Name);
        var patient = Encode(request.PatientName);
        var service = Encode(request.RequiredService);
        var phone = Encode(request.Phone);

        return $"""
        <!DOCTYPE html>
        <html lang="en">
        <body style="margin:0;padding:0;background:#f1f5f9;font-family:Arial,Helvetica,sans-serif;color:#172033;">
          <div style="padding:32px 16px;">
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:640px;margin:0 auto;background:#ffffff;border-radius:18px;overflow:hidden;">
              <tr>
                <td style="padding:30px 32px;background:#0f766e;color:#ffffff;text-align:center;">
                  <div style="display:inline-block;padding:9px 12px;border-radius:50%;background:#2dd4bf;color:#064e3b;font-size:22px;">✓</div>
                  <h1 style="margin:14px 0 8px;font-size:25px;line-height:1.25;">Your request is confirmed</h1>
                  <p style="margin:0;font-size:14px;line-height:1.5;color:#d5fbf4;">Thank you for contacting Sri Subramanya Home Healthcare.</p>
                </td>
              </tr>
              <tr>
                <td style="padding:30px 32px;">
                  <p style="margin:0 0 18px;font-size:16px;line-height:1.6;">Dear <strong>{name}</strong>,</p>
                  <p style="margin:0;font-size:14px;line-height:1.7;color:#475569;">
                    We have received your home care request and our coordinator will review it shortly.
                  </p>
                  <div style="margin:24px 0;padding:18px;background:#f0fdfa;border:1px solid #ccfbf1;border-radius:12px;">
                    <div style="margin-bottom:10px;font-size:12px;font-weight:bold;letter-spacing:1px;text-transform:uppercase;color:#0f766e;">Your request</div>
                    <p style="margin:6px 0;font-size:14px;color:#334155;"><strong>Service:</strong> {service}</p>
                    <p style="margin:6px 0;font-size:14px;color:#334155;"><strong>Patient:</strong> {patient}</p>
                  </div>
                  <div style="padding:16px 18px;background:#fff7ed;border-left:4px solid #f59e0b;border-radius:8px;color:#9a3412;font-size:13px;line-height:1.6;">
                    Our care coordinator will contact you at <strong>{phone}</strong> within 15 minutes.
                  </div>
                  <p style="margin:24px 0 0;font-size:13px;line-height:1.6;color:#64748b;">
                    Need immediate assistance? Call <strong style="color:#0f766e;">+91 96325 54468</strong> or <strong style="color:#0f766e;">+91 97392 113907</strong>.
                  </p>
                </td>
              </tr>
              <tr>
                <td style="padding:20px 32px;background:#f8fafc;color:#64748b;font-size:12px;line-height:1.6;">
                  <strong style="color:#334155;">Sri Subramanya Home Healthcare</strong><br>
                  Compassionate home nursing and patient care · Belagavi
                </td>
              </tr>
            </table>
          </div>
        </body>
        </html>
        """;
    }

    private static string DetailRow(string label, string value) =>
        $"""
        <tr>
          <td style="padding:11px 12px 11px 0;border-bottom:1px solid #e2e8f0;color:#64748b;width:38%;">{label}</td>
          <td style="padding:11px 0;border-bottom:1px solid #e2e8f0;color:#172033;font-weight:bold;">{value}</td>
        </tr>
        """;

    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    private SecureSocketOptions GetSocketOption()
    {
        if (!_settings.UseSsl)
        {
            return SecureSocketOptions.None;
        }

        // Port 465 uses implicit TLS. The common submission ports 587 and
        // 2525 negotiate TLS with STARTTLS after connecting.
        return _settings.SmtpPort == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;
    }

    private static MailboxAddress ParseMailbox(string address, string addressType)
    {
        try
        {
            return MailboxAddress.Parse(address);
        }
        catch (ParseException ex)
        {
            throw new InvalidOperationException(
                $"Email settings contain an invalid {addressType} address: '{address}'.",
                ex);
        }
    }

    private void ValidateSettings()
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(_settings.SmtpHost)) missing.Add("SmtpHost");
        if (_settings.SmtpPort <= 0) missing.Add("SmtpPort");
        if (string.IsNullOrWhiteSpace(_settings.Username)) missing.Add("Username");
        if (string.IsNullOrWhiteSpace(_settings.Password)) missing.Add("Password");

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Email settings are incomplete. Configure: {string.Join(", ", missing)}.");
        }

        _ = ParseMailbox(_settings.Username, "username");
    }
}