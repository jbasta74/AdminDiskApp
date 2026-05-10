using System.Text.Json.Serialization;

using AdminDiskApp.Models;

namespace AdminDiskApp.Services;

// Tento atribut řekne kompilátoru, aby vygeneroval kód pro serializaci CleanupTask už při buildu
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<CleanupTask>))]
internal partial class CleanupJsonContext : JsonSerializerContext
{
}