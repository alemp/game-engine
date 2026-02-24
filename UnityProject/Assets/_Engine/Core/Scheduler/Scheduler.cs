using System;

namespace GameEngine.Core.Scheduler
{
    /// <summary>
    /// Manages ticks for idle economy. Call Tick(deltaTime) each frame.
    /// Tick rate is configurable per game.
    /// </summary>
    public sealed class Scheduler
    {
        private readonly double _tickIntervalSeconds;
        private double _accumulatedTime;
        private int _tickCount;

        public int TickCount => _tickCount;
        public double AccumulatedTime => _accumulatedTime;
        public double TickIntervalSeconds => _tickIntervalSeconds;

        public event Action<int> OnTick;

        public Scheduler(double tickIntervalSeconds = 1.0)
        {
            if (tickIntervalSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(tickIntervalSeconds), "Must be positive.");
            _tickIntervalSeconds = tickIntervalSeconds;
        }

        /// <summary>
        /// Call each frame with delta time. Fires OnTick when a full tick interval has elapsed.
        /// </summary>
        public void Tick(double deltaTimeSeconds)
        {
            _accumulatedTime += deltaTimeSeconds;

            while (_accumulatedTime >= _tickIntervalSeconds)
            {
                _accumulatedTime -= _tickIntervalSeconds;
                _tickCount++;
                OnTick?.Invoke(_tickCount);
            }
        }

        /// <summary>
        /// Calculates how many ticks would have occurred in the given elapsed seconds (e.g. offline time).
        /// </summary>
        public int GetTicksForElapsedSeconds(double elapsedSeconds)
        {
            if (elapsedSeconds <= 0)
                return 0;
            return (int)Math.Floor(elapsedSeconds / _tickIntervalSeconds);
        }

        public void Reset()
        {
            _accumulatedTime = 0;
            _tickCount = 0;
        }

        /// <summary>
        /// Restores scheduler state from save (tick count and accumulated time).
        /// </summary>
        public void SetState(int tickCount, double accumulatedTime)
        {
            _tickCount = Math.Max(0, tickCount);
            _accumulatedTime = Math.Max(0, Math.Min(accumulatedTime, _tickIntervalSeconds));
        }
    }
}
