﻿using Communication.Bus.PhysicalPort;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Config;
using Config.Model;
using GbTest.Service;
using HJ212;
using HJ212.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

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
        private string? _Content;

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
        private int? _OverTime = null;
        [ObservableProperty]
        private int? _ReCount = null;
        [ObservableProperty]
        private int _RtdInterval = 30;
        [ObservableProperty]
        private int _MinInterval = 10;
        [ObservableProperty]
        private bool _Rtd;
        [ObservableProperty]
        private bool _RunningState;
        [ObservableProperty]
        private ObservableCollection<Model.RealTimeData> _RealTimeDatas = [];
        [ObservableProperty]
        private ObservableCollection<Model.RunningStateData> _RunningStateDatas = [];
        [ObservableProperty]
        private ObservableCollection<Model.StatisticsData> _MinuteDatas = [];
        [ObservableProperty]
        private ObservableCollection<Model.StatisticsData> _HourDatas = [];
        [ObservableProperty]
        private ObservableCollection<Model.StatisticsData> _DayDatas = [];
        [ObservableProperty]
        private ObservableCollection<Model.RunningTimeData> _RunningTimeDatas = [];

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

        partial void OnSTChanged(ST value)
        {
            if (_gb != null)
            {
                _gb.ST = value;
            }
        }

        partial void OnMNChanged(string value)
        {
            if (_gb != null)
            {
                _gb.MN = value;
            }
        }

        partial void OnPWChanged(string value)
        {
            if (_gb != null)
            {
                _gb.PW = value;
            }
        }

        partial void OnVersionChanged(HJ212.Version value)
        {
            if (_gb != null)
            {
                _gb.Version = value;
            }
        }

        #region C1
        [ObservableProperty]
        private bool _C1;
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

        #region C6
        [ObservableProperty]
        private bool _C6;
        partial void OnC6Changed(bool value)
        {
            if (value)
            {
                _gb!.OnSetRealTimeDataInterval += MainViewModel_OnSetRealTimeDataInterval;
            }
            else
            {
                _gb!.OnSetRealTimeDataInterval -= MainViewModel_OnSetRealTimeDataInterval;
            }
        }

        private async Task MainViewModel_OnSetRealTimeDataInterval((int RtdInterval, RspInfo RspInfo) objects)
        {
            RtdInterval = objects.RtdInterval;
            await Task.CompletedTask;
        }
        #endregion

        #region C7
        [ObservableProperty]
        private bool _C7;
        partial void OnC7Changed(bool value)
        {
            if (value)
            {
                _gb!.OnGetMinuteDataInterval += MainViewModel_OnGetMinuteDataInterval;
            }
            else
            {
                _gb!.OnGetMinuteDataInterval -= MainViewModel_OnGetMinuteDataInterval;
            }
        }

        private async Task<int> MainViewModel_OnGetMinuteDataInterval(RspInfo objects)
        {
            return await Task.FromResult(MinInterval);
        }
        #endregion

        #region C8
        [ObservableProperty]
        private bool _C8;
        partial void OnC8Changed(bool value)
        {
            if (value)
            {
                _gb!.OnSetMinuteDataInterval += MainViewModel_OnSetMinuteDataInterval;
            }
            else
            {
                _gb!.OnSetMinuteDataInterval -= MainViewModel_OnSetMinuteDataInterval;
            }
        }

        private async Task MainViewModel_OnSetMinuteDataInterval((int MinInterval, RspInfo RspInfo) objects)
        {
            MinInterval = objects.MinInterval;
            await Task.CompletedTask;
        }
        #endregion

        #region C9
        [ObservableProperty]
        private bool _C9;
        partial void OnC9Changed(bool value)
        {
            if (value)
            {
                _gb!.OnSetNewPW += MainViewModel_OnSetNewPW;
            }
            else
            {
                _gb!.OnSetNewPW -= MainViewModel_OnSetNewPW;
            }
        }

        private async Task MainViewModel_OnSetNewPW((string NewPW, RspInfo RspInfo) objects)
        {
            PW = objects.NewPW;
            await Task.CompletedTask;
        }
        #endregion

        #region C10
        [ObservableProperty]
        private bool _C10;
        partial void OnC10Changed(bool value)
        {
            if (value)
            {
                _gb!.OnStartRealTimeData += MainViewModel_OnStartRealTimeData;
            }
            else
            {
                _gb!.OnStartRealTimeData -= MainViewModel_OnStartRealTimeData;
            }
        }

        private async Task MainViewModel_OnStartRealTimeData(RspInfo objects)
        {
            Rtd = true;
            await Task.CompletedTask;
        }
        #endregion

        #region C11
        [ObservableProperty]
        private bool _C11;
        partial void OnC11Changed(bool value)
        {
            if (value)
            {
                _gb!.OnStopRealTimeData += MainViewModel_OnStopRealTimeData;
            }
            else
            {
                _gb!.OnStopRealTimeData -= MainViewModel_OnStopRealTimeData;
            }
        }

        private async Task MainViewModel_OnStopRealTimeData(RspInfo objects)
        {
            Rtd = false;
            await Task.CompletedTask;
        }
        #endregion

        #region C12
        [ObservableProperty]
        private bool _C12;
        partial void OnC12Changed(bool value)
        {
            if (value)
            {
                _gb!.OnStartRunningStateData += MainViewModel_OnStartRunningStateData;
            }
            else
            {
                _gb!.OnStartRunningStateData -= MainViewModel_OnStartRunningStateData;
            }
        }

        private async Task MainViewModel_OnStartRunningStateData(RspInfo objects)
        {
            RunningState = true;
            await Task.CompletedTask;
        }
        #endregion

        #region C13
        [ObservableProperty]
        private bool _C13;
        partial void OnC13Changed(bool value)
        {
            if (value)
            {
                _gb!.OnStopRunningStateData += MainViewModel_OnStopRunningStateData;
            }
            else
            {
                _gb!.OnStopRunningStateData -= MainViewModel_OnStopRunningStateData;
            }
        }

        private async Task MainViewModel_OnStopRunningStateData(RspInfo objects)
        {
            RunningState = false;
            await Task.CompletedTask;
        }
        #endregion

        #region C14
        [ObservableProperty]
        private int _TimeOut_C14 = 120000;
        [RelayCommand]
        private async Task C14TestAsync()
        {
            try
            {
                await _gb!.UploadRealTimeData(DateTime.Now, RealTimeDatas.Select(_ => new RealTimeData(_.Name!) { Rtd = _.Rtd.ToString(), Flag = _.Flag, SampleTime = _.SampleTime, EFlag = _.EFlag }).ToList(), TimeOut_C14);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("请求超时");
            }
        }
        [ObservableProperty]
        private bool _C14;
        [ObservableProperty]
        private float _Fluctuation_C14 = 1f;
        private Task? _task_C14;
        private CancellationTokenSource? _token_C14;
        partial void OnC14Changed(bool value)
        {
            if (value)
            {
                _token_C14 = new CancellationTokenSource();
                _task_C14 = Task.Run(async () =>
                {
                    while (!_token_C14.IsCancellationRequested)
                    {
                        Random random = new Random();
                        double randomPercentage = random.NextDouble() * 2 * Fluctuation_C14 - Fluctuation_C14;
                        try
                        {
                            await _gb!.UploadRealTimeData(DateTime.Now, RealTimeDatas.Select(_ => new RealTimeData(_.Name!) { Rtd = (_.Rtd * (1 + randomPercentage / 100)).ToString("0.00"), Flag = _.Flag, SampleTime = _.SampleTime, EFlag = _.EFlag }).ToList(), TimeOut_C14);
                        }
                        catch (TimeoutException)
                        {
                            //MessageBox.Show("请求超时");
                        }
                        await Task.Delay(RtdInterval * 1000, _token_C14.Token);
                    }
                }, _token_C14.Token);
            }
            else
            {
                _token_C14?.Cancel();
            }
        }
        #endregion

        #region C15
        [ObservableProperty]
        private int _TimeOut_C15 = 120000;
        [RelayCommand]
        private async Task C15TestAsync()
        {
            try
            {
                await _gb!.UploadRunningStateData(DateTime.Now, RunningStateDatas.Select(_ => new RunningStateData(_.Name, _.RS)).ToList(), TimeOut_C15);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("请求超时");
            }
        }
        #endregion

        #region C16
        [ObservableProperty]
        private int _TimeOut_C16 = 120000;
        [RelayCommand]
        private async Task C16TestAsync()
        {
            try
            {
                await _gb!.UploadMinuteData(DateTime.Now, MinuteDatas.Select(_ => new StatisticsData(_.Name) { Cou = _.Cou.HasValue ? _.Cou.ToString() : null, Min = _.Min.ToString(), Avg = _.Avg.ToString(), Max = _.Max.ToString(), Flag = _.Flag }).ToList(), TimeOut_C16);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("请求超时");
            }
        }
        [ObservableProperty]
        private bool _C16;
        [ObservableProperty]
        private float _Fluctuation_C16 = 1f;
        private Task? _task_C16;
        private CancellationTokenSource? _token_C16;
        partial void OnC16Changed(bool value)
        {
            if (value)
            {
                _token_C16 = new CancellationTokenSource();
                _task_C16 = Task.Run(async () =>
                {
                    var random = new Random();
                    while (!_token_C16.IsCancellationRequested)
                    {
                        double randomPercentage = random.NextDouble() * 2 * Fluctuation_C16 - Fluctuation_C16;
                        try
                        {
                            await _gb!.UploadMinuteData(DateTime.Now, MinuteDatas.Select(_ => new StatisticsData(_.Name) { Cou = _.Cou.HasValue ? ((float)_.Cou * (1 + randomPercentage / 100)).ToString("0.00") : null, Min = (_.Min * (1 + randomPercentage / 100)).ToString("0.00"), Avg = (_.Avg * (1 + randomPercentage / 100)).ToString("0.00"), Max = (_.Max * (1 + randomPercentage / 100)).ToString("0.00"), Flag = _.Flag }).ToList(), TimeOut_C16);
                        }
                        catch (TimeoutException)
                        {
                            //MessageBox.Show("请求超时");
                        }
                        await Task.Delay(MinInterval * 60 * 1000, _token_C16.Token);
                    }
                }, _token_C16.Token);
            }
            else
            {
                _token_C16?.Cancel();
            }
        }
        #endregion

        #region C17
        [ObservableProperty]
        private int _TimeOut_C17 = 120000;
        [RelayCommand]
        private async Task C17TestAsync()
        {
            try
            {
                await _gb!.UploadHourData(DateTime.Now, HourDatas.Select(_ => new StatisticsData(_.Name) { Cou = _.Cou.HasValue ? _.Cou.ToString() : null, Min = _.Min.ToString(), Avg = _.Avg.ToString(), Max = _.Max.ToString(), Flag = _.Flag }).ToList(), TimeOut_C17);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("请求超时");
            }
        }
        [ObservableProperty]
        private bool _C17;
        [ObservableProperty]
        private float _Fluctuation_C17 = 1f;
        private Task? _task_C17;
        private CancellationTokenSource? _token_C17;
        partial void OnC17Changed(bool value)
        {
            if (value)
            {
                _token_C17 = new CancellationTokenSource();
                _task_C17 = Task.Run(async () =>
                {
                    var random = new Random();
                    while (!_token_C17.IsCancellationRequested)
                    {
                        double randomPercentage = random.NextDouble() * 2 * Fluctuation_C17 - Fluctuation_C17;
                        try
                        {
                            await _gb!.UploadHourData(DateTime.Now, HourDatas.Select(_ => new StatisticsData(_.Name) { Cou = _.Cou.HasValue ? ((float)_.Cou * (1 + randomPercentage / 100)).ToString("0.00") : null, Min = (_.Min * (1 + randomPercentage / 100)).ToString("0.00"), Avg = (_.Avg * (1 + randomPercentage / 100)).ToString("0.00"), Max = (_.Max * (1 + randomPercentage / 100)).ToString("0.00"), Flag = _.Flag }).ToList(), TimeOut_C17);
                        }
                        catch (TimeoutException)
                        {
                            //MessageBox.Show("请求超时");
                        }
                        await Task.Delay(60 * 60 * 1000, _token_C17.Token);
                    }
                }, _token_C17.Token);
            }
            else
            {
                _token_C17?.Cancel();
            }
        }
        #endregion

        #region C18
        [ObservableProperty]
        private int _TimeOut_C18 = 120000;
        [RelayCommand]
        private async Task C18TestAsync()
        {
            try
            {
                await _gb!.UploadDayData(DateTime.Now, DayDatas.Select(_ => new StatisticsData(_.Name) { Cou = _.Cou.HasValue ? _.Cou.ToString() : null, Min = _.Min.ToString(), Avg = _.Avg.ToString(), Max = _.Max.ToString(), Flag = _.Flag }).ToList(), TimeOut_C18);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("请求超时");
            }
        }
        [ObservableProperty]
        private bool _C18;
        [ObservableProperty]
        private float _Fluctuation_C18 = 1f;
        private Task? _task_C18;
        private CancellationTokenSource? _token_C18;
        partial void OnC18Changed(bool value)
        {
            if (value)
            {
                _token_C18 = new CancellationTokenSource();
                _task_C18 = Task.Run(async () =>
                {
                    var random = new Random();
                    while (!_token_C18.IsCancellationRequested)
                    {
                        double randomPercentage = random.NextDouble() * 2 * Fluctuation_C18 - Fluctuation_C18;
                        try
                        {
                            await _gb!.UploadDayData(DateTime.Now, DayDatas.Select(_ => new StatisticsData(_.Name) { Cou = _.Cou.HasValue ? ((float)_.Cou * (1 + randomPercentage / 100)).ToString("0.00") : null, Min = (_.Min * (1 + randomPercentage / 100)).ToString("0.00"), Avg = (_.Avg * (1 + randomPercentage / 100)).ToString("0.00"), Max = (_.Max * (1 + randomPercentage / 100)).ToString("0.00"), Flag = _.Flag }).ToList(), TimeOut_C18);
                        }
                        catch (TimeoutException)
                        {
                            //MessageBox.Show("请求超时");
                        }
                        await Task.Delay(24 * 60 * 60 * 1000, _token_C18.Token);
                    }
                }, _token_C18.Token);
            }
            else
            {
                _token_C18?.Cancel();
            }
        }
        #endregion

        #region C19
        [ObservableProperty]
        private int _TimeOut_C19 = 120000;
        [RelayCommand]
        private async Task C19TestAsync()
        {
            try
            {
                await _gb!.UploadRunningTimeData(DateTime.Now, RunningTimeDatas.Select(_ => new RunningTimeData(_.Name, _.RT)).ToList(), TimeOut_C19);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("请求超时");
            }
        }
        #endregion

        #region C20
        [ObservableProperty]
        private bool _C20;
        [ObservableProperty]
        private int _Count_C20 = 2;
        [ObservableProperty]
        private bool _ReturnValue;

        partial void OnC20Changed(bool value)
        {
            if (value)
            {
                _gb!.OnGetMinuteData += MainViewModel_OnGetMinuteData;
            }
            else
            {
                _gb!.OnGetMinuteData -= MainViewModel_OnGetMinuteData;
            }
        }

        private async Task<(List<HistoryData> HistoryDatas, bool ReturnValue, int? Timeout)> MainViewModel_OnGetMinuteData((DateTime BeginTime, DateTime EndTime, RspInfo RspInfo) objects)
        {
            var historyDatas = new List<HistoryData>();
            var random = new Random();
            for (int i = 0; i < Count_C20; i++)
            {
                double randomPercentage = random.NextDouble() * 2 * Fluctuation_C16 - Fluctuation_C16;
                historyDatas.Add(new HistoryData(objects.BeginTime.AddMinutes(i), MinuteDatas.Select(_ => new StatisticsData(_.Name) { Cou = _.Cou.HasValue ? ((float)_.Cou * (1 + randomPercentage / 100)).ToString("0.00") : null, Min = (_.Min * (1 + randomPercentage / 100)).ToString("0.00"), Avg = (_.Avg * (1 + randomPercentage / 100)).ToString("0.00"), Max = (_.Max * (1 + randomPercentage / 100)).ToString("0.00"), Flag = _.Flag }).ToList()));
            }
            return await Task.FromResult((historyDatas, ReturnValue, TimeOut_C16));
        }
        #endregion
    }

    internal class StatusMessage(Connection value) : ValueChangedMessage<Connection>(value) { }
}
