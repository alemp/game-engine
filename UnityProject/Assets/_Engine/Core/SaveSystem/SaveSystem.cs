using System;
using System.Collections.Generic;
using System.IO;
using GameEngine.Core.Economy;
using Newtonsoft.Json;

namespace GameEngine.Core.SaveSystem
{
    /// <summary>
    /// Persists and loads game state to/from JSON.
    /// Path is injected (e.g. Application.persistentDataPath) to keep Core Unity-agnostic.
    /// </summary>
    public sealed class SaveSystem
    {
        private const string SaveFileName = "save.json";
        private readonly string _basePath;

        public SaveSystem(string basePath)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }

        /// <summary>
        /// Saves game state. Creates directory if needed.
        /// </summary>
        public void Save(SaveDataSchema data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var dir = GetSaveDirectory(data.GameId);
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, SaveFileName);
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Loads save data if it exists. Returns null otherwise.
        /// </summary>
        public SaveDataSchema Load(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
                throw new ArgumentException("Game ID cannot be null or empty.", nameof(gameId));

            var path = GetSavePath(gameId);
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<SaveDataSchema>(json);
        }

        /// <summary>
        /// Returns true if a save file exists for the given game.
        /// </summary>
        public bool HasSave(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
                return false;
            return File.Exists(GetSavePath(gameId));
        }

        private string GetSaveDirectory(string gameId)
        {
            return Path.Combine(_basePath, "Game", gameId);
        }

        private string GetSavePath(string gameId)
        {
            return Path.Combine(GetSaveDirectory(gameId), SaveFileName);
        }

        /// <summary>
        /// Converts BigNumber to serializable format.
        /// </summary>
        public static BigNumberSaveData ToSaveData(BigNumber value)
        {
            return new BigNumberSaveData
            {
                Mantissa = value.Mantissa,
                Exponent = value.Exponent
            };
        }

        /// <summary>
        /// Converts save data back to BigNumber.
        /// </summary>
        public static BigNumber FromSaveData(BigNumberSaveData data)
        {
            if (data == null)
                return BigNumber.Zero;
            return new BigNumber(data.Mantissa, data.Exponent);
        }
    }
}
