# Portierung qml4dart nach qml4net (.NET 10 / C#)

## 1. Projektbeschreibung

**qml4dart** ist ein reiner Dart-Codec und Objektmodell fuer QGIS QML (`.qml`) Layer-Style-Dateien.
Die Bibliothek kann QML-XML-Dateien bidirektional in ein typisiertes Objektmodell parsen und
zurueck nach XML serialisieren -- ohne QGIS-Laufzeitabhaengigkeit.

**qml4net** soll die funktional identische Portierung nach .NET 10 / C# 14 sein.

| Eigenschaft        | qml4dart (Quelle)       | qml4net (Ziel)                  |
|--------------------|-------------------------|---------------------------------|
| Sprache            | Dart 3.7                | C# 14 / .NET 10                 |
| XML-Bibliothek     | `package:xml ^6.5.0`   | `System.Xml.Linq` (XDocument)   |
| Testframework      | `package:test ^1.25.0`  | xUnit + FluentAssertions        |
| Lizenz             | MIT                     | MIT                             |

---

## 2. Quell-Projektstruktur (Dart)

```
qml4dart/
  lib/
    qml4dart.dart                         # Public API Barrel-Export
    src/
      qml_codec.dart                      # Abstrakte Codec-Schnittstelle
      qml4dart_codec.dart                 # Konkrete Implementierung
      model/
        qml_types.dart                    # Enums: RendererType, SymbolType, SymbolLayerType
        qml_document.dart                 # QmlDocument (Root)
        qml_renderer.dart                 # QmlRenderer
        qml_symbol.dart                   # QmlSymbol
        qml_symbol_layer.dart             # QmlSymbolLayer
        qml_rule.dart                     # QmlRule (rekursiv)
        qml_category.dart                 # QmlCategory
        qml_range.dart                    # QmlRange
      read/
        read_result.dart                  # ReadQmlSuccess / ReadQmlFailure (sealed)
        read_renderer.dart                # <renderer-v2> Parser
        read_symbol.dart                  # <symbol> / <layer> Parser
        read_rule.dart                    # <rule> rekursiver Parser
      write/
        write_result.dart                 # WriteQmlSuccess / WriteQmlFailure (sealed)
        write_renderer.dart               # <renderer-v2> Serialisierer
        write_symbol.dart                 # <symbol> / <layer> Serialisierer
        write_rule.dart                   # <rule> rekursiver Serialisierer
      xml/
        xml_reader.dart                   # Top-Level XML-Parser
        xml_writer.dart                   # Top-Level XML-Generator
        xml_helpers.dart                  # Shared XML-Hilfsfunktionen
  test/
    qml4dart_test.dart                    # 31 Unit-Tests (96.6 % Coverage)
    fixtures/                             # 11 QML-Testdateien
  example/
    qml4dart_example.dart
```

---

## 3. Ziel-Projektstruktur (.NET 10)

```
qml4net/
  qml4net.sln
  src/
    Qml4Net/
      Qml4Net.csproj                      # net10.0 Class Library
      IQmlCodec.cs                        # Interface (aus qml_codec.dart)
      QmlCodec.cs                         # Konkrete Implementierung
      Model/
        QmlRendererType.cs                # enum
        QmlSymbolType.cs                  # enum
        QmlSymbolLayerType.cs             # enum
        QmlDocument.cs                    # record / class
        QmlRenderer.cs                    # record / class
        QmlSymbol.cs                      # record / class
        QmlSymbolLayer.cs                 # record / class
        QmlRule.cs                        # record / class (rekursiv)
        QmlCategory.cs                    # record / class
        QmlRange.cs                       # record / class
      Read/
        ReadQmlResult.cs                  # Abstract + Success/Failure
        RendererReader.cs
        SymbolReader.cs
        RuleReader.cs
      Write/
        WriteQmlResult.cs                 # Abstract + Success/Failure
        RendererWriter.cs
        SymbolWriter.cs
        RuleWriter.cs
      Xml/
        QmlXmlReader.cs
        QmlXmlWriter.cs
        XmlHelpers.cs
  tests/
    Qml4Net.Tests/
      Qml4Net.Tests.csproj               # xUnit Testprojekt
      QmlCodecTests.cs
      Fixtures/                           # 11 QML-Dateien (1:1 kopiert)
  docs/
    port2net.md                           # Dieses Dokument
```

---

## 4. Sprachmapping Dart -> C#

### 4.1 Sealed Classes -> Abstract Records

Dart:
```dart
sealed class ReadQmlResult {}

final class ReadQmlSuccess extends ReadQmlResult {
  final QmlDocument document;
  final List<String> warnings;
}

final class ReadQmlFailure extends ReadQmlResult {
  final String message;
  final Object? cause;
}
```

C#:
```csharp
public abstract record ReadQmlResult
{
    private ReadQmlResult() { }

    public sealed record Success(
        QmlDocument Document,
        IReadOnlyList<string> Warnings
    ) : ReadQmlResult;

    public sealed record Failure(
        string Message,
        Exception? Cause = null
    ) : ReadQmlResult;
}
```

Pattern Matching:
```csharp
switch (result)
{
    case ReadQmlResult.Success { Document: var doc, Warnings: var w }:
        // ...
        break;
    case ReadQmlResult.Failure { Message: var msg }:
        // ...
        break;
}
```

### 4.2 Enums mit String-Konvertierung

Dart:
```dart
enum QmlRendererType {
  singleSymbol,
  categorizedSymbol,
  graduatedSymbol,
  ruleRenderer,
  unknown;

  static QmlRendererType fromString(String value) => switch (value) {
    'singleSymbol' => singleSymbol,
    'categorizedSymbol' => categorizedSymbol,
    'graduatedSymbol' => graduatedSymbol,
    'RuleRenderer' => ruleRenderer,
    _ => unknown,
  };

  String toQmlString() => switch (this) {
    singleSymbol => 'singleSymbol',
    categorizedSymbol => 'categorizedSymbol',
    graduatedSymbol => 'graduatedSymbol',
    ruleRenderer => 'RuleRenderer',
    unknown => 'singleSymbol',  // Achtung: nicht 'unknown'!
  };
}
```

C#:
```csharp
public enum QmlRendererType
{
    SingleSymbol,
    CategorizedSymbol,
    GraduatedSymbol,
    RuleRenderer,
    Unknown
}

public static class QmlRendererTypeExtensions
{
    public static QmlRendererType FromQmlString(string value) => value switch
    {
        "singleSymbol"     => QmlRendererType.SingleSymbol,
        "categorizedSymbol" => QmlRendererType.CategorizedSymbol,
        "graduatedSymbol"   => QmlRendererType.GraduatedSymbol,
        "RuleRenderer"      => QmlRendererType.RuleRenderer,
        _                   => QmlRendererType.Unknown,
    };

    public static string ToQmlString(this QmlRendererType type) => type switch
    {
        QmlRendererType.SingleSymbol     => "singleSymbol",
        QmlRendererType.CategorizedSymbol => "categorizedSymbol",
        QmlRendererType.GraduatedSymbol   => "graduatedSymbol",
        QmlRendererType.RuleRenderer      => "RuleRenderer",
        _ => throw new InvalidOperationException(
            "Unknown renderer types must preserve the original XML value via QmlRenderer.RawTypeName."),
    };
}
```

> **Wichtig:** Beim Rueckweg nach XML weicht `qml4net` hier bewusst vom Dart-Fallback
> `unknown => "singleSymbol"` ab. Der Port soll unbekannte XML-Typen verlustfrei
> round-trippen, nicht stillschweigend in einen Standard-Typ umbiegen.

> **Hinweis:** Die Enums `QmlSymbolType` und `QmlSymbolLayerType` folgen demselben Muster,
> haben aber eigene Methodennamen. Unbekannte XML-Werte werden im Modell als Rohwert
> erhalten und duerfen beim Schreiben nicht auf einen Default-Typ fallen:

```csharp
// Unbekannte Symbol-Typen werden ueber QmlSymbol.RawTypeName erhalten.
public static class QmlSymbolTypeExtensions
{
    public static QmlSymbolType FromQmlString(string value) => value switch
    {
        "marker" => QmlSymbolType.Marker,
        "line"   => QmlSymbolType.Line,
        "fill"   => QmlSymbolType.Fill,
        _        => QmlSymbolType.Unknown,
    };

    public static string ToQmlString(this QmlSymbolType type) => type switch
    {
        QmlSymbolType.Marker => "marker",
        QmlSymbolType.Line   => "line",
        QmlSymbolType.Fill   => "fill",
        _ => throw new InvalidOperationException(
            "Unknown symbol types must preserve the original XML value via QmlSymbol.RawTypeName."),
    };
}

// QmlSymbolLayerType: Dart nutzt fromClassName/toClassName (nicht fromString/toQmlString)
// Unbekannte Klassen werden ueber QmlSymbolLayer.ClassName erhalten.
public static class QmlSymbolLayerTypeExtensions
{
    public static QmlSymbolLayerType FromClassName(string className) => className switch
    {
        "SimpleMarker" => QmlSymbolLayerType.SimpleMarker,
        "SvgMarker"    => QmlSymbolLayerType.SvgMarker,
        "SimpleLine"   => QmlSymbolLayerType.SimpleLine,
        "SimpleFill"   => QmlSymbolLayerType.SimpleFill,
        "RasterFill"   => QmlSymbolLayerType.RasterFill,
        _              => QmlSymbolLayerType.Unknown,
    };

    public static string ToClassName(this QmlSymbolLayerType type) => type switch
    {
        QmlSymbolLayerType.SimpleMarker => "SimpleMarker",
        QmlSymbolLayerType.SvgMarker    => "SvgMarker",
        QmlSymbolLayerType.SimpleLine   => "SimpleLine",
        QmlSymbolLayerType.SimpleFill   => "SimpleFill",
        QmlSymbolLayerType.RasterFill   => "RasterFill",
        _ => throw new InvalidOperationException(
            "Unknown symbol layer types must preserve the original XML value via QmlSymbolLayer.ClassName."),
    };
}
```

### 4.3 Wertorientierte Datenmodelle mit defensiven Kopien

Dart:
```dart
class QmlDocument {
  final String? version;
  final QmlRenderer renderer;
  final bool hasScaleBasedVisibility;
  final double? maxScale;
  final double? minScale;

  const QmlDocument({
    this.version,
    required this.renderer,
    this.hasScaleBasedVisibility = false,
    this.maxScale,
    this.minScale,
  });
}
```

C#:
```csharp
public sealed record QmlDocument(
    QmlRenderer Renderer,
    string? Version = null,
    bool HasScaleBasedVisibility = false,
    double? MaxScale = null,
    double? MinScale = null
);
```

### 4.4 Weitere Typzuordnungen

| Dart                        | C#                                         |
|-----------------------------|--------------------------------------------|
| `String`                    | `string`                                   |
| `int`                       | `int`                                      |
| `double`                    | `double`                                   |
| `bool`                      | `bool`                                     |
| `String?`                   | `string?`                                  |
| `List<T>`                   | `IReadOnlyList<T>`                         |
| `Map<String, String>`       | `IReadOnlyDictionary<string, string>`      |
| `Map<String, QmlSymbol>`    | `IReadOnlyDictionary<string, QmlSymbol>`   |
| `Future<T>`                 | `Task<T>`                                  |
| `Object?`                   | `Exception?`                               |
| `const` Konstruktor         | `record` (wertbasierte Gleichheit)         |
| `sealed class`              | `abstract record` + nested `sealed record` |
| `final class`               | `sealed record`                            |

---

## 5. Datenmodelle (vollstaendig)

### QmlDocument
```csharp
public sealed record QmlDocument(
    QmlRenderer Renderer,
    string? Version = null,
    bool HasScaleBasedVisibility = false,
    double? MaxScale = null,
    double? MinScale = null
);
```

### QmlRenderer

> **Hinweis:** In Dart sind `symbols`, `categories`, `ranges`, `rules` und `properties`
> non-nullable mit leeren Default-Werten. In C# muessen wir eingehende Collections
> defensiv kopieren und nur read-only Views exponieren, damit Records nicht mit
> mutablen Listen/Dictionaries aliasen.

```csharp
public sealed record QmlRenderer
{
    public QmlRendererType Type { get; }
    public string? RawTypeName { get; }
    public string? Attribute { get; }
    public string? GraduatedMethod { get; }
    public IReadOnlyDictionary<string, QmlSymbol> Symbols { get; }
    public IReadOnlyList<QmlCategory> Categories { get; }
    public IReadOnlyList<QmlRange> Ranges { get; }
    public IReadOnlyList<QmlRule> Rules { get; }
    public IReadOnlyDictionary<string, string> Properties { get; }

    public QmlRenderer(
        QmlRendererType type,
        string? rawTypeName = null,
        string? attribute = null,
        string? graduatedMethod = null,
        IEnumerable<KeyValuePair<string, QmlSymbol>>? symbols = null,
        IEnumerable<QmlCategory>? categories = null,
        IEnumerable<QmlRange>? ranges = null,
        IEnumerable<QmlRule>? rules = null,
        IEnumerable<KeyValuePair<string, string>>? properties = null)
    {
        Type = type;
        RawTypeName = rawTypeName;
        Attribute = attribute;
        GraduatedMethod = graduatedMethod;
        Symbols = new ReadOnlyDictionary<string, QmlSymbol>(
            symbols?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, QmlSymbol>());
        Categories = categories?.ToArray() ?? Array.Empty<QmlCategory>();
        Ranges = ranges?.ToArray() ?? Array.Empty<QmlRange>();
        Rules = rules?.ToArray() ?? Array.Empty<QmlRule>();
        Properties = new ReadOnlyDictionary<string, string>(
            properties?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>());
    }
}
```

### QmlSymbol

> **Hinweis:** In Dart hat `layers` den Default `const <QmlSymbolLayer>[]` (non-nullable).
> Auch hier wird im Konstruktor defensiv kopiert.

```csharp
public sealed record QmlSymbol
{
    public QmlSymbolType Type { get; }
    public string? RawTypeName { get; }
    public string? Name { get; }
    public double Alpha { get; }
    public bool ClipToExtent { get; }
    public bool ForceRhr { get; }
    public IReadOnlyList<QmlSymbolLayer> Layers { get; }

    public QmlSymbol(
        QmlSymbolType type,
        string? rawTypeName = null,
        string? name = null,
        double alpha = 1.0,
        bool clipToExtent = true,
        bool forceRhr = false,
        IEnumerable<QmlSymbolLayer>? layers = null)
    {
        Type = type;
        RawTypeName = rawTypeName;
        Name = name;
        Alpha = alpha;
        ClipToExtent = clipToExtent;
        ForceRhr = forceRhr;
        Layers = layers?.ToArray() ?? Array.Empty<QmlSymbolLayer>();
    }
}
```

### QmlSymbolLayer

> **Hinweis:** In Dart hat `properties` den Default `const <String, String>{}` (non-nullable).
> `ClassName` bleibt das kanonische Schreibfeld fuer den Writer und erhaelt auch
> unbekannte XML-Klassennamen.

```csharp
public sealed record QmlSymbolLayer
{
    public QmlSymbolLayerType Type { get; }
    public string ClassName { get; }
    public bool Enabled { get; }
    public bool Locked { get; }
    public int Pass { get; }
    public IReadOnlyDictionary<string, string> Properties { get; }

    public QmlSymbolLayer(
        QmlSymbolLayerType type,
        string className,
        bool enabled = true,
        bool locked = false,
        int @pass = 0,
        IEnumerable<KeyValuePair<string, string>>? properties = null)
    {
        Type = type;
        ClassName = className;
        Enabled = enabled;
        Locked = locked;
        Pass = @pass;
        Properties = new ReadOnlyDictionary<string, string>(
            properties?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>());
    }
}
```

### QmlCategory
```csharp
public sealed record QmlCategory(
    string Value,
    string SymbolKey,
    string? Label = null,
    bool Render = true
);
```

### QmlRange
```csharp
public sealed record QmlRange(
    double Lower,
    double Upper,
    string SymbolKey,
    string? Label = null,
    bool Render = true
);
```

### QmlRule (rekursiv)

> **Hinweis:** In Dart hat `children` den Default `const <QmlRule>[]` (non-nullable).

```csharp
public sealed record QmlRule
{
    public string? Key { get; }
    public string? SymbolKey { get; }
    public string? Label { get; }
    public string? Filter { get; }
    public double? ScaleMinDenominator { get; }
    public double? ScaleMaxDenominator { get; }
    public bool Enabled { get; }
    public IReadOnlyList<QmlRule> Children { get; }

    public QmlRule(
        string? key = null,
        string? symbolKey = null,
        string? label = null,
        string? filter = null,
        double? scaleMinDenominator = null,
        double? scaleMaxDenominator = null,
        bool enabled = true,
        IEnumerable<QmlRule>? children = null)
    {
        Key = key;
        SymbolKey = symbolKey;
        Label = label;
        Filter = filter;
        ScaleMinDenominator = scaleMinDenominator;
        ScaleMaxDenominator = scaleMaxDenominator;
        Enabled = enabled;
        Children = children?.ToArray() ?? Array.Empty<QmlRule>();
    }
}
```

---

## 6. Codec-Interface und Implementierung

### IQmlCodec
```csharp
public interface IQmlCodec
{
    ReadQmlResult ParseString(string xml);
    Task<ReadQmlResult> ParseFileAsync(string path);
    WriteQmlResult EncodeString(QmlDocument document);
    Task<WriteQmlResult> EncodeFileAsync(string path, QmlDocument document);
}
```

### QmlCodec

> **Hinweis:** Im Dart delegiert der Codec direkt an `_reader.read(xml)`, das bereits
> ein `ReadQmlResult` (Success/Failure) zurueckgibt. XML-Parsing-Fehler werden
> *innerhalb* des Readers abgefangen, nicht im Codec. Die C#-Portierung sollte
> dasselbe Pattern beibehalten.

```csharp
public sealed class QmlCodec : IQmlCodec
{
    private static readonly QmlXmlReader Reader = new();
    private static readonly QmlXmlWriter Writer = new();

    // Dart: _reader.read(xml) gibt ReadQmlResult direkt zurueck
    public ReadQmlResult ParseString(string xml) => Reader.Read(xml);

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

    // Dart: _writer.write(document) gibt WriteQmlResult direkt zurueck
    public WriteQmlResult EncodeString(QmlDocument document) => Writer.Write(document);

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
```

---

## 7. XML-Verarbeitung mit System.Xml.Linq

### Lesen (XDocument)

Dart verwendet `package:xml` mit `XmlDocument.parse()`. In C# nutzen wir `System.Xml.Linq.XDocument`:

```csharp
// Dart: XmlDocument.parse(xml)
// C#:
var xdoc = XDocument.Parse(xml);
var root = xdoc.Root; // <qgis>

// Attribut lesen
string? version = root?.Attribute("version")?.Value;

// Kind-Element suchen
var renderer = root?.Element("renderer-v2");
string? type = renderer?.Attribute("type")?.Value;

// Alle Kind-Elemente eines bestimmten Namens
var symbols = renderer?.Element("symbols")?.Elements("symbol");
```

### Schreiben (XDocument)

Dart verwendet `XmlBuilder`. In C# nutzen wir LINQ to XML direkt:

```csharp
// Dart schreibt optionale Attribute nur wenn nicht null!
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
qgis.Add(RendererWriter.Write(document.Renderer));

var doc = new XDocument(
    new XDeclaration("1.0", "utf-8", null),
    qgis);
return doc.ToString(SaveOptions.None); // Pretty-Print
```

### Property-Extraktion (beide Formate)

```csharp
public static class XmlHelpers
{
    /// Liest Properties aus <layer>: neues Format (Option) oder Legacy (<prop>)
    public static Dictionary<string, string> ExtractProperties(XElement layerElement)
    {
        var props = new Dictionary<string, string>();

        // Neues Format: <Option type="Map"><Option name="..." value="..."/></Option>
        var optionMap = layerElement.Elements("Option")
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
            return props;
        }

        // Legacy-Format: <prop k="..." v="..."/>
        foreach (var prop in layerElement.Elements("prop"))
        {
            var k = prop.Attribute("k")?.Value;
            var v = prop.Attribute("v")?.Value;
            if (k is not null && v is not null)
                props[k] = v;
        }

        return props;
    }
}
```

---

## 8. Unterstuetzte QML-Features

### Renderer-Typen

| Typ                  | XML `type`          | Beschreibung                           |
|----------------------|---------------------|----------------------------------------|
| SingleSymbol         | `singleSymbol`      | Ein Symbol fuer alle Features          |
| CategorizedSymbol    | `categorizedSymbol`  | Klassifizierung nach Feldwert          |
| GraduatedSymbol      | `graduatedSymbol`    | Numerische Wertebereiche               |
| RuleRenderer         | `RuleRenderer`       | Filterausdruecke, rekursiv verschachtelt |

### Symbol-Layer-Typen

| Typ           | XML `class`    | Beschreibung                    |
|---------------|----------------|---------------------------------|
| SimpleMarker  | `SimpleMarker` | Punkt: Kreis, Quadrat, Dreieck  |
| SvgMarker     | `SvgMarker`    | SVG-basierte Punkt-Marker       |
| SimpleLine    | `SimpleLine`   | Linienstil                      |
| SimpleFill    | `SimpleFill`   | Flaechenfuellung mit Umriss     |
| RasterFill    | `RasterFill`   | Raster/Bild-Muster              |

### Property-Formate

- **Neues Format** (QGIS >= 3.26): `<Option type="Map"><Option name="..." value="..." type="QString"/></Option>`
- **Legacy-Format** (QGIS < 3.26): `<prop k="..." v="..."/>`
- Reader unterstuetzt beide Formate, Writer erzeugt nur neues Format.

### Skalierungsbasierte Sichtbarkeit

- **Dokument-Ebene**: `hasScaleBasedVisibilityFlag`, `maxScale`, `minScale` auf `<qgis>`
- **Regel-Ebene**: `scalemindenom`, `scalemaxdenom` auf `<rule>`

---

## 9. Portierungsreihenfolge

### Phase 1: Projektgeruest
1. `dotnet new sln -n qml4net`
2. `dotnet new classlib -n Qml4Net -o src/Qml4Net --framework net10.0`
3. `dotnet new xunit -n Qml4Net.Tests -o tests/Qml4Net.Tests --framework net10.0`
4. `dotnet sln add src/Qml4Net tests/Qml4Net.Tests`
5. `dotnet add tests/Qml4Net.Tests reference src/Qml4Net`
6. `dotnet add tests/Qml4Net.Tests package FluentAssertions`
7. `.gitignore` fuer .NET hinzufuegen

### Phase 2: Enums und Modelle
1. `QmlRendererType.cs` + Extensions
2. `QmlSymbolType.cs` + Extensions
3. `QmlSymbolLayerType.cs` + Extensions
4. `QmlDocument.cs`
5. `QmlRenderer.cs`
6. `QmlSymbol.cs`
7. `QmlSymbolLayer.cs`
8. `QmlCategory.cs`
9. `QmlRange.cs`
10. `QmlRule.cs`

### Phase 3: Result-Typen
1. `ReadQmlResult.cs` (Success / Failure)
2. `WriteQmlResult.cs` (Success / Failure)

### Phase 4: XML Lesen (Read)
1. `XmlHelpers.cs` -- Property-Extraktion (beide Formate)
2. `SymbolReader.cs` -- `<symbol>` und `<layer>` parsen
3. `RuleReader.cs` -- rekursives `<rule>` parsen
4. `RendererReader.cs` -- `<renderer-v2>` parsen
5. `QmlXmlReader.cs` -- Top-Level `<qgis>` parsen

### Phase 5: XML Schreiben (Write)
1. `SymbolWriter.cs` -- `<symbol>` und `<layer>` erzeugen
2. `RuleWriter.cs` -- rekursives `<rule>` erzeugen
3. `RendererWriter.cs` -- `<renderer-v2>` erzeugen
4. `QmlXmlWriter.cs` -- Top-Level `<qgis>` erzeugen

### Phase 6: Codec
1. `IQmlCodec.cs` -- Interface
2. `QmlCodec.cs` -- Implementierung

### Phase 7: Tests
1. Test-Fixtures (11 QML-Dateien) nach `tests/Qml4Net.Tests/Fixtures/` kopieren
2. Modell-Tests (Konstruktion, Enum-Konvertierung)
3. Parse-Tests (alle Renderer-Typen, beide Property-Formate, Fehler)
4. Write-Tests (Round-Trip fuer alle Typen)
5. Datei-I/O-Tests

---

## 10. Testmatrix

Die 31 Dart-Tests sollen 1:1 portiert und um zwei C#-spezifische Regressionstests ergaenzt werden:

| # | Testgruppe          | Test (Dart-Name)                                    | Fixture-Datei                    |
|---|---------------------|-----------------------------------------------------|----------------------------------|
| 1 | Model               | QmlDocument can be constructed                      | --                               |
| 2 | Model               | QmlRendererType round-trips through string          | --                               |
| 3 | Model               | QmlSymbolType round-trips through string            | --                               |
| 4 | Model               | QmlSymbolLayerType round-trips through className    | --                               |
| 5 | Parse singleSymbol  | SimpleFill new format                               | `single_symbol_fill_new.qml`     |
| 6 | Parse singleSymbol  | SimpleFill old prop format                          | `single_symbol_fill_old.qml`     |
| 7 | Parse singleSymbol  | SimpleLine                                          | `single_symbol_line.qml`         |
| 8 | Parse singleSymbol  | SimpleMarker                                        | `single_symbol_marker.qml`       |
| 9 | Parse singleSymbol  | SvgMarker                                           | `svg_marker.qml`                 |
|10 | Parse singleSymbol  | Multi-layer symbol                                  | `multi_layer_symbol.qml`         |
|11 | Parse categorized   | categories and symbols                              | `categorized.qml`                |
|12 | Parse graduated     | ranges and symbols                                  | `graduated.qml`                  |
|13 | Parse RuleRenderer  | flat rules                                          | `rule_renderer.qml`              |
|14 | Parse RuleRenderer  | nested rules                                        | `nested_rules.qml`               |
|15 | Parse scale         | layer-level scale visibility                        | `scale_visibility.qml`           |
|16 | Parse errors        | invalid XML                                         | --                               |
|17 | Parse errors        | wrong root element                                  | --                               |
|18 | Parse errors        | missing renderer-v2                                 | --                               |
|19 | Parse errors        | unknown renderer type produces warning              | --                               |
|20 | Parse errors        | unknown symbol layer class produces warning         | --                               |
|21 | Write               | singleSymbol round-trip                             | `single_symbol_fill_new.qml`     |
|22 | Write               | categorized round-trip                              | `categorized.qml`                |
|23 | Write               | graduated round-trip                                | `graduated.qml`                  |
|24 | Write               | RuleRenderer round-trip                             | `rule_renderer.qml`              |
|25 | Write               | nested rules round-trip                             | `nested_rules.qml`               |
|26 | Write               | scale visibility round-trip                         | `scale_visibility.qml`           |
|27 | Write               | multi-layer symbol round-trip                       | `multi_layer_symbol.qml`         |
|28 | Write               | written XML contains expected structure             | --                               |
|29 | File I/O            | parseFile reads fixture                             | `single_symbol_fill_new.qml`     |
|30 | File I/O            | parseFile returns failure for missing file          | --                               |
|31 | File I/O            | encodeFile writes and reads back                    | --                               |
|32 | Parse warnings      | unknown symbol type produces warning                | --                               |
|33 | Parse warnings      | symbol without name produces warning                | --                               |

---

## 11. Wichtige Implementierungsdetails

### 11.1 Numerische Praezision
- **Range-Grenzen** (lower/upper): Dart nutzt `.toStringAsFixed(15)` -> C#: `value.ToString("F15", CultureInfo.InvariantCulture)`
- **Scale-Denominatoren** (scalemindenom/scalemaxdenom in Rules): `.toStringAsFixed(0)` -> C#: `value.ToString("F0", CultureInfo.InvariantCulture)`
- **Dokument-Level Scales** (maxScale/minScale): Dart nutzt einfaches `.toString()` -> C#: `value.ToString(CultureInfo.InvariantCulture)`
- Alle numerischen Strings muessen mit `CultureInfo.InvariantCulture` geparst werden

### 11.2 Dictionary-Reihenfolge
- Dart `Map` bewahrt Einfuegereihenfolge
- C# `Dictionary<K,V>` bewahrt ebenfalls Einfuegereihenfolge (de facto, nicht garantiert)
- Fuer garantierte Reihenfolge: Tests sollten Reihenfolge nicht als harten Kontrakt testen
- Alternative: `OrderedDictionary<string, QmlSymbol>` aus `System.Collections.Generic` (.NET 10)

### 11.3 XML Pretty-Print
- Dart erzeugt eingeruecktes XML
- C#: `XDocument.ToString(SaveOptions.None)` erzeugt eingeruecktes XML
- XML-Deklaration: `new XDeclaration("1.0", "utf-8", null)` setzen

### 11.4 Bool-Attribute in XML
- QML nutzt `"0"` / `"1"` fuer die meisten Bool-Werte (enabled, locked, clip_to_extent, etc.)
- Ausnahme: `<category render="true">` und `<range render="true">` nutzen `"true"` / `"false"`
- Dart `XmlHelpers.parseBool()` akzeptiert beides: `"0"/"1"` UND `"true"/"false"` (case-insensitive)
- Sonderfall `checkstate`: Auf `<rule>` bestimmt `checkstate` das `enabled`-Flag.
  `checkstate="0"` bedeutet deaktiviert; fehlend oder anderer Wert = aktiviert
- Beim **Schreiben**: `"0"/"1"` fuer Attribute, `"true"/"false"` fuer `<category>`/`<range>` `render`
- `checkstate` wird nur geschrieben wenn `enabled == false` (Wert `"0"`)

### 11.5 Null-Behandlung
- C# Nullable Reference Types aktivieren (`<Nullable>enable</Nullable>`)
- Alle optionalen Felder mit `?` markieren
- Beim XML-Lesen: `?.Value` Pattern konsistent verwenden

### 11.6 Rekursives Rule-Parsing
- `QmlRule.Children` kann beliebig tief verschachtelt sein
- Reader und Writer muessen rekursiv arbeiten
- Beispiel: `ReadRules(XElement parent)` ruft sich selbst fuer Kind-`<rule>`-Elemente auf

### 11.7 Renderer-Level Properties Whitelist
Der Reader liest nur eine **explizite Whitelist** von Attributen als `Properties` auf dem Renderer:
- `forceraster`
- `symbollevels`
- `enableorderby`
- `referencescale`

Andere Attribute auf `<renderer-v2>` (wie `type`, `attr`, `graduatedMethod`) werden
in dedizierte Felder geparst, nicht in `Properties`. Der Writer schreibt `Properties`
als zusaetzliche Attribute auf `<renderer-v2>` zurueck.

### 11.8 XML-Element-Reihenfolge im Writer
Der Writer muss die Kindelemente von `<renderer-v2>` in dieser Reihenfolge erzeugen:
1. `<categories>` (nur wenn nicht leer)
2. `<ranges>` (nur wenn nicht leer)
3. `<rules key="renderer_rules">` (nur wenn nicht leer)
4. `<symbols>`
5. `<rotation/>` (immer leer, QGIS-Kompatibilitaet)
6. `<sizescale/>` (immer leer, QGIS-Kompatibilitaet)

### 11.8 Reader-Defaults fuer Category und Range
Beim Parsen von `<category>` und `<range>` Elementen werden fehlende Attribute
mit diesen Defaults ersetzt:

| Element      | Attribut   | Default              |
|-------------|------------|----------------------|
| `<category>` | `value`    | `""` (leerer String) |
| `<category>` | `symbol`   | `"0"`                |
| `<category>` | `render`   | `true`               |
| `<range>`    | `lower`    | `0.0`                |
| `<range>`    | `upper`    | `0.0`                |
| `<range>`    | `symbol`   | `"0"`                |
| `<range>`    | `render`   | `true`               |

### 11.9 Warnings-Akkumulation
- Warnungen werden in einer mutablen `List<string>` gesammelt
- Diese Liste wird per Referenz durch alle Reader-Methoden durchgereicht:
  `ReadRenderer(element, warnings)` -> `ReadSymbol(element, warnings)` etc.
- Warnungen entstehen bei: unbekanntem Renderer-Typ, unbekanntem Symbol-Typ,
  unbekanntem SymbolLayer-Klassenname, Symbol ohne `name`-Attribut
- Unbekannte Typen fuehren NICHT zu Fehlern, sondern zu Warnungen (Forward-Kompatibilitaet)
- Bei unbekanntem `renderer-v2/@type` bleibt der Originalwert in `QmlRenderer.RawTypeName` erhalten
- Bei unbekanntem `symbol/@type` bleibt der Originalwert in `QmlSymbol.RawTypeName` erhalten
- Bei unbekanntem `layer/@class` bleibt der Originalwert in `QmlSymbolLayer.ClassName` erhalten
- Der Writer serialisiert zuerst diese Rohwerte; nur bekannte Enums werden ueber
  `ToQmlString()` / `ToClassName()` geschrieben
- Wenn ein intern konstruiertes Modell `Unknown` enthaelt, aber kein Rohwert vorhanden ist,
  muss der Writer mit `WriteQmlResult.Failure` abbrechen statt einen Default-Typ zu schreiben

### 11.10 XmlHelpers Hilfsmethoden
Neben `ExtractProperties` gibt es im Dart-Quellcode drei weitere Hilfsmethoden,
die ebenfalls portiert werden muessen:
```csharp
public static class XmlHelpers
{
    // ... ExtractProperties (bereits dokumentiert) ...

    public static bool ParseBool(string? value, bool defaultValue = false)
    {
        if (value is null) return defaultValue;
        return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    public static double? ParseDouble(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        return double.TryParse(value, CultureInfo.InvariantCulture, out var d) ? d : null;
    }

    public static int? ParseInt(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        return int.TryParse(value, out var i) ? i : null;
    }
}
```

---

## 12. NuGet-Abhaengigkeiten

### Qml4Net (Library)
- Keine externen Abhaengigkeiten (nur `System.Xml.Linq` aus dem BCL)

### Qml4Net.Tests
- `xunit` >= 2.9
- `xunit.runner.visualstudio`
- `Microsoft.NET.Test.Sdk`
- `FluentAssertions` >= 8.0

---

## 13. csproj-Konfiguration

### src/Qml4Net/Qml4Net.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>Qml4Net</RootNamespace>
    <PackageId>Qml4Net</PackageId>
    <Version>0.1.0</Version>
    <Description>QGIS QML layer style codec for .NET</Description>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
  </PropertyGroup>
</Project>
```

### tests/Qml4Net.Tests/Qml4Net.Tests.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="FluentAssertions" Version="8.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/Qml4Net/Qml4Net.csproj" />
  </ItemGroup>
  <ItemGroup>
    <None Update="Fixtures/**/*.qml">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

---

## 14. Namespace-Zuordnung

| Dart-Pfad                      | C# Namespace              |
|--------------------------------|---------------------------|
| `lib/src/model/`               | `Qml4Net.Model`           |
| `lib/src/read/`                | `Qml4Net.Read`            |
| `lib/src/write/`               | `Qml4Net.Write`           |
| `lib/src/xml/`                 | `Qml4Net.Xml`             |
| `lib/src/` (Codec)             | `Qml4Net`                 |
| `test/`                        | `Qml4Net.Tests`           |

---

## 15. Beispiel-Nutzung (C#)

```csharp
using Qml4Net;

var codec = new QmlCodec();

// QML-String parsen
var result = codec.ParseString(qmlXml);
switch (result)
{
    case ReadQmlResult.Success { Document: var doc, Warnings: var warnings }:
        Console.WriteLine($"Renderer: {doc.Renderer.Type}");
        Console.WriteLine($"Symbole: {doc.Renderer.Symbols.Count}");
        foreach (var w in warnings)
            Console.WriteLine($"Warnung: {w}");
        break;
    case ReadQmlResult.Failure { Message: var msg }:
        Console.WriteLine($"Fehler: {msg}");
        break;
}

// QML-Datei parsen (async)
var fileResult = await codec.ParseFileAsync("style.qml");

// Nach String serialisieren
var writeResult = codec.EncodeString(doc);
if (writeResult is WriteQmlResult.Success { Xml: var xml })
    Console.WriteLine(xml);

// In Datei schreiben (async)
await codec.EncodeFileAsync("output.qml", doc);
```
