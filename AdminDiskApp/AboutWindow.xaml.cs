using System.Windows;

namespace AdminDiskApp;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = $"Verze {version?.Major}.{version?.Minor}.{version?.Build}";
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}