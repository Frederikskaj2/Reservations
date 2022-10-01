using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.EmailSender;

public interface ITemplate
{
    void BeginWriteAttribute(string name, string prefix, int prefixOffset, string suffix, int suffixOffset, int attributeValuesCount);
    void WriteAttributeValue(string prefix, int prefixOffset, object? value, int valueOffset, int valueLength, bool isLiteral);
    void EndWriteAttribute();
    void WriteLiteral(string? literal);
    void Write(object? value);
    Task ExecuteAsync();
    string Render();
}

public interface ITemplate<TModel> : ITemplate where TModel : class
{
    TModel? Model { get; set; }
}