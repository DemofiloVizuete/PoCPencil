using System.Windows;
using PromptMarket.Desktop.Api;

namespace PromptMarket.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly LocalApiHost apiHost = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoadedAsync;
        Closed += OnClosedAsync;
    }

    private async void OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await apiHost.StartAsync();
            await Browser.EnsureCoreWebView2Async();
            Browser.Source = apiHost.BaseAddress;
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Prompt Market", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    private async void OnClosedAsync(object? sender, EventArgs e) => await apiHost.DisposeAsync();
}