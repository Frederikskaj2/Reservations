using System.Text.Json;

namespace Frederikskaj2.Reservations.Emails;

public class EmailQueueOptions
{
    const string emulatorConnectionString =
        "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;";

    const string defaultQueueName = "email";

    public string ConnectionString { get; init; } = emulatorConnectionString;
    public string QueueName { get; set; } = defaultQueueName;
    public JsonSerializerOptions SerializerOptions { get; set; } = new();
}
