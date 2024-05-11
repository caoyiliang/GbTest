using Communication.Bus.PhysicalPort;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Config;
using Config.Model;
using GbTest.Service;
using HJ212;
using Microsoft.Extensions.DependencyInjection;

namespace GbTest.ViewModel
{
    internal partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _status;
        [ObservableProperty]
        private bool _isConnect;
        [ObservableProperty]
        private bool _isOpen;
        [ObservableProperty]
        private string _MN = "88888888";
        [ObservableProperty]
        private string _PW = "123456";

        private Connection _connection;
        private IGB? _gb;
        public MainViewModel(ConfigManager configManager)
        {
            _connection = configManager.Connection;
            Status = _connection.ToString();
            WeakReferenceMessenger.Default.Register<StatusMessage>(this, (r, m) =>
            {
                _connection = m.Value;
                Status = m.Value.ToString();
            });
        }

        [RelayCommand]
        private void ConnectionConfig()
        {
            var popupService = App.Current.Services.GetRequiredService<IPopupService>();
            popupService.ShowConnectionConfig();
        }

        [RelayCommand(CanExecute = nameof(CanOpen))]
        private async Task ConnectAsync()
        {
            if (IsConnect)
            {
                await _gb!.CloseAsync();
                IsOpen = false;
            }
            else
            {
                switch (_connection.Type)
                {
                    case CommunicationType.SerialPort:
                        var serialPort = new SerialPort(_connection.PortName, _connection.BaudRate, _connection.Parity, _connection.DataBits, _connection.StopBits)
                        {
                            DtrEnable = _connection.DTR,
                            RtsEnable = _connection.RTS
                        };
                        _gb = new GB("串口连接", serialPort, MN, PW);
                        break;
                    case CommunicationType.TcpClient:
                        _gb = new GB("TCP连接", new TcpClient(_connection.HostName, _connection.Port), MN, PW);
                        break;
                    default: throw new NotSupportedException();
                }
                _gb.OnConnect += _gb_OnConnect;
                _gb.OnDisconnect += _gb_OnDisconnect;
                try
                {
                    await _gb.OpenAsync();
                    IsOpen = true;
                }
                catch
                {
                    //ExceptionStr = "连接失败，检查链路";
                }
            }
        }

        private async Task _gb_OnDisconnect()
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                IsConnect = false;
                //Exception = "通讯断连,等待连接...";
            });
        }

        private async Task _gb_OnConnect()
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                IsConnect = true;
                //Exception = "";
            });
        }

        private bool CanOpen()
        {
            if (IsOpen && !IsConnect)
            {
                return false;
            }
            return true;
        }
    }

    internal class StatusMessage(Connection value) : ValueChangedMessage<Connection>(value) { }
}
