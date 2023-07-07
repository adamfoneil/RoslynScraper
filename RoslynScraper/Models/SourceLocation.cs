namespace RoslynScraper.Models;

public class SourceLocation
{
	public string Path { get; set; } = default!;
	public int StartLine { get; set; }
	public int EndLine { get; set; }
}
