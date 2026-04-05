namespace Qml4Net.Model;

/// <summary>Symbol geometry type (<c>type</c> attribute on <c>&lt;symbol&gt;</c>).</summary>
public enum QmlSymbolType
{
    /// <summary>Point symbols.</summary>
    Marker,
    /// <summary>Line/stroke symbols.</summary>
    Line,
    /// <summary>Polygon fill symbols.</summary>
    Fill,
    /// <summary>Unrecognised symbol type.</summary>
    Unknown
}

/// <summary>String conversion helpers for <see cref="QmlSymbolType"/>.</summary>
public static class QmlSymbolTypeExtensions
{
    /// <summary>Parses the <c>type</c> attribute of a <c>&lt;symbol&gt;</c> element.</summary>
    public static QmlSymbolType FromQmlString(string value) => value switch
    {
        "marker" => QmlSymbolType.Marker,
        "line" => QmlSymbolType.Line,
        "fill" => QmlSymbolType.Fill,
        _ => QmlSymbolType.Unknown,
    };

    /// <summary>Serializes back to the QML XML attribute value.</summary>
    public static string ToQmlString(this QmlSymbolType type) => type switch
    {
        QmlSymbolType.Marker => "marker",
        QmlSymbolType.Line => "line",
        QmlSymbolType.Fill => "fill",
        _ => "marker",
    };
}
