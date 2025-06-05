using Frederikskaj2.Reservations.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Users;

class AccountAmountsConverter : JsonConverter<AccountAmounts>
{
    static readonly Type type = typeof(Dictionary<string, Amount>);

    public override AccountAmounts Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var converter = (JsonConverter<Dictionary<string, Amount>>) options.GetConverter(type);
        var dictionary = converter.Read(ref reader, type, options);
        return dictionary is not null
            ? AccountAmounts.Create(dictionary.Select(kvp => (Enum.Parse<Account>(kvp.Key, ignoreCase: true), kvp.Value)).ToArray())
            : AccountAmounts.Empty;
    }

    public override void Write(Utf8JsonWriter writer, AccountAmounts value, JsonSerializerOptions options)
    {
        var converter = (JsonConverter<Dictionary<string, Amount>>) options.GetConverter(type);
        converter.Write(writer, value.ToDictionary(kvp => kvp.Account.ToString().ToCamelCase(), kvp => kvp.Amount), options);
    }
}
