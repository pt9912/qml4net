using System.Xml.Linq;
using Qml4Net.Model;
using Qml4Net.Xml;

namespace Qml4Net.Read;

/// <summary>Reads &lt;symbols&gt; and &lt;symbol&gt; elements from QML XML.</summary>
internal sealed class SymbolReader
{
    /// <summary>Parses a &lt;symbols&gt; block into a name-keyed dictionary.</summary>
    public Dictionary<string, QmlSymbol> ReadSymbols(XElement symbolsElement, List<string> warnings)
    {
        var result = new Dictionary<string, QmlSymbol>();
        foreach (var el in symbolsElement.Elements("symbol"))
        {
            var name = el.Attribute("name")?.Value;
            if (name is null)
            {
                warnings.Add("Symbol without name attribute, skipping");
                continue;
            }
            result[name] = ReadSymbol(el, warnings);
        }
        return result;
    }

    /// <summary>Parses a single &lt;symbol&gt; element.</summary>
    public QmlSymbol ReadSymbol(XElement element, List<string> warnings)
    {
        var typeStr = element.Attribute("type")?.Value ?? "";
        var type = QmlSymbolTypeExtensions.FromQmlString(typeStr);
        if (type == QmlSymbolType.Unknown)
            warnings.Add($"Unknown symbol type: {typeStr}");

        var layers = new List<QmlSymbolLayer>();
        foreach (var layerEl in element.Elements("layer"))
            layers.Add(ReadSymbolLayer(layerEl, warnings));

        return new QmlSymbol(
            type: type,
            name: element.Attribute("name")?.Value,
            alpha: XmlHelpers.ParseDouble(element.Attribute("alpha")?.Value) ?? 1.0,
            clipToExtent: XmlHelpers.ParseBool(element.Attribute("clip_to_extent")?.Value, defaultValue: true),
            forceRhr: XmlHelpers.ParseBool(element.Attribute("force_rhr")?.Value),
            layers: layers);
    }

    private static QmlSymbolLayer ReadSymbolLayer(XElement element, List<string> warnings)
    {
        var className = element.Attribute("class")?.Value ?? "Unknown";
        var type = QmlSymbolLayerTypeExtensions.FromClassName(className);
        if (type == QmlSymbolLayerType.Unknown)
            warnings.Add($"Unknown symbol layer class: {className}");

        return new QmlSymbolLayer(
            type: type,
            className: className,
            enabled: XmlHelpers.ParseBool(element.Attribute("enabled")?.Value, defaultValue: true),
            locked: XmlHelpers.ParseBool(element.Attribute("locked")?.Value),
            pass: XmlHelpers.ParseInt(element.Attribute("pass")?.Value) ?? 0,
            properties: XmlHelpers.ExtractProperties(element));
    }
}
