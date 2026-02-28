using System.Collections.Generic;
using FormAtlas.Tool.Core;
using FormAtlas.Tool.Metadata;
using FormAtlas.Tool.Exporter;
using Xunit;

namespace FormAtlas.Tool.Tests.Exporter
{
    /// <summary>
    /// Deterministic traversal and bounds tests for ControlWalker (T020).
    /// Uses a simple mock control object that mimics WinForms reflection surface.
    /// </summary>
    public class ControlWalkerTests
    {
        private static ControlWalker CreateWalker(int maxDepth = 0) =>
            new ControlWalker(new AdapterRegistry(), maxDepth);

        [Fact]
        public void Walk_SingleControl_ProducesOneNode()
        {
            var control = new MockControl("root", "Form", 0, 0, 800, 600);
            var warnings = new PipelineWarnings();

            var nodes = CreateWalker().Walk(control, warnings);

            Assert.Single(nodes);
            Assert.Equal("root", nodes[0].Name);
        }

        [Fact]
        public void Walk_ControlWithChildren_ProducesHierarchy()
        {
            var child1 = new MockControl("btn1", "Button", 10, 10, 80, 30, zIndex: 1);
            var child2 = new MockControl("btn2", "Button", 100, 10, 80, 30, zIndex: 0);
            var root = new MockControl("root", "Form", 0, 0, 800, 600, children: new[] { child1, child2 });
            var warnings = new PipelineWarnings();

            var nodes = CreateWalker().Walk(root, warnings);

            Assert.Single(nodes);
            var rootNode = nodes[0];
            Assert.Equal(2, rootNode.Children.Count);
        }

        [Fact]
        public void Walk_Children_AreSortedDeterministicallyByZIndexThenName()
        {
            // The ControlWalker assigns ZIndex from enumeration order (0, 1, 2...)
            // then sorts deterministically by that ZIndex. So the order in Children
            // after sort equals the order of the Controls collection.
            var child1 = new MockControl("btn2", "Button", 100, 10, 80, 30);  // comes first in collection → ZIndex 0
            var child2 = new MockControl("btn1", "Button", 10, 10, 80, 30);   // comes second in collection → ZIndex 1
            var root = new MockControl("root", "Form", 0, 0, 800, 600, children: new[] { child1, child2 });
            var warnings = new PipelineWarnings();

            var nodes = CreateWalker().Walk(root, warnings);

            var children = nodes[0].Children;
            Assert.Equal("btn2", children[0].Name); // ZIndex 0 (first in collection)
            Assert.Equal("btn1", children[1].Name); // ZIndex 1 (second in collection)
        }

        [Fact]
        public void Walk_MaxDepth_LimitsTraversal()
        {
            var grandchild = new MockControl("gc", "Label", 0, 0, 50, 20);
            var child = new MockControl("child", "Panel", 0, 0, 100, 100, children: new[] { grandchild });
            var root = new MockControl("root", "Form", 0, 0, 800, 600, children: new[] { child });
            var warnings = new PipelineWarnings();

            var nodes = CreateWalker(maxDepth: 1).Walk(root, warnings);

            // root -> child (depth 1), grandchild would be at depth 2 which is > maxDepth
            var rootNode = nodes[0];
            Assert.Single(rootNode.Children);
            Assert.Empty(rootNode.Children[0].Children);
        }

        [Fact]
        public void Walk_NodeBounds_ArePreserved()
        {
            var control = new MockControl("root", "Form", 10, 20, 800, 600);
            var warnings = new PipelineWarnings();

            var nodes = CreateWalker().Walk(control, warnings);

            var bounds = nodes[0].Bounds;
            Assert.Equal(10, bounds.X);
            Assert.Equal(20, bounds.Y);
            Assert.Equal(800, bounds.W);
            Assert.Equal(600, bounds.H);
        }

        // ---- Mock control helpers ----

        private sealed class MockControl
        {
            public string Name { get; }
            public string Type { get; }
            public bool Visible => true;
            public bool Enabled => true;
            public MockBounds Bounds { get; }
            public MockControlCollection Controls { get; }

            public MockControl(string name, string type, int x, int y, int w, int h,
                int zIndex = 0, MockControl[]? children = null)
            {
                Name = name;
                Type = type;
                Bounds = new MockBounds(x, y, w, h);
                Controls = new MockControlCollection(children ?? System.Array.Empty<MockControl>());
            }
        }

        private sealed class MockBounds
        {
            public int X { get; }
            public int Y { get; }
            public int Width { get; }
            public int Height { get; }

            public MockBounds(int x, int y, int w, int h)
            {
                X = x; Y = y; Width = w; Height = h;
            }
        }

        private sealed class MockControlCollection : System.Collections.IEnumerable
        {
            private readonly MockControl[] _items;
            public MockControlCollection(MockControl[] items) => _items = items;
            public System.Collections.IEnumerator GetEnumerator() => _items.GetEnumerator();
        }
    }
}
