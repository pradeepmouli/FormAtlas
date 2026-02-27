using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace FormAtlas.Semantic.Normalization
{
    /// <summary>
    /// Normalizes UI node features from a UiDumpBundle for semantic inference.
    /// Computes absolute bounds and extracts feature vectors.
    /// </summary>
    public static class FeatureNormalizer
    {
        /// <summary>
        /// Recursively walks nodes and computes absolute bounds from parent-relative coordinates.
        /// Returns a flat list of normalized node feature dictionaries.
        /// </summary>
        public static List<NormalizedNode> Normalize(JArray nodes, int parentX = 0, int parentY = 0)
        {
            var result = new List<NormalizedNode>();

            foreach (var node in nodes)
            {
                if (node is not JObject obj) continue;

                var bounds = obj["bounds"] as JObject;
                int relX = bounds?["x"]?.Value<int>() ?? 0;
                int relY = bounds?["y"]?.Value<int>() ?? 0;
                int w = bounds?["w"]?.Value<int>() ?? 0;
                int h = bounds?["h"]?.Value<int>() ?? 0;

                int absX = parentX + relX;
                int absY = parentY + relY;

                var normalized = new NormalizedNode
                {
                    Id = obj["id"]?.Value<string>() ?? string.Empty,
                    Type = obj["type"]?.Value<string>() ?? string.Empty,
                    Name = obj["name"]?.Value<string>() ?? string.Empty,
                    Text = obj["text"]?.Value<string>(),
                    Visible = obj["visible"]?.Value<bool>() ?? true,
                    Enabled = obj["enabled"]?.Value<bool>() ?? true,
                    AbsX = absX,
                    AbsY = absY,
                    W = w,
                    H = h,
                    DevExpressKind = obj["metadata"]?["devexpress"]?["kind"]?.Value<string>()
                };

                result.Add(normalized);

                var children = obj["children"] as JArray;
                if (children != null)
                    result.AddRange(Normalize(children, absX, absY));
            }

            return result;
        }
    }

    public sealed class NormalizedNode
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Text { get; set; }
        public bool Visible { get; set; }
        public bool Enabled { get; set; }
        public int AbsX { get; set; }
        public int AbsY { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public string? DevExpressKind { get; set; }
    }
}
