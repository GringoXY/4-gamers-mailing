using Contracts.Events;

namespace Shared.Apis;

public interface IDocsApi
{
    Task<(string filename, byte[] content)> GeneratePdfAsync(IEvent @event);
}
