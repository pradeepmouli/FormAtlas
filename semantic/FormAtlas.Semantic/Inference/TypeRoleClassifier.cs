using System;
using System.Collections.Generic;
using FormAtlas.Semantic.Contracts;
using FormAtlas.Semantic.Normalization;

namespace FormAtlas.Semantic.Inference
{
    /// <summary>
    /// Classifies semantic roles for UI nodes based on their .NET/DevExpress type names.
    /// Produces high-confidence role assignments for well-known type patterns.
    /// </summary>
    public static class TypeRoleClassifier
    {
        private static readonly Dictionary<string, (string role, double confidence)> TypeMap =
            new Dictionary<string, (string, double)>(StringComparer.OrdinalIgnoreCase)
        {
            ["Button"] = ("Action", 0.95),
            ["TextBox"] = ("InputField", 0.95),
            ["Label"] = ("Label", 0.90),
            ["ComboBox"] = ("SelectField", 0.90),
            ["CheckBox"] = ("ToggleField", 0.90),
            ["RadioButton"] = ("ToggleField", 0.85),
            ["ListBox"] = ("ListControl", 0.85),
            ["DataGridView"] = ("DataGrid", 0.95),
            ["TreeView"] = ("TreeControl", 0.90),
            ["TabControl"] = ("TabContainer", 0.90),
            ["Panel"] = ("Container", 0.70),
            ["GroupBox"] = ("GroupContainer", 0.80),
            ["Form"] = ("FormRoot", 0.99),
            ["MenuStrip"] = ("Menu", 0.90),
            ["ToolStrip"] = ("Toolbar", 0.85),
            ["StatusStrip"] = ("StatusBar", 0.85),
            ["PictureBox"] = ("Image", 0.85),
            ["ProgressBar"] = ("ProgressIndicator", 0.90),
            ["NumericUpDown"] = ("NumericInput", 0.85),
            ["DateTimePicker"] = ("DateInput", 0.90),
        };

        private static readonly Dictionary<string, (string role, double confidence)> DevExpressKindMap =
            new Dictionary<string, (string, double)>(StringComparer.OrdinalIgnoreCase)
        {
            ["GridControl"] = ("DataGrid", 0.95),
            ["PivotGridControl"] = ("PivotTable", 0.95),
            ["XtraTabControl"] = ("TabContainer", 0.95),
            ["LayoutControl"] = ("LayoutContainer", 0.90),
            ["RibbonControl"] = ("Ribbon", 0.95),
            ["BarManager"] = ("Toolbar", 0.90),
        };

        /// <summary>
        /// Classifies roles for all nodes in a normalized list.
        /// Returns annotations with type-based roles where recognizable.
        /// </summary>
        public static List<Annotation> Classify(IEnumerable<NormalizedNode> nodes)
        {
            var annotations = new List<Annotation>();

            foreach (var node in nodes)
            {
                var annotation = new Annotation { NodeId = node.Id };

                // Check DevExpress kind first
                if (!string.IsNullOrEmpty(node.DevExpressKind) &&
                    DevExpressKindMap.TryGetValue(node.DevExpressKind!, out var dxRole))
                {
                    annotation.Roles.Add(new RoleConfidence
                    {
                        Role = dxRole.role,
                        Confidence = dxRole.confidence,
                        Evidence = new List<string> { $"devexpress.kind={node.DevExpressKind}" }
                    });
                }
                else
                {
                    // Try short type name match
                    var shortType = GetShortTypeName(node.Type);
                    if (TypeMap.TryGetValue(shortType, out var typeRole))
                    {
                        annotation.Roles.Add(new RoleConfidence
                        {
                            Role = typeRole.role,
                            Confidence = typeRole.confidence,
                            Evidence = new List<string> { $"type={node.Type}" }
                        });
                    }
                    else
                    {
                        // Default unknown role
                        annotation.Roles.Add(new RoleConfidence
                        {
                            Role = "Unknown",
                            Confidence = 0.10,
                            Evidence = new List<string> { $"type={node.Type}" }
                        });
                    }
                }

                annotations.Add(annotation);
            }

            return annotations;
        }

        private static string GetShortTypeName(string fullTypeName)
        {
            if (string.IsNullOrEmpty(fullTypeName)) return fullTypeName;
            var lastDot = fullTypeName.LastIndexOf('.');
            return lastDot >= 0 ? fullTypeName.Substring(lastDot + 1) : fullTypeName;
        }
    }
}
