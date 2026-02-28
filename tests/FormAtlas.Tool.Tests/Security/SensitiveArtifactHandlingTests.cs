using System.IO;
using FormAtlas.Tool.Contracts;
using FormAtlas.Tool.Exporter;
using Xunit;

namespace FormAtlas.Tool.Tests.Security
{
    /// <summary>
    /// Sensitive artifact handling and path-boundary validation tests (T078).
    /// </summary>
    public class SensitiveArtifactHandlingTests
    {
        [Fact]
        public void BundleWriter_WritesToConfiguredDirectory()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tmpDir);

            try
            {
                var writer = new UiDumpBundleWriter();
                var bundle = new UiDumpBundle
                {
                    SchemaVersion = "1.0",
                    Form = new FormInfo { Name = "F", Type = "T", Width = 100, Height = 100 },
                };
                var outputPath = writer.Write(bundle, tmpDir);

                Assert.StartsWith(tmpDir, outputPath);
                Assert.True(File.Exists(outputPath));
            }
            finally
            {
                Directory.Delete(tmpDir, true);
            }
        }

        [Fact]
        public void BundleWriter_DoesNotWriteOutsideOutputDirectory()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tmpDir);

            try
            {
                var writer = new UiDumpBundleWriter();
                var bundle = new UiDumpBundle
                {
                    SchemaVersion = "1.0",
                    Form = new FormInfo { Name = "F", Type = "T", Width = 100, Height = 100 }
                };

                // Verify the output path is within the output directory
                var outputPath = writer.Write(bundle, tmpDir);
                var fullOutput = Path.GetFullPath(outputPath);
                var fullDir = Path.GetFullPath(tmpDir);

                Assert.StartsWith(fullDir, fullOutput);
            }
            finally
            {
                Directory.Delete(tmpDir, true);
            }
        }
    }
}
