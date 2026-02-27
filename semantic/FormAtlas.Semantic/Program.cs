using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FormAtlas.Semantic.Contracts;
using FormAtlas.Semantic.Inference;
using FormAtlas.Semantic.IO;
using FormAtlas.Semantic.Normalization;
using Newtonsoft.Json.Linq;

namespace FormAtlas.Semantic
{
    /// <summary>
    /// Entry point for the FormAtlas semantic transformer.
    /// Usage: FormAtlas.Semantic &lt;form.json&gt; [output-dir]
    /// </summary>
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Usage: FormAtlas.Semantic <form.json> [output-dir]");
                return 1;
            }

            var inputPath = args[0];
            var outputDir = args.Length >= 2 ? args[1] : Path.GetDirectoryName(inputPath) ?? ".";

            try
            {
                var reader = new UiDumpBundleReader();
                var bundle = reader.ReadFromFile(inputPath);

                var formNode = bundle["form"] as JObject
                    ?? throw new InvalidOperationException("Missing 'form' in bundle.");

                var nodesArray = bundle["nodes"] as JArray ?? new JArray();
                var schemaVersion = bundle["schemaVersion"]?.Value<string>() ?? "1.0";

                // Normalize
                var normalized = FeatureNormalizer.Normalize(nodesArray);

                // Classify roles
                var annotations = TypeRoleClassifier.Classify(normalized);

                // Apply heuristics
                HeuristicRoleScorer.Score(annotations, normalized);

                // Detect regions and patterns
                int formW = formNode["width"]?.Value<int>() ?? 0;
                int formH = formNode["height"]?.Value<int>() ?? 0;
                var regions = RegionPatternDetector.DetectRegions(normalized, formW, formH);
                var patterns = RegionPatternDetector.DetectPatterns(normalized, annotations);

                // Build semantic bundle
                var semantic = new SemanticBundle
                {
                    SemanticVersion = "1.0",
                    SourceSchemaVersion = schemaVersion,
                    Form = new SemanticFormInfo
                    {
                        Name = formNode["name"]?.Value<string>() ?? string.Empty,
                        Type = formNode["type"]?.Value<string>() ?? string.Empty,
                        Width = formW,
                        Height = formH,
                        Dpi = formNode["dpi"]?.Value<int?>()
                    },
                    Annotations = annotations,
                    Regions = regions.Count > 0 ? regions : null,
                    Patterns = patterns.Count > 0 ? patterns : null
                };

                var writer = new SemanticBundleWriter();
                var outputPath = writer.Write(semantic, outputDir);
                Console.WriteLine($"[FormAtlas.Semantic] Written to: {outputPath}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[FormAtlas.Semantic] Error: {ex.Message}");
                return 2;
            }
        }
    }
}
