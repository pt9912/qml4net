using System.Xml.Linq;
using Qml4Net.Model;
using Qml4Net.Xml;

namespace Qml4Net.Read;

/// <summary>Reads a &lt;renderer-v2&gt; element into a <see cref="QmlRenderer"/>.</summary>
internal sealed class RendererReader
{
    private static readonly SymbolReader SymbolReader = new();
    private static readonly RuleReader RuleReader = new();

    private static readonly string[] PropertyWhitelist =
        ["forceraster", "symbollevels", "enableorderby", "referencescale"];

    /// <summary>Parses a renderer element, appending non-fatal issues to warnings.</summary>
    public QmlRenderer ReadRenderer(XElement element, List<string> warnings)
    {
        // Renderer type — unknown values produce a warning but don't fail
        var typeStr = element.Attribute("type")?.Value ?? "";
        var type = QmlRendererTypeExtensions.FromQmlString(typeStr);
        if (type == QmlRendererType.Unknown)
            warnings.Add($"Unknown renderer type: {typeStr}");

        // Shared symbols referenced by categories, ranges, and rules
        var symbols = new Dictionary<string, QmlSymbol>();
        var symbolsEl = element.Element("symbols");
        if (symbolsEl is not null)
            symbols = SymbolReader.ReadSymbols(symbolsEl, warnings);

        // Only whitelisted renderer-level attributes are kept as properties
        var properties = new Dictionary<string, string>();
        foreach (var attr in PropertyWhitelist)
        {
            var val = element.Attribute(attr)?.Value;
            if (val is not null)
                properties[attr] = val;
        }

        // Categories (categorizedSymbol renderer)
        var categories = new List<QmlCategory>();
        var categoriesEl = element.Element("categories");
        if (categoriesEl is not null)
        {
            foreach (var el in categoriesEl.Elements("category"))
                categories.Add(ReadCategory(el));
        }

        // Ranges (graduatedSymbol renderer)
        var ranges = new List<QmlRange>();
        var rangesEl = element.Element("ranges");
        if (rangesEl is not null)
        {
            foreach (var el in rangesEl.Elements("range"))
                ranges.Add(ReadRange(el));
        }

        // Rules (RuleRenderer — may be nested)
        var rules = new List<QmlRule>();
        var rulesEl = element.Element("rules");
        if (rulesEl is not null)
            rules = RuleReader.ReadRules(rulesEl, warnings);

        return new QmlRenderer(
            type: type,
            attribute: element.Attribute("attr")?.Value,
            graduatedMethod: element.Attribute("graduatedMethod")?.Value,
            symbols: symbols,
            categories: categories,
            ranges: ranges,
            rules: rules,
            properties: properties);
    }

    private static QmlCategory ReadCategory(XElement element) =>
        new(
            Value: element.Attribute("value")?.Value ?? "",
            SymbolKey: element.Attribute("symbol")?.Value ?? "0",
            Label: element.Attribute("label")?.Value,
            Render: XmlHelpers.ParseBool(element.Attribute("render")?.Value, defaultValue: true));

    private static QmlRange ReadRange(XElement element) =>
        new(
            Lower: XmlHelpers.ParseDouble(element.Attribute("lower")?.Value) ?? 0,
            Upper: XmlHelpers.ParseDouble(element.Attribute("upper")?.Value) ?? 0,
            SymbolKey: element.Attribute("symbol")?.Value ?? "0",
            Label: element.Attribute("label")?.Value,
            Render: XmlHelpers.ParseBool(element.Attribute("render")?.Value, defaultValue: true));
}
