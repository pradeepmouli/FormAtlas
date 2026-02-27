using System;
using FormAtlas.Tool.Contracts;
using FormAtlas.Tool.Core;

namespace FormAtlas.Tool.Metadata.Adapters
{
    /// <summary>
    /// Extracts metadata from DevExpress RibbonControl and BarManager via reflection.
    /// </summary>
    public sealed class RibbonBarAdapter : DevExpressReflectionBase
    {
        public override string Kind => "RibbonControl";

        public override bool CanHandle(Type controlType) =>
            TypeChainContains(controlType, "RibbonControl") ||
            TypeChainContains(controlType, "BarManager");

        public NodeMetadata? Extract(object control, PipelineWarnings warnings)
        {
            try
            {
                var kind = TypeChainContains(control.GetType(), "BarManager") ? "BarManager" : "RibbonControl";
                var meta = new RibbonMeta();

                var pages = SafeGet<System.Collections.IEnumerable>(control, "Pages");
                if (pages != null)
                {
                    foreach (var page in pages)
                    {
                        try
                        {
                            var ribbonPage = new RibbonPage
                            {
                                Caption = SafeGet<string>(page, "Caption")
                            };

                            var groups = SafeGet<System.Collections.IEnumerable>(page, "Groups");
                            if (groups != null)
                            {
                                foreach (var group in groups)
                                {
                                    try
                                    {
                                        var ribbonGroup = new RibbonGroup
                                        {
                                            Caption = SafeGet<string>(group, "Caption")
                                        };

                                        var groupItems = SafeGet<System.Collections.IEnumerable>(group, "ItemLinks");
                                        if (groupItems != null)
                                        {
                                            foreach (var item in groupItems)
                                            {
                                                try
                                                {
                                                    ribbonGroup.Items.Add(new RibbonItem
                                                    {
                                                        Caption = SafeGet<string>(item, "Caption")
                                                    });
                                                }
                                                catch { /* non-fatal */ }
                                            }
                                        }

                                        ribbonPage.Groups.Add(ribbonGroup);
                                    }
                                    catch (Exception ex)
                                    {
                                        warnings.AddWarning("RIBBON_GROUP_EXTRACT",
                                            $"Failed to extract ribbon group: {ex.Message}");
                                    }
                                }
                            }

                            meta.Pages.Add(ribbonPage);
                        }
                        catch (Exception ex)
                        {
                            warnings.AddWarning("RIBBON_PAGE_EXTRACT",
                                $"Failed to extract ribbon page: {ex.Message}");
                        }
                    }
                }

                return new NodeMetadata
                {
                    DevExpress = new DevExpressMetadata { Kind = kind, Ribbon = meta }
                };
            }
            catch (Exception ex)
            {
                warnings.AddWarning("RIBBON_EXTRACT_FAILED",
                    $"Ribbon/BarManager metadata extraction failed: {ex.Message}");
                return new NodeMetadata
                {
                    DevExpress = new DevExpressMetadata { Kind = Kind }
                };
            }
        }
    }
}
