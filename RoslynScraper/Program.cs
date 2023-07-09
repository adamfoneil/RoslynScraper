using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Newtonsoft.Json;
using RoslynScraper.Models;

namespace RoslynScraper
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        private const string InterfaceKeyword = "interface ";
        private const string NamespaceKeyword = "namespace ";
        private const string ClassKeyword = "class ";

        /// <summary>
        /// Main method, entry point of the program.
        /// </summary>
        public static async Task Main()
        {
            var solutionPath = GetInput("Enter solution path:");
            var outputPath = GetInput("Enter output path:");
            var interfaceNames = GetInput("Enter interface names, separated by comma:")
                .Split(",")
                .Select(name => name.Trim())
                .ToList();

            var outputs = await ScrapeSolutionAsync(solutionPath, interfaceNames);
            
            await WriteOutputsAsync(outputs, outputPath);

            Console.WriteLine("Done");
        }

        /// <summary>
        /// Fetches user input with the provided prompt.
        /// </summary>
        /// <param name="prompt">The prompt to display to the user.</param>
        /// <returns>User's input as string.</returns>
        private static string GetInput(string prompt)
        {
            Console.WriteLine(prompt);
            return Console.ReadLine()!;
        }

        /// <summary>
        /// Scans a given solution for interfaces and extracts the relevant data.
        /// </summary>
        /// <param name="solutionPath">Path to the solution file.</param>
        /// <param name="interfaceNames">List of interface names to search for.</param>
        /// <returns>A list of extracted data items.</returns>
        private static async Task<List<Output.Item>> ScrapeSolutionAsync(string solutionPath, List<string> interfaceNames)
        {
            if (!MSBuildLocator.IsRegistered)
                MSBuildLocator.RegisterDefaults();
            
            var msWorkspace = MSBuildWorkspace.Create();
            var solution = await msWorkspace.OpenSolutionAsync(solutionPath);
            var outputs = new List<Output.Item>();

            foreach (var project in solution.Projects)
            {
                var interfaces = await GetInterfacesFromProjectAsync(project, interfaceNames);

                foreach (var document in project.Documents)
                {
                    var documentInterfaces = await GetInterfacesFromDocumentAsync(document, interfaceNames, interfaces);

                    outputs.AddRange(documentInterfaces);
                }
            }

            return outputs;
        }

        /// <summary>
        /// Extracts interfaces from a given project.
        /// </summary>
        /// <param name="project">The project to scan.</param>
        /// <param name="interfaceNames">List of interface names to search for.</param>
        /// <returns>A list of found interfaces.</returns>
        private static async Task<List<InterfaceMethods>> GetInterfacesFromProjectAsync(Project project, List<string> interfaceNames)
        {
            var interfaces = new List<InterfaceMethods>();

            foreach (var document in project.Documents)
            {
                var textLines = await GetDocumentLinesAsync(document);

                foreach (var name in interfaceNames)
                {
                    if (!textLines.Any(line => line.Contains(InterfaceKeyword + name))) 
                        continue;
                    
                    var interfaceModel = new InterfaceMethods
                    {
                        InterfaceName = name,
                        MethodNames = textLines
                            .Where(line => line.Contains('('))
                            .Select(line => line.Split('(')[0].Split(' ').Last())
                            .ToList()
                    };

                    interfaces.Add(interfaceModel);
                }
            }

            return interfaces;
        }

        /// <summary>
        /// Reads the text lines of a document if it is a regular source code document.
        /// </summary>
        /// <param name="document">The document to read.</param>
        /// <returns>A list of lines, or an empty list if the document is not a regular source code document.</returns>
        private static async Task<List<string>> GetDocumentLinesAsync(Document document)
        {
            if (document.SourceCodeKind != SourceCodeKind.Regular || string.IsNullOrEmpty(document.FilePath))
                return new List<string>();

            return (await File.ReadAllLinesAsync(document.FilePath)).ToList();
        }

        /// <summary>
        /// Extracts interfaces from a given document.
        /// </summary>
        /// <param name="document">The document to scan.</param>
        /// <param name="interfaceNames">List of interface names to search for.</param>
        /// <param name="interfaces">List of already found interfaces.</param>
        /// <returns>A list of extracted data items.</returns>
        private static async Task<List<Output.Item>> GetInterfacesFromDocumentAsync(Document document, List<string> interfaceNames, List<InterfaceMethods> interfaces)
        {
            var outputs = new List<Output.Item>();
            var textLines = await GetDocumentLinesAsync(document);

            foreach (var name in interfaceNames)
            {
                if (!textLines.Any(line => line.Replace(" ", "").Contains($":{name}")) 
                    && !textLines.Any(line => line.Replace(" ", "").Contains($",{name}")))
                    continue;

                var namespaceLine = textLines.FirstOrDefault(line => line.Contains(NamespaceKeyword));
                var interfaceModel = interfaces.SingleOrDefault(model => model.InterfaceName == name);
                
                if (interfaceModel == null) 
                    continue;

                foreach (var methodName in interfaceModel.MethodNames)
                {
                    if (namespaceLine == null) continue;
                    if (document.FilePath == null) continue;
                    var output = CreateOutputFromMethod(name, textLines, namespaceLine, methodName, document.FilePath);

                    outputs.Add(output);
                }
            }

            return outputs;
        }

        /// <summary>
        /// Creates an output item for a given method.
        /// </summary>
        /// <param name="name">The name of the interface.</param>
        /// <param name="textLines">Lines of text in the document.</param>
        /// <param name="namespaceLine">The namespace line of the document.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="filePath">Path to the document file.</param>
        /// <returns>A populated output item.</returns>
        private static Output.Item CreateOutputFromMethod(string name, List<string> textLines, string namespaceLine, string methodName, string filePath)
        {
            var classLine = textLines.First(line => line.Contains(name));

            string className;
            if (classLine.Contains(ClassKeyword))
            {
                var parts = classLine.Split(ClassKeyword);
                className = parts.Length > 1 ? parts[1].Split(" ")[0] : string.Empty;
            }
            else
            {
                className = string.Empty;
            }

            var output = new Output.Item
            {
                Namespace = namespaceLine.Split(" ").Length > 1 ? namespaceLine.Split(" ").Last().Replace(";", "") : string.Empty,
                ClassName = className,
                MemberName = methodName,
                InterfaceName = name
            };

            var methodLine = textLines.FindIndex(line => line.Contains(methodName));
            var startMethodLine = methodLine + 1;
            var endMethodLine = FindEndMethodLine(textLines, startMethodLine);

            output.Body = string.Join('\n', textLines.Skip(startMethodLine).Take(endMethodLine - startMethodLine));
            output.Location = new SourceLocation
            {
                EndLine = endMethodLine,
                Path = filePath,
                StartLine = startMethodLine
            };

            return output;
        }

        /// <summary>
        /// Determines the line where the method ends in the document text lines.
        /// </summary>
        /// <param name="textLines">Lines of text in the document.</param>
        /// <param name="startLine">The line where the method starts.</param>
        /// <returns>The line where the method ends.</returns>
        private static int FindEndMethodLine(List<string> textLines, int startLine)
        {
            var endLine = 0;

            for (var i = startLine + 1; i < textLines.Count; i++)
            {
                if (textLines[i].Contains("public") || textLines[i].Contains("private"))
                {
                    endLine = i - 1;
                    break;
                }
            }

            return endLine;
        }

        /// <summary>
        /// Writes the output data to a file in JSON format.
        /// </summary>
        /// <param name="outputs">The output data to write.</param>
        /// <param name="outputPath">The path where to write the output.</param>
        private static async Task WriteOutputsAsync(List<Output.Item> outputs, string outputPath)
        {
            var outputString = JsonConvert.SerializeObject(outputs, Formatting.Indented);
            await File.WriteAllTextAsync(outputPath, outputString);
        }
    }
}
