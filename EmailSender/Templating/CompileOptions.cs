using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Email;
using Microsoft.CodeAnalysis;
using NodaTime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Frederikskaj2.Reservations.EmailSender;

public class CompileOptions
{
    public HashSet<Assembly> ReferencedAssemblies { get; } = new()
    {
        typeof(User).Assembly, // Shared.Core
        typeof(MessageBase).Assembly, // Shared.Email
        typeof(EmailCleaningSchedule).Assembly, // EmailSender
        typeof(IEnumerable<>).Assembly,
        typeof(IList).Assembly,
        typeof(Instant).Assembly,
        typeof(object).Assembly,
        typeof(Uri).Assembly,
        Assembly.Load(new AssemblyName("Microsoft.CSharp")),
        Assembly.Load(new AssemblyName("netstandard")),
        Assembly.Load(new AssemblyName("System.Collections")),
        Assembly.Load(new AssemblyName("System.Linq")),
        Assembly.Load(new AssemblyName("System.Linq.Expressions")),
        Assembly.Load(new AssemblyName("System.Runtime"))
    };

    public HashSet<MetadataReference> MetadataReferences { get; set; } = new();

    public string TemplateNamespace { get; set; } = nameof(TemplateNamespace);

    public HashSet<string> DefaultUsings { get; } = new()
    {
        "Frederikskaj2.Reservations.EmailSender",
        "Frederikskaj2.Reservations.Shared.Core",
        "System.Collections",
        "System.Collections.Generic",
        "System.Linq"
    };
}
