using FormAtlas.Semantic.Normalization;
using Newtonsoft.Json.Linq;
using Xunit;

namespace FormAtlas.Semantic.Tests.Normalization
{
    /// <summary>
    /// Tests for the semantic pipeline normalization phase (T061).
    /// </summary>
    public class NormalizationTests
    {
        [Fact]
        public void Normalize_SingleNode_ProducesOneEntry()
        {
            var nodes = JArray.Parse(@"[{
                ""id"": ""n1"", ""type"": ""System.Windows.Forms.Form"",
                ""name"": ""Root"", ""bounds"": {""x"":0,""y"":0,""w"":800,""h"":600},
                ""children"": []
            }]");

            var result = FeatureNormalizer.Normalize(nodes);

            Assert.Single(result);
            Assert.Equal("n1", result[0].Id);
        }

        [Fact]
        public void Normalize_ChildNode_ComputesAbsoluteBounds()
        {
            var nodes = JArray.Parse(@"[{
                ""id"": ""parent"", ""type"": ""Panel"", ""name"": ""P"",
                ""bounds"": {""x"":10,""y"":20,""w"":500,""h"":400},
                ""children"": [{
                    ""id"": ""child"", ""type"": ""Button"", ""name"": ""B"",
                    ""bounds"": {""x"":5,""y"":5,""w"":80,""h"":30},
                    ""children"": []
                }]
            }]");

            var result = FeatureNormalizer.Normalize(nodes);

            Assert.Equal(2, result.Count);
            var child = result[1];
            Assert.Equal(15, child.AbsX); // 10+5
            Assert.Equal(25, child.AbsY); // 20+5
        }

        [Fact]
        public void Normalize_DevExpressKind_IsExtracted()
        {
            var nodes = JArray.Parse(@"[{
                ""id"": ""grid1"", ""type"": ""DevExpress.XtraGrid.GridControl"",
                ""name"": ""gridControl1"",
                ""bounds"": {""x"":0,""y"":0,""w"":800,""h"":500},
                ""metadata"": { ""devexpress"": { ""kind"": ""GridControl"" } },
                ""children"": []
            }]");

            var result = FeatureNormalizer.Normalize(nodes);

            Assert.Single(result);
            Assert.Equal("GridControl", result[0].DevExpressKind);
        }

        [Fact]
        public void Normalize_EmptyNodes_ReturnsEmpty()
        {
            var nodes = new JArray();
            var result = FeatureNormalizer.Normalize(nodes);
            Assert.Empty(result);
        }
    }
}
