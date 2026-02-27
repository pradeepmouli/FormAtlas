using System;
using System.Collections.Generic;
using System.Linq;
using FormAtlas.Semantic.Contracts;
using FormAtlas.Semantic.Normalization;

namespace FormAtlas.Semantic.Inference
{
    /// <summary>
    /// Detects high-level layout regions and multi-node interaction patterns
    /// from normalized node sets.
    /// </summary>
    public static class RegionPatternDetector
    {
        private static readonly HashSet<string> ActionRoles = new HashSet<string>(StringComparer.Ordinal)
        {
            "Action"
        };

        /// <summary>
        /// Detects layout regions (toolbar, action bar, content area) from node positions.
        /// </summary>
        public static List<SemanticRegion> DetectRegions(IReadOnlyList<NormalizedNode> nodes,
            int formWidth, int formHeight)
        {
            var regions = new List<SemanticRegion>();
            if (nodes.Count == 0) return regions;

            // Action bar: bottom strip with action buttons
            var actionNodes = nodes
                .Where(n => n.H <= 50 && n.AbsY > formHeight * 0.8)
                .ToList();

            if (actionNodes.Count >= 1)
            {
                var minX = actionNodes.Min(n => n.AbsX);
                var minY = actionNodes.Min(n => n.AbsY);
                var maxX = actionNodes.Max(n => n.AbsX + n.W);
                var maxY = actionNodes.Max(n => n.AbsY + n.H);

                regions.Add(new SemanticRegion
                {
                    Name = "ActionBar",
                    Bounds = new SemanticRect
                    {
                        X = minX, Y = minY,
                        W = maxX - minX, H = maxY - minY
                    },
                    Confidence = 0.75,
                    NodeIds = actionNodes.Select(n => n.Id).ToList()
                });
            }

            // Content area: largest node that spans most of the form
            var contentNodes = nodes
                .Where(n => n.W > formWidth * 0.5 && n.H > formHeight * 0.3)
                .OrderByDescending(n => n.W * n.H)
                .Take(1)
                .ToList();

            if (contentNodes.Count > 0)
            {
                var content = contentNodes[0];
                regions.Add(new SemanticRegion
                {
                    Name = "ContentArea",
                    Bounds = new SemanticRect
                    {
                        X = content.AbsX, Y = content.AbsY,
                        W = content.W, H = content.H
                    },
                    Confidence = 0.80,
                    NodeIds = new List<string> { content.Id }
                });
            }

            return regions;
        }

        /// <summary>
        /// Detects common multi-node interaction patterns (e.g., primary/secondary actions).
        /// </summary>
        public static List<SemanticPattern> DetectPatterns(
            IReadOnlyList<NormalizedNode> nodes,
            IReadOnlyList<Annotation> annotations)
        {
            var patterns = new List<SemanticPattern>();

            // Primary+Secondary action pair
            var actionAnnotations = annotations
                .Where(a => a.Roles.Any(r => r.Role == "Action"))
                .ToList();

            if (actionAnnotations.Count >= 2)
            {
                var primary = actionAnnotations
                    .OrderByDescending(a => a.Roles.Max(r => r.Confidence))
                    .First();
                var secondary = actionAnnotations
                    .Where(a => a.NodeId != primary.NodeId)
                    .OrderByDescending(a => a.Roles.Max(r => r.Confidence))
                    .First();

                patterns.Add(new SemanticPattern
                {
                    Name = "PrimarySecondaryActions",
                    Confidence = 0.75,
                    NodeIds = new List<string> { primary.NodeId, secondary.NodeId },
                    Evidence = new List<string> { "Two or more action-role nodes detected" }
                });
            }

            return patterns;
        }
    }
}
