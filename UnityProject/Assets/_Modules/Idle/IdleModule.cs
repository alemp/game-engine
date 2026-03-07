using System;
using System.Collections.Generic;
using GameEngine.Core.Economy;
using GameEngine.Core.EventBus;
using GameEngine.Core.Scheduler;

namespace GameEngine.Modules.Idle
{
    /// <summary>
    /// Idle game module: production chains, resources, tick-based economy.
    /// Multiple inputs → one output per production.
    /// </summary>
    public sealed class IdleModule
    {
        private readonly EventBus _eventBus;
        private readonly Scheduler _scheduler;
        private readonly Dictionary<string, BigNumber> _resources = new();
        private readonly List<ProductionRule> _productionRules = new();
        private readonly Dictionary<string, double> _productionMultipliers = new();
        private readonly Dictionary<string, double> _eventMultipliers = new();
        private double _prestigeMultiplier = 1.0;
        private double _tierMultiplier = 1.0;
        private double _artifactMultiplier = 1.0;

        public IReadOnlyDictionary<string, BigNumber> Resources => _resources;

        public IdleModule(EventBus eventBus, Scheduler scheduler)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _scheduler.OnTick += OnTick;
        }

        public void RegisterResource(string id, BigNumber initialAmount)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Resource id cannot be null or empty.", nameof(id));
            _resources[id] = initialAmount;
        }

        public void AddProductionRule(ProductionRule rule)
        {
            _productionRules.Add(rule);
        }

        /// <summary>
        /// Clears all production rules. Used for config hot reload.
        /// </summary>
        public void ClearProductionRules()
        {
            _productionRules.Clear();
        }

        /// <summary>
        /// Registers a resource only if it does not exist. Preserves existing amount for hot reload.
        /// </summary>
        public void RegisterResourceIfNew(string id, BigNumber initialAmount)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Resource id cannot be null or empty.", nameof(id));
            if (!_resources.ContainsKey(id))
                _resources[id] = initialAmount;
        }

        public BigNumber GetResource(string id)
        {
            return _resources.TryGetValue(id, out var amount) ? amount : BigNumber.Zero;
        }

        public void AddResource(string id, BigNumber amount)
        {
            if (_resources.TryGetValue(id, out var current))
                _resources[id] = current + amount;
            else
                _resources[id] = amount;
        }

        public bool TrySpend(string id, BigNumber amount)
        {
            if (!_resources.TryGetValue(id, out var current) || current < amount)
                return false;
            _resources[id] = current - amount;
            return true;
        }

        /// <summary>
        /// Sets the upgrade multiplier for a production (e.g. 2.0 = 2x from upgrades).
        /// </summary>
        public void SetProductionMultiplier(string productionId, double modifier)
        {
            _productionMultipliers[productionId] = modifier > 0 ? modifier : 1.0;
        }

        /// <summary>
        /// Gets the current multiplier for a production (upgrade * prestige * tier * artifact * event).
        /// </summary>
        public double GetProductionMultiplier(string productionId)
        {
            var upgrade = _productionMultipliers.TryGetValue(productionId, out var m) ? m : 1.0;
            var eventMult = _eventMultipliers.TryGetValue(productionId, out var em) ? em : 1.0;
            return upgrade * _prestigeMultiplier * _tierMultiplier * _artifactMultiplier * eventMult;
        }

        /// <summary>
        /// Sets the event multiplier for a production. Used by EventModule.
        /// </summary>
        public void SetEventMultiplier(string productionId, double multiplier)
        {
            if (string.IsNullOrEmpty(productionId))
                return;
            _eventMultipliers[productionId] = multiplier > 0 ? multiplier : 1.0;
        }

        /// <summary>
        /// Clears all event multipliers. Called when an event ends.
        /// </summary>
        public void ClearEventMultipliers()
        {
            _eventMultipliers.Clear();
        }

        /// <summary>
        /// Sets the prestige multiplier (applies to all productions). Used by PrestigeModule.
        /// </summary>
        public void SetPrestigeMultiplier(double multiplier)
        {
            _prestigeMultiplier = multiplier > 0 ? multiplier : 1.0;
        }

        /// <summary>
        /// Sets the tier multiplier (applies to all productions). Used by TierModule.
        /// </summary>
        public void SetTierMultiplier(double multiplier)
        {
            _tierMultiplier = multiplier > 0 ? multiplier : 1.0;
        }

        /// <summary>
        /// Sets the artifact multiplier (applies to all productions). Used by ArtifactModule.
        /// </summary>
        public void SetArtifactMultiplier(double multiplier)
        {
            _artifactMultiplier = multiplier > 0 ? multiplier : 1.0;
        }

        /// <summary>
        /// Runs production for the given number of ticks (e.g. for offline progress).
        /// Only tick-triggered productions run; manual productions are excluded.
        /// </summary>
        public void SimulateTicks(int count)
        {
            for (var i = 0; i < count; i++)
            {
                foreach (var rule in _productionRules)
                {
                    if (rule.Trigger == "manual")
                        continue;
                    if (rule.Inputs.Count > 0 && !CanAfford(rule.Inputs))
                        continue;

                    foreach (var (resId, amount) in rule.Inputs)
                        TrySpend(resId, amount);

                    var upgradeMult = GetProductionMultiplier(rule.Id);
                    AddResource(rule.OutputId, rule.GetEffectiveOutput(upgradeMult));
                }
            }
        }

        /// <summary>
        /// Triggers a manual production (tap/click action). Returns true if production ran.
        /// </summary>
        public bool TriggerManualProduction(string productionId)
        {
            if (string.IsNullOrEmpty(productionId))
                return false;

            foreach (var rule in _productionRules)
            {
                if (rule.Id != productionId || rule.Trigger != "manual")
                    continue;

                if (rule.Inputs.Count > 0 && !CanAfford(rule.Inputs))
                    return false;

                foreach (var (resId, amount) in rule.Inputs)
                    TrySpend(resId, amount);

                var upgradeMult = GetProductionMultiplier(rule.Id);
                AddResource(rule.OutputId, rule.GetEffectiveOutput(upgradeMult));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns IDs of manual-triggered productions (for UI tap binding).
        /// </summary>
        public IReadOnlyList<string> GetManualProductionIds()
        {
            var list = new List<string>();
            foreach (var rule in _productionRules)
            {
                if (rule.Trigger == "manual")
                    list.Add(rule.Id);
            }
            return list;
        }

        /// <summary>
        /// Applies saved resources. Only updates resources already registered (ignores removed/unknown).
        /// </summary>
        public void ApplyResources(IReadOnlyDictionary<string, BigNumber> snapshot)
        {
            if (snapshot == null)
                return;
            foreach (var (id, amount) in snapshot)
            {
                if (_resources.ContainsKey(id))
                    _resources[id] = amount;
            }
        }

        /// <summary>
        /// Returns a copy of current resources for save.
        /// </summary>
        public IReadOnlyDictionary<string, BigNumber> GetResourceSnapshot()
        {
            var copy = new Dictionary<string, BigNumber>(_resources);
            return copy;
        }

        /// <summary>
        /// Returns net production per tick for a resource (outputs minus inputs).
        /// Only tick-triggered productions are included.
        /// </summary>
        public BigNumber GetNetProductionPerTick(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
                return BigNumber.Zero;

            var net = BigNumber.Zero;
            foreach (var rule in _productionRules)
            {
                if (rule.Trigger == "manual")
                    continue;
                if (rule.OutputId == resourceId)
                {
                    var mult = GetProductionMultiplier(rule.Id);
                    net = net + rule.GetEffectiveOutput(mult);
                }

                foreach (var (resId, amount) in rule.Inputs)
                {
                    if (resId == resourceId)
                        net = net - amount;
                }
            }
            return net;
        }

        private void OnTick(int tick)
        {
            foreach (var rule in _productionRules)
            {
                if (rule.Trigger == "manual")
                    continue;
                if (rule.Inputs.Count > 0 && !CanAfford(rule.Inputs))
                    continue;

                foreach (var (resId, amount) in rule.Inputs)
                    TrySpend(resId, amount);

                AddResource(rule.OutputId, rule.GetEffectiveOutput(GetProductionMultiplier(rule.Id)));
            }
        }

        private bool CanAfford(IReadOnlyList<(string ResourceId, BigNumber Amount)> inputs)
        {
            if (inputs == null || inputs.Count == 0)
                return true;
            foreach (var (resId, amount) in inputs)
            {
                if (GetResource(resId) < amount)
                    return false;
            }
            return true;
        }

        public void Shutdown()
        {
            _scheduler.OnTick -= OnTick;
        }
    }

    public sealed class ProductionRule
    {
        public string Id { get; }
        public IReadOnlyList<(string ResourceId, BigNumber Amount)> Inputs { get; }
        public string OutputId { get; }
        public BigNumber OutputAmount { get; }
        public double BaseMultiplier { get; }
        public string Trigger { get; }

        public ProductionRule(
            string id,
            IReadOnlyList<(string ResourceId, BigNumber Amount)> inputs,
            string outputId,
            BigNumber outputAmount,
            double baseMultiplier = 1.0,
            string trigger = "tick")
        {
            Id = id ?? outputId ?? throw new ArgumentNullException(nameof(id));
            Inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
            OutputId = outputId ?? throw new ArgumentNullException(nameof(outputId));
            OutputAmount = outputAmount;
            BaseMultiplier = baseMultiplier > 0 ? baseMultiplier : 1.0;
            Trigger = string.Equals(trigger, "manual", StringComparison.OrdinalIgnoreCase) ? "manual" : "tick";
        }

        /// <summary>
        /// Effective output after applying base multiplier and optional upgrade modifier.
        /// </summary>
        public BigNumber GetEffectiveOutput(double upgradeMultiplier = 1.0) =>
            OutputAmount * BigNumber.FromDouble(BaseMultiplier * upgradeMultiplier);
    }
}
