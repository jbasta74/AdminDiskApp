using System.Windows;
using System.Diagnostics;
using System.Windows.Navigation;

namespace AdminDiskApp;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = $"Verze {version?.Major}.{version?.Minor}.{version?.Build}";
    }

    // Otevře prohlížeč s GitHubem
    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}