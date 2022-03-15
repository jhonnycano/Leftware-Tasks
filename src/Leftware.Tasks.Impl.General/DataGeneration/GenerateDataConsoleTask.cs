using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General.DataGeneration;

[Descriptor("Database - Generate data", Enabled = false)]
internal class GenerateDataConsoleTask : CommonTaskBase
{
    internal const string DEFINITION = "definition";
    internal const string TARGET = "target";
    //private readonly IColumnGeneratorFactory _columnGeneratorFactory;
    //private readonly DataGenerationParser _dataGenerationParser;

    public GenerateDataConsoleTask(
    //DataGenerationParser dataGenerationParser,
    //IColumnGeneratorFactory columnGeneratorFactory
    )
    {
        //_dataGenerationParser = dataGenerationParser;
        //_columnGeneratorFactory = columnGeneratorFactory;
    }

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadFileTaskParameter(DEFINITION, "Definition", true),
            new ReadFileTaskParameter(TARGET, "Target", false),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        //var definitionFile = input.Get<string>(DEFINITION);
        //var target = input.Get<string>(TARGET);

        //var json = File.ReadAllText(definitionFile);
        //var dataGenerationInfo = _dataGenerationParser.Load(json);
        //var generator = new DataGeneratorDataSet(_columnGeneratorFactory);
        //var ds = generator.Generate(dataGenerationInfo);
        //ds.WriteXml(target);
    }
}
