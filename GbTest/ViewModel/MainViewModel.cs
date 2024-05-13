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
using System.Diagnostics;
using System.Text;
using System.Windows;
using Utils;

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
        [ObservableProperty]
        private IEnumerable<ST> _STs;
        [ObservableProperty]
        private ST _ST = ST.大气环境污染源;
        [ObservableProperty]
        private IEnumerable<HJ212.Version> _Versions;
        [ObservableProperty]
        private HJ212.Version _Version = HJ212.Version.HJT212_2017;
        [ObservableProperty]
        private string? _Content;

        private Connection _connection;
        private IGB? _gb;
        public MainViewModel(ConfigManager configManager)
        {
            _connection = configManager.Connection;
            Status = _connection.ToString();
            STs = Enum.GetValues(typeof(ST)).Cast<ST>();
            Versions = Enum.GetValues(typeof(HJ212.Version)).Cast<HJ212.Version>();
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
                        _gb = new GB("串口连接", serialPort, MN, PW, true, ST, Version);
                        break;
                    case CommunicationType.TcpClient:
                        _gb = new GB("TCP连接", new TcpClient(_connection.HostName, _connection.Port), MN, PW, true, ST, Version);
                        break;
                    default: throw new NotSupportedException();
                }
                _gb.OnSentData += _gb_OnSentData;
                _gb.OnReceivedData += _gb_OnReceivedData;
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

        private async Task _gb_OnReceivedData(byte[] data)
        {
            Content += $"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} GB Rec:<-- {Encoding.ASCII.GetString(data)}\r\n";
            await Task.CompletedTask;
        }

        private async Task _gb_OnSentData(byte[] data)
        {
            Content += $"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} GB Sent:<-- {Encoding.ASCII.GetString(data)}";
            await Task.CompletedTask;
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

        #region C1
        [ObservableProperty]
        private bool _C1;
        [ObservableProperty]
        private int? _OverTime = null;
        [ObservableProperty]
        private int? _ReCount = null;
        partial void OnC1Changed(bool value)
        {
            if (value)
            {
                _gb!.OnSetOverTimeAndReCount += _gb_OnSetOverTimeAndReCount;
            }
            else
            {
                _gb!.OnSetOverTimeAndReCount -= _gb_OnSetOverTimeAndReCount;
            }
        }

        private async Task _gb_OnSetOverTimeAndReCount((int OverTime, int ReCount, HJ212.Model.RspInfo RspInfo) objects)
        {
            OverTime = objects.OverTime;
            ReCount = objects.ReCount;
            await Task.CompletedTask;
        }
        #endregion

        #region C2
        [ObservableProperty]
        private bool _C2;
        partial void OnC2Changed(bool value)
        {
            if (value)
            {
                _gb!.OnGetSystemTime += MainViewModel_OnGetSystemTime;
            }
            else
            {
                _gb!.OnGetSystemTime -= MainViewModel_OnGetSystemTime;
            }
        }

        private async Task<DateTime?> MainViewModel_OnGetSystemTime((string? PolId, HJ212.Model.RspInfo RspInfo) objects)
        {
            return await Task.FromResult(DateTime.Now);
        }
        #endregion

        #region C3
        [ObservableProperty]
        private bool _C3;
        [ObservableProperty]
        private string? _SystemTime;
        partial void OnC3Changed(bool value)
        {
            if (value)
            {
                _gb!.OnSetSystemTime += MainViewModel_OnSetSystemTime;
            }
            else
            {
                _gb!.OnSetSystemTime -= MainViewModel_OnSetSystemTime;
            }
        }

        private async Task MainViewModel_OnSetSystemTime((string? PolId, DateTime SystemTime, HJ212.Model.RspInfo RspInfo) objects)
        {
            SystemTime = objects.SystemTime.ToString("yyyy-MM-dd HH:mm:ss");
            await Task.CompletedTask;
        }
        #endregion

        #region C4
        [ObservableProperty]
        private string _PolId_C4 = "w01018";
        [ObservableProperty]
        private int _TimeOut_C4 = 120000;
        [RelayCommand]
        private async Task C4TestAsync()
        {
            try
            {
                await _gb!.AskSetSystemTime("a1001", TimeOut_C4);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("请求超时");
            }
        }
        #endregion

        #region C5
        [ObservableProperty]
        private bool _C5;
        [ObservableProperty]
        private int _RtdInterval = 30;
        partial void OnC5Changed(bool value)
        {
            if (value)
            {
                _gb!.OnGetRealTimeDataInterval += MainViewModel_OnGetRealTimeDataInterval;
            }
            else
            {
                _gb!.OnGetRealTimeDataInterval -= MainViewModel_OnGetRealTimeDataInterval;
            }
        }

        private async Task<int> MainViewModel_OnGetRealTimeDataInterval(HJ212.Model.RspInfo objects)
        {
            return await Task.FromResult(RtdInterval);
        }
        #endregion
    }

    internal class StatusMessage(Connection value) : ValueChangedMessage<Connection>(value) { }
}
