using System;
using System.Collections.Generic;
using FormAtlas.Tool.Contracts;
using FormAtlas.Tool.Core;
using FormAtlas.Tool.Metadata.Adapters;

namespace FormAtlas.Tool.Metadata
{
    /// <summary>
    /// Registry of DevExpress metadata adapters.
    /// Dispatches control extraction to matching adapter, with guarded execution.
    /// </summary>
    public sealed class AdapterRegistry
    {
        private readonly List<(DevExpressReflectionBase adapter, Func<object, PipelineWarnings, NodeMetadata?> extract)> _entries;

        public AdapterRegistry()
        {
            _entries = new List<(DevExpressReflectionBase, Func<object, PipelineWarnings, NodeMetadata?>)>();
            RegisterDefaults();
        }

        private void RegisterDefaults()
        {
            var gridAdapter = new GridControlAdapter();
            _entries.Add((gridAdapter, (c, w) => gridAdapter.Extract(c, w)));

            var pivotAdapter = new PivotGridControlAdapter();
            _entries.Add((pivotAdapter, (c, w) => pivotAdapter.Extract(c, w)));

            var tabAdapter = new XtraTabControlAdapter();
            _entries.Add((tabAdapter, (c, w) => tabAdapter.Extract(c, w)));

            var layoutAdapter = new LayoutControlAdapter();
            _entries.Add((layoutAdapter, (c, w) => layoutAdapter.Extract(c, w)));

            var ribbonAdapter = new RibbonBarAdapter();
            _entries.Add((ribbonAdapter, (c, w) => ribbonAdapter.Extract(c, w)));
        }

        /// <summary>
        /// Attempts to find and execute an adapter for the given control.
        /// Returns null if no adapter matches or extraction fails.
        /// </summary>
        public NodeMetadata? TryExtract(object control, PipelineWarnings warnings)
        {
            if (control == null) throw new ArgumentNullException(nameof(control));
            if (warnings == null) throw new ArgumentNullException(nameof(warnings));

            var controlType = control.GetType();

            foreach (var (adapter, extract) in _entries)
            {
                bool canHandle;
                try
                {
                    canHandle = adapter.CanHandle(controlType);
                }
                catch (Exception ex)
                {
                    warnings.AddWarning("ADAPTER_CANHANDLE_FAILED",
                        $"Adapter '{adapter.Kind}' CanHandle check threw: {ex.Message}");
                    continue;
                }

                if (!canHandle)
                    continue;

                try
                {
                    return extract(control, warnings);
                }
                catch (Exception ex)
                {
                    warnings.AddWarning("ADAPTER_EXTRACT_FAILED",
                        $"Adapter '{adapter.Kind}' extract threw unexpectedly: {ex.Message}");
                    return new NodeMetadata
                    {
                        DevExpress = new DevExpressMetadata { Kind = adapter.Kind }
                    };
                }
            }

            return null;
        }
    }
}
