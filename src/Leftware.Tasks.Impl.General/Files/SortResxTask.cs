using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;
using System.Xml;
using System.Xml.Linq;

namespace Leftware.Tasks.Impl.General.Files;

[Descriptor("FileSystem - Sort resources (resx) file")]
internal class SortResxTask : CommonTaskBase
{
    private const string SOURCE = "source";
    private const string TARGET = "target";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
            {
                new ReadFileTaskParameter(SOURCE, "Resx source file", true),
                new ReadFileTaskParameter(TARGET, "Resx target file", false),
            };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var source = Common.UtilCollection.Get<string>(input, SOURCE);
        var target = Common.UtilCollection.Get<string>(input, TARGET);

        XDocument doc;
        using (var inputStream = XmlReader.Create(source))
        {
            doc = XDocument.Load(inputStream);
            inputStream.Close();
        }

        // Create a sorted version of the XML
        var sortedDoc = SortDataByName(doc);
        sortedDoc.Save(target);
    }

    private static XDocument SortDataByName(XDocument resx)
    {
        if (resx.Root == null) return new XDocument();

        var result = new XDocument(
            new XElement(resx.Root.Name,
                from comment in resx.Root.Nodes() where comment.NodeType == XmlNodeType.Comment select comment,
                from schema in resx.Root.Elements() where schema.Name.LocalName == "schema" select schema,
                from resheader in resx.Root.Elements("resheader")
                let name = ((string?) resheader.Attribute("name")) ?? ""
                orderby name
                select resheader,
                from assembly in resx.Root.Elements("assembly")
                orderby ((string?)assembly.Attribute("name") ?? "")
                select assembly,
                from metadata in resx.Root.Elements("metadata")
                orderby ((string?)metadata.Attribute("name") ?? "")
                select metadata,
                (from data in resx.Root.Elements("data") orderby (string?)data.Attribute("name") select data)
                .DistinctBy(p => (string?)p.Attribute("name"))
            )
        );

        return result;
    }
}
