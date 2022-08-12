using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace Leftware.Tasks.Impl.General.Files;

[Descriptor("Files - Query Json Files")]
internal class QueryJsonFilesTask : CommonTaskBase
{
    private const string FOLDER = "folder";
    private IList<JsonQueryElement> _items;
    private string _folder;

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadFolderTaskParameter(FOLDER, "Folder")
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        _folder = input.Get<string>(FOLDER) ?? "";

        ReloadJsonItems();
        var re = new Regex(@"^(\w+)( (.*))?$");
        while(true)
        {
            Console.WriteLine($"item count: {_items.Count}");
            Console.Write(":> ");
            var commandLine = Console.ReadLine() ?? "";
            var match = re.Match(commandLine);
            if (!match.Success) continue;
            var command = match.Groups[1].Value;
            var args = match.Groups[3].Value;
            try
            {
                var result = ExecuteCommand(command, args);
                if (!result) break;
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }
        }
    }

    private bool ExecuteCommand(string command, string args)
    {
        switch(command.ToLowerInvariant())
        {
            case "exit":
                return false;
            case "clear":
                Console.Clear();
                break;
            case "show":
                Show(args);
                break;
            case "reload":
                ReloadJsonItems();
                break;
            case "source":
                ChangeSource(args);
                break;
            case "filterbykeys":
                FilterByKeys(args);
                break;
            case "filterbyfoundprop":
                FilterByFoundProp(args);
                break;
            case "filterbynotfoundprop":
                FilterByNotFoundProp(args);
                break;
            case "replacewithinnervalue":
                ReplaceWithInnerValue(args);
                break;
            case "filterbypropexpression":
                FilterByPropExpression(args);
                break;
            case "filter":
                FilterItems(args);
                break;
            default:
                Console.WriteLine("Command not found");
                break;
        }
        return true;
    }

    private void Show(string args)
    {
        var source = _items;
        if (!string.IsNullOrEmpty(args))
        {
            if (int.TryParse(args, out int value))
            {
                var item = _items.ElementAtOrDefault(value);
                Console.WriteLine(item);
                return;
            }

            source = _items.Select(t => new JsonQueryElement { File = t.File, Content = t.SelectToken(args) }).ToList();
        }

        foreach (var token in source)
        {
            Console.WriteLine(token);
            Console.WriteLine();
        }
    }

    private void ReloadJsonItems()
    {
        var result = new List<JsonQueryElement>();
        foreach (var file in Directory.GetFiles(_folder))
        {
            var content = File.ReadAllText(file);
            var token = JToken.Parse(content);
            result.Add(new JsonQueryElement { File = file, Content = token });
        }

        _items = result;
    }

    private void ChangeSource(string query)
    {
        var newList = new List<JsonQueryElement>();
        foreach (var item in _items)
        {
            var newToken = item.SelectToken(query);
            if (newToken != null && newToken.Type != JTokenType.Null)
            {
                newList.Add(new JsonQueryElement { File = item.File, Content = newToken });
            }
        }
        if (newList.Count == 0)
        {
            Console.WriteLine("Expression returned no results");
            return;
        }
        _items = newList;
    }

    private void FilterByKeys(string filter)
    {
        var newList = new List<JsonQueryElement>();
        foreach (var item in _items)
        {
            if (item.Type != JTokenType.Object) continue;
            var newObject = new JObject();
            foreach (var prop in (JObject)item.Content)
            {
                if (prop.Key.Contains(filter))
                    newObject[prop.Key] = prop.Value;
            }
            if (!newObject.Properties().Any()) continue;
            newList.Add(new JsonQueryElement { File = item.File, Content = newObject });
        }
        if (newList.Count == 0)
        {
            Console.WriteLine("Expression returned no results");
            return;
        }
        _items = newList;
    }

    private void FilterByFoundProp(string filter)
    {
        var newList = new List<JsonQueryElement>();
        foreach (var item in _items)
        {
            if (item.Type != JTokenType.Object) continue;
            var obj = (JObject)item.Content;
            foreach (var prop in obj)
            {
                if (prop.Value == null) continue;

                var resultToken = prop.Value.SelectToken(filter);
                if (resultToken != null)
                {
                    newList.Add(item);
                    break;
                }
            }
        }
        if (newList.Count == 0)
        {
            Console.WriteLine("Expression returned no results");
            return;
        }
        _items = newList;
    }

    private void FilterByNotFoundProp(string filter)
    {
        var newList = new List<JsonQueryElement>();
        foreach (var item in _items)
        {
            if (item.Type != JTokenType.Object) continue;
            var obj = (JObject)item.Content;
            foreach (var prop in obj)
            {
                if (prop.Value == null) continue;

                var resultToken = prop.Value.SelectToken(filter);
                if (resultToken == null)
                {
                    newList.Add(item);
                    break;
                }
            }
        }
        if (newList.Count == 0)
        {
            Console.WriteLine("Expression returned no results");
            return;
        }
        _items = newList;
    }

    private void FilterByPropExpression(string filter)
    {
        var exp = new Regex(@"^(.*)\s(eq|ne)\s(.*)$");
        var match = exp.Match(filter);
        if (!match.Success)
        {
            Console.WriteLine("Invalid expression");
            return;
        }

        var jsonPath = match.Groups[1].Value;
        var op = match.Groups[2].Value;
        var value = match.Groups[3].Value;

        var newList = new List<JsonQueryElement>();
        foreach (var item in _items)
        {
            if (item.Type != JTokenType.Object) continue;
            var obj = (JObject)item.Content;
            foreach (var prop in obj)
            {
                var resultToken = prop.Value.SelectToken(jsonPath);
                if (resultToken != null)
                {
                    switch (op)
                    {
                        case "eq":
                            if (resultToken.Value<string>() == value) newList.Add(item);
                            break;
                        case "ne":
                            if (resultToken.Value<string>() != value) newList.Add(item);
                            break;
                    }
                }
            }
        }
        if (newList.Count == 0)
        {
            Console.WriteLine("Expression returned no results");
            return;
        }
        _items = newList;
    }

    private void ReplaceWithInnerValue(string filter)
    {
        var newList = new List<JsonQueryElement>();
        foreach (var item in _items)
        {
            if (item.Type != JTokenType.Object) continue;
            var newObject = new JObject();
            foreach(var prop in (JObject) item.Content)
            {
                if (prop.Value == null) continue;

                var resultToken = prop.Value.SelectToken(filter);
                if (resultToken != null)
                    newObject[prop.Key] = resultToken;
            }
            if (!newObject.Properties().Any()) continue;
            newList.Add(new JsonQueryElement { File = item.File, Content = newObject });
        }
        if (newList.Count == 0)
        {
            Console.WriteLine("Expression returned no results");
            return;
        }
        _items = newList;
    }

    private void FilterItems(string query)
    {
        var newList = new List<JsonQueryElement>();
        foreach (var token in _items)
        {
            var newToken = token.SelectToken(query);
            if (false)
            {
                newList.Add(token);
            }
        }
        _items = newList;
    }
}

internal class JsonQueryElement
{
    public string File { get; set; } = "";
    public JToken Content { get; set; } = JToken.Parse("null");

    public JTokenType Type => Content.Type;

    public JToken? SelectToken(string query)
    {
        return Content.SelectToken(query);
    }

    public override string ToString()
    {
        return @$"File: {File}" + Environment.NewLine + @$"Content: {Content}";
    }
}