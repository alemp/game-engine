using GameEngine.Core.Config.Schemas;
using GameEngine.Core.Economy;
using GameEngine.Modules.Idle;
using GameEngine.Modules.Upgrades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Modules.Prestige
{
    /// <summary>
    /// Prestige: partial reset, earn special currency, apply permanent boost.
    /// </summary>
    public sealed class PrestigeModule
    {
        private readonly IdleModule _idleModule;
        private readonly UpgradeModule _upgradeModule;
        private PrestigeSchema _config;
        private BigNumber _prestigeCurrency;

        public PrestigeModule(IdleModule idleModule, UpgradeModule upgradeModule)
        {
            _idleModule = idleModule ?? throw new ArgumentNullException(nameof(idleModule));
            _upgradeModule = upgradeModule ?? throw new ArgumentNullException(nameof(upgradeModule));
        }

        public void Configure(PrestigeSchema config)
        {
            _config = config;
        }

        public string GetCurrencyResourceId()
        {
            return _config?.CurrencyResourceId ?? "souls";
        }

        public BigNumber GetPrestigeCurrency()
        {
            return _prestigeCurrency;
        }

        public void SetPrestigeCurrency(BigNumber amount)
        {
            _prestigeCurrency = amount;
            ApplyBoost();
        }

        public bool CanPrestige()
        {
            if (_config == null || string.IsNullOrEmpty(_config.CurrencyResourceId))
                return false;

            var sum = GetSourceResourceSum();
            return sum.ToDouble() >= _config.MinResourceValue;
        }

        public double GetPrestigeCurrencyEarned()
        {
            var sum = GetSourceResourceSum();
            return CalculatePrestigeAmount(sum.ToDouble());
        }

        public bool TryPrestige(Action onSchedulerReset)
        {
            if (!CanPrestige())
                return false;

            var earned = CalculatePrestigeAmount(GetSourceResourceSum().ToDouble());
            _prestigeCurrency = _prestigeCurrency + BigNumber.FromDouble(earned);

            ResetResources();
            _upgradeModule?.ApplyPurchasedLevels(new Dictionary<string, int>());
            onSchedulerReset?.Invoke();

            ApplyBoost();
            return true;
        }

        public double GetBoostMultiplier()
        {
            var perUnit = _config?.BoostPerUnit ?? 0;
            return 1.0 + _prestigeCurrency.ToDouble() * perUnit;
        }

        private BigNumber GetSourceResourceSum()
        {
            var snapshot = _idleModule.GetResourceSnapshot();
            var currencyId = _config?.CurrencyResourceId;
            var sourceIds = _config?.SourceResourceIds;

            BigNumber sum = BigNumber.Zero;
            foreach (var (id, amount) in snapshot)
            {
                if (id == currencyId)
                    continue;
                if (sourceIds != null && sourceIds.Length > 0 && !sourceIds.Contains(id))
                    continue;
                sum = sum + amount;
            }
            return sum;
        }

        private double CalculatePrestigeAmount(double resourceSum)
        {
            if (_config == null || resourceSum <= 0)
                return 0;

            return _config.Formula?.ToLowerInvariant() switch
            {
                "sqrt" => Math.Sqrt(resourceSum),
                "log" => Math.Log10(resourceSum + 1),
                "linear" => resourceSum * _config.FormulaFactor,
                _ => Math.Sqrt(resourceSum)
            };
        }

        private void ResetResources()
        {
            var currencyId = _config?.CurrencyResourceId;
            var resources = _idleModule.GetResourceSnapshot();
            var initial = new Dictionary<string, BigNumber>();
            foreach (var (id, _) in resources)
                initial[id] = id == currencyId ? _prestigeCurrency : BigNumber.Zero;
            _idleModule.ApplyResources(initial);
        }

        private void ApplyBoost()
        {
            if (_config == null)
                return;

            _idleModule.SetPrestigeMultiplier(GetBoostMultiplier());
        }
    }
}
