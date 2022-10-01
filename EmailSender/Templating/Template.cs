using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.EmailSender;

public abstract class Template : ITemplate, IDisposable
{
    readonly HtmlEncoder htmlEncoder = HtmlEncoder.Default;
    readonly TextWriter textWriter = new StringWriter();

    AttributeData? currentAttribute;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void BeginWriteAttribute(string name, string prefix, int prefixOffset, string suffix, int suffixOffset, int attributeValuesCount)
    {
        if (currentAttribute is not null)
            throw new InvalidOperationException();
        new StringWriter().Dispose();

        currentAttribute = new AttributeData(name, prefix, suffix, attributeValuesCount);
        if (attributeValuesCount > 1)
            WriteLiteral(prefix);
    }

    public void WriteAttributeValue(string prefix, int prefixOffset, object? value, int valueOffset, int valueLength, bool isLiteral)
    {
        if (currentAttribute is null)
            throw new InvalidOperationException();

        if (currentAttribute.AttributeValuesCount > 1)
            MultiValueAttributeWriteAttributeValue(prefix, value, isLiteral);
        else
            currentAttribute.IsValueOmitted = SingleValueAttributeWriteAttributeExcludingSuffix(prefix, value, isLiteral, currentAttribute);
    }

    public void EndWriteAttribute()
    {
        if (currentAttribute is null)
            throw new InvalidOperationException();

        if (!currentAttribute.IsValueOmitted)
            WriteLiteral(currentAttribute.Suffix);
        currentAttribute = null;
    }

    public void WriteLiteral(string? literal) => textWriter.Write(literal);

    public void Write(object? value)
    {
        if (value is not IHtmlContent content)
            Write(value?.ToString());
        else
            content.WriteTo(textWriter, htmlEncoder);
    }

#pragma warning disable CA2119
    public virtual Task ExecuteAsync() => Task.CompletedTask;
#pragma warning restore CA2119

    public string Render()
    {
        textWriter.Flush();
        return textWriter.ToString()!;
    }

    protected virtual void Dispose(bool isDisposing)
    {
        if (isDisposing)
            textWriter.Dispose();
    }

    void Write(string? value)
    {
        if (value is not null)
            htmlEncoder.Encode(textWriter, value);
    }

    void MultiValueAttributeWriteAttributeValue(string prefix, object? value, bool isLiteral)
    {
        if (value is null or string { Length: 0 })
            return;
        if (!string.IsNullOrEmpty(prefix))
            WriteLiteral(prefix);
        WriteAttributeValue(value, isLiteral);
    }

    bool SingleValueAttributeWriteAttributeExcludingSuffix(string prefix, object? value, bool isLiteral, AttributeData attribute)
    {
        switch (value)
        {
            case null or false or string { Length: 0 }:
                return true;
            case true:
                WriteLiteral(" " + attribute.Name);
                return true;
            default:
            {
                WriteLiteral(!string.IsNullOrEmpty(prefix) ? prefix : attribute.Prefix);
                WriteAttributeValue(value, isLiteral);
                return false;
            }
        }
    }

    void WriteAttributeValue(object? value, bool isLiteral)
    {
        if (isLiteral)
            WriteLiteral(value?.ToString());
        else
            Write(value);
    }

    record AttributeData(string Name, string Prefix, string Suffix, int AttributeValuesCount)
    {
        public bool IsValueOmitted { get; set; }
    }
}

public abstract class Template<TModel> : Template, ITemplate<TModel> where TModel : class
{
    public TModel? Model { get; set; }
}
