using System.Reflection;
using Azure.Core;

namespace Thon.Hotels.FishBus.Options;

public class MessagingOptions
{
    public TokenCredential TokenCredential { get; set; }
    public string FullyQualifiedNamespace { get; set; }
    public Assembly[] Assemblies { get; set; }
    public bool WithCorrelationLogging { get; set; }
    public LogCorrelationOptions LogCorrelationOptions { get; set; }
}