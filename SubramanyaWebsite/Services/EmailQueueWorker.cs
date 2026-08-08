using Microsoft.Extensions.Hosting;
using SubramanyaWebsite.Models;

namespace SubramanyaWebsite.Services;

public sealed class EmailQueueWorker : BackgroundService
{
    private readonly IEmailQueue _queue;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailQueueWorker> _logger;

    public EmailQueueWorker(
        IEmailQueue queue,
        IEmailService emailService,
        ILogger<EmailQueueWorker> logger)
    {
        _queue = queue;
        _emailService = emailService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Care request email worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            CareRequest request;

            try
            {
                request = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await _emailService.SendCareRequestEmailsAsync(request, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Background delivery failed for care request from {RequestEmail}.",
                    request.Email);
            }
        }

        _logger.LogInformation("Care request email worker stopped.");
    }
}