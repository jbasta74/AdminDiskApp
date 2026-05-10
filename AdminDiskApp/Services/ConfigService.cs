using System.IO;
using System.Text.Json;

using AdminDiskApp.Models;

namespace AdminDiskApp.Services;

public class ConfigService(string configPath = "config.json")
{
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public async Task SaveTasksAsync(IEnumerable<CleanupTask> tasks)
    {
        // Klasická serializace bez nutnosti Source Generatoru
        var json = JsonSerializer.Serialize(tasks, _options);
        await File.WriteAllTextAsync(configPath, json);
    }

    public async Task<List<CleanupTask>> LoadTasksAsync()
    {
        if (!File.Exists(configPath)) return [];
        try
        {
            var json = await File.ReadAllTextAsync(configPath);
            return JsonSerializer.Deserialize<List<CleanupTask>>(json, _options) ?? [];
        }
        catch { return []; }
    }
}