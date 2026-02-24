using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GameEngine.Core.Config
{
    /// <summary>
    /// Loads game configuration from JSON files.
    /// Supports editor hot-reload during development.
    /// </summary>
    public sealed class ConfigLoader
    {
        private readonly string _basePath;

        public ConfigLoader(string basePath)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }

        public async Task<string> LoadJsonAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            var fullPath = Path.Combine(_basePath, relativePath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Config file not found: {fullPath}", fullPath);

            return await File.ReadAllTextAsync(fullPath, cancellationToken).ConfigureAwait(false);
        }

        public string LoadJson(string relativePath)
        {
            var fullPath = Path.Combine(_basePath, relativePath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Config file not found: {fullPath}", fullPath);

            return File.ReadAllText(fullPath);
        }

        public bool Exists(string relativePath)
        {
            var fullPath = Path.Combine(_basePath, relativePath);
            return File.Exists(fullPath);
        }
    }
}
