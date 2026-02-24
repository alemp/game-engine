using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameEngine.Core.SaveSystem
{
    /// <summary>
    /// Serializable save data. Persisted as JSON.
    /// </summary>
    public sealed class SaveDataSchema
    {
        [JsonProperty("gameId")]
        public string GameId { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; } = 1;

        [JsonProperty("lastSavedUtc")]
        public string LastSavedUtc { get; set; }

        [JsonProperty("scheduler")]
        public SchedulerSaveData Scheduler { get; set; }

        [JsonProperty("resources")]
        public Dictionary<string, BigNumberSaveData> Resources { get; set; }

        [JsonProperty("upgrades")]
        public Dictionary<string, int> Upgrades { get; set; }
    }

    public sealed class SchedulerSaveData
    {
        [JsonProperty("tickCount")]
        public int TickCount { get; set; }

        [JsonProperty("accumulatedTime")]
        public double AccumulatedTime { get; set; }
    }

    public sealed class BigNumberSaveData
    {
        [JsonProperty("mantissa")]
        public double Mantissa { get; set; }

        [JsonProperty("exponent")]
        public int Exponent { get; set; }
    }
}
