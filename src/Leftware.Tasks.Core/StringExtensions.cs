using DotLiquid;
using Microsoft.CodeAnalysis;
using System.Globalization;

namespace Leftware.Tasks.Core;

public static class StringExtensions
{
    public static string FormatLiquid(this string input, object source)
    {
        var template = Template.Parse(input);
        var result = template.Render(Hash.FromAnonymousObject(source));
        return result;
    }

    public static (string result, IList<string> errors) ApplyLiquid(string template, string source, string? customFiltersFile = null)
    {
        var inputHash = LiquidRequestParser.ParseRequest(source);
        var liquidTemplate = Template.Parse(template);
        var filters = new List<Type>();
        if (customFiltersFile is not null)
        {
            var filterCode = File.ReadAllText(customFiltersFile);
            var (type, compileErrors) = UtilCompile.Compile(filterCode);
            if (type == null) return ("", compileErrors);
            filters.Add(type);
        }

        var renderParameters = new RenderParameters(CultureInfo.InvariantCulture)
        {
            LocalVariables = inputHash,
            Filters = filters,
        };
        var result = liquidTemplate.Render(renderParameters);
        var errors = liquidTemplate.Errors.Select(e => e.Message).ToList();
        return (result, errors);
    }

    public static (string result, IList<string> errors) ApplyLiquid(Template template, string source)
    {
        var inputHash = LiquidRequestParser.ParseRequest(source);

        var result = template.Render(inputHash);
        var errors = template.Errors.Select(e => e.Message).ToList();
        return (result, errors);
    }

}
