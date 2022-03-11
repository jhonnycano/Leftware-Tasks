using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Leftware.Tasks.Impl.General.DataGeneration;

/*
[Descriptor("Database - Create data generation file", Enabled = false)]
internal class CreateGenerationFileTask : CommonTaskBase
{
    private const string FILE = "file";
    private readonly IColumnModelFactory _columnModelFactory;
    private readonly IConsoleLoaderFactory _consoleLoaderFactory;

    public CreateGenerationFileTask(
        IColumnModelFactory columnModelFactory,
        IConsoleLoaderFactory consoleLoaderFactory
    )
    {
        _columnModelFactory = columnModelFactory;
        _consoleLoaderFactory = consoleLoaderFactory;
    }

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        bool reader(ConsoleReadContext ctx)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            var file = UtilConsole.ReadString("Target file. Press <enter> to use a default name");
            if (string.IsNullOrEmpty(file))
            {
                var filename = Guid.NewGuid() + ".data-gen.json";
                file = Path.Combine(Environment.CurrentDirectory, filename);
                UtilConsole.ColorWriteLine(ConsoleColor.DarkYellow, "  Target file would be {0}", file);
            }

            if (!GetInputForNewFile(result)) return false;
            result[FILE] = file;
            return true;
        }

        return new List<TaskParameter>
        {
            new CustomConsoleTaskParameter(reader),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var columnList = input["columns"] as List<ColumnModel>;
        var sourceList = input["sources"] as List<ItemSourceModel>;
        var file = input.Get<string>(FILE);

        var obj = new DataGenerationInfo { Columns = columnList, Sources = sourceList, RowsToGenerate = 1000 };
        var jss = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };
        var json = JsonConvert.SerializeObject(obj, jss);
        File.WriteAllText(file, json);
    }

    private bool GetInputForNewFile(IDictionary<string, object> result)
    {
        Console.WriteLine("First, we are going to add column definitions...");

        var definitionList = new List<ColumnModel>();
        var sourceList = new List<ItemSourceModel>();
        result["columns"] = definitionList;
        result["sources"] = sourceList;

        if (!ReadColumnDefinitions(definitionList)) return false;

        var readSources = UtilConsole.ReadBool("Add external sources to the definition file?");
        if (readSources == null || readSources == false) return true;

        if (!ReadSourceDefinition(sourceList)) return false;

        return true;
    }

    private bool ReadColumnDefinitions(List<ColumnModel> columnList)
    {
        var i = 0;
        while (true)
        {
            Console.WriteLine($"Column # {i + 1}");
            var def = CreateColumnDefinition();
            if (def == null) continue;

            columnList.Add(def);
            i++;

            var shouldContinue = UtilConsole.ReadBool("Continue adding columns?", true);
            if (shouldContinue == null) return false;
            if (!shouldContinue.Value) break;
        }

        return true;
    }

    private ColumnModel CreateColumnDefinition()
    {
        var colName = UtilConsole.ReadString("Column name");

        var dataType = UtilConsole.SelectFromEnum("Data type", DataType.None);
        if (dataType == DataType.None) return null;

        var sourceDic = new Dictionary<DataType, List<ColumnDefinitionType>>
        {
            {
                DataType.Integer,
                new List<ColumnDefinitionType>
                    {ColumnDefinitionType.IntegerRange, ColumnDefinitionType.DatabaseQuery}
            },
            {
                DataType.Double,
                new List<ColumnDefinitionType>
                    {ColumnDefinitionType.DoubleRange, ColumnDefinitionType.DatabaseQuery}
            },
            {
                DataType.DateTime,
                new List<ColumnDefinitionType>
                {
                    ColumnDefinitionType.DateRange, ColumnDefinitionType.DateTimeRange,
                    ColumnDefinitionType.DatabaseQuery
                }
            },
            {
                DataType.String,
                new List<ColumnDefinitionType>
                {
                    ColumnDefinitionType.RandomPattern, ColumnDefinitionType.RandomChars,
                    ColumnDefinitionType.Template, ColumnDefinitionType.DatabaseQuery
                }
            },
            {DataType.Guid, new List<ColumnDefinitionType> {ColumnDefinitionType.Guid}}
        };
        var sourceList = sourceDic[dataType];
        var sourceListAsString = sourceList.Select(itm => itm.ToString()).ToList();
        var sourceTypeAsString = UtilConsole.SelectFromList(sourceListAsString, "Select generator source");
        var sourceType = UtilEnum.Get<ColumnDefinitionType>(sourceTypeAsString);

        var def = _columnModelFactory.GetInstance(sourceType);
        var loader = _consoleLoaderFactory.GetInstance(sourceType);

        def.Name = colName;
        loader.LoadFromConsole(def);

        return def;
    }

    private static bool ReadSourceDefinition(List<ItemSourceModel> sourceList)
    {
        var i = 0;
        while (true)
        {
            Console.WriteLine($"External item source # {i + 1}");
            var def = CreateSourceDefinition();
            if (def == null) continue;

            sourceList.Add(def);
            i++;

            var shouldContinue = UtilConsole.ReadBool("Continue adding sources?", true);
            if (shouldContinue == null) return false;
            if (!shouldContinue.Value) break;
        }

        return true;
    }

    private static ItemSourceModel CreateSourceDefinition()
    {
        var sourceName = UtilConsole.ReadString("Source name");
        if (sourceName == null) return null;

        var sourceType = UtilConsole.SelectFromEnum("Source type", ItemSourceType.None);
        if (sourceType == ItemSourceType.None) return null;

        switch (sourceType)
        {
            case ItemSourceType.Inline:
                var i = 0;
                HashSet<string> options = new();
                while (true)
                {
                    var option = UtilConsole.ReadString($"Option # {i}");
                    options.Add(option);

                    var shouldContinue = UtilConsole.ReadBool("Continue adding options?", true);
                    if (shouldContinue == null) return null;
                    if (!shouldContinue.Value) break;
                }

                return new InlineSourceModel { Name = sourceName, Content = options.Cast<object>().ToList() };
            case ItemSourceType.File:
                var props = new Dictionary<string, string>();
                var sourcePath = UtilConsole.ReadFile("Source file");
                if (sourcePath == null) return null;
                var sourceFormat = UtilConsole.SelectFromEnum("Source format", ItemSourceFormat.None);
                if (sourceFormat == ItemSourceFormat.None) return null;
                if (sourceFormat == ItemSourceFormat.JsonArrayOfObjects)
                {
                    var propName = UtilConsole.ReadString("Property name");
                    if (propName == null) return null;
                    props["propertyName"] = propName;
                }

                return new FileSourceModel
                { Name = sourceName, Path = sourcePath, Format = sourceFormat, Props = props };
            case ItemSourceType.Query:
                var sourceProvider = UtilConsole.SelectFromEnum("Source format", DatabaseEngine.None);
                if (sourceProvider == DatabaseEngine.None) return null;
                var sourceConnection = UtilConsole.ReadString("Connection string");
                if (sourceConnection == null) return null;
                var sourceQuery = UtilConsole.ReadString("Query");
                if (sourceQuery == null) return null;

                return new QuerySourceModel
                {
                    Name = sourceName,
                    ProviderType = sourceProvider,
                    ConnectionString = sourceConnection,
                    Query = sourceQuery
                };
            default:
                return null;
        }
    }
}
*/