using System;
using System.Collections.Generic;
using FormAtlas.Tool.Contracts;
using FormAtlas.Tool.Core;

namespace FormAtlas.Tool.Metadata.Adapters
{
    /// <summary>
    /// Extracts metadata from DevExpress PivotGridControl via reflection.
    /// </summary>
    public sealed class PivotGridControlAdapter : DevExpressReflectionBase
    {
        public override string Kind => "PivotGridControl";

        public override bool CanHandle(Type controlType) =>
            TypeChainContains(controlType, "PivotGridControl");

        public NodeMetadata? Extract(object control, PipelineWarnings warnings)
        {
            try
            {
                var meta = new PivotMeta();

                var fields = SafeGet<System.Collections.IEnumerable>(control, "Fields");
                if (fields != null)
                {
                    foreach (var field in fields)
                    {
                        try
                        {
                            meta.Fields.Add(new PivotField
                            {
                                Caption = SafeGet<string>(field, "Caption"),
                                FieldName = SafeGet<string>(field, "FieldName"),
                                Area = SafeGet<object>(field, "Area")?.ToString(),
                                AreaIndex = SafeGetValue<int>(field, "AreaIndex"),
                                Visible = SafeGetValue<bool>(field, "Visible"),
                                Width = SafeGetValue<int>(field, "Width")
                            });
                        }
                        catch (Exception ex)
                        {
                            warnings.AddWarning("PIVOT_FIELD_EXTRACT",
                                $"Failed to extract pivot field: {ex.Message}");
                        }
                    }
                }

                return new NodeMetadata
                {
                    DevExpress = new DevExpressMetadata { Kind = Kind, Pivot = meta }
                };
            }
            catch (Exception ex)
            {
                warnings.AddWarning("PIVOT_EXTRACT_FAILED",
                    $"PivotGridControl metadata extraction failed: {ex.Message}");
                return new NodeMetadata
                {
                    DevExpress = new DevExpressMetadata { Kind = Kind }
                };
            }
        }
    }
}
