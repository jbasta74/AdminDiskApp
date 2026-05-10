using System.IO;
using System.Text.Json;

using AdminDiskApp.Models;

namespace AdminDiskApp.Services;

public class ConfigService
{
    private readonly string _configPath;
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    
    public ConfigService()
    {    
        // Vždy hledat config tam, kde je aplikace
        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
    }

    public async Task SaveTasksAsync(IEnumerable<CleanupTask> tasks)
    {
        // Klasická serializace bez nutnosti Source Generatoru
        var json = JsonSerializer.Serialize(tasks, _options);
        await File.WriteAllTextAsync(_configPath, json);
    }

    public async Task<List<CleanupTask>> LoadTasksAsync()
    {
        if (!File.Exists(_configPath)) return [];
        try
        {
            var json = await File.ReadAllTextAsync(_configPath);
            return JsonSerializer.Deserialize<List<CleanupTask>>(json, _options) ?? [];
        }
        catch { return []; }
    }
}