using GameEngine.Core.Config.Schemas;
using GameEngine.Core.Economy;
using GameEngine.Modules.Idle;
using System;
using System.Collections.Generic;

namespace GameEngine.Modules.Upgrades
{
    /// <summary>
    /// Handles upgrades: purchase, apply effects to production multipliers.
    /// Linear: cost = base + level * perLevel. Effect = 1 + level * effectValue.
    /// </summary>
    public sealed class UpgradeModule
    {
        private readonly IdleModule _idleModule;
        private readonly Dictionary<string, int> _purchasedLevels = new();
        private readonly Dictionary<string, UpgradeEntry> _upgradesById = new();

        public UpgradeModule(IdleModule idleModule)
        {
            _idleModule = idleModule ?? throw new ArgumentNullException(nameof(idleModule));
        }

        public void RegisterUpgrades(IReadOnlyList<UpgradeEntry> entries)
        {
            if (entries == null)
                return;
            foreach (var e in entries)
            {
                if (!string.IsNullOrEmpty(e.Id))
                    _upgradesById[e.Id] = e;
            }
        }

        public int GetLevel(string upgradeId)
        {
            return _purchasedLevels.TryGetValue(upgradeId, out var level) ? level : 0;
        }

        public bool CanPurchase(string upgradeId)
        {
            if (!_upgradesById.TryGetValue(upgradeId, out var upgrade))
                return false;

            var level = GetLevel(upgradeId);
            if (level >= upgrade.MaxLevel)
                return false;

            if (upgrade.UnlockCondition != null &&
                !string.IsNullOrEmpty(upgrade.UnlockCondition.ResourceId))
            {
                var amount = _idleModule.GetResource(upgrade.UnlockCondition.ResourceId);
                if (amount < BigNumber.FromDouble(upgrade.UnlockCondition.MinAmount))
                    return false;
            }

            var cost = GetCost(upgrade, level);
            return _idleModule.GetResource(upgrade.CostResourceId) >= cost;
        }

        public bool TryPurchase(string upgradeId)
        {
            if (!CanPurchase(upgradeId) || !_upgradesById.TryGetValue(upgradeId, out var upgrade))
                return false;

            var level = GetLevel(upgradeId);
            var cost = GetCost(upgrade, level);

            if (!_idleModule.TrySpend(upgrade.CostResourceId, cost))
                return false;

            _purchasedLevels[upgradeId] = level + 1;
            ApplyEffects();
            return true;
        }

        public void ApplyEffects()
        {
            var modifiers = new Dictionary<string, double>();

            foreach (var (upgradeId, level) in _purchasedLevels)
            {
                if (level <= 0 || !_upgradesById.TryGetValue(upgradeId, out var upgrade))
                    continue;

                if (string.IsNullOrEmpty(upgrade.TargetProductionId))
                    continue;

                var effect = 1.0 + level * upgrade.EffectValue;
                if (modifiers.TryGetValue(upgrade.TargetProductionId, out var current))
                    modifiers[upgrade.TargetProductionId] = current * effect;
                else
                    modifiers[upgrade.TargetProductionId] = effect;
            }

            foreach (var (productionId, modifier) in modifiers)
                _idleModule.SetProductionMultiplier(productionId, modifier);
        }

        public void ApplyPurchasedLevels(IReadOnlyDictionary<string, int> levels)
        {
            if (levels == null)
                return;
            foreach (var (id, level) in levels)
            {
                if (level > 0 && _upgradesById.ContainsKey(id))
                    _purchasedLevels[id] = level;
            }
            ApplyEffects();
        }

        public IReadOnlyDictionary<string, int> GetPurchasedLevels()
        {
            return new Dictionary<string, int>(_purchasedLevels);
        }

        private static BigNumber GetCost(UpgradeEntry upgrade, int level)
        {
            var amount = upgrade.CostAmount + level * upgrade.CostPerLevel;
            return BigNumber.FromDouble(Math.Max(0, amount));
        }
    }
}
