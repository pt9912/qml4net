using System.Globalization;
using System.Xml.Linq;
using Qml4Net.Model;

namespace Qml4Net.Write;

/// <summary>Writes rules as &lt;rules&gt;/&lt;rule&gt; elements.</summary>
internal static class RuleWriter
{
    /// <summary>Creates a &lt;rules key="renderer_rules"&gt; element.</summary>
    public static XElement WriteRules(IReadOnlyList<QmlRule> rules)
    {
        var element = new XElement("rules", new XAttribute("key", "renderer_rules"));
        foreach (var rule in rules)
            element.Add(WriteRule(rule));
        return element;
    }

    private static XElement WriteRule(QmlRule rule)
    {
        var el = new XElement("rule");

        if (rule.Key is not null)
            el.Add(new XAttribute("key", rule.Key));
        if (rule.SymbolKey is not null)
            el.Add(new XAttribute("symbol", rule.SymbolKey));
        if (rule.Label is not null)
            el.Add(new XAttribute("label", rule.Label));
        if (rule.Filter is not null)
            el.Add(new XAttribute("filter", rule.Filter));
        // Scale denominators as integers (Dart: .toStringAsFixed(0))
        if (rule.ScaleMinDenominator is not null)
            el.Add(new XAttribute("scalemindenom",
                rule.ScaleMinDenominator.Value.ToString("F0", CultureInfo.InvariantCulture)));
        if (rule.ScaleMaxDenominator is not null)
            el.Add(new XAttribute("scalemaxdenom",
                rule.ScaleMaxDenominator.Value.ToString("F0", CultureInfo.InvariantCulture)));
        // checkstate is only written when disabled
        if (!rule.Enabled)
            el.Add(new XAttribute("checkstate", "0"));

        foreach (var child in rule.Children)
            el.Add(WriteRule(child));

        return el;
    }
}
