using System.IO;
using System.Text.Json;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: Validator <path-to-game-folder>");
    Environment.Exit(1);
}

var gamePath = args[0];
var gameJsonPath = Path.Combine(gamePath, "game.json");

if (!File.Exists(gameJsonPath))
{
    Console.Error.WriteLine($"Error: game.json not found at {gameJsonPath}");
    Environment.Exit(2);
}

var json = await File.ReadAllTextAsync(gameJsonPath);
JsonDocument doc;
try
{
    doc = JsonDocument.Parse(json);
}
catch (JsonException ex)
{
    Console.Error.WriteLine($"Error: Invalid JSON in game.json - {ex.Message}");
    Environment.Exit(3);
    return;
}

var errors = new List<string>();
using (doc)
{
    var root = doc.RootElement;

    if (!root.TryGetProperty("gameId", out var gameIdEl) || gameIdEl.GetString() is not { Length: > 0 })
        errors.Add("gameId is required.");

    if (root.TryGetProperty("economy", out var economy))
    {
        if (economy.TryGetProperty("tickIntervalSeconds", out var tickEl))
        {
            var tick = tickEl.GetDouble();
            if (tick <= 0)
                errors.Add("economy.tickIntervalSeconds must be positive.");
        }
        if (economy.TryGetProperty("maxOfflineSeconds", out var offlineEl))
        {
            var offline = offlineEl.GetDouble();
            if (offline < 0)
                errors.Add("economy.maxOfflineSeconds cannot be negative.");
        }
    }
}

if (errors.Count > 0)
{
    foreach (var err in errors)
        Console.Error.WriteLine($"  - {err}");
    Environment.Exit(4);
}

Console.WriteLine("Validation passed.");
