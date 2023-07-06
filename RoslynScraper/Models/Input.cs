namespace RoslynScraper.Models;

public class Input
{
	/// <summary>
	/// VS .sln file to inspect
	/// </summary>
	public string SolutionFile { get; set; } = default!;

	/// <summary>
	/// what interfaces do I want to inspect?
	/// </summary>
	public IEnumerable<string> InspectInterfaces { get; set; } = Enumerable.Empty<string>();	
}
