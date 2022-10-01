using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Frederikskaj2.Reservations.EmailSender;

[Serializable]
public class TemplateException : Exception
{
    string[]? errors;
    string? generatedCode;

    public TemplateException() { }

    public TemplateException(string message) : base(message) { }

    public TemplateException(string message, Exception inner) : base(message, inner) { }

    public TemplateException(IEnumerable<Diagnostic> diagnostics, string generatedCode) : base(GetMessage(GetErrors(diagnostics))) =>
        (errors, this.generatedCode) = (GetErrors(diagnostics).Select(diagnostic => diagnostic.ToString()).ToArray(), generatedCode);

    protected TemplateException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        errors = (string[]?) info.GetValue(nameof(errors), typeof(string[]));
        generatedCode = info.GetString(nameof(generatedCode));
    }

    public IEnumerable<string> Errors => errors ?? Enumerable.Empty<string>();

    public string GeneratedCode => generatedCode ?? "";

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(errors), errors);
        info.AddValue(nameof(generatedCode), generatedCode);
    }

    static string GetMessage(IEnumerable<Diagnostic> errors) =>
        "Template failed to compile: " + string.Join("\n", errors.Select(diagnostic => diagnostic.ToString()));

    static IEnumerable<Diagnostic> GetErrors(IEnumerable<Diagnostic> diagnostics) =>
        diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error || diagnostic.IsWarningAsError);
}