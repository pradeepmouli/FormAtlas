using System.Collections.Generic;
using Newtonsoft.Json;

namespace FormAtlas.Tool.Contracts
{
    public sealed class UiDumpBundle
    {
        [JsonProperty("schemaVersion", Required = Required.Always)]
        public string SchemaVersion { get; set; } = string.Empty;

        [JsonProperty("form", Required = Required.Always)]
        public FormInfo Form { get; set; } = new FormInfo();

        [JsonProperty("nodes", Required = Required.Always)]
        public List<UiNode> Nodes { get; set; } = new List<UiNode>();

        [JsonProperty("screenshot")]
        public string? Screenshot { get; set; }

        [JsonProperty("errors")]
        public List<string> Errors { get; set; } = new List<string>();
    }

    public sealed class FormInfo
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

    public sealed class UiNode
    {
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("text")]
        public string? Text { get; set; }

        [JsonProperty("visible")]
        public bool Visible { get; set; } = true;

        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonProperty("dock")]
        public string? Dock { get; set; }

        [JsonProperty("anchor")]
        public string? Anchor { get; set; }

        [JsonProperty("zIndex")]
        public int ZIndex { get; set; }

        [JsonProperty("bounds", Required = Required.Always)]
        public Rect Bounds { get; set; } = new Rect();

        [JsonProperty("metadata")]
        public NodeMetadata? Metadata { get; set; }

        [JsonProperty("children", Required = Required.Always)]
        public List<UiNode> Children { get; set; } = new List<UiNode>();
    }

    public sealed class Rect
    {
        [JsonProperty("x", Required = Required.Always)]
        public int X { get; set; }

        [JsonProperty("y", Required = Required.Always)]
        public int Y { get; set; }

        [JsonProperty("w", Required = Required.Always)]
        public int W { get; set; }

        [JsonProperty("h", Required = Required.Always)]
        public int H { get; set; }
    }

    public sealed class NodeMetadata
    {
        [JsonProperty("devexpress")]
        public DevExpressMetadata? DevExpress { get; set; }
    }

    public sealed class DevExpressMetadata
    {
        [JsonProperty("kind", Required = Required.Always)]
        public string Kind { get; set; } = string.Empty;

        [JsonProperty("grid")]
        public GridMeta? Grid { get; set; }

        [JsonProperty("pivot")]
        public PivotMeta? Pivot { get; set; }

        [JsonProperty("tabs")]
        public TabMeta? Tabs { get; set; }

        [JsonProperty("layout")]
        public LayoutMeta? Layout { get; set; }

        [JsonProperty("ribbon")]
        public RibbonMeta? Ribbon { get; set; }
    }

    public sealed class GridMeta
    {
        [JsonProperty("viewType")]
        public string? ViewType { get; set; }

        [JsonProperty("columns")]
        public List<GridColumn> Columns { get; set; } = new List<GridColumn>();
    }

    public sealed class GridColumn
    {
        [JsonProperty("caption")]
        public string? Caption { get; set; }

        [JsonProperty("fieldName")]
        public string? FieldName { get; set; }

        [JsonProperty("visibleIndex")]
        public int? VisibleIndex { get; set; }

        [JsonProperty("width")]
        public int? Width { get; set; }

        [JsonProperty("groupIndex")]
        public int? GroupIndex { get; set; }

        [JsonProperty("sortOrder")]
        public string? SortOrder { get; set; }
    }

    public sealed class PivotMeta
    {
        [JsonProperty("fields")]
        public List<PivotField> Fields { get; set; } = new List<PivotField>();
    }

    public sealed class PivotField
    {
        [JsonProperty("caption")]
        public string? Caption { get; set; }

        [JsonProperty("fieldName")]
        public string? FieldName { get; set; }

        [JsonProperty("area")]
        public string? Area { get; set; }

        [JsonProperty("areaIndex")]
        public int? AreaIndex { get; set; }

        [JsonProperty("visible")]
        public bool? Visible { get; set; }

        [JsonProperty("width")]
        public int? Width { get; set; }
    }

    public sealed class TabMeta
    {
        [JsonProperty("selectedIndex")]
        public int? SelectedIndex { get; set; }

        [JsonProperty("pages")]
        public List<TabPage> Pages { get; set; } = new List<TabPage>();
    }

    public sealed class TabPage
    {
        [JsonProperty("text")]
        public string? Text { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }
    }

    public sealed class LayoutMeta
    {
        [JsonProperty("groups")]
        public List<LayoutGroup> Groups { get; set; } = new List<LayoutGroup>();
    }

    public sealed class LayoutGroup
    {
        [JsonProperty("caption")]
        public string? Caption { get; set; }

        [JsonProperty("items")]
        public List<LayoutItem> Items { get; set; } = new List<LayoutItem>();
    }

    public sealed class LayoutItem
    {
        [JsonProperty("controlName")]
        public string? ControlName { get; set; }

        [JsonProperty("label")]
        public string? Label { get; set; }

        [JsonProperty("bounds")]
        public Rect? Bounds { get; set; }
    }

    public sealed class RibbonMeta
    {
        [JsonProperty("pages")]
        public List<RibbonPage> Pages { get; set; } = new List<RibbonPage>();
    }

    public sealed class RibbonPage
    {
        [JsonProperty("caption")]
        public string? Caption { get; set; }

        [JsonProperty("groups")]
        public List<RibbonGroup> Groups { get; set; } = new List<RibbonGroup>();
    }

    public sealed class RibbonGroup
    {
        [JsonProperty("caption")]
        public string? Caption { get; set; }

        [JsonProperty("items")]
        public List<RibbonItem> Items { get; set; } = new List<RibbonItem>();
    }

    public sealed class RibbonItem
    {
        [JsonProperty("caption")]
        public string? Caption { get; set; }
    }
}
