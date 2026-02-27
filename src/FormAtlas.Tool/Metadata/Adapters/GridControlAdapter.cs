using System;
using System.Collections.Generic;
using FormAtlas.Tool.Contracts;
using FormAtlas.Tool.Core;

namespace FormAtlas.Tool.Metadata.Adapters
{
    /// <summary>
    /// Extracts metadata from DevExpress GridControl via reflection.
    /// </summary>
    public sealed class GridControlAdapter : DevExpressReflectionBase
    {
        public override string Kind => "GridControl";

        public override bool CanHandle(Type controlType) =>
            TypeChainContains(controlType, "GridControl");

        public NodeMetadata? Extract(object control, PipelineWarnings warnings)
        {
            try
            {
                var meta = new GridMeta();

                // Try to get the main view
                var mainView = SafeGet<object>(control, "MainView");
                if (mainView != null)
                {
                    meta.ViewType = mainView.GetType().Name;

                    // Extract columns
                    var columns = SafeGet<System.Collections.IEnumerable>(mainView, "Columns");
                    if (columns != null)
                    {
                        foreach (var col in columns)
                        {
                            try
                            {
                                meta.Columns.Add(new GridColumn
                                {
                                    Caption = SafeGet<string>(col, "Caption"),
                                    FieldName = SafeGet<string>(col, "FieldName"),
                                    VisibleIndex = SafeGetValue<int>(col, "VisibleIndex"),
                                    Width = SafeGetValue<int>(col, "Width"),
                                    GroupIndex = SafeGetValue<int>(col, "GroupIndex"),
                                    SortOrder = SafeGet<object>(col, "SortOrder")?.ToString()
                                });
                            }
                            catch (Exception ex)
                            {
                                warnings.AddWarning("GRID_COL_EXTRACT",
                                    $"Failed to extract grid column: {ex.Message}");
                            }
                        }
                    }
                }

                return new NodeMetadata
                {
                    DevExpress = new DevExpressMetadata { Kind = Kind, Grid = meta }
                };
            }
            catch (Exception ex)
            {
                warnings.AddWarning("GRID_EXTRACT_FAILED",
                    $"GridControl metadata extraction failed: {ex.Message}");
                return new NodeMetadata
                {
                    DevExpress = new DevExpressMetadata { Kind = Kind }
                };
            }
        }
    }
}
