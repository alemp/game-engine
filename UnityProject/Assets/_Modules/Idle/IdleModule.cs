using GameEngine.Core.Economy;
using GameEngine.Core.EventBus;
using GameEngine.Core.Scheduler;
using System;
using System.Collections.Generic;

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

        private void OnTick(int tick)
        {
            foreach (var rule in _productionRules)
            {
                if (rule.Inputs.Count > 0 && !CanAfford(rule.Inputs))
                    continue;

                foreach (var (resId, amount) in rule.Inputs)
                    TrySpend(resId, amount);

                AddResource(rule.OutputId, rule.OutputAmount);
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
        public IReadOnlyList<(string ResourceId, BigNumber Amount)> Inputs { get; }
        public string OutputId { get; }
        public BigNumber OutputAmount { get; }

        public ProductionRule(
            IReadOnlyList<(string ResourceId, BigNumber Amount)> inputs,
            string outputId,
            BigNumber outputAmount)
        {
            Inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
            OutputId = outputId ?? throw new ArgumentNullException(nameof(outputId));
            OutputAmount = outputAmount;
        }
    }
}
