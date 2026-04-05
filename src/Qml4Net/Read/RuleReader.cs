using System.Xml.Linq;
using Qml4Net.Model;
using Qml4Net.Xml;

namespace Qml4Net.Read;

/// <summary>Reads &lt;rules&gt; and &lt;rule&gt; elements from QML XML.</summary>
internal sealed class RuleReader
{
    /// <summary>Parses all &lt;rule&gt; children of a &lt;rules&gt; element.</summary>
    public List<QmlRule> ReadRules(XElement rulesElement, List<string> warnings)
    {
        var rules = new List<QmlRule>();
        foreach (var el in rulesElement.Elements("rule"))
            rules.Add(ReadRule(el, warnings));
        return rules;
    }

    /// <summary>Parses a single &lt;rule&gt; element including nested children.</summary>
    public QmlRule ReadRule(XElement element, List<string> warnings)
    {
        // Recursively parse nested <rule> children first
        var children = new List<QmlRule>();
        foreach (var childEl in element.Elements("rule"))
            children.Add(ReadRule(childEl, warnings));

        // checkstate="0" means disabled; absent or any other value means enabled
        return new QmlRule(
            key: element.Attribute("key")?.Value,
            symbolKey: element.Attribute("symbol")?.Value,
            label: element.Attribute("label")?.Value,
            filter: element.Attribute("filter")?.Value,
            scaleMinDenominator: XmlHelpers.ParseDouble(element.Attribute("scalemindenom")?.Value),
            scaleMaxDenominator: XmlHelpers.ParseDouble(element.Attribute("scalemaxdenom")?.Value),
            enabled: element.Attribute("checkstate")?.Value != "0",
            children: children);
    }
}
