using System.Globalization;
using System.Xml.Linq;
using Qml4Net.Model;
using Qml4Net.Write;

namespace Qml4Net.Xml;

/// <summary>Top-level XML writer that encodes a QmlDocument into a QML XML string.</summary>
internal sealed class QmlXmlWriter
{
    /// <summary>Encodes the document and returns a WriteQmlResult.</summary>
    public WriteQmlResult Write(QmlDocument document)
    {
        try
        {
            var warnings = new List<string>();

            // Build <qgis> root — optional attributes only written when not null
            var qgis = new XElement("qgis");
            if (document.Version is not null)
                qgis.Add(new XAttribute("version", document.Version));
            qgis.Add(new XAttribute("hasScaleBasedVisibilityFlag",
                document.HasScaleBasedVisibility ? "1" : "0"));
            if (document.MaxScale is not null)
                qgis.Add(new XAttribute("maxScale",
                    document.MaxScale.Value.ToString(CultureInfo.InvariantCulture)));
            if (document.MinScale is not null)
                qgis.Add(new XAttribute("minScale",
                    document.MinScale.Value.ToString(CultureInfo.InvariantCulture)));

            qgis.Add(RendererWriter.WriteRenderer(document.Renderer));

            var doc = new XDocument(qgis);
            var xml = doc.ToString(SaveOptions.None);

            return new WriteQmlResult.Success(xml, warnings);
        }
        catch (Exception e)
        {
            return new WriteQmlResult.Failure($"Failed to encode QML: {e.Message}", e);
        }
    }
}
