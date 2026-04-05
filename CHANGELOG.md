# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-04-05

Initial release — complete port of qml4dart to .NET 10 / C# 14.

### Added

#### Core Model
- Domain model: `QmlDocument`, `QmlRenderer`, `QmlSymbol`, `QmlSymbolLayer`
- Enums: `QmlRendererType`, `QmlSymbolType`, `QmlSymbolLayerType` with bidirectional string conversion
- Category, Range, and Rule models with recursive nesting support
- Sealed result types: `ReadQmlResult`, `WriteQmlResult` (Success/Failure)

#### QML Parsing
- `QmlCodec.ParseString` / `ParseFileAsync` for reading QML XML
- Support for all renderer types: singleSymbol, categorizedSymbol, graduatedSymbol, RuleRenderer
- Support for both property formats: new `<Option type="Map">` (QGIS >= 3.26) and legacy `<prop k v>`
- Scale-based visibility at document and rule level
- Tolerant parsing with warnings for unknown types (forward compatibility)

#### QML Writing
- `QmlCodec.EncodeString` / `EncodeFileAsync` for writing QML XML
- Round-trip fidelity for all renderer types and property formats
- QGIS-compatible element ordering and empty `<rotation>`/`<sizescale>` elements

#### Public API
- `IQmlCodec` interface for dependency injection
- `QmlCodec` default implementation
- Zero external dependencies (only `System.Xml.Linq` from BCL)

### Infrastructure
- .NET 10 / C# 14 target framework
- Multi-stage Dockerfile (restore, build, test, pack, push)
- 90% line coverage gate via coverlet (actual: 95.6%)
- GitHub Actions CI workflow + publish workflow
- 31 unit tests with 11 QML fixture files
- XML documentation on all public API members
