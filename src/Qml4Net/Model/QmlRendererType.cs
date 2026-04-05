namespace Qml4Net.Model;

/// <summary>Renderer kinds supported by QGIS QML.</summary>
public enum QmlRendererType
{
    /// <summary>One symbol applied to all features.</summary>
    SingleSymbol,
    /// <summary>Features classified by a single field value.</summary>
    CategorizedSymbol,
    /// <summary>Features classified into numeric value ranges.</summary>
    GraduatedSymbol,
    /// <summary>Features selected by filter expressions, optionally nested.</summary>
    RuleRenderer,
    /// <summary>Unrecognised renderer type (forward compatibility).</summary>
    Unknown
}

/// <summary>String conversion helpers for <see cref="QmlRendererType"/>.</summary>
public static class QmlRendererTypeExtensions
{
    /// <summary>Parses the <c>type</c> attribute of a <c>&lt;renderer-v2&gt;</c> element.</summary>
    public static QmlRendererType FromQmlString(string value) => value switch
    {
        "singleSymbol" => QmlRendererType.SingleSymbol,
        "categorizedSymbol" => QmlRendererType.CategorizedSymbol,
        "graduatedSymbol" => QmlRendererType.GraduatedSymbol,
        "RuleRenderer" => QmlRendererType.RuleRenderer,
        _ => QmlRendererType.Unknown,
    };

    /// <summary>Serializes back to the QML XML attribute value.</summary>
    public static string ToQmlString(this QmlRendererType type) => type switch
    {
        QmlRendererType.SingleSymbol => "singleSymbol",
        QmlRendererType.CategorizedSymbol => "categorizedSymbol",
        QmlRendererType.GraduatedSymbol => "graduatedSymbol",
        QmlRendererType.RuleRenderer => "RuleRenderer",
        _ => "singleSymbol",
    };
}
