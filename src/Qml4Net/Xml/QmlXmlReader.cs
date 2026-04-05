using System.Xml;
using System.Xml.Linq;
using Qml4Net.Model;
using Qml4Net.Read;

namespace Qml4Net.Xml;

/// <summary>Top-level XML reader that parses a QML XML string into a QmlDocument.</summary>
internal sealed class QmlXmlReader
{
    private static readonly RendererReader RendererReader = new();

    /// <summary>Parses an XML string and returns a ReadQmlResult.</summary>
    public ReadQmlResult Read(string xmlString)
    {
        // Step 1: Parse raw XML — separate catch for XML syntax errors
        XDocument document;
        try
        {
            document = XDocument.Parse(xmlString);
        }
        catch (XmlException e)
        {
            return new ReadQmlResult.Failure($"XML parsing error: {e.Message}", e);
        }

        // Step 2: Validate structure and build domain model
        try
        {
            var root = document.Root;
            if (root is null || root.Name.LocalName != "qgis")
            {
                var name = root?.Name.LocalName ?? "(empty)";
                return new ReadQmlResult.Failure($"Expected root element <qgis>, got <{name}>");
            }

            // Mutable list passed through all reader methods to collect non-fatal issues
            var warnings = new List<string>();

            var rendererEl = root.Element("renderer-v2");
            if (rendererEl is null)
                return new ReadQmlResult.Failure("Missing <renderer-v2> element");

            var renderer = RendererReader.ReadRenderer(rendererEl, warnings);

            // Extract document-level attributes from <qgis>
            return new ReadQmlResult.Success(
                new QmlDocument(
                    Renderer: renderer,
                    Version: root.Attribute("version")?.Value,
                    HasScaleBasedVisibility: XmlHelpers.ParseBool(
                        root.Attribute("hasScaleBasedVisibilityFlag")?.Value),
                    MaxScale: XmlHelpers.ParseDouble(root.Attribute("maxScale")?.Value),
                    MinScale: XmlHelpers.ParseDouble(root.Attribute("minScale")?.Value)),
                warnings);
        }
        catch (Exception e)
        {
            return new ReadQmlResult.Failure($"Unexpected error: {e.Message}", e);
        }
    }
}
