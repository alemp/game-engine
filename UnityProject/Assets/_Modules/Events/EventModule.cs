using GameEngine.Core.Config.Schemas;
using GameEngine.Modules.Idle;
using System;
using System.Collections.Generic;

namespace GameEngine.Modules.Events
{
    /// <summary>
    /// Temporary events with production modifiers. Duration-based or manual end.
    /// </summary>
    public sealed class EventModule
    {
        private readonly IdleModule _idleModule;
        private readonly Dictionary<string, EventEntry> _eventsById = new();
        private string _activeEventId;
        private double _activeEventRemainingSeconds;

        public EventModule(IdleModule idleModule)
        {
            _idleModule = idleModule ?? throw new ArgumentNullException(nameof(idleModule));
        }

        public void RegisterEvents(IReadOnlyList<EventEntry> entries)
        {
            if (entries == null)
                return;
            foreach (var e in entries)
            {
                if (!string.IsNullOrEmpty(e.Id))
                    _eventsById[e.Id] = e;
            }
        }

        /// <summary>
        /// Starts an event. Returns false if event not found or another event is active.
        /// </summary>
        public bool StartEvent(string eventId, double elapsedSeconds = 0)
        {
            if (!_eventsById.TryGetValue(eventId, out var evt))
                return false;

            if (!string.IsNullOrEmpty(_activeEventId))
                return false;

            _activeEventId = eventId;
            _activeEventRemainingSeconds = evt.DurationSeconds > 0 ? evt.DurationSeconds - elapsedSeconds : double.MaxValue;

            ApplyEventMultipliers(evt);
            return true;
        }

        /// <summary>
        /// Ends the active event. No-op if none active.
        /// </summary>
        public void EndEvent()
        {
            if (string.IsNullOrEmpty(_activeEventId))
                return;

            _idleModule.ClearEventMultipliers();
            _activeEventId = null;
            _activeEventRemainingSeconds = 0;
        }

        /// <summary>
        /// Advances event timer. Call from game loop. Ends event when duration expires.
        /// </summary>
        public void Tick(double deltaSeconds)
        {
            if (string.IsNullOrEmpty(_activeEventId) || _activeEventRemainingSeconds <= 0)
                return;

            _activeEventRemainingSeconds -= deltaSeconds;
            if (_activeEventRemainingSeconds <= 0)
                EndEvent();
        }

        public string GetActiveEventId()
        {
            return _activeEventId;
        }

        public double GetActiveEventRemainingSeconds()
        {
            return _activeEventRemainingSeconds;
        }

        public EventEntry GetEvent(string eventId)
        {
            return _eventsById.TryGetValue(eventId, out var e) ? e : null;
        }

        public IReadOnlyList<string> GetEventIds()
        {
            return new List<string>(_eventsById.Keys);
        }

        private void ApplyEventMultipliers(EventEntry evt)
        {
            if (evt.ProductionMultipliers == null)
                return;
            foreach (var (productionId, mult) in evt.ProductionMultipliers)
            {
                if (!string.IsNullOrEmpty(productionId) && mult > 0)
                    _idleModule.SetEventMultiplier(productionId, mult);
            }
        }
    }
}
