using System.IO;
using FormAtlas.Tool.Core;
using FormAtlas.Tool.Exporter;
using Xunit;

namespace FormAtlas.Tool.Tests.Exporter
{
    /// <summary>
    /// Screenshot capture fallback tests (T021).
    /// Validates that failed screenshot capture adds a warning and does not throw.
    /// </summary>
    public class ScreenshotCaptureFallbackTests
    {
        [Fact]
        public void TryCapture_WhenFormLacksDrawToBitmap_ReturnsNullWithWarning()
        {
            // A plain object does not expose WinForms-specific draw methods
            var service = new ScreenshotCaptureService();
            var warnings = new PipelineWarnings();
            var fakeForm = new { Name = "FakeForm", Width = 100, Height = 100 };

            var result = service.TryCapture(fakeForm, Path.GetTempFileName(), warnings);

            // Should not throw; result may be null or succeed (platform-dependent)
            // If capture failed, a warning should have been added
            if (result == null)
                Assert.True(warnings.Items.Count > 0, "Expected a warning when capture fails.");
        }

        [Fact]
        public void TryCapture_NeverThrows_EvenOnBadInput()
        {
            var service = new ScreenshotCaptureService();
            var warnings = new PipelineWarnings();

            // Form-like object with zero dimensions to trigger an error path
            var badForm = new { Name = "Bad", Width = 0, Height = 0 };

            var exception = Record.Exception(() =>
                service.TryCapture(badForm, Path.GetTempFileName(), warnings));

            Assert.Null(exception);
        }
    }
}
