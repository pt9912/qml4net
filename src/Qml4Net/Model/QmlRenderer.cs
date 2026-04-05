using System.Collections.ObjectModel;

namespace Qml4Net.Model;

/// <summary>Renderer definition inside a QML document (<c>&lt;renderer-v2&gt;</c>).</summary>
public sealed class QmlRenderer
{
    /// <summary>The renderer kind.</summary>
    public QmlRendererType Type { get; }

    /// <summary>Classification field name (<c>attr</c> attribute).</summary>
    public string? Attribute { get; }

    /// <summary>Graduated classification method, e.g. <c>"GraduatedColor"</c>.</summary>
    public string? GraduatedMethod { get; }

    /// <summary>Shared symbol map keyed by string index.</summary>
    public IReadOnlyDictionary<string, QmlSymbol> Symbols { get; }

    /// <summary>Category entries (for categorized renderer).</summary>
    public IReadOnlyList<QmlCategory> Categories { get; }

    /// <summary>Range entries (for graduated renderer).</summary>
    public IReadOnlyList<QmlRange> Ranges { get; }

    /// <summary>Filter-based rules (for rule-based renderer).</summary>
    public IReadOnlyList<QmlRule> Rules { get; }

    /// <summary>Renderer-level XML attributes (<c>forceraster</c>, <c>symbollevels</c>, etc.).</summary>
    public IReadOnlyDictionary<string, string> Properties { get; }

    /// <summary>Creates a new renderer.</summary>
    public QmlRenderer(
        QmlRendererType type,
        string? attribute = null,
        string? graduatedMethod = null,
        IEnumerable<KeyValuePair<string, QmlSymbol>>? symbols = null,
        IEnumerable<QmlCategory>? categories = null,
        IEnumerable<QmlRange>? ranges = null,
        IEnumerable<QmlRule>? rules = null,
        IEnumerable<KeyValuePair<string, string>>? properties = null)
    {
        Type = type;
        Attribute = attribute;
        GraduatedMethod = graduatedMethod;
        Symbols = new ReadOnlyDictionary<string, QmlSymbol>(
            symbols?.ToDictionary(x => x.Key, x => x.Value)
            ?? new Dictionary<string, QmlSymbol>());
        Categories = (categories?.ToArray() ?? []).AsReadOnly();
        Ranges = (ranges?.ToArray() ?? []).AsReadOnly();
        Rules = (rules?.ToArray() ?? []).AsReadOnly();
        Properties = new ReadOnlyDictionary<string, string>(
            properties?.ToDictionary(x => x.Key, x => x.Value)
            ?? new Dictionary<string, string>());
    }
}
