namespace GameEngine.Core.Config
{
    /// <summary>
    /// Root configuration interface for a game definition.
    /// Implementations load from JSON and map to runtime structures.
    /// </summary>
    public interface IGameConfig
    {
        string GameId { get; }
    }
}
