using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.EmailSender;

class EmailApiService
{
    static readonly JsonSerializerOptions serializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    readonly HttpClient httpClient;
    readonly ILogger logger;
    readonly IOptionsMonitor<EmailApiOptions> options;

    public EmailApiService(HttpClient httpClient, ILogger<EmailApiService> logger, IOptionsMonitor<EmailApiOptions> options)
    {
        this.httpClient = httpClient;
        this.logger = logger;
        this.options = options;
    }

    public async ValueTask SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        // Don't try to stream the JSON by using PostAsJsonAsync. This will
        // set Content-Length to 0 and the Azure request gateway will strip
        // the body resulting in an invalid request.
        using var content = new ByteArrayContent(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, serializerOptions)));
        content.Headers.Add("Content-Type", "application/json; charset=utf-8");
        var url = options.CurrentValue.Url ?? throw new ConfigurationException("Missing URL value.");
        var response = await httpClient.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        logger.LogInformation("Sent email '{Subject}' to {Recipients}", message.Subject, message.To.Select(email => email.MaskEmail()));
    }
}
