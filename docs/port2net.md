# Portierung qml4dart nach qml4net (.NET 10 / C#)

## 1. Projektbeschreibung

**qml4dart** ist ein reiner Dart-Codec und Objektmodell fuer QGIS QML (`.qml`) Layer-Style-Dateien.
Die Bibliothek kann QML-XML-Dateien bidirektional in ein typisiertes Objektmodell parsen und
zurueck nach XML serialisieren -- ohne QGIS-Laufzeitabhaengigkeit.

**qml4net** ist die funktional identische Portierung nach .NET 10 / C# 14.

| Eigenschaft        | qml4dart (Quelle)       | qml4net (Ziel)                  |
|--------------------|-------------------------|---------------------------------|
| Sprache            | Dart 3.7                | C# 14 / .NET 10                 |
| XML-Bibliothek     | `package:xml ^6.5.0`   | `System.Xml.Linq` (XDocument)   |
| Testframework      | `package:test ^1.25.0`  | xUnit v3 + FluentAssertions     |
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
      IQmlCodec.cs                        # Interface
      QmlCodec.cs                         # Konkrete Implementierung
      Model/
        QmlRendererType.cs                # enum + Extensions
        QmlSymbolType.cs                  # enum + Extensions
        QmlSymbolLayerType.cs             # enum + Extensions
        QmlDocument.cs                    # record
        QmlRenderer.cs                    # sealed class
        QmlSymbol.cs                      # sealed class
        QmlSymbolLayer.cs                 # sealed class
        QmlRule.cs                        # sealed class (rekursiv)
        QmlCategory.cs                    # record
        QmlRange.cs                       # record
      Read/
        ReadQmlResult.cs                  # abstract record + Success/Failure
        RendererReader.cs
        SymbolReader.cs
        RuleReader.cs
      Write/
        WriteQmlResult.cs                 # abstract record + Success/Failure
        RendererWriter.cs
        SymbolWriter.cs
        RuleWriter.cs
      Xml/
        QmlXmlReader.cs
        QmlXmlWriter.cs
        XmlHelpers.cs                     # internal
  tests/
    Qml4Net.Tests/
      Qml4Net.Tests.csproj               # xUnit v3 Testprojekt
      QmlCodecTests.cs
      Fixtures/                           # 11 QML-Dateien (1:1 kopiert)
  docs/
    port2net.md                           # Dieses Dokument
```

---

## 4. Sprachmapping Dart -> C#

### 4.1 Typzuordnungen

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

### 4.2 Sprachkonstrukte

| Dart                        | C#                                              | Siehe                              |
|-----------------------------|--------------------------------------------------|------------------------------------|
| `sealed class` + `final class` | `abstract record` + nested `sealed record`    | `ReadQmlResult.cs`, `WriteQmlResult.cs` |
| `enum` mit Methoden         | `enum` + statische Extension-Klasse              | `QmlRendererType.cs` u.a.         |
| `const` Konstruktor (immutabel) | `sealed class` mit get-only Properties        | `QmlRenderer.cs`, `QmlSymbol.cs`  |
| `const` Konstruktor (einfach)  | `sealed record` (positionale Parameter)       | `QmlDocument.cs`, `QmlCategory.cs` |

### 4.3 Designentscheidungen gegenueber Dart

- **`sealed class` statt `sealed record`** fuer `QmlRenderer`, `QmlSymbol`, `QmlSymbolLayer`, `QmlRule`:
  Diese Typen enthalten Collection-Properties (`IReadOnlyList`, `IReadOnlyDictionary`).
  Als `record` wuerde `==` Referenz-Gleichheit auf den Collections verwenden und damit
  irrefuehrend Value-Equality suggerieren. `sealed class` vermeidet diese Falle.

- **Defensives Kopieren** im Konstruktor: Eingehende `IEnumerable<>` werden per
  `.ToArray().AsReadOnly()` bzw. `new ReadOnlyDictionary<>(.ToDictionary())` kopiert,
  damit externe Aufrufer keine mutable Referenz behalten.

- **Enum-Fallbacks**: Die `ToQmlString()`/`ToClassName()` Methoden geben fuer `Unknown`
  einen Default-Wert zurueck (`"singleSymbol"`, `"marker"`, `"Unknown"`) -- identisch
  zum Dart-Original.

---

## 5. Namespace-Zuordnung

| Dart-Pfad                      | C# Namespace              |
|--------------------------------|---------------------------|
| `lib/src/model/`               | `Qml4Net.Model`           |
| `lib/src/read/`                | `Qml4Net.Read`            |
| `lib/src/write/`               | `Qml4Net.Write`           |
| `lib/src/xml/`                 | `Qml4Net.Xml`             |
| `lib/src/` (Codec)             | `Qml4Net`                 |
| `test/`                        | `Qml4Net.Tests`           |

---

## 6. Unterstuetzte QML-Features

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

## 7. Implementierungsreihenfolge

### Phase 1: Projektgeruest
Solution, csproj-Dateien, .gitignore

### Phase 2: Enums und Modelle
`QmlRendererType`, `QmlSymbolType`, `QmlSymbolLayerType` mit Extensions,
dann `QmlDocument`, `QmlRenderer`, `QmlSymbol`, `QmlSymbolLayer`,
`QmlCategory`, `QmlRange`, `QmlRule`

### Phase 3: Result-Typen
`ReadQmlResult` und `WriteQmlResult` (jeweils Success/Failure)

### Phase 4: XML Lesen
`XmlHelpers` → `SymbolReader` → `RuleReader` → `RendererReader` → `QmlXmlReader`

### Phase 5: XML Schreiben
`SymbolWriter` → `RuleWriter` → `RendererWriter` → `QmlXmlWriter`

### Phase 6: Codec
`IQmlCodec` Interface, `QmlCodec` Implementierung

### Phase 7: Tests
11 QML-Fixtures kopieren, 31 Unit-Tests portieren

---

## 8. Testmatrix

| # | Testgruppe          | Test                                                | Fixture-Datei                    |
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

---

## 9. Wichtige Implementierungsdetails

### 9.1 Numerische Praezision
- **Range-Grenzen** (lower/upper): `value.ToString("F15", CultureInfo.InvariantCulture)`
- **Scale-Denominatoren** in Rules: `value.ToString("F0", CultureInfo.InvariantCulture)`
- **Dokument-Level Scales**: `value.ToString(CultureInfo.InvariantCulture)`
- Alle numerischen Strings werden mit `CultureInfo.InvariantCulture` geparst

### 9.2 Dictionary-Reihenfolge
- Dart `Map` und C# `Dictionary<K,V>` bewahren de facto Einfuegereihenfolge
- Tests sollten Reihenfolge nicht als harten Kontrakt testen

### 9.3 XML Pretty-Print
- `XDocument.ToString(SaveOptions.None)` erzeugt eingeruecktes XML

### 9.4 Bool-Attribute in XML
- `"0"` / `"1"` fuer die meisten Attribute (enabled, locked, clip_to_extent, etc.)
- `"true"` / `"false"` fuer `<category>`/`<range>` `render`-Attribut
- `ParseBool()` akzeptiert beides (case-insensitive)
- `checkstate="0"` auf `<rule>` bedeutet deaktiviert; fehlend = aktiviert
- `checkstate` wird nur geschrieben wenn `enabled == false`

### 9.5 Rekursives Rule-Parsing
- `QmlRule.Children` kann beliebig tief verschachtelt sein
- Reader und Writer arbeiten rekursiv

### 9.6 Renderer-Level Properties Whitelist
Nur diese Attribute werden als `Properties` gelesen: `forceraster`, `symbollevels`,
`enableorderby`, `referencescale`. Andere Attribute (`type`, `attr`, `graduatedMethod`)
werden in dedizierte Felder geparst.

### 9.7 XML-Element-Reihenfolge im Writer
Kindelemente von `<renderer-v2>` in dieser Reihenfolge:
1. `<categories>` (nur wenn nicht leer)
2. `<ranges>` (nur wenn nicht leer)
3. `<rules key="renderer_rules">` (nur wenn nicht leer)
4. `<symbols>`
5. `<rotation/>` (immer leer, QGIS-Kompatibilitaet)
6. `<sizescale/>` (immer leer, QGIS-Kompatibilitaet)

### 9.8 Reader-Defaults

| Element      | Attribut   | Default              |
|-------------|------------|----------------------|
| `<category>` | `value`    | `""` (leerer String) |
| `<category>` | `symbol`   | `"0"`                |
| `<category>` | `render`   | `true`               |
| `<range>`    | `lower`    | `0.0`                |
| `<range>`    | `upper`    | `0.0`                |
| `<range>`    | `symbol`   | `"0"`                |
| `<range>`    | `render`   | `true`               |

### 9.9 Warnings-Akkumulation
- Mutable `List<string>` wird per Referenz durch alle Reader-Methoden durchgereicht
- Warnungen bei: unbekanntem Renderer-Typ, unbekanntem Symbol-Typ,
  unbekanntem SymbolLayer-Klassenname, Symbol ohne `name`-Attribut
- Unbekannte Typen fuehren nicht zu Fehlern (Forward-Kompatibilitaet)

---

## 10. Abhaengigkeiten

### Qml4Net (Library)
Keine externen Abhaengigkeiten (nur `System.Xml.Linq` aus dem BCL).

### Qml4Net.Tests
- `xunit.v3` >= 1.0
- `xunit.runner.visualstudio` >= 3.0
- `Microsoft.NET.Test.Sdk` 17.13.0
- `FluentAssertions` 8.3.0
- `coverlet.msbuild` 6.0.4
