using System;
using System.Collections.Generic;
using FormAtlas.Tool.Contracts;
using FormAtlas.Tool.Core;
using FormAtlas.Tool.Metadata;

namespace FormAtlas.Tool.Exporter
{
    /// <summary>
    /// Recursively walks a WinForms control tree and produces UiNode hierarchies.
    /// Traversal is guarded against exceptions on individual controls.
    /// </summary>
    public sealed class ControlWalker
    {
        private readonly AdapterRegistry _adapterRegistry;
        private readonly int _maxDepth;
        private int _counter;

        public ControlWalker(AdapterRegistry adapterRegistry, int maxDepth = 0)
        {
            _adapterRegistry = adapterRegistry ?? throw new ArgumentNullException(nameof(adapterRegistry));
            _maxDepth = maxDepth;
        }

        /// <summary>
        /// Walks the provided root control object and returns UiNode hierarchy.
        /// The control type is resolved by full type name to avoid compile-time WinForms dependency in netstandard2.0.
        /// </summary>
        public List<UiNode> Walk(object rootControl, PipelineWarnings warnings)
        {
            if (rootControl == null) throw new ArgumentNullException(nameof(rootControl));
            if (warnings == null) throw new ArgumentNullException(nameof(warnings));

            _counter = 0;
            return WalkControl(rootControl, depth: 0, warnings: warnings);
        }

        private List<UiNode> WalkControl(object control, int depth, PipelineWarnings warnings)
        {
            var nodes = new List<UiNode>();

            if (_maxDepth > 0 && depth > _maxDepth)
                return nodes;

            try
            {
                var node = MapControl(control, depth, warnings);
                nodes.Add(node);
            }
            catch (Exception ex)
            {
                warnings.AddWarning("WALK_CONTROL_FAILED",
                    $"Failed to walk control at depth {depth}: {ex.Message}");
            }

            return nodes;
        }

        private UiNode MapControl(object control, int depth, PipelineWarnings warnings)
        {
            var type = control.GetType();
            var node = new UiNode
            {
                Id = DeterministicOrdering.FallbackId(_counter++),
                Type = type.FullName ?? type.Name,
                Name = GetProperty<string>(control, "Name") ?? string.Empty,
                Text = TryGetProperty<string>(control, "Text"),
                Visible = GetProperty<bool>(control, "Visible"),
                Enabled = GetProperty<bool>(control, "Enabled"),
                Dock = TryGetProperty<object>(control, "Dock")?.ToString(),
                Anchor = TryGetProperty<object>(control, "Anchor")?.ToString(),
                ZIndex = 0,
                Bounds = GetBounds(control),
                Metadata = _adapterRegistry.TryExtract(control, warnings)
            };

            // Walk children via Controls collection
            var controlsCollection = TryGetProperty<object>(control, "Controls");
            if (controlsCollection != null)
            {
                var childControls = new List<object>();
                try
                {
                    foreach (var child in (System.Collections.IEnumerable)controlsCollection)
                        childControls.Add(child);
                }
                catch (Exception ex)
                {
                    warnings.AddWarning("CONTROLS_ENUM_FAILED",
                        $"Could not enumerate Controls on '{node.Name}': {ex.Message}");
                }

                int zIdx = 0;
                foreach (var child in childControls)
                {
                    var childNodes = WalkControl(child, depth + 1, warnings);
                    foreach (var childNode in childNodes)
                    {
                        childNode.ZIndex = zIdx++;
                        node.Children.Add(childNode);
                    }
                }

                // Sort children deterministically
                node.Children = new List<UiNode>(
                    DeterministicOrdering.Sort(node.Children));
            }

            return node;
        }

        private static Rect GetBounds(object control)
        {
            try
            {
                var boundsObj = TryGetProperty<object>(control, "Bounds");
                if (boundsObj != null)
                {
                    var bType = boundsObj.GetType();
                    int x = GetStructField<int>(boundsObj, bType, "X");
                    int y = GetStructField<int>(boundsObj, bType, "Y");
                    int w = GetStructField<int>(boundsObj, bType, "Width");
                    int h = GetStructField<int>(boundsObj, bType, "Height");
                    return new Rect { X = x, Y = y, W = w, H = h };
                }
            }
            catch { /* fall through */ }

            return new Rect { X = 0, Y = 0, W = 0, H = 0 };
        }

        private static T GetProperty<T>(object obj, string name)
        {
            var prop = obj.GetType().GetProperty(name);
            if (prop == null) return default!;
            var val = prop.GetValue(obj);
            if (val is T t) return t;
            return default!;
        }

        private static T? TryGetProperty<T>(object obj, string name) where T : class
        {
            try
            {
                var prop = obj.GetType().GetProperty(name);
                return prop?.GetValue(obj) as T;
            }
            catch { return null; }
        }

        private static T GetStructField<T>(object obj, Type type, string name) where T : struct
        {
            var prop = type.GetProperty(name);
            if (prop == null) return default;
            var val = prop.GetValue(obj);
            if (val is T t) return t;
            return default;
        }
    }
}
