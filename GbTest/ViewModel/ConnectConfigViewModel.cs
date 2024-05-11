using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Config;
using Config.Model;
using System.ComponentModel;
using System.IO.Ports;

namespace GbTest.ViewModel
{
    internal partial class ConnectConfigViewModel : ObservableObject
    {
        [ObservableProperty]
        private string[]? _portNames;
        [ObservableProperty]
        private int[] _baudRates = [300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 43000, 57000, 57600, 115200];
        [ObservableProperty]
        private int[] _dataBits = [8, 7, 6];
        [ObservableProperty]
        private IEnumerable<StopBits> _stopBits = [];
        [ObservableProperty]
        private IEnumerable<Parity> _parity = [];
        [ObservableProperty]
        Connection _connection;
        public ConnectConfigViewModel(ConfigManager configManager)
        {
            PortNames = SerialPort.GetPortNames();
            StopBits = Enum.GetValues(typeof(StopBits)).Cast<StopBits>();
            Parity = Enum.GetValues(typeof(Parity)).Cast<Parity>();
            Connection = configManager.Connection;
            Connection.PropertyChanged += OnConnectionChanged;
        }

        private async void OnConnectionChanged(object? sender, PropertyChangedEventArgs e)
        {
            await Connection.TrySaveChangeAsync();
            WeakReferenceMessenger.Default.Send(new StatusMessage(Connection));
        }
    }
}
