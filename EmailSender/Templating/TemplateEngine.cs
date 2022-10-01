using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Frederikskaj2.Reservations.EmailSender;

class TemplateEngine
{
    const string razorGeneratedClassName = "Template";
    readonly ILogger logger;

    public TemplateEngine(ILogger<TemplateEngine> logger) => this.logger = logger;

    public CompiledTemplate CompileTemplate(string content)
    {
        var options = new CompileOptions();
        options.ReferencedAssemblies.Add(typeof(Template).Assembly);
        using var memoryStream = CreateAndCompileToStream(content, options, typeof(Template));
        return new CompiledTemplate(memoryStream, $"{options.TemplateNamespace}.{razorGeneratedClassName}");
    }

    public CompiledTemplate<TModel> CompileTemplate<TModel>(string content) where TModel : class
    {
        var options = new CompileOptions();
        options.ReferencedAssemblies.Add(typeof(Template<>).Assembly);
        AddModelReferences(options, typeof(TModel));
        using var memoryStream = CreateAndCompileToStream(content, options, typeof(Template<TModel>));
        return new CompiledTemplate<TModel>(memoryStream, $"{options.TemplateNamespace}.{razorGeneratedClassName}");
    }

    static void AddModelReferences(CompileOptions options, Type modelType)
    {
        options.ReferencedAssemblies.Add(modelType.Assembly);
        if (!modelType.IsGenericType)
            return;
        foreach (var type in modelType.GetGenericArguments())
            AddModelReferences(options, type);
    }

    MemoryStream CreateAndCompileToStream(string razorSource, CompileOptions options, Type baseType)
    {
        var source = GetRazorSourceCode(razorSource, options, baseType);

        var engine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            RazorProjectFileSystem.Create(@"."),
            builder => builder.SetNamespace(options.TemplateNamespace));

        var fileName = Path.GetRandomFileName();
        var document = RazorSourceDocument.Create(source, fileName);

        var codeDocument = engine.Process(
            document,
            null,
            new List<RazorSourceDocument>(),
            new List<TagHelperDescriptor>());

        var cSharpDocument = codeDocument.GetCSharpDocument();
        logger.LogTrace("{TemplateSource}", cSharpDocument.GeneratedCode);

        var syntaxTree = CSharpSyntaxTree.ParseText(cSharpDocument.GeneratedCode);

        var compilation = CSharpCompilation.Create(
            fileName,
            new[] { syntaxTree },
            options.ReferencedAssemblies
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
                .Concat(options.MetadataReferences)
                .ToList(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var memoryStream = new MemoryStream();
        var emitResult = compilation.Emit(memoryStream);
        if (!emitResult.Success)
            throw new TemplateException(emitResult.Diagnostics, cSharpDocument.GeneratedCode);
        memoryStream.Position = 0;
        return memoryStream;
    }

    static string GetRazorSourceCode(string razorSource, CompileOptions options, Type baseType)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"@inherits {baseType.CSharpName(true)}");
        foreach (var entry in options.DefaultUsings)
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"@using {entry}");
        stringBuilder.Append(razorSource);
        return stringBuilder.ToString();
    }
}
