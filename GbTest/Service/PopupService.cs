using GbTest.View.window;
using GbTest.ViewModel;

namespace GbTest.Service
{
    internal class PopupService(ConnectConfig connectConfig, ConnectConfigViewModel connectConfigViewModel) : IPopupService
    {
        public void ShowConnectionConfig()
        {
            connectConfig.DataContext = connectConfigViewModel;
            connectConfig.Owner = App.Current.MainWindow;
            connectConfig.ShowDialog();
        }
    }
}
