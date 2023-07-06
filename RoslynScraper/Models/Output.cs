namespace RoslynScraper.Models;

public class Output
{
	public IEnumerable<Item> Items { get; set; } = Enumerable.Empty<Item>();

	public class Item
	{
		/// <summary>
		/// class namespace
		/// </summary>
		public string Namespace { get; set; } = default!;
		/// <summary>
		/// containing class
		/// </summary>
		public string ClassName { get; set; } = default!;
		/// <summary>
		/// method or property name (or event or other member type, honestly)
		/// </summary>
		public string MemberName { get; set; } = default!;
		/// <summary>
		/// C# method or property body
		/// </summary>
		public string Body { get; set; } = default!;
	}
}

