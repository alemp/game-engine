using GameEngine.Core.Config.Schemas;
using GameEngine.Modules.Idle;
using System;
using System.Collections.Generic;

namespace GameEngine.Modules.Artifacts
{
    /// <summary>
    /// Collectible artifacts that provide passive bonuses (e.g. production multiplier).
    /// </summary>
    public sealed class ArtifactModule
    {
        private readonly IdleModule _idleModule;
        private readonly Dictionary<string, ArtifactEntry> _artifactsById = new();
        private readonly HashSet<string> _collectedIds = new();

        public ArtifactModule(IdleModule idleModule)
        {
            _idleModule = idleModule ?? throw new ArgumentNullException(nameof(idleModule));
        }

        public void RegisterArtifacts(IReadOnlyList<ArtifactEntry> entries)
        {
            _artifactsById.Clear();
            if (entries == null)
                return;

            foreach (var e in entries)
            {
                if (!string.IsNullOrEmpty(e.Id))
                    _artifactsById[e.Id] = e;
            }
        }

        public bool IsCollected(string artifactId)
        {
            return _collectedIds.Contains(artifactId);
        }

        public IReadOnlyCollection<string> GetCollectedIds()
        {
            return _collectedIds;
        }

        /// <summary>
        /// Collects an artifact. Returns true if newly collected.
        /// </summary>
        public bool CollectArtifact(string artifactId)
        {
            if (string.IsNullOrEmpty(artifactId) || !_artifactsById.ContainsKey(artifactId))
                return false;

            if (_collectedIds.Add(artifactId))
            {
                ApplyEffects();
                return true;
            }
            return false;
        }

        public void SetCollectedIds(IReadOnlyList<string> ids)
        {
            _collectedIds.Clear();
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    if (_artifactsById.ContainsKey(id))
                        _collectedIds.Add(id);
                }
            }
            ApplyEffects();
        }

        public ArtifactEntry GetArtifact(string artifactId)
        {
            return _artifactsById.TryGetValue(artifactId, out var a) ? a : null;
        }

        private void ApplyEffects()
        {
            var mult = 1.0;
            foreach (var id in _collectedIds)
            {
                if (!_artifactsById.TryGetValue(id, out var artifact))
                    continue;
                if (string.Equals(artifact.EffectType, "production_multiplier", StringComparison.OrdinalIgnoreCase) &&
                    artifact.EffectValue > 0)
                {
                    mult *= artifact.EffectValue;
                }
            }
            _idleModule.SetArtifactMultiplier(mult);
        }
    }
}
