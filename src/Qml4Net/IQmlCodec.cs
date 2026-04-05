using Qml4Net.Model;
using Qml4Net.Read;
using Qml4Net.Write;

namespace Qml4Net;

/// <summary>Codec interface for reading and writing QGIS QML files.</summary>
public interface IQmlCodec
{
    /// <summary>Parses a QML XML string.</summary>
    ReadQmlResult ParseString(string xml);

    /// <summary>Parses a QML file.</summary>
    Task<ReadQmlResult> ParseFileAsync(string path);

    /// <summary>Encodes a QML document to an XML string.</summary>
    WriteQmlResult EncodeString(QmlDocument document);

    /// <summary>Encodes a QML document to a file.</summary>
    Task<WriteQmlResult> EncodeFileAsync(string path, QmlDocument document);
}
