/**
 * Domain types for UI dump bundles consumed by the Figma importer.
 */

export interface UiRect {
  x: number;
  y: number;
  w: number;
  h: number;
}

export interface GridColumn {
  caption?: string | null;
  fieldName?: string | null;
  visibleIndex?: number | null;
  width?: number | null;
  groupIndex?: number | null;
  sortOrder?: string | null;
}

export interface GridMeta {
  viewType?: string;
  columns?: GridColumn[];
}

export interface PivotField {
  caption?: string | null;
  fieldName?: string | null;
  area?: string | null;
  areaIndex?: number | null;
  visible?: boolean | null;
  width?: number | null;
}

export interface PivotMeta {
  fields?: PivotField[];
}

export interface TabPage {
  text?: string | null;
  index: number;
}

export interface TabMeta {
  selectedIndex?: number | null;
  pages?: TabPage[];
}

export interface LayoutItem {
  controlName?: string | null;
  label?: string | null;
  bounds?: UiRect;
}

export interface LayoutGroup {
  caption?: string | null;
  items?: LayoutItem[];
}

export interface LayoutMeta {
  groups?: LayoutGroup[];
}

export interface RibbonItem {
  caption?: string | null;
}

export interface RibbonGroup {
  caption?: string | null;
  items?: RibbonItem[];
}

export interface RibbonPage {
  caption?: string | null;
  groups?: RibbonGroup[];
}

export interface RibbonMeta {
  pages?: RibbonPage[];
}

export type DevExpressKind =
  | "GridControl"
  | "PivotGridControl"
  | "XtraTabControl"
  | "LayoutControl"
  | "RibbonControl"
  | "BarManager";

export interface DevExpressMetadata {
  kind: string; // intentionally 'string' to tolerate unknown kinds
  grid?: GridMeta;
  pivot?: PivotMeta;
  tabs?: TabMeta;
  layout?: LayoutMeta;
  ribbon?: RibbonMeta;
}

export interface NodeMetadata {
  devexpress?: DevExpressMetadata;
}

export interface UiNode {
  id: string;
  type: string;
  name: string;
  text?: string | null;
  visible?: boolean;
  enabled?: boolean;
  dock?: string | null;
  anchor?: string | null;
  zIndex?: number;
  bounds: UiRect;
  metadata?: NodeMetadata | null;
  children: UiNode[];
}

export interface FormInfo {
  name: string;
  type: string;
  width: number;
  height: number;
  dpi?: number | null;
}

export interface UiDumpBundle {
  schemaVersion: string;
  form: FormInfo;
  nodes: UiNode[];
  screenshot?: string | null;
  errors?: string[];
}
