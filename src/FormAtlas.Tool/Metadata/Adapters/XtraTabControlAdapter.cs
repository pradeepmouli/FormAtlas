using System;
using FormAtlas.Tool.Contracts;
using FormAtlas.Tool.Core;

namespace FormAtlas.Tool.Metadata.Adapters
{
    /// <summary>
    /// Extracts metadata from DevExpress XtraTabControl via reflection.
    /// </summary>
    public sealed class XtraTabControlAdapter : DevExpressReflectionBase
    {
        public override string Kind => "XtraTabControl";

        public override bool CanHandle(Type controlType) =>
            TypeChainContains(controlType, "XtraTabControl");

        public NodeMetadata? Extract(object control, PipelineWarnings warnings)
        {
            try
            {
                var meta = new TabMeta
                {
                    SelectedIndex = SafeGetValue<int>(control, "SelectedTabPageIndex")
                };

                var pages = SafeGet<System.Collections.IEnumerable>(control, "TabPages");
                if (pages != null)
                {
                    int idx = 0;
                    foreach (var page in pages)
                    {
                        try
                        {
                            meta.Pages.Add(new TabPage
                            {
                                Text = SafeGet<string>(page, "Text"),
                                Index = idx++
                            });
                        }
                        catch (Exception ex)
                        {
                            warnings.AddWarning("TAB_PAGE_EXTRACT",
                                $"Failed to extract tab page: {ex.Message}");
                        }
                    }
                }

                return new NodeMetadata
                {
                    DevExpress = new DevExpressMetadata { Kind = Kind, Tabs = meta }
                };
            }
            catch (Exception ex)
            {
                warnings.AddWarning("TAB_EXTRACT_FAILED",
                    $"XtraTabControl metadata extraction failed: {ex.Message}");
                return new NodeMetadata
                {
                    DevExpress = new DevExpressMetadata { Kind = Kind }
                };
            }
        }
    }
}
