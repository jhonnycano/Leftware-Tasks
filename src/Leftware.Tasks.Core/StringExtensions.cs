using DotLiquid;

namespace Leftware.Tasks.Core;

public static class StringExtensions
{
    public static string FormatLiquid(this string input, object source)
    {
        var template = Template.Parse(input);
        var result = template.Render(Hash.FromAnonymousObject(source));
        return result;
    }

    public static (string result, IList<string> errors) ApplyLiquid(string template, string source)
    {
        var inputHash = new LiquidRequestParser().ParseRequest(source);
        var liquidTemplate = Template.Parse(template);

        var result = liquidTemplate.Render(inputHash);
        var errors = liquidTemplate.Errors.Select(e => e.Message).ToList();
        return (result, errors);
    }
}
