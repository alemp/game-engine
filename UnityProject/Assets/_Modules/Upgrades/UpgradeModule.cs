using GameEngine.Core.Config.Schemas;
using GameEngine.Core.Economy;
using GameEngine.Modules.Idle;
using System;
using System.Collections.Generic;

namespace GameEngine.Modules.Upgrades
{
    /// <summary>
    /// Handles upgrades: purchase, apply effects to production multipliers.
    /// Supports linear and exponential curves for cost and effect.
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

        public BigNumber GetCostForNextLevel(string upgradeId)
        {
            if (!_upgradesById.TryGetValue(upgradeId, out var upgrade))
                return BigNumber.Zero;
            var level = GetLevel(upgradeId);
            return GetCost(upgrade, level);
        }

        public IReadOnlyList<string> GetUpgradeIds()
        {
            var list = new List<string>(_upgradesById.Count);
            foreach (var id in _upgradesById.Keys)
                list.Add(id);
            return list;
        }

        public UpgradeEntry GetUpgrade(string upgradeId)
        {
            return _upgradesById.TryGetValue(upgradeId, out var upgrade) ? upgrade : null;
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

                var effect = GetEffect(upgrade, level);
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
            var formula = upgrade.CostFormula ?? "linear";
            if (string.Equals(formula, "exponential", StringComparison.OrdinalIgnoreCase))
            {
                var mult = upgrade.CostMultiplier > 0 ? upgrade.CostMultiplier : 1.0;
                var baseAmount = BigNumber.FromDouble(Math.Max(0, upgrade.CostAmount));
                var pow = BigNumber.Pow(BigNumber.FromDouble(mult), level);
                return baseAmount * pow;
            }

            var amount = upgrade.CostAmount + level * upgrade.CostPerLevel;
            return BigNumber.FromDouble(Math.Max(0, amount));
        }

        private static double GetEffect(UpgradeEntry upgrade, int level)
        {
            var formula = upgrade.EffectFormula ?? "linear";
            if (string.Equals(formula, "exponential", StringComparison.OrdinalIgnoreCase))
            {
                var mult = upgrade.EffectMultiplier > 0 ? upgrade.EffectMultiplier : 1.0;
                return Math.Pow(mult, level);
            }

            return 1.0 + level * upgrade.EffectValue;
        }
    }
}
