using System;
using System.IO;
using FormAtlas.Semantic.Contracts;
using Newtonsoft.Json;

namespace FormAtlas.Semantic.IO
{
    /// <summary>
    /// Serializes a SemanticBundle to semantic.json in the specified output directory.
    /// </summary>
    public sealed class SemanticBundleWriter
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include
        };

        /// <summary>
        /// Writes the bundle as semantic.json in the given directory.
        /// </summary>
        public string Write(SemanticBundle bundle, string outputDirectory)
        {
            if (bundle == null) throw new ArgumentNullException(nameof(bundle));
            if (string.IsNullOrWhiteSpace(outputDirectory))
                throw new ArgumentNullException(nameof(outputDirectory));

            Directory.CreateDirectory(outputDirectory);

            var jsonText = JsonConvert.SerializeObject(bundle, Settings);
            var outputPath = Path.Combine(outputDirectory, "semantic.json");
            File.WriteAllText(outputPath, jsonText);
            return outputPath;
        }
    }
}
