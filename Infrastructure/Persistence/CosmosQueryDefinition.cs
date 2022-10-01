using System.Text;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

record CosmosQueryDefinition(bool Distinct, string? Top, string? Projection, string? Join, string Where, string? OrderBy)
{
    public string ToSql()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("select");
        if (Distinct)
            stringBuilder.Append(" distinct");
        if (Top is not null)
            stringBuilder.Append(Top);
        stringBuilder.Append(' ');
        stringBuilder.Append(Projection ?? "value root");
        stringBuilder.Append(" from root");
        if (Join is not null)
        {
            stringBuilder.Append(" join ");
            stringBuilder.Append(Join);
        }
        stringBuilder.Append(" where ");
        stringBuilder.Append(Where);
        if (OrderBy is not null)
        {
            stringBuilder.Append(" order by ");
            stringBuilder.Append(OrderBy);
        }
        return stringBuilder.ToString();
    }
}