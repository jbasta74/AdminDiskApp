using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

using AdminDiskApp.Models;
using AdminDiskApp.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;

namespace AdminDiskApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly CleanupService _cleanupService = new();
    private readonly ConfigService _configService = new();
    private static readonly string LogFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cleanup.log");

    [ObservableProperty] private ObservableCollection<CleanupTask> _tasks = [];
    [ObservableProperty] private string _logOutput = string.Empty;
    [ObservableProperty] private string _scheduledTime = "03:00";
    [ObservableProperty] private string _windowTitle = IsAdmin() ? "AdminDiskApp (Administrator)" : "AdminDiskApp";

    public MainViewModel()
    {
        // Načtení konfigurace a historie logů
        _ = LoadDataAsync();

        // Spuštění vnitřního časovače pro běh v Trayi
        _ = StartBackgroundSchedulerAsync();

        // Pokud je aplikace spuštěna systémem s parametrem --auto
        if (Environment.GetCommandLineArgs().Contains("--auto"))
        {
            _ = RunAutoAndCloseAsync();
        }
    }

    /// <summary>
    /// Načte seznam úloh z JSONu a historii posledních zpráv z logu.
    /// </summary>
    private async Task LoadDataAsync()
    {
        // 1. Načtení úloh
        var loaded = await _configService.LoadTasksAsync();
        Tasks = new ObservableCollection<CleanupTask>(loaded);

        // 2. Načtení historie logů (posledních 100 řádků pro přehled)
        if (File.Exists(LogFileName))
        {
            try
            {
                var lines = await File.ReadAllLinesAsync(LogFileName);
                LogOutput = string.Join(Environment.NewLine, lines.TakeLast(100)) + Environment.NewLine;
                AppendLog("--- APLIKACE SPUŠTĚNA ---");
            }
            catch
            {
                AppendLog("Nepodařilo se načíst historii logů z disku.");
            }
        }
        else
        {
            AppendLog("Aplikace připravena.");
        }
    }

    /// <summary>
    /// Zapíše zprávu do UI okna a zároveň ji připojí do souboru na disku.
    /// </summary>
    private void AppendLog(string message)
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

        // Zápis do UI okna
        LogOutput += logEntry + Environment.NewLine;

        // Log Rotation logika
        try
        {
            var fileInfo = new FileInfo(LogFileName);

            // Pokud soubor existuje a má víc než 1 MB (1024 * 1024 bajtů)
            if (fileInfo.Exists && fileInfo.Length > 1 * 1024 * 1024)
            {
                string backupPath = LogFileName + ".old";

                // Moderní .NET 10 způsob: Přesuneme a rovnou přepíšeme starou zálohu
                File.Move(LogFileName, backupPath, overwrite: true);
            }

            // Zapíšeme nový řádek (pokud soubor po rotaci neexistuje, automaticky se vytvoří)
            File.AppendAllText(LogFileName, logEntry + Environment.NewLine);
        }
        catch
        {
            // Tichá chyba - nechceme, aby problém se zápisem logu shodil celý úklid
        }
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
            AppendLog($"➕ Přidán adresář: {dialog.FolderName}");
        }
    }

    [RelayCommand]
    private async Task RemoveTaskAsync(CleanupTask task)
    {
        if (task == null) return;
        Tasks.Remove(task);
        await _configService.SaveTasksAsync(Tasks);
        AppendLog($"➖ Odebrán adresář: {task.FolderPath}");
    }

    [RelayCommand]
    private async Task RunAllTasksAsync() => await ExecuteInternalAsync(isManual: true);

    [RelayCommand]
    private void RegisterTask()
    {
        try
        {
            string exePath = Environment.ProcessPath!;
            // Vytvoření úlohy v Plánovači úloh Windows s nejvyšším oprávněním
            string command = $"/Create /TN \"AdminDiskApp_Cleanup\" /TR \"'{exePath}' --auto\" /SC DAILY /ST {ScheduledTime} /RL HIGHEST /F";

            Process.Start(new ProcessStartInfo("schtasks.exe", command)
            {
                UseShellExecute = true,
                Verb = "runas"
            });

            AppendLog($"✅ Úloha zaregistrována do systému na {ScheduledTime}.");
        }
        catch (Exception ex)
        {
            AppendLog($"❌ Chyba registrace úlohy: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenLogFile()
    {
        if (File.Exists(LogFileName))
        {
            Process.Start(new ProcessStartInfo(LogFileName) { UseShellExecute = true });
        }
        else
        {
            MessageBox.Show("Soubor s logy zatím neexistuje.", "Informace");
        }
    }
    [RelayCommand]
    private void Exit()
    {
        // Musíme použít Shutdown, protože OnClosing v MainWindow 
        // by aplikaci jen schovalo do Traye.
        Application.Current.Shutdown();
    }

    // okno O aplikaci
    [RelayCommand]
    private void ShowAbout()
    {
        // Vytvoříme instanci našeho nového okna
        var aboutWin = new AboutWindow();

        // Nastavíme hlavní okno jako vlastníka (aby se centrovalo k němu)
        aboutWin.Owner = Application.Current.MainWindow;

        // Zobrazíme jako modální dialog
        aboutWin.ShowDialog();
    }
    private async Task StartBackgroundSchedulerAsync()
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync())
        {
            if (DateTime.Now.ToString("HH:mm") == ScheduledTime)
            {
                await ExecuteInternalAsync(isManual: false);
            }
        }
    }

    private async Task RunAutoAndCloseAsync()
    {
        await ExecuteInternalAsync(isManual: false);
        Application.Current.Shutdown();
    }

    private async Task ExecuteInternalAsync(bool isManual)
    {
        long totalBytesSaved = 0;
        // Rozlišení textu podle toho, jak byl úklid spuštěn
        string header = isManual ? ">>> SPUŠTĚN RUČNÍ ÚKLID <<<" : ">>> SPUŠTĚN AUTOMATICKÝ ÚKLID <<<";
        AppendLog(header);

        foreach (var task in Tasks.Where(t => t.IsEnabled))
        {
            // Přijímáme zprávu i počet bajtů
            await foreach (var result in _cleanupService.ExecuteCleanupAsync(task))
            {
                AppendLog(result.Message);
                totalBytesSaved += result.Bytes;
            }
        }

        string formattedSize = FormatBytes(totalBytesSaved);
        AppendLog($">>> ÚKLID DOKONČEN. Celkem ušetřeno místa: {formattedSize} <<<");
    }
    /// <summary>
    /// Převede počet bajtů na čitelný formát (B, KB, MB, GB, TB).
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        double size = bytes;
        int index = 0;

        while (size >= 1024 && index < suffixes.Length - 1)
        {
            size /= 1024;
            index++;
        }

        return $"{size:N2} {suffixes[index]}";
    }

    private static bool IsAdmin()
    {
        if (!OperatingSystem.IsWindows()) return false;
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }
}