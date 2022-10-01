using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Frederikskaj2.Reservations.Server;

class ConfigureJsonOptions : IConfigureOptions<JsonOptions>
{
    readonly IConfigureOptions<JsonSerializerOptions> configureJsonSerializerOptions;

    public ConfigureJsonOptions(IConfigureOptions<JsonSerializerOptions> configureJsonSerializerOptions) =>
        this.configureJsonSerializerOptions = configureJsonSerializerOptions;

    public void Configure(JsonOptions options) =>
        configureJsonSerializerOptions.Configure(options.JsonSerializerOptions);
}