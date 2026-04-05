using Qml4Net.Model;

namespace Qml4Net.Read;

/// <summary>Result of parsing QML XML input.</summary>
public abstract record ReadQmlResult
{
    private ReadQmlResult() { }

    /// <summary>Successful parse result containing the parsed document and any non-fatal warnings.</summary>
    /// <param name="Document">The parsed QML document.</param>
    /// <param name="Warnings">Non-fatal issues encountered during parsing (e.g. unknown types).</param>
    public sealed record Success(
        QmlDocument Document,
        IReadOnlyList<string> Warnings
    ) : ReadQmlResult;

    /// <summary>Failed parse result containing an error description.</summary>
    /// <param name="Message">Human-readable description of the failure.</param>
    /// <param name="Cause">The underlying exception, if any.</param>
    public sealed record Failure(
        string Message,
        Exception? Cause = null
    ) : ReadQmlResult;
}
