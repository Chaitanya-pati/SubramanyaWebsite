using System.Threading.Channels;
using SubramanyaWebsite.Models;

namespace SubramanyaWebsite.Services;

public sealed class EmailQueue : IEmailQueue
{
    private const int Capacity = 100;
    private readonly Channel<CareRequest> _requests = Channel.CreateBounded<CareRequest>(
        new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

    public bool TryEnqueue(CareRequest request) => _requests.Writer.TryWrite(request);

    public ValueTask<CareRequest> DequeueAsync(CancellationToken cancellationToken) =>
        _requests.Reader.ReadAsync(cancellationToken);
}