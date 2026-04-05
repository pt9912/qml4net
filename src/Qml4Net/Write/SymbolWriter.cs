using System.Globalization;
using System.Xml.Linq;
using Qml4Net.Model;

namespace Qml4Net.Write;

/// <summary>Writes symbols as &lt;symbols&gt;/&lt;symbol&gt;/&lt;layer&gt; elements.</summary>
internal static class SymbolWriter
{
    /// <summary>Creates a &lt;symbols&gt; element containing all symbols.</summary>
    public static XElement WriteSymbols(IReadOnlyDictionary<string, QmlSymbol> symbols)
    {
        var element = new XElement("symbols");
        foreach (var (key, symbol) in symbols)
            element.Add(WriteSymbol(key, symbol));
        return element;
    }

    private static XElement WriteSymbol(string key, QmlSymbol symbol)
    {
        var el = new XElement("symbol",
            new XAttribute("type", symbol.Type.ToQmlString()),
            new XAttribute("name", key),
            new XAttribute("alpha", symbol.Alpha.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("clip_to_extent", symbol.ClipToExtent ? "1" : "0"),
            new XAttribute("force_rhr", symbol.ForceRhr ? "1" : "0"));

        foreach (var layer in symbol.Layers)
            el.Add(WriteSymbolLayer(layer));

        return el;
    }

    /// <remarks>Always writes new format (<c>&lt;Option type="Map"&gt;</c>, QGIS &gt;= 3.26).</remarks>
    private static XElement WriteSymbolLayer(QmlSymbolLayer layer)
    {
        // Properties are always written in new <Option type="Map"> format
        var optionMap = new XElement("Option", new XAttribute("type", "Map"));
        foreach (var (name, value) in layer.Properties)
        {
            optionMap.Add(new XElement("Option",
                new XAttribute("name", name),
                new XAttribute("value", value),
                new XAttribute("type", "QString")));
        }

        return new XElement("layer",
            new XAttribute("class", layer.ClassName),
            new XAttribute("enabled", layer.Enabled ? "1" : "0"),
            new XAttribute("locked", layer.Locked ? "1" : "0"),
            new XAttribute("pass", layer.Pass.ToString()),
            optionMap);
    }
}
