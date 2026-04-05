using FluentAssertions;
using Qml4Net;
using Qml4Net.Model;
using Qml4Net.Read;
using Qml4Net.Write;
using Xunit;

namespace Qml4Net.Tests;

public class QmlCodecTests
{
    private static readonly QmlCodec Codec = new();

    private static string Fixture(string name) =>
        File.ReadAllText(Path.Combine("Fixtures", name));

    // ── Model ────────────────────────────────────────────────────

    [Fact]
    public void QmlDocument_can_be_constructed()
    {
        var renderer = new QmlRenderer(QmlRendererType.SingleSymbol);
        var doc = new QmlDocument(renderer, Version: "3.28.0");

        doc.Version.Should().Be("3.28.0");
        doc.Renderer.Type.Should().Be(QmlRendererType.SingleSymbol);
        doc.HasScaleBasedVisibility.Should().BeFalse();
        doc.MaxScale.Should().BeNull();
        doc.MinScale.Should().BeNull();
    }

    [Fact]
    public void QmlRendererType_round_trips_through_string()
    {
        foreach (var type in new[] {
            QmlRendererType.SingleSymbol,
            QmlRendererType.CategorizedSymbol,
            QmlRendererType.GraduatedSymbol,
            QmlRendererType.RuleRenderer })
        {
            var str = type.ToQmlString();
            QmlRendererTypeExtensions.FromQmlString(str).Should().Be(type);
        }
    }

    [Fact]
    public void QmlSymbolType_round_trips_through_string()
    {
        foreach (var type in new[] {
            QmlSymbolType.Marker,
            QmlSymbolType.Line,
            QmlSymbolType.Fill })
        {
            var str = type.ToQmlString();
            QmlSymbolTypeExtensions.FromQmlString(str).Should().Be(type);
        }
    }

    [Fact]
    public void QmlSymbolLayerType_round_trips_through_className()
    {
        foreach (var type in new[] {
            QmlSymbolLayerType.SimpleMarker,
            QmlSymbolLayerType.SvgMarker,
            QmlSymbolLayerType.SimpleLine,
            QmlSymbolLayerType.SimpleFill,
            QmlSymbolLayerType.RasterFill })
        {
            var str = type.ToClassName();
            QmlSymbolLayerTypeExtensions.FromClassName(str).Should().Be(type);
        }
    }

    // ── Parse singleSymbol ───────────────────────────────────────

    [Fact]
    public void Parse_SimpleFill_new_format()
    {
        var result = Codec.ParseString(Fixture("single_symbol_fill_new.qml"));
        var success = result.Should().BeOfType<ReadQmlResult.Success>().Subject;

        success.Document.Version.Should().Be("3.28.0-Firenze");
        var renderer = success.Document.Renderer;
        renderer.Type.Should().Be(QmlRendererType.SingleSymbol);
        renderer.Symbols.Should().ContainKey("0");

        var symbol = renderer.Symbols["0"];
        symbol.Type.Should().Be(QmlSymbolType.Fill);
        symbol.Layers.Should().HaveCount(1);

        var layer = symbol.Layers[0];
        layer.Type.Should().Be(QmlSymbolLayerType.SimpleFill);
        layer.Properties["color"].Should().Be("181,121,72,255");
        layer.Properties["outline_width"].Should().Be("0.26");
    }

    [Fact]
    public void Parse_SimpleFill_old_prop_format()
    {
        var result = Codec.ParseString(Fixture("single_symbol_fill_old.qml"));
        var success = result.Should().BeOfType<ReadQmlResult.Success>().Subject;

        var layer = success.Document.Renderer.Symbols["0"].Layers[0];
        layer.Properties["color"].Should().Be("181,121,72,255");
        layer.Properties["outline_width"].Should().Be("0.26");
    }

    [Fact]
    public void Parse_SimpleLine()
    {
        var result = Codec.ParseString(Fixture("single_symbol_line.qml"));
        var success = result.Should().BeOfType<ReadQmlResult.Success>().Subject;

        var layer = success.Document.Renderer.Symbols["0"].Layers[0];
        layer.Type.Should().Be(QmlSymbolLayerType.SimpleLine);
        layer.Properties["line_color"].Should().Be("255,0,255,255");
        layer.Properties["line_width"].Should().Be("3");
        layer.Properties["use_custom_dash"].Should().Be("1");
    }

    [Fact]
    public void Parse_SimpleMarker()
    {
        var result = Codec.ParseString(Fixture("single_symbol_marker.qml"));
        var success = result.Should().BeOfType<ReadQmlResult.Success>().Subject;

        var symbol = success.Document.Renderer.Symbols["0"];
        symbol.Type.Should().Be(QmlSymbolType.Marker);
        symbol.Alpha.Should().BeApproximately(0.8, 0.001);

        var layer = symbol.Layers[0];
        layer.Type.Should().Be(QmlSymbolLayerType.SimpleMarker);
        layer.Properties["name"].Should().Be("circle");
        layer.Properties["size"].Should().Be("4");
    }

    [Fact]
    public void Parse_SvgMarker()
    {
        var result = Codec.ParseString(Fixture("svg_marker.qml"));
        var success = result.Should().BeOfType<ReadQmlResult.Success>().Subject;

        var layer = success.Document.Renderer.Symbols["0"].Layers[0];
        layer.Type.Should().Be(QmlSymbolLayerType.SvgMarker);
        layer.ClassName.Should().Be("SvgMarker");
        layer.Properties["angle"].Should().Be("45");
    }

    [Fact]
    public void Parse_multi_layer_symbol()
    {
        var result = Codec.ParseString(Fixture("multi_layer_symbol.qml"));
        var success = result.Should().BeOfType<ReadQmlResult.Success>().Subject;

        var symbol = success.Document.Renderer.Symbols["0"];
        symbol.Layers.Should().HaveCount(2);
        symbol.Layers[0].Locked.Should().BeFalse();
        symbol.Layers[0].Pass.Should().Be(0);
        symbol.Layers[1].Locked.Should().BeTrue();
        symbol.Layers[1].Pass.Should().Be(1);
    }

    // ── Parse categorizedSymbol ──────────────────────────────────

    [Fact]
    public void Parse_categories_and_symbols()
    {
        var result = Codec.ParseString(Fixture("categorized.qml"));
        var success = result.Should().BeOfType<ReadQmlResult.Success>().Subject;

        var renderer = success.Document.Renderer;
        renderer.Type.Should().Be(QmlRendererType.CategorizedSymbol);
        renderer.Attribute.Should().Be("landuse");
        renderer.Categories.Should().HaveCount(4);
        renderer.Symbols.Should().HaveCount(4);

        renderer.Categories[0].Value.Should().Be("residential");
        renderer.Categories[0].Label.Should().Be("Wohngebiet");
        renderer.Categories[0].Render.Should().BeTrue();
        renderer.Categories[2].Render.Should().BeFalse();
        renderer.Categories[3].Value.Should().BeEmpty();
    }

    // ── Parse graduatedSymbol ────────────────────────────────────

    [Fact]
    public void Parse_ranges_and_symbols()
    {
        var result = Codec.ParseString(Fixture("graduated.qml"));
        var success = result.Should().BeOfType<ReadQmlResult.Success>().Subject;

        var renderer = success.Document.Renderer;
        renderer.Type.Should().Be(QmlRendererType.GraduatedSymbol);
        renderer.Attribute.Should().Be("population");
        renderer.GraduatedMethod.Should().Be("GraduatedColor");
        renderer.Ranges.Should().HaveCount(3);
        renderer.Symbols.Should().HaveCount(3);

        renderer.Ranges[0].Lower.Should().Be(0);
        renderer.Ranges[0].Upper.Should().Be(1000);
        renderer.Ranges[1].Lower.Should().Be(1000);
        renderer.Ranges[2].Upper.Should().Be(10000);
    }

    // ── Parse RuleRenderer ───────────────────────────────────────

    [Fact]
    public void Parse_flat_rules()
    {
        var result = Codec.ParseString(Fixture("rule_renderer.qml"));
        var success = result.Should().BeOfType<ReadQmlResult.Success>().Subject;

        var renderer = success.Document.Renderer;
        renderer.Type.Should().Be(QmlRendererType.RuleRenderer);
        renderer.Rules.Should().HaveCount(3);

        renderer.Rules[0].Filter.Should().Be("Bildpositi = 1");
        renderer.Rules[0].ScaleMinDenominator.Should().Be(100);
        renderer.Rules[0].ScaleMaxDenominator.Should().Be(2000);
        renderer.Rules[0].Enabled.Should().BeTrue();
        renderer.Rules[2].Enabled.Should().BeFalse();
    }

    [Fact]
    public void Parse_nested_rules()
    {
        var result = Codec.ParseString(Fixture("nested_rules.qml"));
        var success = result.Should().BeOfType<ReadQmlResult.Success>().Subject;

        var rules = success.Document.Renderer.Rules;
        rules.Should().HaveCount(2);
        rules[0].Children.Should().HaveCount(2);
        rules[0].SymbolKey.Should().BeNull();
        rules[0].Children[0].SymbolKey.Should().Be("0");
        rules[1].Children.Should().BeEmpty();
        rules[1].SymbolKey.Should().Be("2");
    }

    // ── Parse scale visibility ───────────────────────────────────

    [Fact]
    public void Parse_layer_level_scale_visibility()
    {
        var result = Codec.ParseString(Fixture("scale_visibility.qml"));
        var success = result.Should().BeOfType<ReadQmlResult.Success>().Subject;

        success.Document.HasScaleBasedVisibility.Should().BeTrue();
        success.Document.MaxScale.Should().Be(1000);
        success.Document.MinScale.Should().Be(50000);
    }

    // ── Parse errors ─────────────────────────────────────────────

    [Fact]
    public void Parse_invalid_XML()
    {
        var result = Codec.ParseString("<<<not xml>>>");
        var failure = result.Should().BeOfType<ReadQmlResult.Failure>().Subject;
        failure.Message.Should().Contain("XML parsing error");
    }

    [Fact]
    public void Parse_wrong_root_element()
    {
        var result = Codec.ParseString("<html/>");
        var failure = result.Should().BeOfType<ReadQmlResult.Failure>().Subject;
        failure.Message.Should().Contain("Expected root element <qgis>");
    }

    [Fact]
    public void Parse_missing_renderer_v2()
    {
        var result = Codec.ParseString("<qgis/>");
        var failure = result.Should().BeOfType<ReadQmlResult.Failure>().Subject;
        failure.Message.Should().Contain("Missing <renderer-v2>");
    }

    [Fact]
    public void Parse_unknown_renderer_type_produces_warning()
    {
        var xml = """
            <qgis>
              <renderer-v2 type="unknownRenderer">
                <symbols/>
              </renderer-v2>
            </qgis>
            """;
        var result = Codec.ParseString(xml);
        var success = result.Should().BeOfType<ReadQmlResult.Success>().Subject;
        success.Document.Renderer.Type.Should().Be(QmlRendererType.Unknown);
        success.Warnings.Should().Contain(w => w.Contains("Unknown renderer type"));
    }

    [Fact]
    public void Parse_unknown_symbol_layer_class_produces_warning()
    {
        var xml = """
            <qgis>
              <renderer-v2 type="singleSymbol">
                <symbols>
                  <symbol type="fill" name="0" alpha="1" clip_to_extent="1" force_rhr="0">
                    <layer class="GradientFill" enabled="1" locked="0" pass="0">
                      <Option type="Map"/>
                    </layer>
                  </symbol>
                </symbols>
              </renderer-v2>
            </qgis>
            """;
        var result = Codec.ParseString(xml);
        var success = result.Should().BeOfType<ReadQmlResult.Success>().Subject;
        success.Warnings.Should().Contain(w => w.Contains("Unknown symbol layer class"));
    }

    // ── Write ────────────────────────────────────────────────────

    [Fact]
    public void Write_singleSymbol_round_trip()
    {
        var original = Codec.ParseString(Fixture("single_symbol_fill_new.qml"));
        var doc = ((ReadQmlResult.Success)original).Document;

        var writeResult = Codec.EncodeString(doc);
        var xml = ((WriteQmlResult.Success)writeResult).Xml;

        var reparsed = Codec.ParseString(xml);
        var doc2 = ((ReadQmlResult.Success)reparsed).Document;

        doc2.Renderer.Type.Should().Be(doc.Renderer.Type);
        doc2.Renderer.Symbols.Should().HaveCount(doc.Renderer.Symbols.Count);
        doc2.Renderer.Symbols["0"].Layers[0].Properties["color"]
            .Should().Be(doc.Renderer.Symbols["0"].Layers[0].Properties["color"]);
    }

    [Fact]
    public void Write_categorized_round_trip()
    {
        var original = Codec.ParseString(Fixture("categorized.qml"));
        var doc = ((ReadQmlResult.Success)original).Document;
        var xml = ((WriteQmlResult.Success)Codec.EncodeString(doc)).Xml;
        var doc2 = ((ReadQmlResult.Success)Codec.ParseString(xml)).Document;

        doc2.Renderer.Type.Should().Be(QmlRendererType.CategorizedSymbol);
        doc2.Renderer.Attribute.Should().Be("landuse");
        doc2.Renderer.Categories.Should().HaveCount(4);
        doc2.Renderer.Symbols.Should().HaveCount(4);
    }

    [Fact]
    public void Write_graduated_round_trip()
    {
        var original = Codec.ParseString(Fixture("graduated.qml"));
        var doc = ((ReadQmlResult.Success)original).Document;
        var xml = ((WriteQmlResult.Success)Codec.EncodeString(doc)).Xml;
        var doc2 = ((ReadQmlResult.Success)Codec.ParseString(xml)).Document;

        doc2.Renderer.Type.Should().Be(QmlRendererType.GraduatedSymbol);
        doc2.Renderer.GraduatedMethod.Should().Be("GraduatedColor");
        doc2.Renderer.Ranges.Should().HaveCount(3);
    }

    [Fact]
    public void Write_RuleRenderer_round_trip()
    {
        var original = Codec.ParseString(Fixture("rule_renderer.qml"));
        var doc = ((ReadQmlResult.Success)original).Document;
        var xml = ((WriteQmlResult.Success)Codec.EncodeString(doc)).Xml;
        var doc2 = ((ReadQmlResult.Success)Codec.ParseString(xml)).Document;

        doc2.Renderer.Type.Should().Be(QmlRendererType.RuleRenderer);
        doc2.Renderer.Rules.Should().HaveCount(3);
        doc2.Renderer.Rules[0].Filter.Should().Be("Bildpositi = 1");
        doc2.Renderer.Rules[0].ScaleMinDenominator.Should().Be(100);
        doc2.Renderer.Rules[0].ScaleMaxDenominator.Should().Be(2000);
        doc2.Renderer.Rules[2].Enabled.Should().BeFalse();
    }

    [Fact]
    public void Write_nested_rules_round_trip()
    {
        var original = Codec.ParseString(Fixture("nested_rules.qml"));
        var doc = ((ReadQmlResult.Success)original).Document;
        var xml = ((WriteQmlResult.Success)Codec.EncodeString(doc)).Xml;
        var doc2 = ((ReadQmlResult.Success)Codec.ParseString(xml)).Document;

        doc2.Renderer.Rules.Should().HaveCount(2);
        doc2.Renderer.Rules[0].Children.Should().HaveCount(2);
    }

    [Fact]
    public void Write_scale_visibility_round_trip()
    {
        var original = Codec.ParseString(Fixture("scale_visibility.qml"));
        var doc = ((ReadQmlResult.Success)original).Document;
        var xml = ((WriteQmlResult.Success)Codec.EncodeString(doc)).Xml;
        var doc2 = ((ReadQmlResult.Success)Codec.ParseString(xml)).Document;

        doc2.HasScaleBasedVisibility.Should().BeTrue();
        doc2.MaxScale.Should().Be(1000);
        doc2.MinScale.Should().Be(50000);
    }

    [Fact]
    public void Write_multi_layer_symbol_round_trip()
    {
        var original = Codec.ParseString(Fixture("multi_layer_symbol.qml"));
        var doc = ((ReadQmlResult.Success)original).Document;
        var xml = ((WriteQmlResult.Success)Codec.EncodeString(doc)).Xml;
        var doc2 = ((ReadQmlResult.Success)Codec.ParseString(xml)).Document;

        var symbol = doc2.Renderer.Symbols["0"];
        symbol.Layers.Should().HaveCount(2);
        symbol.Layers[1].Locked.Should().BeTrue();
        symbol.Layers[1].Pass.Should().Be(1);
    }

    [Fact]
    public void Written_XML_contains_expected_structure()
    {
        var original = Codec.ParseString(Fixture("single_symbol_fill_new.qml"));
        var doc = ((ReadQmlResult.Success)original).Document;
        var xml = ((WriteQmlResult.Success)Codec.EncodeString(doc)).Xml;

        xml.Should().Contain("<qgis");
        xml.Should().Contain("version=\"3.28.0-Firenze\"");
        xml.Should().Contain("type=\"singleSymbol\"");
        xml.Should().Contain("class=\"SimpleFill\"");
        xml.Should().Contain("<rotation");
        xml.Should().Contain("<sizescale");
    }

    // ── File I/O ─────────────────────────────────────────────────

    [Fact]
    public async Task ParseFile_reads_fixture()
    {
        var result = await Codec.ParseFileAsync(Path.Combine("Fixtures", "categorized.qml"));
        var success = result.Should().BeOfType<ReadQmlResult.Success>().Subject;
        success.Document.Renderer.Type.Should().Be(QmlRendererType.CategorizedSymbol);
    }

    [Fact]
    public async Task ParseFile_returns_failure_for_missing_file()
    {
        var result = await Codec.ParseFileAsync("/nonexistent/path.qml");
        result.Should().BeOfType<ReadQmlResult.Failure>();
    }

    [Fact]
    public async Task EncodeFile_writes_and_reads_back()
    {
        var original = Codec.ParseString(Fixture("single_symbol_fill_new.qml"));
        var doc = ((ReadQmlResult.Success)original).Document;

        var tempPath = Path.Combine(Path.GetTempPath(), $"qml4net_test_{Guid.NewGuid()}.qml");
        try
        {
            var writeResult = await Codec.EncodeFileAsync(tempPath, doc);
            writeResult.Should().BeOfType<WriteQmlResult.Success>();

            var readBack = await Codec.ParseFileAsync(tempPath);
            var doc2 = ((ReadQmlResult.Success)readBack).Document;
            doc2.Renderer.Type.Should().Be(QmlRendererType.SingleSymbol);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }
}
