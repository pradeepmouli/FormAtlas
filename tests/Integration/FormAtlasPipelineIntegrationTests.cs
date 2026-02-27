using System.IO;
using FormAtlas.Tool.Contracts;
using FormAtlas.Tool.Exporter;
using FormAtlas.Tool.Validation;
using Newtonsoft.Json;
using Xunit;

namespace FormAtlas.Tests.Integration
{
    /// <summary>
    /// End-to-end fixture pipeline validation tests (T072).
    /// Validates that bundle writing produces schema-valid output
    /// and that the full write→read→validate pipeline is deterministic.
    /// </summary>
    public class FormAtlasPipelineIntegrationTests
    {
        private static readonly string SchemaPath = Path.Combine("docs", "ui-dump.schema.json");

        [Fact]
        public void BundleWriter_ProducesSchemaValidOutput()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tmpDir);
            try
            {
                var bundle = SampleBundle();
                var writer = new UiDumpBundleWriter();
                var outputPath = writer.Write(bundle, tmpDir);

                var jsonText = File.ReadAllText(outputPath);
                var validator = SchemaValidator.LoadFromFile(SchemaPath);
                var errors = validator.Validate(jsonText);

                Assert.Empty(errors);
            }
            finally
            {
                Directory.Delete(tmpDir, true);
            }
        }

        [Fact]
        public void BundleWriter_RepeatedWrite_ProducesDeterministicJson()
        {
            var tmpDir1 = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var tmpDir2 = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tmpDir1);
            Directory.CreateDirectory(tmpDir2);

            try
            {
                var bundle = SampleBundle();
                var writer = new UiDumpBundleWriter();
                var path1 = writer.Write(bundle, tmpDir1);
                var path2 = writer.Write(bundle, tmpDir2);

                var json1 = File.ReadAllText(path1);
                var json2 = File.ReadAllText(path2);

                Assert.Equal(json1, json2);
            }
            finally
            {
                Directory.Delete(tmpDir1, true);
                Directory.Delete(tmpDir2, true);
            }
        }

        [Fact]
        public void RoundTrip_WriteAndDeserialize_PreservesAllFields()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tmpDir);
            try
            {
                var bundle = SampleBundle();
                var writer = new UiDumpBundleWriter();
                var outputPath = writer.Write(bundle, tmpDir);

                var jsonText = File.ReadAllText(outputPath);
                var restored = JsonConvert.DeserializeObject<UiDumpBundle>(jsonText)!;

                Assert.Equal(bundle.SchemaVersion, restored.SchemaVersion);
                Assert.Equal(bundle.Form.Name, restored.Form.Name);
                Assert.Equal(bundle.Nodes.Count, restored.Nodes.Count);
                Assert.Equal(bundle.Nodes[0].Id, restored.Nodes[0].Id);
            }
            finally
            {
                Directory.Delete(tmpDir, true);
            }
        }

        private static UiDumpBundle SampleBundle() => new UiDumpBundle
        {
            SchemaVersion = "1.0",
            Form = new FormInfo { Name = "IntegrationForm", Type = "Form", Width = 800, Height = 600, Dpi = 96 },
            Nodes = new System.Collections.Generic.List<UiNode>
            {
                new UiNode
                {
                    Id = "node-0",
                    Type = "System.Windows.Forms.Form",
                    Name = "IntegrationForm",
                    Text = "Integration Form",
                    Visible = true,
                    Enabled = true,
                    ZIndex = 0,
                    Bounds = new Rect { X = 0, Y = 0, W = 800, H = 600 },
                    Children = new System.Collections.Generic.List<UiNode>()
                }
            }
        };
    }
}
