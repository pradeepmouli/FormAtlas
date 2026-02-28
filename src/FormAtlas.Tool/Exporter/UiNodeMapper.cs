using System;
using FormAtlas.Tool.Contracts;
using FormAtlas.Tool.Core;

namespace FormAtlas.Tool.Exporter
{
    /// <summary>
    /// Maps raw WinForms-reflected control state to UiNode contract model fields.
    /// Handles required/optional field assignment with appropriate fallbacks.
    /// </summary>
    public static class UiNodeMapper
    {
        /// <summary>
        /// Maps a control's reflected properties onto a partially-populated UiNode.
        /// Required fields are guaranteed; optional fields use best-effort resolution.
        /// </summary>
        public static UiNode Map(string id, string typeName, string name, Rect bounds,
            string? text = null, bool visible = true, bool enabled = true,
            string? dock = null, string? anchor = null, int zIndex = 0,
            NodeMetadata? metadata = null, PipelineWarnings? warnings = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("TypeName is required.", nameof(typeName));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", nameof(name));
            if (bounds == null)
                throw new ArgumentNullException(nameof(bounds));

            return new UiNode
            {
                Id = id,
                Type = typeName,
                Name = name,
                Text = text,
                Visible = visible,
                Enabled = enabled,
                Dock = dock,
                Anchor = anchor,
                ZIndex = zIndex,
                Bounds = bounds,
                Metadata = metadata,
                Children = new System.Collections.Generic.List<UiNode>()
            };
        }
    }
}
