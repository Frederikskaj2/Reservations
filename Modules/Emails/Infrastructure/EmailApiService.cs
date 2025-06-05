using Frederikskaj2.Reservations.Core;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Emails;

class EmailApiService(HttpClient httpClient, IOptionsMonitor<EmailApiOptions> options) : IEmailApiService
{
    static readonly JsonSerializerOptions serializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async ValueTask Send(EmailMessage message, CancellationToken cancellationToken)
    {
        // Don't try to stream the JSON by using PostAsJsonAsync. This will
        // set Content-Length to 0 and the Azure request gateway will strip
        // the body resulting in an invalid request.
        using var content = new ByteArrayContent(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, serializerOptions)));
        content.Headers.Add("Content-Type", "application/json; charset=utf-8");
        var url = options.CurrentValue.Url ?? throw new ConfigurationException("Missing URL value.");
        var response = await httpClient.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
