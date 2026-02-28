using System.Linq;
using FormAtlas.Semantic.Inference;
using FormAtlas.Semantic.Normalization;
using Xunit;

namespace FormAtlas.Semantic.Tests.Inference
{
    /// <summary>
    /// Role inference tests for WinForms and DevExpress types (T062).
    /// </summary>
    public class RoleInferenceTests
    {
        [Theory]
        [InlineData("System.Windows.Forms.Button", "Action", 0.90)]
        [InlineData("System.Windows.Forms.TextBox", "InputField", 0.90)]
        [InlineData("System.Windows.Forms.Label", "Label", 0.85)]
        [InlineData("System.Windows.Forms.ComboBox", "SelectField", 0.85)]
        [InlineData("System.Windows.Forms.Form", "FormRoot", 0.95)]
        public void WinFormsType_ProducesExpectedRole(string typeName, string expectedRole, double minConfidence)
        {
            var nodes = new[] { new NormalizedNode { Id = "n1", Type = typeName, Name = "ctrl" } };

            var annotations = TypeRoleClassifier.Classify(nodes);

            Assert.Single(annotations);
            var role = annotations[0].Roles.First();
            Assert.Equal(expectedRole, role.Role);
            Assert.True(role.Confidence >= minConfidence,
                $"Confidence {role.Confidence} < {minConfidence} for {typeName}");
        }

        [Theory]
        [InlineData("GridControl", "DataGrid", 0.90)]
        [InlineData("PivotGridControl", "PivotTable", 0.90)]
        [InlineData("XtraTabControl", "TabContainer", 0.90)]
        [InlineData("LayoutControl", "LayoutContainer", 0.85)]
        [InlineData("RibbonControl", "Ribbon", 0.90)]
        [InlineData("BarManager", "Toolbar", 0.85)]
        public void DevExpressKind_ProducesExpectedRole(string kind, string expectedRole, double minConfidence)
        {
            var nodes = new[] { new NormalizedNode { Id = "n1", Type = "DevExpress.SomeType", Name = "ctrl", DevExpressKind = kind } };

            var annotations = TypeRoleClassifier.Classify(nodes);

            Assert.Single(annotations);
            var role = annotations[0].Roles.First();
            Assert.Equal(expectedRole, role.Role);
            Assert.True(role.Confidence >= minConfidence);
        }

        [Fact]
        public void UnknownType_ProducesUnknownRole()
        {
            var nodes = new[] { new NormalizedNode { Id = "n1", Type = "Some.Unknown.CustomControl", Name = "ctrl" } };

            var annotations = TypeRoleClassifier.Classify(nodes);

            Assert.Single(annotations);
            Assert.Equal("Unknown", annotations[0].Roles.First().Role);
        }

        [Fact]
        public void DevExpressAnnotation_IncludesKindEvidence()
        {
            var nodes = new[] { new NormalizedNode { Id = "n1", Type = "DevExpress.XtraGrid.GridControl", Name = "grid", DevExpressKind = "GridControl" } };

            var annotations = TypeRoleClassifier.Classify(nodes);

            var evidence = annotations[0].Roles.First().Evidence;
            Assert.Contains(evidence, e => e.Contains("GridControl"));
        }
    }
}
