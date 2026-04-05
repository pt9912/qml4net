namespace Qml4Net.Model;

/// <summary>A category entry in a <c>categorizedSymbol</c> renderer.</summary>
/// <param name="Value">The field value to match.</param>
/// <param name="SymbolKey">Key referencing a symbol in the renderer's symbol map.</param>
/// <param name="Label">Human-readable display label shown in the QGIS legend.</param>
/// <param name="Render">Whether this category is rendered.</param>
public sealed record QmlCategory(
    string Value,
    string SymbolKey,
    string? Label = null,
    bool Render = true
);
