using System.Globalization;
using System.Xml.Linq;

namespace Qml4Net.Xml;

/// <summary>Shared XML helper functions for QML parsing.</summary>
internal static class XmlHelpers
{
    /// <summary>
    /// Extracts properties from a layer element.
    /// Supports both new (&lt;Option type="Map"&gt;) and legacy (&lt;prop k v&gt;) formats.
    /// </summary>
    public static Dictionary<string, string> ExtractProperties(XElement element)
    {
        var props = new Dictionary<string, string>();

        // New format (QGIS >= 3.26): <Option type="Map"><Option name="..." value="..."/></Option>
        var optionMap = element.Elements("Option")
            .FirstOrDefault(e => e.Attribute("type")?.Value == "Map");

        if (optionMap is not null)
        {
            foreach (var opt in optionMap.Elements("Option"))
            {
                var name = opt.Attribute("name")?.Value;
                var value = opt.Attribute("value")?.Value;
                if (name is not null && value is not null)
                    props[name] = value;
            }
            return props; // New format found — skip legacy fallback
        }

        // Legacy format (QGIS < 3.26): <prop k="..." v="..."/>
        foreach (var prop in element.Elements("prop"))
        {
            var k = prop.Attribute("k")?.Value;
            var v = prop.Attribute("v")?.Value;
            if (k is not null && v is not null)
                props[k] = v;
        }

        return props;
    }

    /// <summary>Parses "0"/"1" or "true"/"false" to bool.</summary>
    public static bool ParseBool(string? value, bool defaultValue = false)
    {
        if (value is null) return defaultValue;
        return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Tolerantly parses a numeric string.</summary>
    public static double? ParseDouble(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        return double.TryParse(value, NumberStyles.Float | NumberStyles.AllowExponent,
            CultureInfo.InvariantCulture, out var d) ? d : null;
    }

    /// <summary>Tolerantly parses an integer string.</summary>
    public static int? ParseInt(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        return int.TryParse(value, out var i) ? i : null;
    }
}
