namespace Qml4Net.Model;

/// <summary>Symbol layer classes (<c>class</c> attribute on <c>&lt;layer&gt;</c>).</summary>
public enum QmlSymbolLayerType
{
    /// <summary>Circle, square, triangle — well-known point shapes.</summary>
    SimpleMarker,
    /// <summary>Point marker rendered from an SVG image file.</summary>
    SvgMarker,
    /// <summary>Stroke rendering with color, width, dash pattern.</summary>
    SimpleLine,
    /// <summary>Polygon fill with optional outline.</summary>
    SimpleFill,
    /// <summary>Polygon fill using a raster/image pattern.</summary>
    RasterFill,
    /// <summary>Unrecognised layer class (forward compatibility).</summary>
    Unknown
}

/// <summary>String conversion helpers for <see cref="QmlSymbolLayerType"/>.</summary>
public static class QmlSymbolLayerTypeExtensions
{
    /// <summary>Parses the <c>class</c> attribute of a <c>&lt;layer&gt;</c> element.</summary>
    public static QmlSymbolLayerType FromClassName(string className) => className switch
    {
        "SimpleMarker" => QmlSymbolLayerType.SimpleMarker,
        "SvgMarker" => QmlSymbolLayerType.SvgMarker,
        "SimpleLine" => QmlSymbolLayerType.SimpleLine,
        "SimpleFill" => QmlSymbolLayerType.SimpleFill,
        "RasterFill" => QmlSymbolLayerType.RasterFill,
        _ => QmlSymbolLayerType.Unknown,
    };

    /// <summary>Serializes back to the QML <c>class</c> attribute value.</summary>
    public static string ToClassName(this QmlSymbolLayerType type) => type switch
    {
        QmlSymbolLayerType.SimpleMarker => "SimpleMarker",
        QmlSymbolLayerType.SvgMarker => "SvgMarker",
        QmlSymbolLayerType.SimpleLine => "SimpleLine",
        QmlSymbolLayerType.SimpleFill => "SimpleFill",
        QmlSymbolLayerType.RasterFill => "RasterFill",
        _ => "Unknown",
    };
}
