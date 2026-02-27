using System;
using System.Collections.Generic;
using System.Linq;
using FormAtlas.Tool.Contracts;

namespace FormAtlas.Tool.Core
{
    /// <summary>
    /// Provides deterministic ordering and stable ID sequencing for UI nodes.
    /// Ordering is by zIndex ascending, then by name ascending for stability.
    /// </summary>
    public static class DeterministicOrdering
    {
        /// <summary>
        /// Returns nodes sorted deterministically: by zIndex ascending, then by name ascending.
        /// </summary>
        public static IEnumerable<UiNode> Sort(IEnumerable<UiNode> nodes)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            return nodes.OrderBy(n => n.ZIndex).ThenBy(n => n.Name, StringComparer.Ordinal);
        }

        /// <summary>
        /// Assigns stable sequential IDs to all nodes in a depth-first traversal.
        /// Existing IDs are replaced only when they are null or empty.
        /// </summary>
        public static void AssignSequentialIds(IEnumerable<UiNode> nodes, ref int counter)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));

            foreach (var node in nodes)
            {
                if (string.IsNullOrEmpty(node.Id))
                    node.Id = $"node-{counter}";

                counter++;
                AssignSequentialIds(node.Children, ref counter);
            }
        }

        /// <summary>
        /// Generates a stable fallback ID string from a counter value.
        /// </summary>
        public static string FallbackId(int counter) => $"node-{counter}";
    }
}
