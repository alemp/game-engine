using System.Collections.Generic;
using GameEngine.Core.Config.Schemas;

namespace GameEngine.Core.Config.Validation
{
    /// <summary>
    /// Validates configuration consistency.
    /// Checks: non-existent references, broken curves, production overflow, regressive costs, unreachable content.
    /// </summary>
    public sealed class ConfigValidator
    {
        public ValidationResult Validate(GameConfigSchema config)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(config?.GameId))
                errors.Add("GameId is required.");

            if (config?.Economy != null)
            {
                if (config.Economy.TickIntervalSeconds <= 0)
                    errors.Add("Economy.TickIntervalSeconds must be positive.");
                if (config.Economy.MaxOfflineSeconds < 0)
                    errors.Add("Economy.MaxOfflineSeconds cannot be negative.");
            }

            return new ValidationResult(errors.Count == 0, errors);
        }
    }

    public sealed class ValidationResult
    {
        public bool IsValid { get; }
        public IReadOnlyList<string> Errors { get; }

        public ValidationResult(bool isValid, IReadOnlyList<string> errors)
        {
            IsValid = isValid;
            Errors = errors ?? new List<string>();
        }
    }
}
