using System.Collections.Generic;

namespace FormAtlas.Tool.Core
{
    /// <summary>
    /// Warning severity levels for pipeline diagnostics.
    /// </summary>
    public enum WarningSeverity
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Single diagnostic entry emitted during pipeline execution.
    /// </summary>
    public sealed class PipelineWarning
    {
        public WarningSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }

        public PipelineWarning(WarningSeverity severity, string code, string message)
        {
            Severity = severity;
            Code = code;
            Message = message;
        }

        public override string ToString() => $"[{Severity}] {Code}: {Message}";
    }

    /// <summary>
    /// Accumulates non-fatal warnings and errors during pipeline execution.
    /// Pipelines should degrade gracefully and collect diagnostics rather than throwing.
    /// </summary>
    public sealed class PipelineWarnings
    {
        private readonly List<PipelineWarning> _items = new List<PipelineWarning>();

        public IReadOnlyList<PipelineWarning> Items => _items;

        public bool HasErrors { get; private set; }

        public void AddInfo(string code, string message) =>
            _items.Add(new PipelineWarning(WarningSeverity.Info, code, message));

        public void AddWarning(string code, string message) =>
            _items.Add(new PipelineWarning(WarningSeverity.Warning, code, message));

        public void AddError(string code, string message)
        {
            _items.Add(new PipelineWarning(WarningSeverity.Error, code, message));
            HasErrors = true;
        }

        public IEnumerable<string> ToStringList()
        {
            foreach (var w in _items)
                yield return w.ToString();
        }
    }
}
