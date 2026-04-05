# Qml4Net

A .NET library for reading and writing QGIS QML (`.qml`) layer style files.

Ported from [qml4dart](https://github.com/pt9912/mapstyler/tree/main/qml4dart).

## Status

v0.1.0 released — 31 tests passing, 95.6% line coverage.

## Features

- **QML Parsing** -- Read QGIS `.qml` layer style files into a typed object model
- **QML Writing** -- Serialize the object model back to QML XML
- **Renderer Types** -- SingleSymbol, CategorizedSymbol, GraduatedSymbol, RuleRenderer (nested)
- **Symbol Layers** -- SimpleMarker, SvgMarker, SimpleLine, SimpleFill, RasterFill
- **Property Formats** -- Both new (`<Option type="Map">`, QGIS >= 3.26) and legacy (`<prop k v>`)
- **Scale Visibility** -- Document-level and rule-level scale-based visibility
- **Zero Dependencies** -- Only BCL APIs (`System.Xml.Linq`)

## API

```csharp
using Qml4Net;

var codec = new QmlCodec();

// Parse QML string
var result = codec.ParseString(qmlXml);
switch (result)
{
    case ReadQmlResult.Success { Document: var doc, Warnings: var warnings }:
        Console.WriteLine($"Renderer: {doc.Renderer.Type}");
        Console.WriteLine($"Symbols: {doc.Renderer.Symbols.Count}");
        foreach (var w in warnings)
            Console.WriteLine($"Warning: {w}");
        break;
    case ReadQmlResult.Failure { Message: var msg }:
        Console.WriteLine($"Error: {msg}");
        break;
}

// Parse QML file (async)
var fileResult = await codec.ParseFileAsync("style.qml");

// Encode to string
var writeResult = codec.EncodeString(doc);
if (writeResult is WriteQmlResult.Success { Xml: var xml })
    Console.WriteLine(xml);

// Encode to file (async)
await codec.EncodeFileAsync("output.qml", doc);
```

## Supported QML Renderer Types

| Type | XML `type` | Description |
|------|-----------|-------------|
| SingleSymbol | `singleSymbol` | One symbol for all features |
| CategorizedSymbol | `categorizedSymbol` | Features classified by field value |
| GraduatedSymbol | `graduatedSymbol` | Features classified into numeric ranges |
| RuleRenderer | `RuleRenderer` | Filter expressions, supports nesting |

## Supported Symbol Layer Types

| Type | XML `class` | Description |
|------|------------|-------------|
| SimpleMarker | `SimpleMarker` | Point: circle, square, triangle, etc. |
| SvgMarker | `SvgMarker` | SVG-based point markers |
| SimpleLine | `SimpleLine` | Line strokes |
| SimpleFill | `SimpleFill` | Polygon fill with optional outline |
| RasterFill | `RasterFill` | Raster/image pattern fill |

## Docker Build and Release

The repository includes a multi-stage [Dockerfile](Dockerfile) for building, testing, and publishing.

- Solution: `qml4net.sln`
- Build stages: `restore`, `build`, `test`, `pack`, `push`
- Package output stage: `artifacts`
- Coverage gate in `test`: at least 90% line coverage
- Public API XML documentation comments are required (CS1591 enforced as error)

```bash
docker buildx build --target test -t qml4net:test .
docker buildx build --target artifacts --build-arg PACKAGE_VERSION=0.1.0 -o type=local,dest=./artifacts .
docker buildx build --target push --secret id=nuget_api_key,src=/path/to/nuget-api-key.txt --build-arg PACKAGE_VERSION=0.1.0 .
```

Notes:

- `push` publishes generated `.nupkg` files to `https://api.nuget.org/v3/index.json`
- The preferred credential flow is a BuildKit secret named `nuget_api_key`
- `NUGET_API_KEY` is also supported as a build argument for CI systems that cannot mount secrets

## GitHub Actions

- `.github/workflows/ci.yml` -- runs on pushes to `main` and on pull requests
- `.github/workflows/publish-qml4net.yml` -- publishes the `Qml4Net` NuGet package on tags matching `Qml4Net-v*` or via manual `workflow_dispatch`

Required GitHub repository setup:

- Create a repository or environment secret named `NUGET_API_KEY`
- If you use GitHub environments, the publish workflow targets the `nuget` environment
- Publish tags must use the format `Qml4Net-v<semver>`, e.g. `Qml4Net-v0.1.0`

Example release flow:

```bash
git tag Qml4Net-v0.1.0
git push origin Qml4Net-v0.1.0
```

## Requirements

- Target: .NET 10.0 (LTS)

## Documentation

- [Porting Design](docs/port2net.md)
- [Architecture](docs/architecture.md)
- [Releasing](docs/releasing.md)

## Related Projects

- [qml4dart](https://github.com/pt9912/mapstyler/tree/main/qml4dart) -- Original Dart implementation

## License

[MIT](LICENSE)
