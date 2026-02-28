using System.Collections.Generic;
using System.Linq;
using FormAtlas.Semantic.Contracts;
using FormAtlas.Semantic.Inference;
using FormAtlas.Semantic.Normalization;
using Xunit;

namespace FormAtlas.Semantic.Tests.Inference
{
    /// <summary>
    /// Region and pattern detection tests (T064).
    /// </summary>
    public class PatternDetectionTests
    {
        [Fact]
        public void DetectRegions_TwoButtonsAtBottom_ProducesActionBar()
        {
            var nodes = new List<NormalizedNode>
            {
                new NormalizedNode { Id = "n1", Type = "Button", Name = "btnOK", AbsX = 620, AbsY = 560, W = 80, H = 30 },
                new NormalizedNode { Id = "n2", Type = "Button", Name = "btnCancel", AbsX = 710, AbsY = 560, W = 80, H = 30 }
            };

            var regions = RegionPatternDetector.DetectRegions(nodes, formWidth: 800, formHeight: 600);

            Assert.Contains(regions, r => r.Name == "ActionBar");
        }

        [Fact]
        public void DetectRegions_LargeDataGridFill_ProducesContentArea()
        {
            var nodes = new List<NormalizedNode>
            {
                new NormalizedNode { Id = "n1", Type = "GridControl", Name = "grid", AbsX = 0, AbsY = 0, W = 800, H = 500 }
            };

            var regions = RegionPatternDetector.DetectRegions(nodes, formWidth: 800, formHeight: 600);

            Assert.Contains(regions, r => r.Name == "ContentArea");
        }

        [Fact]
        public void DetectPatterns_TwoActionNodes_ProducesPrimarySecondaryPattern()
        {
            var annotations = new List<Annotation>
            {
                new Annotation { NodeId = "n1", Roles = new List<RoleConfidence> { new RoleConfidence { Role = "Action", Confidence = 0.95 } } },
                new Annotation { NodeId = "n2", Roles = new List<RoleConfidence> { new RoleConfidence { Role = "Action", Confidence = 0.90 } } }
            };
            var nodes = new List<NormalizedNode>();

            var patterns = RegionPatternDetector.DetectPatterns(nodes, annotations);

            Assert.Contains(patterns, p => p.Name == "PrimarySecondaryActions");
            var pattern = patterns.First(p => p.Name == "PrimarySecondaryActions");
            Assert.True(pattern.Confidence >= 0.7);
        }

        [Fact]
        public void DetectPatterns_SingleActionNode_DoesNotProducePattern()
        {
            var annotations = new List<Annotation>
            {
                new Annotation { NodeId = "n1", Roles = new List<RoleConfidence> { new RoleConfidence { Role = "Action", Confidence = 0.95 } } }
            };
            var nodes = new List<NormalizedNode>();

            var patterns = RegionPatternDetector.DetectPatterns(nodes, annotations);

            Assert.DoesNotContain(patterns, p => p.Name == "PrimarySecondaryActions");
        }

        [Fact]
        public void RegionNodeIds_ContainExpectedNodes()
        {
            var nodes = new List<NormalizedNode>
            {
                new NormalizedNode { Id = "btn1", Type = "Button", Name = "OK", AbsX = 620, AbsY = 560, W = 80, H = 30 }
            };

            var regions = RegionPatternDetector.DetectRegions(nodes, formWidth: 800, formHeight: 600);

            var actionBar = regions.FirstOrDefault(r => r.Name == "ActionBar");
            if (actionBar != null)
                Assert.Contains("btn1", actionBar.NodeIds ?? new List<string>());
        }
    }
}
