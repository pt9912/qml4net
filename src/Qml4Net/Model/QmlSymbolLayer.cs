using System.Collections.ObjectModel;

namespace Qml4Net.Model;

/// <summary>Smallest styling unit inside a QML symbol (<c>&lt;layer&gt;</c> element).</summary>
public sealed class QmlSymbolLayer
{
    /// <summary>Parsed layer type.</summary>
    public QmlSymbolLayerType Type { get; }

    /// <summary>Raw QGIS class name (e.g. <c>"SimpleFill"</c>).</summary>
    public string ClassName { get; }

    /// <summary>Whether this layer is active.</summary>
    public bool Enabled { get; }

    /// <summary>Whether this layer is locked against interactive changes.</summary>
    public bool Locked { get; }

    /// <summary>Rendering pass for symbol level ordering.</summary>
    public int Pass { get; }

    /// <summary>Key-value properties specific to this layer class.</summary>
    public IReadOnlyDictionary<string, string> Properties { get; }

    /// <summary>Creates a new symbol layer.</summary>
    public QmlSymbolLayer(
        QmlSymbolLayerType type,
        string className,
        bool enabled = true,
        bool locked = false,
        int pass = 0,
        IEnumerable<KeyValuePair<string, string>>? properties = null)
    {
        Type = type;
        ClassName = className;
        Enabled = enabled;
        Locked = locked;
        Pass = pass;
        Properties = new ReadOnlyDictionary<string, string>(
            properties?.ToDictionary(x => x.Key, x => x.Value)
            ?? new Dictionary<string, string>());
    }
}
