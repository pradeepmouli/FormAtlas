using System;
using System.IO;
using FormAtlas.Tool.Core;

namespace FormAtlas.Tool.Exporter
{
    /// <summary>
    /// Abstracts screenshot capture for WinForms forms.
    /// Capture is best-effort: failures add warnings and do not abort export.
    /// </summary>
    public sealed class ScreenshotCaptureService
    {
        /// <summary>
        /// Attempts to capture a screenshot of the given WinForms form object.
        /// Returns the output file path on success, or null on failure.
        /// </summary>
        public string? TryCapture(object form, string outputPath, PipelineWarnings warnings)
        {
            if (warnings == null) throw new ArgumentNullException(nameof(warnings));

            try
            {
                return CaptureCore(form, outputPath);
            }
            catch (Exception ex)
            {
                warnings.AddWarning("SCREENSHOT_FAILED",
                    $"Screenshot capture failed (non-fatal): {ex.Message}");
                return null;
            }
        }

        private string CaptureCore(object form, string outputPath)
        {
            // Capture is implemented via reflection to avoid compile-time WinForms dependency
            // on netstandard2.0. On net48, System.Drawing and System.Windows.Forms are available.
            var formType = form.GetType();

            var widthProp = formType.GetProperty("Width");
            var heightProp = formType.GetProperty("Height");
            if (widthProp == null || heightProp == null)
                throw new InvalidOperationException("Form does not expose Width/Height properties.");

            int width = (int)widthProp.GetValue(form)!;
            int height = (int)heightProp.GetValue(form)!;
            if (width <= 0 || height <= 0)
                throw new InvalidOperationException($"Invalid form dimensions: {width}x{height}");

            // Locate System.Drawing.Bitmap and System.Windows.Forms.Screen types via reflection
            var bitmapType = Type.GetType("System.Drawing.Bitmap, System.Drawing") ??
                             Type.GetType("System.Drawing.Bitmap, System.Drawing.Common");
            if (bitmapType == null)
                throw new PlatformNotSupportedException("System.Drawing.Bitmap is not available on this platform.");

            // Create Bitmap(width, height)
            var bitmap = Activator.CreateInstance(bitmapType, width, height)!;

            // Get Graphics from bitmap via CreateGraphics()
            var graphicsType = Type.GetType("System.Drawing.Graphics, System.Drawing") ??
                               Type.GetType("System.Drawing.Graphics, System.Drawing.Common");
            if (graphicsType == null)
                throw new PlatformNotSupportedException("System.Drawing.Graphics is not available on this platform.");

            var createGraphicsMethod = bitmapType.GetMethod("GetGraphics") ??
                graphicsType.GetMethod("FromImage", new[] { bitmapType.BaseType ?? bitmapType });

            // DrawToBitmap(bitmap, bounds)
            var drawToBitmapMethod = formType.GetMethod("DrawToBitmap");
            if (drawToBitmapMethod != null)
            {
                // Create Rectangle(0, 0, width, height)
                var rectangleType = Type.GetType("System.Drawing.Rectangle, System.Drawing") ??
                                    Type.GetType("System.Drawing.Rectangle, System.Drawing.Primitives");
                if (rectangleType == null)
                    throw new PlatformNotSupportedException("System.Drawing.Rectangle is not available.");

                var rect = Activator.CreateInstance(rectangleType, 0, 0, width, height);
                drawToBitmapMethod.Invoke(form, new[] { bitmap, rect });
            }

            // Save bitmap
            var saveMethod = bitmapType.GetMethod("Save", new[] { typeof(string) });
            saveMethod?.Invoke(bitmap, new object[] { outputPath });

            // Dispose bitmap
            if (bitmap is IDisposable disposable)
                disposable.Dispose();

            return outputPath;
        }
    }
}
