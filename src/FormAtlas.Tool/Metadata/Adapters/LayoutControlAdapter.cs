using System;
using FormAtlas.Tool.Contracts;
using FormAtlas.Tool.Core;

namespace FormAtlas.Tool.Metadata.Adapters
{
    /// <summary>
    /// Extracts metadata from DevExpress LayoutControl via reflection.
    /// </summary>
    public sealed class LayoutControlAdapter : DevExpressReflectionBase
    {
        public override string Kind => "LayoutControl";

        public override bool CanHandle(Type controlType) =>
            TypeChainContains(controlType, "LayoutControl");

        public NodeMetadata? Extract(object control, PipelineWarnings warnings)
        {
            try
            {
                var meta = new LayoutMeta();

                var items = SafeGet<System.Collections.IEnumerable>(control, "Items");
                if (items != null)
                {
                    // Flatten into a single group
                    var group = new LayoutGroup { Caption = null };
                    foreach (var item in items)
                    {
                        try
                        {
                            var boundsObj = SafeGet<object>(item, "Bounds");
                            Rect? bounds = null;
                            if (boundsObj != null)
                            {
                                var bType = boundsObj.GetType();
                                bounds = new Rect
                                {
                                    X = (int)(bType.GetProperty("X")?.GetValue(boundsObj) ?? 0),
                                    Y = (int)(bType.GetProperty("Y")?.GetValue(boundsObj) ?? 0),
                                    W = (int)(bType.GetProperty("Width")?.GetValue(boundsObj) ?? 0),
                                    H = (int)(bType.GetProperty("Height")?.GetValue(boundsObj) ?? 0)
                                };
                            }

                            group.Items.Add(new LayoutItem
                            {
                                ControlName = SafeGet<object>(item, "Control")
                                    .Let(c => SafeGet<string>(c, "Name")),
                                Label = SafeGet<string>(item, "Text"),
                                Bounds = bounds
                            });
                        }
                        catch (Exception ex)
                        {
                            warnings.AddWarning("LAYOUT_ITEM_EXTRACT",
                                $"Failed to extract layout item: {ex.Message}");
                        }
                    }
                    if (group.Items.Count > 0)
                        meta.Groups.Add(group);
                }

                return new NodeMetadata
                {
                    DevExpress = new DevExpressMetadata { Kind = Kind, Layout = meta }
                };
            }
            catch (Exception ex)
            {
                warnings.AddWarning("LAYOUT_EXTRACT_FAILED",
                    $"LayoutControl metadata extraction failed: {ex.Message}");
                return new NodeMetadata
                {
                    DevExpress = new DevExpressMetadata { Kind = Kind }
                };
            }
        }
    }

    internal static class ObjectExtensions
    {
        internal static TResult? Let<T, TResult>(this T? obj, Func<T, TResult?> func) where T : class
        {
            if (obj == null) return default;
            return func(obj);
        }
    }
}
