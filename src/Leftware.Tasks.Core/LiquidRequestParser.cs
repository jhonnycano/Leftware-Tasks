using DotLiquid;
using Newtonsoft.Json;

namespace Leftware.Tasks.Core;

public class LiquidRequestParser
{
    public Hash ParseRequest(string content, string? rootElement = null)
    {
        if (rootElement == null)
        {
            var requestJson = JsonConvert.DeserializeObject<IDictionary<string, object>>(content, new DictionaryConverter());
            return Hash.FromDictionary(requestJson);
        }
        else
        {
            var transformInput = new Dictionary<string, object>();
            var requestJson = JsonConvert.DeserializeObject<IDictionary<string, object>>(content, new DictionaryConverter());
            transformInput.Add(rootElement, requestJson);
            return Hash.FromDictionary(transformInput);
        }
    }
}
