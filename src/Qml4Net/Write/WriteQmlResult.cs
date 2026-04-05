namespace Qml4Net.Write;

/// <summary>Result of encoding a QmlDocument into QML XML.</summary>
public abstract record WriteQmlResult
{
    private WriteQmlResult() { }

    /// <summary>Successful encode result containing the generated XML string.</summary>
    /// <param name="Xml">The generated QML XML string.</param>
    /// <param name="Warnings">Non-fatal issues encountered during encoding.</param>
    public sealed record Success(
        string Xml,
        IReadOnlyList<string> Warnings
    ) : WriteQmlResult;

    /// <summary>Failed encode result containing an error description.</summary>
    /// <param name="Message">Human-readable description of the failure.</param>
    /// <param name="Cause">The underlying exception, if any.</param>
    public sealed record Failure(
        string Message,
        Exception? Cause = null
    ) : WriteQmlResult;
}
