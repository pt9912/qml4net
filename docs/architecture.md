# Qml4Net Architecture

## Overview

`Qml4Net` is a layered .NET library for reading and writing QGIS QML (`.qml`) layer style files.
The central design goal is a clean separation between:

- public API entry points
- in-memory domain model
- XML parsing and serialization
- renderer-specific read/write logic

At runtime, the library converts QML XML into a typed object graph and can serialize the same graph
back into QML XML.

## High-Level Flow

### Read path

1. `QmlCodec.ParseString` or `QmlCodec.ParseFileAsync` receives QML XML input.
2. `QmlXmlReader` parses the XML into an `XDocument`.
3. `RendererReader` delegates to lower-level readers such as `SymbolReader` and `RuleReader`.
4. The readers construct the domain model under `Qml4Net.Model`.
5. The result is returned as `ReadQmlResult.Success` or `ReadQmlResult.Failure`.

### Write path

1. `QmlCodec.EncodeString` or `QmlCodec.EncodeFileAsync` receives a `QmlDocument`.
2. `QmlXmlWriter` creates the root `<qgis>` element and document-level attributes.
3. `RendererWriter` delegates to lower-level writers such as `SymbolWriter` and `RuleWriter`.
4. The writers serialize the domain model back into QML XML.
5. The result is returned as `WriteQmlResult.Success` or `WriteQmlResult.Failure`.

## Architectural Layers

### 1. Public API

The public API is intentionally small:

- `IQmlCodec`
- `QmlCodec`
- model types in `Qml4Net.Model`
- read/write result types

`QmlCodec` acts as a facade. It hides the internal XML and renderer-specific implementation details
and exposes a stable entry point for consumers.

### 2. Domain Model

The `Model/` folder contains the typed representation of a QML document:

- `QmlDocument` as the aggregate root
- `QmlRenderer` for renderer metadata and renderer-specific collections
- `QmlSymbol` and `QmlSymbolLayer` for symbol definitions
- `QmlRule` for nested rule trees
- `QmlCategory` and `QmlRange` for categorized and graduated renderers
- enum types for renderer, symbol, and symbol-layer kinds

This layer is independent of XML APIs. That keeps parsing concerns separate from the object model and
makes round-trip testing simpler.

### 3. Read Pipeline

The read pipeline lives in `Read/` plus the top-level XML entry point in `Xml/QmlXmlReader.cs`.

Responsibilities:

- validate the root structure
- parse document-level attributes
- parse `<renderer-v2>` and its nested structures
- map XML values into strongly typed model objects
- collect warnings while still allowing partial success where appropriate
- return structured failures instead of throwing to callers

### 4. Write Pipeline

The write pipeline lives in `Write/` plus the top-level XML entry point in `Xml/QmlXmlWriter.cs`.

Responsibilities:

- create the QGIS root document
- emit renderer-specific XML from the model
- preserve the library's canonical XML structure
- return structured failures instead of throwing to callers

### 5. XML Helpers

`Xml/XmlHelpers.cs` contains shared conversion logic used by both parsing and serialization.
This keeps repetitive XML value handling out of the readers and writers.

## Directory Structure

```text
src/Qml4Net/
  IQmlCodec.cs
  QmlCodec.cs
  Model/
  Read/
  Write/
  Xml/

tests/Qml4Net.Tests/
  QmlCodecTests.cs
  Fixtures/
```

## Design Characteristics

- **Facade-based API**: consumers interact with `QmlCodec`, not with internal readers or writers.
- **Layered internals**: model, XML handling, and renderer logic are separated by responsibility.
- **Typed results**: success and failure are represented explicitly via result objects.
- **Round-trip oriented**: the architecture is designed to support parse -> model -> encode workflows.
- **Dependency-light**: XML handling relies on `System.Xml.Linq` rather than external packages.

## Testing Strategy

The test project validates the architecture primarily through end-to-end codec tests using fixture QML
files. This keeps the public contract stable while exercising the full read/write stack.

## Current Constraints

- The main entry point currently centers on full-document parsing and writing rather than streaming.
- XML compatibility is bounded by the renderer and symbol types implemented in the current readers and
  writers.
- Warning collection exists, but the exact warning surface is intentionally smaller than the full
  exception surface.
