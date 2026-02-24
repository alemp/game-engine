using GameEngine.Core.Config.Schemas;
using GameEngine.Core.Economy;
using GameEngine.Modules.Idle;
using GameEngine.Modules.Upgrades;
using System;
using System.Collections.Generic;

namespace GameEngine.Modules.Quests
{
    /// <summary>
    /// Quests: objectives, progress tracking, rewards.
    /// </summary>
    public sealed class QuestModule
    {
        private readonly IdleModule _idleModule;
        private readonly UpgradeModule _upgradeModule;
        private readonly Dictionary<string, QuestEntry> _questsById = new();
        private readonly HashSet<string> _completedQuestIds = new();

        private readonly Prestige.PrestigeModule _prestigeModule;

        public QuestModule(IdleModule idleModule, UpgradeModule upgradeModule, Prestige.PrestigeModule prestigeModule = null)
        {
            _idleModule = idleModule ?? throw new ArgumentNullException(nameof(idleModule));
            _upgradeModule = upgradeModule ?? throw new ArgumentNullException(nameof(upgradeModule));
            _prestigeModule = prestigeModule;
        }

        public void RegisterQuests(IReadOnlyList<QuestEntry> entries)
        {
            if (entries == null)
                return;
            foreach (var q in entries)
            {
                if (!string.IsNullOrEmpty(q.Id))
                    _questsById[q.Id] = q;
            }
        }

        public void SetCompletedQuests(IEnumerable<string> questIds)
        {
            _completedQuestIds.Clear();
            if (questIds != null)
            {
                foreach (var id in questIds)
                    _completedQuestIds.Add(id);
            }
        }

        public bool IsCompleted(string questId)
        {
            return _completedQuestIds.Contains(questId);
        }

        public double GetProgress(string questId)
        {
            if (!_questsById.TryGetValue(questId, out var quest) || _completedQuestIds.Contains(questId))
                return 1.0;

            return quest.ObjectiveType?.ToLowerInvariant() switch
            {
                "reach_amount" => GetReachAmountProgress(quest),
                "buy_upgrade" => GetBuyUpgradeProgress(quest),
                "prestige" => _prestigeModule != null && _prestigeModule.GetPrestigeCurrency().ToDouble() >= quest.TargetAmount ? 1.0 : 0,
                _ => 0
            };
        }

        public bool TryClaimReward(string questId)
        {
            if (!_questsById.TryGetValue(questId, out var quest) || _completedQuestIds.Contains(questId))
                return false;

            if (GetProgress(questId) < 1.0)
                return false;

            _completedQuestIds.Add(questId);

            if (!string.IsNullOrEmpty(quest.RewardResourceId) && quest.RewardAmount > 0)
                _idleModule.AddResource(quest.RewardResourceId, BigNumber.FromDouble(quest.RewardAmount));

            return true;
        }

        public IReadOnlyList<string> GetQuestIds()
        {
            return new List<string>(_questsById.Keys);
        }

        public QuestEntry GetQuest(string questId)
        {
            return _questsById.TryGetValue(questId, out var q) ? q : null;
        }

        public IReadOnlyCollection<string> GetCompletedQuestIds()
        {
            return new HashSet<string>(_completedQuestIds);
        }

        private double GetReachAmountProgress(QuestEntry quest)
        {
            var current = _idleModule.GetResource(quest.TargetResourceId ?? "").ToDouble();
            var target = quest.TargetAmount;
            return target <= 0 ? 1.0 : Math.Min(1.0, current / target);
        }

        private double GetBuyUpgradeProgress(QuestEntry quest)
        {
            var level = _upgradeModule?.GetLevel(quest.TargetUpgradeId ?? "") ?? 0;
            var target = quest.TargetLevel;
            return target <= 0 ? 1.0 : Math.Min(1.0, (double)level / target);
        }
    }
}
