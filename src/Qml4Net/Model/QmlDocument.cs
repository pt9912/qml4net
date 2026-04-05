namespace Qml4Net.Model;

/// <summary>Root of a parsed QML document (<c>&lt;qgis&gt;</c> element).</summary>
/// <param name="Renderer">The renderer definition.</param>
/// <param name="Version">QGIS version string (e.g. <c>"3.28.0-Firenze"</c>).</param>
/// <param name="HasScaleBasedVisibility">Whether scale-based visibility is enabled.</param>
/// <param name="MaxScale">Most zoomed-in scale denominator (smallest value).</param>
/// <param name="MinScale">Most zoomed-out scale denominator (largest value).</param>
public sealed record QmlDocument(
    QmlRenderer Renderer,
    string? Version = null,
    bool HasScaleBasedVisibility = false,
    double? MaxScale = null,
    double? MinScale = null
);
