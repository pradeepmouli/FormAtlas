using System;

namespace FormAtlas.Tool.Agent
{
    /// <summary>
    /// Configuration options for the UI dump agent and exporter.
    /// </summary>
    public sealed class UiDumpOptions
    {
        /// <summary>
        /// Directory where bundle output directories will be created.
        /// Defaults to the current working directory.
        /// </summary>
        public string OutputDirectory { get; set; } = ".";

        /// <summary>
        /// When true, attempts to capture a screenshot of the form during export.
        /// On capture failure, export continues with a warning (non-fatal).
        /// </summary>
        public bool CaptureScreenshot { get; set; } = true;

        /// <summary>
        /// When true, attempts to extract DevExpress-specific metadata via reflection.
        /// Reflection failures are non-fatal and produce warnings.
        /// </summary>
        public bool ExtractDevExpressMetadata { get; set; } = true;

        /// <summary>
        /// Optional hotkey string used to trigger dumps (e.g., "Ctrl+Shift+D").
        /// Interpretation is host-specific.
        /// </summary>
        public string TriggerHotkey { get; set; } = "Ctrl+Shift+D";

        /// <summary>
        /// Maximum recursion depth for control traversal.
        /// A value of 0 means unlimited.
        /// </summary>
        public int MaxDepth { get; set; } = 0;
    }
}
