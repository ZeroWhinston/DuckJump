using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DuckJump.App.Pages;

public sealed partial class HomePage : Page
{
    private bool _isConnected;

    public HomePage()
    {
        InitializeComponent();
    }

    private void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        _isConnected = !_isConnected;
        StatusTitle.Text = _isConnected ? "Защищённое соединение" : "Соединение не защищено";
        StatusSubtitle.Text = _isConnected
            ? "Трафик направляется через Amsterdam. DNS-защита и kill switch включены."
            : "Выберите сервер и включите защищённый туннель. DuckJump автоматически применит DNS-защиту и kill switch.";
        StatusBadgeText.Text = _isConnected ? "ПОДКЛЮЧЕНО" : "НЕ ПОДКЛЮЧЕНО";
        ConnectButtonText.Text = _isConnected ? "Отключиться" : "Подключиться";
        StatusDot.Fill = new SolidColorBrush(_isConnected
            ? Color.FromArgb(255, 67, 209, 122)
            : Color.FromArgb(255, 251, 191, 36));
        ConnectionInfoBar.IsOpen = true;
    }
}
