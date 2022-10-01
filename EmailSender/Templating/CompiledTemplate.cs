using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.EmailSender;

class CompiledTemplate
{
    readonly Type templateType;

    internal CompiledTemplate(MemoryStream assemblyByteCode, string templateTypeName) =>
        templateType = Assembly.Load(assemblyByteCode.ToArray()).GetType(templateTypeName) ??
                       throw new ArgumentException($"The type {templateTypeName} does not exist.", nameof(templateTypeName));

    public async Task<string> RunAsync()
    {
        var template = (ITemplate) Activator.CreateInstance(templateType)!; // This can only return null when the type is Nullable<T>.
        await template.ExecuteAsync();
        var result = template.Render();
        if (template is IDisposable disposable)
            disposable.Dispose();
        return result;
    }
}

public class CompiledTemplate<TModel> where TModel : class
{
    readonly Type templateType;

    internal CompiledTemplate(MemoryStream assemblyByteCode, string templateTypeName) =>
        templateType = Assembly.Load(assemblyByteCode.ToArray()).GetType(templateTypeName) ??
                       throw new ArgumentException($"The type {templateTypeName} does not exist.", nameof(templateTypeName));

    public async Task<string> RunAsync(TModel? model = null)
    {
        var template = (ITemplate<TModel>) Activator.CreateInstance(templateType)!; // This can only return null when the type is Nullable<T>.
        template.Model = model;
        await template.ExecuteAsync();
        var result = template.Render();
        if (template is IDisposable disposable)
            disposable.Dispose();
        return result;
    }
}