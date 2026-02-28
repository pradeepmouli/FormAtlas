using System.Collections.Generic;
using Newtonsoft.Json;

namespace FormAtlas.Semantic.Contracts
{
    public sealed class SemanticBundle
    {
        [JsonProperty("semanticVersion", Required = Required.Always)]
        public string SemanticVersion { get; set; } = string.Empty;

        [JsonProperty("sourceSchemaVersion", Required = Required.Always)]
        public string SourceSchemaVersion { get; set; } = string.Empty;

        [JsonProperty("form", Required = Required.Always)]
        public SemanticFormInfo Form { get; set; } = new SemanticFormInfo();

        [JsonProperty("annotations", Required = Required.Always)]
        public List<Annotation> Annotations { get; set; } = new List<Annotation>();

        [JsonProperty("regions")]
        public List<SemanticRegion>? Regions { get; set; }

        [JsonProperty("patterns")]
        public List<SemanticPattern>? Patterns { get; set; }

        [JsonProperty("warnings")]
        public List<string>? Warnings { get; set; }
    }

    public sealed class SemanticFormInfo
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("width", Required = Required.Always)]
        public int Width { get; set; }

        [JsonProperty("height", Required = Required.Always)]
        public int Height { get; set; }

        [JsonProperty("dpi")]
        public int? Dpi { get; set; }
    }

    public sealed class Annotation
    {
        [JsonProperty("nodeId", Required = Required.Always)]
        public string NodeId { get; set; } = string.Empty;

        [JsonProperty("roles", Required = Required.Always)]
        public List<RoleConfidence> Roles { get; set; } = new List<RoleConfidence>();

        [JsonProperty("hints")]
        public Dictionary<string, object>? Hints { get; set; }

        [JsonProperty("tags")]
        public List<string>? Tags { get; set; }
    }

    public sealed class RoleConfidence
    {
        [JsonProperty("role", Required = Required.Always)]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("confidence", Required = Required.Always)]
        public double Confidence { get; set; }

        [JsonProperty("evidence")]
        public List<string> Evidence { get; set; } = new List<string>();
    }

    public sealed class SemanticRegion
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("bounds", Required = Required.Always)]
        public SemanticRect Bounds { get; set; } = new SemanticRect();

        [JsonProperty("confidence")]
        public double? Confidence { get; set; }

        [JsonProperty("nodeIds")]
        public List<string>? NodeIds { get; set; }
    }

    public sealed class SemanticRect
    {
        [JsonProperty("x")] public double X { get; set; }
        [JsonProperty("y")] public double Y { get; set; }
        [JsonProperty("w")] public double W { get; set; }
        [JsonProperty("h")] public double H { get; set; }
    }

    public sealed class SemanticPattern
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("confidence", Required = Required.Always)]
        public double Confidence { get; set; }

        [JsonProperty("nodeIds")]
        public List<string>? NodeIds { get; set; }

        [JsonProperty("evidence")]
        public List<string>? Evidence { get; set; }
    }
}
