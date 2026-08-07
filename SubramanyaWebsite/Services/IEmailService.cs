using SubramanyaWebsite.Models;

namespace SubramanyaWebsite.Services;

public interface IEmailService
{
    Task SendCareRequestEmailsAsync(CareRequest request, CancellationToken cancellationToken = default);
}