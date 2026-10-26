// K10DiWpf/GreetingOptions.cs
namespace K10DiWpf;

/// <summary>
/// Klasa odwzorowująca sekcję "Greeting" z appsettings.json.
/// Nazwy właściwości = nazwy kluczy w JSON.
/// </summary>
public class GreetingOptions
{
    public const string SectionName = "Greeting";

    public string UserName { get; set; } = "Gościu";

    public string Language { get; set; } = "pl";
}
