using SubramanyaWebsite.Models;

namespace SubramanyaWebsite.Services;

public interface IEmailQueue
{
    bool TryEnqueue(CareRequest request);

    ValueTask<CareRequest> DequeueAsync(CancellationToken cancellationToken);
}