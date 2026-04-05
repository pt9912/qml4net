using System.Globalization;
using System.Xml.Linq;
using Qml4Net.Model;

namespace Qml4Net.Write;

/// <summary>Writes a QmlRenderer as a &lt;renderer-v2&gt; element.</summary>
internal static class RendererWriter
{
    /// <summary>Creates a &lt;renderer-v2&gt; element for the given renderer.</summary>
    public static XElement WriteRenderer(QmlRenderer renderer)
    {
        var el = new XElement("renderer-v2",
            new XAttribute("type", renderer.Type.ToQmlString()));

        if (renderer.Attribute is not null)
            el.Add(new XAttribute("attr", renderer.Attribute));
        if (renderer.GraduatedMethod is not null)
            el.Add(new XAttribute("graduatedMethod", renderer.GraduatedMethod));

        // Renderer-level properties (forceraster, symbollevels, etc.) as attributes
        foreach (var (key, value) in renderer.Properties)
            el.Add(new XAttribute(key, value));

        // Categories
        if (renderer.Categories.Count > 0)
        {
            var categoriesEl = new XElement("categories");
            foreach (var cat in renderer.Categories)
            {
                var catEl = new XElement("category",
                    new XAttribute("value", cat.Value),
                    new XAttribute("symbol", cat.SymbolKey),
                    new XAttribute("render", cat.Render ? "true" : "false"));
                if (cat.Label is not null)
                    catEl.Add(new XAttribute("label", cat.Label));
                categoriesEl.Add(catEl);
            }
            el.Add(categoriesEl);
        }

        // Ranges
        if (renderer.Ranges.Count > 0)
        {
            var rangesEl = new XElement("ranges");
            foreach (var range in renderer.Ranges)
            {
                var rangeEl = new XElement("range",
                    // Dart uses .toStringAsFixed(15) for range precision
                    new XAttribute("lower", range.Lower.ToString("F15", CultureInfo.InvariantCulture)),
                    new XAttribute("upper", range.Upper.ToString("F15", CultureInfo.InvariantCulture)),
                    new XAttribute("symbol", range.SymbolKey),
                    new XAttribute("render", range.Render ? "true" : "false"));
                if (range.Label is not null)
                    rangeEl.Add(new XAttribute("label", range.Label));
                rangesEl.Add(rangeEl);
            }
            el.Add(rangesEl);
        }

        // Rules
        if (renderer.Rules.Count > 0)
            el.Add(RuleWriter.WriteRules(renderer.Rules));

        // Symbols
        if (renderer.Symbols.Count > 0)
            el.Add(SymbolWriter.WriteSymbols(renderer.Symbols));

        // Empty elements required by QGIS for compatibility
        el.Add(new XElement("rotation"));
        el.Add(new XElement("sizescale"));

        return el;
    }
}
