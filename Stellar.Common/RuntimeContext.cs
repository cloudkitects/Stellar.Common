using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace Stellar.Common;

/// <summary>
/// Compile and runtime symbols helper.
/// </summary>
public static class RuntimeContext
{
    /// <summary>
    /// Whether a debugger is attached.
    /// </summary>
    public static bool IsDebugging { get; }

    /// <summary>
    /// Jon Skeet's pragmatic way to detect whether we're in a unit testing context.
    /// </summary>
    public static bool IsTesting { get; }

    /// <summary>
    /// The executing assembly's name.
    /// </summary>
    public static string ExecutingAssembly { get; }

    /// <summary>
    /// The calling assembly's name.
    /// </summary>
    public static string EntryAssembly { get; }

    /// <summary>
    /// The entry assembly's version.
    /// </summary>
    public static string Version { get; }

    /// <summary>
    /// Constructor populates every property.
    /// </summary>
    static RuntimeContext()
    {
        IsDebugging = Debugger.IsAttached;

        ExecutingAssembly = Assembly.GetExecutingAssembly().GetName().Name ?? "Unknown";

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetName().Name ?? string.Empty)
            .ToList();

        var testAssembly = assemblies.FirstOrDefault(assembly => assembly.EndsWith(".Tests"));

        IsTesting = !string.IsNullOrWhiteSpace(testAssembly);

        EntryAssembly = testAssembly ?? Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
        
        var dependencies = $"{EntryAssembly}.deps.json";

        string contents;

        if (!File.Exists(dependencies) || string.IsNullOrWhiteSpace(contents = File.ReadAllText(dependencies)))
        {
            Version = string.Empty;
        }
        else
        {
            try
            {
                var libraries = JsonDocument.Parse(contents).RootElement
                    .GetProperty("libraries")
                    .EnumerateObject()
                    .ToList();

                Version = libraries
                    .FirstOrDefault(p => p.Name.StartsWith($"{ExecutingAssembly}/"))
                    .Name.Split('/').LastOrDefault() ?? string.Empty;
            }
            catch
            {
                Version = string.Empty;
            }
        }
    }
}