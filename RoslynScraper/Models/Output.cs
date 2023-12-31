﻿namespace RoslynScraper.Models;

public class Output
{
	public IEnumerable<Item> Items { get; set; } = Enumerable.Empty<Item>();

	public class Item
	{
		public string InterfaceName { get; set; } = string.Empty;
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
		/// <summary>
		/// specific location in source file where Body is found
		/// </summary>
		public SourceLocation? Location { get; set; }
	}
}

