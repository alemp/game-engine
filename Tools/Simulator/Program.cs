using GameEngine.Core.Config;
using GameEngine.Core.Economy;
using GameEngine.Core.EventBus;
using GameEngine.Core.Scheduler;
using GameEngine.Modules.Idle;

if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: Simulator <path-to-game-folder> <duration>");
    Console.Error.WriteLine("  duration: 1h | 24h | 7d");
    Environment.Exit(1);
}

var gamePath = args[0];
var durationArg = args[1].ToLowerInvariant();

var definitionsPath = Path.Combine(gamePath, "Definitions", "game.json");
if (!File.Exists(definitionsPath))
{
    Console.Error.WriteLine($"Error: game.json not found at {definitionsPath}");
    Environment.Exit(2);
}

double durationSeconds = durationArg switch
{
    "1h" => 3600,
    "24h" => 86400,
    "7d" => 604800,
    _ => 0
};

if (durationSeconds <= 0)
{
    Console.Error.WriteLine("Error: duration must be 1h, 24h, or 7d");
    Environment.Exit(3);
}

var loader = new GameLoader(gamePath);
var gameConfig = loader.LoadGameConfig();
var tickInterval = gameConfig.Economy?.TickIntervalSeconds ?? 1.0;
var ticks = (int)Math.Floor(durationSeconds / tickInterval);

var eventBus = new EventBus();
var scheduler = new Scheduler(tickInterval);
var idleModule = new IdleModule(eventBus, scheduler);

foreach (var (id, amount) in loader.GetResourceDefinitions())
    idleModule.RegisterResource(id, amount);

foreach (var (inputs, outputId, outputAmount, multiplier) in loader.GetProductionRules())
    idleModule.AddProductionRule(new ProductionRule(inputs, outputId, outputAmount, multiplier));

idleModule.SimulateTicks(ticks);

Console.WriteLine($"Simulation: {durationArg} ({ticks} ticks @ {tickInterval}s/tick)");
Console.WriteLine($"Game: {gameConfig.GameId}");
Console.WriteLine();
Console.WriteLine("Resources:");
foreach (var (id, amount) in idleModule.GetResourceSnapshot().OrderBy(x => x.Key))
{
    Console.WriteLine($"  {id}: {amount}");
}
