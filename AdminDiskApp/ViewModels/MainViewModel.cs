using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;


using AdminDiskApp.Models;
using AdminDiskApp.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Application = System.Windows.Application;

namespace AdminDiskApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly CleanupService _cleanupService = new();
    private readonly ConfigService _configService = new();

    [ObservableProperty] private ObservableCollection<CleanupTask> _tasks = [];
    [ObservableProperty] private string _logOutput = string.Empty;
    [ObservableProperty] private string _scheduledTime = "03:00";
    [ObservableProperty] private string _windowTitle = IsAdmin() ? "AdminDiskApp (Administrator)" : "AdminDiskApp";

    public MainViewModel()
    {
        _ = LoadDataAsync();
        _ = StartBackgroundSchedulerAsync();

        // Kontrola, zda jsme spuštěni z Task Scheduleru
        if (Environment.GetCommandLineArgs().Contains("--auto"))
        {
            _ = RunAutoAndCloseAsync();
        }
    }

    private async Task LoadDataAsync()
    {
        var loaded = await _configService.LoadTasksAsync();
        Tasks = new ObservableCollection<CleanupTask>(loaded);
        LogOutput += $"[{DateTime.Now:HH:mm:ss}] Aplikace připravena.\n";
    }

    [RelayCommand]
    private async Task AddFolderAsync()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog();
        if (dialog.ShowDialog() == true)
        {
            var newTask = new CleanupTask(dialog.FolderName);
            Tasks.Add(newTask);
            await _configService.SaveTasksAsync(Tasks);
        }
    }

    [RelayCommand]
    private async Task RemoveTaskAsync(CleanupTask task)
    {
        if (task == null) return;
        Tasks.Remove(task);
        await _configService.SaveTasksAsync(Tasks);
    }

    [RelayCommand]
    private async Task RunAllTasksAsync() => await ExecuteInternalAsync();

    [RelayCommand]
    private void RegisterTask()
    {
        try
        {
            string exePath = Environment.ProcessPath!;
            string command = $"/Create /TN \"AdminDiskApp_Cleanup\" /TR \"'{exePath}' --auto\" /SC DAILY /ST {ScheduledTime} /RL HIGHEST /F";
            Process.Start(new ProcessStartInfo("schtasks.exe", command) { UseShellExecute = true, Verb = "runas" });
            LogOutput += $"✅ Úloha zaregistrována do Windows na {ScheduledTime}.\n";
        }
        catch (Exception ex) { LogOutput += $"❌ Chyba registrace: {ex.Message}\n"; }
    }

    private async Task StartBackgroundSchedulerAsync()
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync())
        {
            if (DateTime.Now.ToString("HH:mm") == ScheduledTime) await ExecuteInternalAsync();
        }
    }

    private async Task RunAutoAndCloseAsync()
    {
        await ExecuteInternalAsync();
        Application.Current.Shutdown();
    }

    private async Task ExecuteInternalAsync()
    {
        LogOutput += $"\n[{DateTime.Now:HH:mm:ss}] SPUŠTĚN ÚKLID...\n";
        foreach (var task in Tasks.Where(t => t.IsEnabled))
        {
            await foreach (var status in _cleanupService.ExecuteCleanupAsync(task))
                LogOutput += $"{status}\n";
        }
        LogOutput += $"[{DateTime.Now:HH:mm:ss}] Hotovo.\n";
    }

    private static bool IsAdmin()
    {
        if (!OperatingSystem.IsWindows()) return false;
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }
}