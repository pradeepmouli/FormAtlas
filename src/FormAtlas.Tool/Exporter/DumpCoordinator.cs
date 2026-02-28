using System;
using System.IO;
using FormAtlas.Tool.Agent;
using FormAtlas.Tool.Contracts;
using FormAtlas.Tool.Core;
using FormAtlas.Tool.Metadata;

namespace FormAtlas.Tool.Exporter
{
    /// <summary>
    /// Orchestrates the dump pipeline: walk controls, capture screenshot, write bundle.
    /// </summary>
    public sealed class DumpCoordinator
    {
        private readonly UiDumpOptions _options;
        private readonly ControlWalker _walker;
        private readonly ScreenshotCaptureService _screenshotService;
        private readonly UiDumpBundleWriter _writer;

        public DumpCoordinator(UiDumpOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            var registry = new AdapterRegistry();
            _walker = new ControlWalker(registry, options.MaxDepth);
            _screenshotService = new ScreenshotCaptureService();
            _writer = new UiDumpBundleWriter();
        }

        /// <summary>
        /// Executes a full dump of the given WinForms form object.
        /// Returns the path to the written bundle directory.
        /// </summary>
        public string Execute(object form, PipelineWarnings warnings)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            if (warnings == null) throw new ArgumentNullException(nameof(warnings));

            // Gather form info
            var formType = form.GetType();
            var formName = GetString(form, "Name") ?? formType.Name;
            var formText = GetString(form, "Text");
            var width = GetInt(form, "Width");
            var height = GetInt(form, "Height");
            var dpi = TryGetDpi(form);

            var formInfo = new FormInfo
            {
                Name = formName,
                Type = formType.FullName ?? formType.Name,
                Width = width,
                Height = height,
                Dpi = dpi
            };

            // Create output directory — sanitize formName to prevent path traversal
            var safeName = SanitizeFileName(formName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var outputDirFull = Path.GetFullPath(_options.OutputDirectory);
            var bundleDir = Path.GetFullPath(
                Path.Combine(outputDirFull, $"{safeName}-{timestamp}"));
            // Require bundleDir to be a direct subdirectory of outputDirFull
            if (!bundleDir.StartsWith(outputDirFull + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Bundle directory would escape the configured OutputDirectory.");
            Directory.CreateDirectory(bundleDir);

            // Walk controls
            var nodes = _walker.Walk(form, warnings);

            // Screenshot (best-effort)
            string? screenshotRelPath = null;
            if (_options.CaptureScreenshot)
            {
                var screenshotPath = Path.Combine(bundleDir, "form.png");
                var capturedPath = _screenshotService.TryCapture(form, screenshotPath, warnings);
                if (capturedPath != null)
                    screenshotRelPath = "form.png";
            }

            // Write bundle
            var bundle = new UiDumpBundle
            {
                SchemaVersion = SchemaVersionPolicy.CurrentVersion,
                Form = formInfo,
                Nodes = nodes,
                Screenshot = screenshotRelPath,
                Errors = new System.Collections.Generic.List<string>(warnings.ToStringList())
            };

            _writer.Write(bundle, bundleDir);
            return bundleDir;
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new System.Text.StringBuilder(name.Length);
            foreach (var c in name)
                sb.Append(Array.IndexOf(invalid, c) >= 0 ? '_' : c);
            var result = sb.ToString().Trim();
            return result.Length == 0 ? "form" : result;
        }

        private static string? GetString(object obj, string prop) =>
            obj.GetType().GetProperty(prop)?.GetValue(obj) as string;

        private static int GetInt(object obj, string prop)
        {
            var val = obj.GetType().GetProperty(prop)?.GetValue(obj);
            return val is int i ? i : 0;
        }

        private static int? TryGetDpi(object form)
        {
            try
            {
                var graphicsProp = form.GetType().GetProperty("DeviceDpi");
                if (graphicsProp == null) return null;
                var val = graphicsProp.GetValue(form);
                if (val is int i) return i;
                return null;
            }
            catch { return null; }
        }
    }
}
