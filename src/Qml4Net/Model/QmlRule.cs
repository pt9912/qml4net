namespace Qml4Net.Model;

/// <summary>A rule in a QGIS rule-based renderer (supports nesting).</summary>
public sealed class QmlRule
{
    /// <summary>Unique rule identifier.</summary>
    public string? Key { get; }

    /// <summary>Key referencing a symbol in the renderer's symbol map.</summary>
    public string? SymbolKey { get; }

    /// <summary>Human-readable display label.</summary>
    public string? Label { get; }

    /// <summary>QGIS filter expression string.</summary>
    public string? Filter { get; }

    /// <summary>Minimum scale denominator at which the rule is active.</summary>
    public double? ScaleMinDenominator { get; }

    /// <summary>Maximum scale denominator at which the rule is active.</summary>
    public double? ScaleMaxDenominator { get; }

    /// <summary>Whether this rule is active.</summary>
    public bool Enabled { get; }

    /// <summary>Nested child rules.</summary>
    public IReadOnlyList<QmlRule> Children { get; }

    /// <summary>Creates a new rule.</summary>
    public QmlRule(
        string? key = null,
        string? symbolKey = null,
        string? label = null,
        string? filter = null,
        double? scaleMinDenominator = null,
        double? scaleMaxDenominator = null,
        bool enabled = true,
        IEnumerable<QmlRule>? children = null)
    {
        Key = key;
        SymbolKey = symbolKey;
        Label = label;
        Filter = filter;
        ScaleMinDenominator = scaleMinDenominator;
        ScaleMaxDenominator = scaleMaxDenominator;
        Enabled = enabled;
        Children = (children?.ToArray() ?? []).AsReadOnly();
    }
}
