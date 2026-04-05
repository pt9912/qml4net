using Qml4Net.Model;
using Qml4Net.Read;
using Qml4Net.Write;
using Qml4Net.Xml;

namespace Qml4Net;

/// <summary>Default codec implementation for reading and writing QGIS QML files.</summary>
public sealed class QmlCodec : IQmlCodec
{
    private static readonly QmlXmlReader Reader = new();
    private static readonly QmlXmlWriter Writer = new();

    /// <inheritdoc/>
    public ReadQmlResult ParseString(string xml) => Reader.Read(xml);

    /// <inheritdoc/>
    public async Task<ReadQmlResult> ParseFileAsync(string path)
    {
        try
        {
            var content = await File.ReadAllTextAsync(path);
            return Reader.Read(content);
        }
        catch (Exception ex)
        {
            return new ReadQmlResult.Failure($"Failed to read file: {path}", ex);
        }
    }

    /// <inheritdoc/>
    public WriteQmlResult EncodeString(QmlDocument document) => Writer.Write(document);

    /// <inheritdoc/>
    public async Task<WriteQmlResult> EncodeFileAsync(string path, QmlDocument document)
    {
        var result = Writer.Write(document);
        if (result is WriteQmlResult.Success success)
        {
            try
            {
                await File.WriteAllTextAsync(path, success.Xml);
            }
            catch (Exception ex)
            {
                return new WriteQmlResult.Failure($"Failed to write file: {path}", ex);
            }
        }
        return result;
    }
}
