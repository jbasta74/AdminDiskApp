using System.Windows;
using System.ComponentModel;

using Application = System.Windows.Application;

namespace AdminDiskApp;

public partial class MainWindow : Window
{
    private readonly System.Windows.Forms.NotifyIcon _notifyIcon;

    public MainWindow()
    {
        InitializeComponent();

        _notifyIcon = new System.Windows.Forms.NotifyIcon();
        try
        {
            _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!);
        }
        catch { _notifyIcon.Icon = System.Drawing.SystemIcons.Shield; }

        _notifyIcon.Visible = true;
        _notifyIcon.Text = "AdminDiskApp 2026";
        _notifyIcon.DoubleClick += (s, e) => ShowWindow();

        // Menu pro Tray ikonu
        _notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
        _notifyIcon.ContextMenuStrip.Items.Add("Otevřít", null, (s, e) => ShowWindow());
        _notifyIcon.ContextMenuStrip.Items.Add("Ukončit", null, (s, e) => {
            _notifyIcon.Dispose();
            Application.Current.Shutdown();
        });
    }

    private void ShowWindow()
    {
        this.Show();
        this.WindowState = WindowState.Normal;
        this.Activate();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        this.Hide();
    }
}