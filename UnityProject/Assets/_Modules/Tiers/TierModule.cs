using GameEngine.Core.Config.Schemas;
using GameEngine.Core.Economy;
using GameEngine.Modules.Idle;
using GameEngine.Modules.Upgrades;
using System;
using System.Collections.Generic;
namespace GameEngine.Modules.Tiers
{
    /// <summary>
    /// Progression tiers: ascend to next tier at threshold. Higher tier = higher production multiplier.
    /// Ascending resets resources and upgrades (except persisted), similar to prestige.
    /// </summary>
    public sealed class TierModule
    {
        private readonly IdleModule _idleModule;
        private readonly UpgradeModule _upgradeModule;
        private List<TierEntry> _tiers = new();
        private int _currentTierIndex;
        private HashSet<string> _persistedResourceIds = new();
        private HashSet<string> _persistedUpgradeIds = new();

        public TierModule(IdleModule idleModule, UpgradeModule upgradeModule)
        {
            _idleModule = idleModule ?? throw new ArgumentNullException(nameof(idleModule));
            _upgradeModule = upgradeModule ?? throw new ArgumentNullException(nameof(upgradeModule));
        }

        public void RegisterTiers(IReadOnlyList<TierEntry> entries)
        {
            _tiers = entries != null && entries.Count > 0
                ? new List<TierEntry>(entries)
                : new List<TierEntry>();
            ApplyCurrentTierMultiplier();
        }

        public void SetPersistedResourceIds(IReadOnlyList<string> ids)
        {
            _persistedResourceIds = ids != null ? new HashSet<string>(ids) : new HashSet<string>();
        }

        public void SetPersistedUpgradeIds(IReadOnlyList<string> ids)
        {
            _persistedUpgradeIds = ids != null ? new HashSet<string>(ids) : new HashSet<string>();
        }

        public int CurrentTierIndex => _currentTierIndex;

        public string CurrentTierId => _currentTierIndex >= 0 && _currentTierIndex < _tiers.Count
            ? _tiers[_currentTierIndex].Id
            : null;

        public TierEntry GetCurrentTier()
        {
            return _currentTierIndex >= 0 && _currentTierIndex < _tiers.Count
                ? _tiers[_currentTierIndex]
                : null;
        }

        public TierEntry GetNextTier()
        {
            var next = _currentTierIndex + 1;
            return next < _tiers.Count ? _tiers[next] : null;
        }

        public bool CanAscend()
        {
            var next = GetNextTier();
            if (next == null || string.IsNullOrEmpty(next.UnlockResourceId))
                return false;

            var amount = _idleModule.GetResource(next.UnlockResourceId);
            return amount >= BigNumber.FromDouble(next.UnlockMinAmount);
        }

        /// <summary>
        /// Ascends to next tier. Resets resources, upgrades, scheduler. Returns true on success.
        /// </summary>
        public bool TryAscend(Action onSchedulerReset)
        {
            if (!CanAscend())
                return false;

            _currentTierIndex++;
            ApplyCurrentTierMultiplier();
            ResetResources();
            ApplyUpgradeReset();
            onSchedulerReset?.Invoke();
            return true;
        }

        public void SetTierIndex(int index)
        {
            _currentTierIndex = Math.Max(0, Math.Min(index, _tiers.Count > 0 ? _tiers.Count - 1 : 0));
            ApplyCurrentTierMultiplier();
        }

        private void ApplyCurrentTierMultiplier()
        {
            var tier = GetCurrentTier();
            _idleModule.SetTierMultiplier(tier?.ProductionMultiplier ?? 1.0);
        }

        private void ResetResources()
        {
            var resources = _idleModule.GetResourceSnapshot();
            var initial = new Dictionary<string, BigNumber>();
            foreach (var (id, amount) in resources)
            {
                initial[id] = _persistedResourceIds.Contains(id) ? amount : BigNumber.Zero;
            }
            _idleModule.ApplyResources(initial);
        }

        private void ApplyUpgradeReset()
        {
            if (_upgradeModule == null)
                return;

            var current = _upgradeModule.GetPurchasedLevels();
            var levelsToApply = new Dictionary<string, int>();
            foreach (var (id, level) in current)
            {
                levelsToApply[id] = _persistedUpgradeIds.Contains(id) ? level : 0;
            }
            _upgradeModule.ApplyPurchasedLevels(levelsToApply);
        }
    }
}
