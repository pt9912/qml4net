namespace Qml4Net.Model;

/// <summary>A range entry in a <c>graduatedSymbol</c> renderer.</summary>
/// <param name="Lower">Inclusive lower bound of the interval.</param>
/// <param name="Upper">Exclusive upper bound of the interval.</param>
/// <param name="SymbolKey">Key referencing a symbol in the renderer's symbol map.</param>
/// <param name="Label">Human-readable display label shown in the QGIS legend.</param>
/// <param name="Render">Whether this range is rendered.</param>
public sealed record QmlRange(
    double Lower,
    double Upper,
    string SymbolKey,
    string? Label = null,
    bool Render = true
);
