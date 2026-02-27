using System;
using System.IO;
using FormAtlas.Tool.Contracts;
using Newtonsoft.Json;

namespace FormAtlas.Tool.Exporter
{
    /// <summary>
    /// Serializes a UiDumpBundle to form.json in the specified output directory.
    /// Emits schemaVersion as a required root field.
    /// </summary>
    public sealed class UiDumpBundleWriter
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include,
            DefaultValueHandling = DefaultValueHandling.Include
        };

        /// <summary>
        /// Writes the bundle as form.json in the given directory.
        /// </summary>
        public string Write(UiDumpBundle bundle, string outputDirectory)
        {
            if (bundle == null) throw new ArgumentNullException(nameof(bundle));
            if (string.IsNullOrWhiteSpace(outputDirectory))
                throw new ArgumentNullException(nameof(outputDirectory));

            Directory.CreateDirectory(outputDirectory);

            var jsonText = JsonConvert.SerializeObject(bundle, Settings);
            var outputPath = Path.Combine(outputDirectory, "form.json");
            File.WriteAllText(outputPath, jsonText);
            return outputPath;
        }
    }
}
