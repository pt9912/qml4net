namespace Qml4Net.Model;

/// <summary>A QML symbol composed of one or more <see cref="QmlSymbolLayer"/>s.</summary>
public sealed class QmlSymbol
{
    /// <summary>The geometry type this symbol renders.</summary>
    public QmlSymbolType Type { get; }

    /// <summary>The symbol key reference.</summary>
    public string? Name { get; }

    /// <summary>Overall symbol opacity (0.0–1.0).</summary>
    public double Alpha { get; }

    /// <summary>Whether to clip symbol rendering to the map extent.</summary>
    public bool ClipToExtent { get; }

    /// <summary>Whether to force right-hand rule for polygon orientation.</summary>
    public bool ForceRhr { get; }

    /// <summary>Ordered list of symbol layers (rendered bottom-to-top).</summary>
    public IReadOnlyList<QmlSymbolLayer> Layers { get; }

    /// <summary>Creates a new symbol.</summary>
    public QmlSymbol(
        QmlSymbolType type,
        string? name = null,
        double alpha = 1.0,
        bool clipToExtent = true,
        bool forceRhr = false,
        IEnumerable<QmlSymbolLayer>? layers = null)
    {
        Type = type;
        Name = name;
        Alpha = alpha;
        ClipToExtent = clipToExtent;
        ForceRhr = forceRhr;
        Layers = (layers?.ToArray() ?? []).AsReadOnly();
    }
}
