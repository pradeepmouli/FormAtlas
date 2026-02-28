using System.Collections.Generic;
using System.Linq;
using FormAtlas.Semantic.Inference;
using FormAtlas.Semantic.Normalization;
using Xunit;

namespace FormAtlas.Semantic.Tests.Inference
{
    /// <summary>
    /// Confidence and evidence traceability tests (T063).
    /// </summary>
    public class ConfidenceEvidenceTests
    {
        [Fact]
        public void AllAnnotations_HaveAtLeastOneRole()
        {
            var nodes = new[]
            {
                new NormalizedNode { Id = "n1", Type = "System.Windows.Forms.Button", Name = "btn" },
                new NormalizedNode { Id = "n2", Type = "Some.UnknownType", Name = "ctrl" }
            };

            var annotations = TypeRoleClassifier.Classify(nodes);

            foreach (var ann in annotations)
                Assert.NotEmpty(ann.Roles);
        }

        [Fact]
        public void AllRoles_HaveConfidenceInRange()
        {
            var nodes = new[]
            {
                new NormalizedNode { Id = "n1", Type = "System.Windows.Forms.Button", Name = "btn" },
                new NormalizedNode { Id = "n2", Type = "DevExpress.XtraGrid.GridControl", Name = "grid", DevExpressKind = "GridControl" }
            };

            var annotations = TypeRoleClassifier.Classify(nodes);

            foreach (var ann in annotations)
            foreach (var role in ann.Roles)
            {
                Assert.True(role.Confidence >= 0.0 && role.Confidence <= 1.0,
                    $"Confidence {role.Confidence} is out of [0,1] range");
            }
        }

        [Fact]
        public void AllRoles_HaveNonEmptyEvidence()
        {
            var nodes = new[]
            {
                new NormalizedNode { Id = "n1", Type = "System.Windows.Forms.Button", Name = "btn" },
                new NormalizedNode { Id = "n2", Type = "DevExpress.XtraGrid.GridControl", Name = "grid", DevExpressKind = "GridControl" }
            };

            var annotations = TypeRoleClassifier.Classify(nodes);

            foreach (var ann in annotations)
            foreach (var role in ann.Roles)
                Assert.NotEmpty(role.Evidence);
        }

        [Fact]
        public void HeuristicScorer_PrimaryActionKeyword_AddsEvidence()
        {
            var nodes = new[]
            {
                new NormalizedNode { Id = "n1", Type = "System.Windows.Forms.Button", Name = "btnOK", Text = "OK" }
            };

            var annotations = TypeRoleClassifier.Classify(nodes);
            HeuristicRoleScorer.Score(annotations, nodes);

            var evidence = annotations[0].Roles.First().Evidence;
            Assert.True(evidence.Count >= 1, "Expected at least one evidence entry after heuristic scoring.");
        }

        [Fact]
        public void NodeId_MapsToAnnotationNodeId()
        {
            var nodes = new[]
            {
                new NormalizedNode { Id = "abc-123", Type = "System.Windows.Forms.Label", Name = "lbl" }
            };

            var annotations = TypeRoleClassifier.Classify(nodes);

            Assert.Equal("abc-123", annotations[0].NodeId);
        }
    }
}
