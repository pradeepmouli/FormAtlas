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

            // Create output directory
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var bundleDir = Path.Combine(_options.OutputDirectory, $"{formName}-{timestamp}");
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
