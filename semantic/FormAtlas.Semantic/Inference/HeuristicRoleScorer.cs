using System;
using System.Collections.Generic;
using System.Linq;
using FormAtlas.Semantic.Contracts;
using FormAtlas.Semantic.Normalization;

namespace FormAtlas.Semantic.Inference
{
    /// <summary>
    /// Applies text and layout heuristics to refine role confidence scores.
    /// Supplements type-based classification with behavioral/positional signals.
    /// </summary>
    public static class HeuristicRoleScorer
    {
        // Text patterns associated with action buttons
        private static readonly HashSet<string> ActionKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "OK", "Cancel", "Save", "Close", "Submit", "Apply", "Delete",
            "Add", "Remove", "Edit", "New", "Open", "Exit", "Yes", "No",
            "Next", "Back", "Finish", "Refresh", "Search", "Find", "Export",
            "Import", "Print", "Help"
        };

        private static readonly HashSet<string> PrimaryActionKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "OK", "Save", "Submit", "Apply", "Finish", "Next"
        };

        /// <summary>
        /// Enhances annotations with heuristic-based role adjustments.
        /// Modifies the annotations list in-place.
        /// </summary>
        public static void Score(List<Annotation> annotations, IEnumerable<NormalizedNode> nodes)
        {
            var nodeMap = nodes.ToDictionary(n => n.Id, StringComparer.Ordinal);

            foreach (var annotation in annotations)
            {
                if (!nodeMap.TryGetValue(annotation.NodeId, out var node)) continue;

                ApplyTextHeuristics(annotation, node);
                ApplyLayoutHeuristics(annotation, node);
            }
        }

        private static void ApplyTextHeuristics(Annotation annotation, NormalizedNode node)
        {
            if (string.IsNullOrEmpty(node.Text)) return;

            var text = node.Text!.Trim();
            var currentRole = annotation.Roles.FirstOrDefault()?.Role ?? "";

            if (ActionKeywords.Contains(text) && currentRole == "Action")
            {
                var role = annotation.Roles.First();
                if (PrimaryActionKeywords.Contains(text))
                {
                    role.Confidence = Math.Min(1.0, role.Confidence + 0.03);
                    role.Evidence.Add($"text='{text}' matches primary action keyword");
                }
                else
                {
                    role.Evidence.Add($"text='{text}' matches action keyword");
                }
            }
        }

        private static void ApplyLayoutHeuristics(Annotation annotation, NormalizedNode node)
        {
            // Nodes in the bottom-right quadrant of reasonably-sized forms tend to be action buttons
            if (node.W > 0 && node.H > 0 && node.AbsY > 0)
            {
                var role = annotation.Roles.FirstOrDefault();
                if (role != null && role.Role == "Action" && node.H <= 40 && node.W <= 200)
                {
                    role.Evidence.Add("bounds=compact-button-region");
                }
            }
        }
    }
}
