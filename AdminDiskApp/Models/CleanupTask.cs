namespace AdminDiskApp.Models;

public class CleanupTask
{
    public CleanupTask() { }
    public CleanupTask(string folderPath) => FolderPath = folderPath;

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FolderPath { get; set; } = string.Empty;
    public string Filter { get; set; } = "*.*";
    public int DaysOld { get; set; } = 30;
    public bool Recursive { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
}