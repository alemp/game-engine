using GameEngine.Core.Config.Schemas;
using GameEngine.Core.Economy;
using GameEngine.Modules.Idle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Modules.RandomRewards
{
    /// <summary>
    /// Periodic/chance-based rewards (e.g. drones, trucks). Grants resources at configurable intervals.
    /// </summary>
    public sealed class RandomRewardModule
    {
        private readonly IdleModule _idleModule;
        private readonly Dictionary<string, RandomRewardEntry> _entriesById = new();
        private readonly Dictionary<string, double> _accumulatedTime = new();
        private readonly Random _random = new();

        public RandomRewardModule(IdleModule idleModule)
        {
            _idleModule = idleModule ?? throw new ArgumentNullException(nameof(idleModule));
        }

        public void RegisterRewards(IReadOnlyList<RandomRewardEntry> entries)
        {
            if (entries == null)
                return;

            _entriesById.Clear();
            _accumulatedTime.Clear();

            foreach (var e in entries)
            {
                if (!string.IsNullOrEmpty(e.Id) && e.RewardOptions != null && e.RewardOptions.Count > 0)
                    _entriesById[e.Id] = e;
            }
        }

        /// <summary>
        /// Advances timers and grants rewards when intervals elapse. Call from game loop.
        /// </summary>
        public void Tick(double deltaSeconds)
        {
            foreach (var (id, entry) in _entriesById)
            {
                var accumulated = _accumulatedTime.TryGetValue(id, out var t) ? t : 0;
                accumulated += deltaSeconds;
                var interval = GetNextInterval(entry);

                while (accumulated >= interval)
                {
                    GrantReward(entry);
                    accumulated -= interval;
                    interval = GetNextInterval(entry);
                }

                _accumulatedTime[id] = accumulated;
            }
        }

        private double GetNextInterval(RandomRewardEntry entry)
        {
            var baseInterval = entry.IntervalSeconds > 0 ? entry.IntervalSeconds : 120;
            var jitter = entry.JitterSeconds;
            if (jitter <= 0)
                return baseInterval;

            var offset = (_random.NextDouble() * 2 - 1) * jitter;
            return Math.Max(1, baseInterval + offset);
        }

        private void GrantReward(RandomRewardEntry entry)
        {
            var totalWeight = entry.RewardOptions.Sum(r => r.Weight > 0 ? r.Weight : 1);
            if (totalWeight <= 0)
                return;

            var roll = _random.NextDouble() * totalWeight;
            foreach (var opt in entry.RewardOptions)
            {
                var w = opt.Weight > 0 ? opt.Weight : 1;
                roll -= w;
                if (roll <= 0)
                {
                    if (!string.IsNullOrEmpty(opt.ResourceId) && opt.Amount > 0)
                    {
                        _idleModule.AddResource(opt.ResourceId, BigNumber.FromDouble(opt.Amount));
                    }
                    return;
                }
            }

            var fallback = entry.RewardOptions.FirstOrDefault();
            if (fallback != null && !string.IsNullOrEmpty(fallback.ResourceId) && fallback.Amount > 0)
            {
                _idleModule.AddResource(fallback.ResourceId, BigNumber.FromDouble(fallback.Amount));
            }
        }
    }
}
