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
}
