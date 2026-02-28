using System;

namespace FormAtlas.Tool.Contracts
{
    /// <summary>
    /// Implements the interop schema version compatibility policy for UI dump bundles.
    /// Consumers accept same MAJOR version; reject higher MAJOR by default.
    /// </summary>
    public static class SchemaVersionPolicy
    {
        public const string CurrentVersion = "1.0";

        /// <summary>
        /// Returns true when the bundle's schema version is compatible with the current consumer.
        /// </summary>
        public static bool IsCompatible(string bundleVersion, bool allowHigherMajor = false)
        {
            if (string.IsNullOrWhiteSpace(bundleVersion))
                return false;

            if (!TryParse(bundleVersion, out int bundleMajor, out int bundleMinor))
                return false;

            if (!TryParse(CurrentVersion, out int currentMajor, out _))
                return false;

            if (bundleMajor > currentMajor)
                return allowHigherMajor;

            // Same MAJOR, any MINOR accepted (higher minor is fine per contract)
            return bundleMajor == currentMajor;
        }

        /// <summary>
        /// Validates the bundle schema version and throws if incompatible.
        /// </summary>
        public static void Validate(string bundleVersion, bool allowHigherMajor = false)
        {
            if (!IsCompatible(bundleVersion, allowHigherMajor))
                throw new InvalidOperationException(
                    $"Bundle schemaVersion '{bundleVersion}' is incompatible with consumer version '{CurrentVersion}'.");
        }

        private static bool TryParse(string version, out int major, out int minor)
        {
            major = 0;
            minor = 0;
            if (string.IsNullOrWhiteSpace(version))
                return false;

            var parts = version.Split('.');
            if (parts.Length < 2)
                return false;

            return int.TryParse(parts[0], out major) && int.TryParse(parts[1], out minor);
        }
    }
}
