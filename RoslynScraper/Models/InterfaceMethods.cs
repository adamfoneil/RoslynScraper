namespace RoslynScraper.Models;

public class InterfaceMethods
{
    public string InterfaceName { get; set; } = string.Empty;
    public List<string> MethodNames { get; set; } = new();
}