using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;
using Leftware.Tasks.Core.TaskParameters.Conditions;
using Newtonsoft.Json;
using System.Xml;

namespace Leftware.Tasks.Impl.General.Files;

[Descriptor("Xml - Replicate Xml element")]
internal class ReplicateXmlElementConsoleTask : CommonTaskBase
{
    private const string SOURCE = "source";
    private const string TARGET = "target";
    private const string USE_NS = "useNs";
    private const string NS_SOURCE = "nsSource";
    private const string ELEM_PATH = "elementPath";

    public ReplicateXmlElementConsoleTask()
    {
    }

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadFileTaskParameter(SOURCE, "Xml source file"),
            new ReadFileTaskParameter(TARGET, "Xml target file"),
            new ReadStringTaskParameter(ELEM_PATH, "Element path"),
            new ReadBoolTaskParameter(USE_NS, "Use namespaces?"),
            new SelectFromCollectionTaskParameter(NS_SOURCE, "Namespaces file", Defs.Collections.XML_NS_FILE)
                .When(new EqualsCondition(USE_NS, true)),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var source = input.Get<string>(SOURCE);
        var target = input.Get<string>(TARGET);
        var path = input.Get<string>(ELEM_PATH);
        var nsFile = input.Get<string>(SOURCE);

        var ns = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(nsFile))
        {
            var json = File.ReadAllText(nsFile);
            var fileContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            fileContent.ForEach(itm => ns[itm.Key] = itm.Value);
        }

        if (ns.Keys.Count == 0) ns = null;

        var sourceDoc = new XmlDocument();
        sourceDoc.Load(source);

        var sourceNode = SelectSingleNode(sourceDoc, path, ns);
        if (sourceNode == null)
        {
            Console.WriteLine("Path {0} not found on source file", path);
            return;
        }

        var targetDoc = new XmlDocument();
        targetDoc.Load(target);
        var targetNode = SelectSingleNode(targetDoc, path, ns);
        if (targetNode == null)
        {
            Console.WriteLine("Path {0} not found on target file", path);
            return;
        }

        var currentParent = targetNode.ParentNode;
        var refNode = targetNode.PreviousSibling;
        var removedNode = currentParent.RemoveChild(targetNode);

        var importedNode = targetDoc.ImportNode(sourceNode, true);
        if (refNode != null)
            currentParent.InsertAfter(importedNode, refNode);
        else
            currentParent.AppendChild(importedNode);
        targetDoc.Save(target);
    }

    private static XmlNode SelectSingleNode(XmlDocument doc, string path, IDictionary<string, string> nsDic)
    {
        if (nsDic == null) return doc.SelectSingleNode(path);
        var nsmgr = GetNamespaceManager(doc, nsDic);
        return doc.SelectSingleNode(path, nsmgr);
    }

    private static XmlNamespaceManager GetNamespaceManager(XmlDocument doc, IDictionary<string, string> nsDic)
    {
        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        foreach (var itm in nsDic) nsmgr.AddNamespace(itm.Key, itm.Value);
        return nsmgr;
    }
}
