using System.IO;

using AdminDiskApp.Models;

namespace AdminDiskApp.Services;

public class CleanupService
{
    public async IAsyncEnumerable<string> ExecuteCleanupAsync(CleanupTask task)
    {
        if (!Directory.Exists(task.FolderPath))
        {
            yield return $"❌ Cesta neexistuje: {task.FolderPath}";
            yield break;
        }

        var threshold = DateTime.Now.AddDays(-task.DaysOld);
        var options = new EnumerationOptions { RecurseSubdirectories = task.Recursive, IgnoreInaccessible = true };

        foreach (var filePath in Directory.EnumerateFiles(task.FolderPath, task.Filter, options))
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.LastWriteTime < threshold)
            {
                string status;
                try
                {
                    await Task.Run(() => fileInfo.Delete());
                    status = $"✔ Smazáno: {fileInfo.Name}";
                }
                catch (Exception ex) { status = $"⚠ Nelze smazat {fileInfo.Name}: {ex.Message}"; }
                yield return status;
            }
        }
    }
}