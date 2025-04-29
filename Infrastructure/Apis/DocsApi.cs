using Contracts.Events;
using Contracts.Events.Order;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using Shared.Apis;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Apis;

public class DocsApi : IDocsApi, IDisposable
{
    private readonly HttpClient _httpClient;

    public DocsApi(
        IHttpClientFactory httpClientFactory,
        IOptions<DocsApiOptions> docsApiOptions
    )
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = docsApiOptions.Value.Uri;
    }

    public async Task<(string filename, byte[] content)> GeneratePdfAsync(IEvent @event)
    {
        var endpointPath = @event switch
        {
            OrderCreatedEvent => "documents/order",
            _ => throw new ArgumentOutOfRangeException(nameof(@event)),
        };
        var requestContent = new StringContent(
            JsonSerializer.Serialize(@event, @event.GetType()),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);
        var requestUri = new Uri(endpointPath, UriKind.Relative);
        var response = await _httpClient.PostAsync(requestUri, requestContent);

        if (response.IsSuccessStatusCode)
        {
            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = response.Content.Headers.ContentDisposition?.FileName ?? "default.pdf";

            return (fileName, fileBytes);
        }

        throw new InvalidOperationException(
          $"Failed to generate PDF. StatusCode: {response.StatusCode}, Content: {await response.Content.ReadAsStringAsync()}");
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
