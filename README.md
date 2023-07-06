I'm looking for help to build this console app to take this [Input](https://github.com/adamfoneil/RoslynScraper/blob/master/RoslynScraper/Models/Input.cs), use Roslyn to inspect a solution, and generate this [Output](https://github.com/adamfoneil/RoslynScraper/blob/master/RoslynScraper/Models/Output.cs).

The end goal is for me to be able to provide some metadata to stakeholders of a different project. In that other project, we have various permission-related interfaces, and I'd like to be able to surface implementation details to stakeholders who don't have Visual Studio and who aren't going to go digging around in actual code. Having a way to get all the interface implementations across an application lets me present them in my format of choice. This will help stakeholders and business analysts understand what's been implemented. With a little help, they can follow the C# to understand what their app is doing. My hope is to close some gaps in understanding between expected and actual behaviors in complex applications.

# Usage
I picture using this console app with a command line like this:

```csharp
RoslynScraper -s "c:\users\anyone\repos\MySolution.sln" -i "IPermissions, IWhatever, ISomethingElse" -o "c:\users\anyone\Desktop\scrapeoutput.json"
```
