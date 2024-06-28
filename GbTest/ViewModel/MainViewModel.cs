using Communication.Bus.PhysicalPort;
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
        [ObservableProperty]
        private ObservableCollection<Model.NoiseLevelData> _NoiseLevelDatas = [];
        [ObservableProperty]
        private ObservableCollection<Model.NoiseLevelData> _HourNoiseLevelDatas = [];
        [ObservableProperty]
        private ObservableCollection<Model.NoiseLevelData_Day> _DayNoiseLevelDatas = [];
        [ObservableProperty]
        private string _CstartTime = "000000";
        [ObservableProperty]
        private int _Ctime;
        [ObservableProperty]
        private int _Stime;
        [ObservableProperty]
        private ObservableCollection<Model.SampleExtractionTime> _SampleExtractionTime = [];
        [ObservableProperty]
        private ObservableCollection<Model.SNInfo> _SNInfos = [];
        [ObservableProperty]
        private ObservableCollection<Model.LogInfo> _LogInfos = [];
        [ObservableProperty]
        private ObservableCollection<Model.DeviceInfo> _DeviceInfos = [];
        [ObservableProperty]
        private ObservableCollection<Model.DeviceParameterInfo> _DeviceParameterInfos = [];

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
                _gb.OnSentData += Gb_OnSentData;
                _gb.OnReceivedData += Gb_OnReceivedData;
                _gb.OnConnect += Gb_OnConnect;
                _gb.OnDisconnect += Gb_OnDisconnect;
                try
                {
                    await _gb.OpenAsync();
                    Init();
                    IsOpen = true;
                }
                catch
                {
                    //ExceptionStr = "连接失败，检查链路";
                }
            }
        }

        private void Init()
        {
            OnC1Changed(C1);
            OnC2Changed(C2);
            OnC3Changed(C3);
            OnC5Changed(C5);
            OnC6Changed(C6);
            OnC7Changed(C7);
            OnC8Changed(C8);
            OnC9Changed(C9);
            OnC10Changed(C10);
            OnC11Changed(C11);
            OnC12Changed(C12);
            OnC13Changed(C13);
            OnC14Changed(C14);
            OnC16Changed(C16);
            OnC17Changed(C17);
            OnC18Changed(C18);
            OnC20Changed(C20);
            OnC21Changed(C21);
            OnC22Changed(C22);
            OnC23Changed(C23);
            OnC30Changed(C30);
            OnC31Changed(C31);
            OnC32Changed(C32);
            OnC33Changed(C33);
            OnC34Changed(C34);
            OnC35Changed(C35);
            OnC36Changed(C36);
            OnC37Changed(C37);
            OnC38Changed(C38);
            OnC41Changed(C41);
            OnC43Changed(C43);
            OnC46Changed(C46);
        }

        private async Task Gb_OnReceivedData(byte[] data)
        {
            Content += $"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} GB Rec:<-- {Encoding.UTF8.GetString(data)}\r\n";
            await Task.CompletedTask;
        }

        private async Task Gb_OnSentData(byte[] data)
        {
            Content += $"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} GB Sent:<-- {Encoding.UTF8.GetString(data)}";
            await Task.CompletedTask;
        }

        private async Task Gb_OnDisconnect()
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                IsConnect = false;
                //Exception = "通讯断连,等待连接...";
            });
        }

        private async Task Gb_OnConnect()
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
                _gb!.OnSetOverTimeAndReCount += Gb_OnSetOverTimeAndReCount;
            }
            else
            {
                _gb!.OnSetOverTimeAndReCount -= Gb_OnSetOverTimeAndReCount;
            }
        }

        private async Task Gb_OnSetOverTimeAndReCount((int OverTime, int ReCount, HJ212.Model.RspInfo RspInfo) objects)
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
                await _gb!.AskSetSystemTime(PolId_C4, TimeOut_C4);
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

        #region C14、29
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
        private bool _ReturnValue_C20;

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
            return await Task.FromResult((historyDatas, ReturnValue_C20, TimeOut_C16));
        }
        #endregion

        #region C21
        [ObservableProperty]
        private bool _C21;
        [ObservableProperty]
        private int _Count_C21 = 2;
        [ObservableProperty]
        private bool _ReturnValue_C21;

        partial void OnC21Changed(bool value)
        {
            if (value)
            {
                _gb!.OnGetHourData += MainViewModel_OnGetHourData;
            }
            else
            {
                _gb!.OnGetHourData -= MainViewModel_OnGetHourData;
            }
        }

        private async Task<(List<HistoryData> HistoryDatas, bool ReturnValue, int? Timeout)> MainViewModel_OnGetHourData((DateTime BeginTime, DateTime EndTime, RspInfo RspInfo) objects)
        {
            var historyDatas = new List<HistoryData>();
            var random = new Random();
            for (int i = 0; i < Count_C21; i++)
            {
                double randomPercentage = random.NextDouble() * 2 * Fluctuation_C17 - Fluctuation_C17;
                historyDatas.Add(new HistoryData(objects.BeginTime.AddHours(i), HourDatas.Select(_ => new StatisticsData(_.Name) { Cou = _.Cou.HasValue ? ((float)_.Cou * (1 + randomPercentage / 100)).ToString("0.00") : null, Min = (_.Min * (1 + randomPercentage / 100)).ToString("0.00"), Avg = (_.Avg * (1 + randomPercentage / 100)).ToString("0.00"), Max = (_.Max * (1 + randomPercentage / 100)).ToString("0.00"), Flag = _.Flag }).ToList()));
            }
            return await Task.FromResult((historyDatas, ReturnValue_C21, TimeOut_C17));
        }
        #endregion

        #region C22
        [ObservableProperty]
        private bool _C22;
        [ObservableProperty]
        private int _Count_C22 = 2;
        [ObservableProperty]
        private bool _ReturnValue_C22;
        partial void OnC22Changed(bool value)
        {
            if (value)
            {
                _gb!.OnGetDayData += MainViewModel_OnGetDayData;
            }
            else
            {
                _gb!.OnGetDayData -= MainViewModel_OnGetDayData;
            }
        }

        private async Task<(List<HistoryData> HistoryDatas, bool ReturnValue, int? Timeout)> MainViewModel_OnGetDayData((DateTime BeginTime, DateTime EndTime, RspInfo RspInfo) objects)
        {
            var historyDatas = new List<HistoryData>();
            var random = new Random();
            for (int i = 0; i < Count_C22; i++)
            {
                double randomPercentage = random.NextDouble() * 2 * Fluctuation_C18 - Fluctuation_C18;
                historyDatas.Add(new HistoryData(objects.BeginTime.AddDays(i), DayDatas.Select(_ => new StatisticsData(_.Name) { Cou = _.Cou.HasValue ? ((float)_.Cou * (1 + randomPercentage / 100)).ToString("0.00") : null, Min = (_.Min * (1 + randomPercentage / 100)).ToString("0.00"), Avg = (_.Avg * (1 + randomPercentage / 100)).ToString("0.00"), Max = (_.Max * (1 + randomPercentage / 100)).ToString("0.00"), Flag = _.Flag }).ToList()));
            }
            return await Task.FromResult((historyDatas, ReturnValue_C22, TimeOut_C18));
        }
        #endregion

        #region C23
        [ObservableProperty]
        private bool _C23;
        [ObservableProperty]
        private int _Count_C23 = 2;
        partial void OnC23Changed(bool value)
        {
            if (value)
            {
                _gb!.OnGetRunningTimeData += MainViewModel_OnGetRunningTimeData;
            }
            else
            {
                _gb!.OnGetRunningTimeData -= MainViewModel_OnGetRunningTimeData;
            }
        }

        private async Task<List<RunningTimeHistory>> MainViewModel_OnGetRunningTimeData((DateTime BeginTime, DateTime EndTime, RspInfo RspInfo) objects)
        {
            var historyDatas = new List<RunningTimeHistory>();
            for (int i = 0; i < Count_C23; i++)
            {
                historyDatas.Add(new(DateTime.Now.AddDays(i), RunningTimeDatas.Select(_ => new RunningTimeData(_.Name, _.RT)).ToList()));
            }
            return await Task.FromResult(historyDatas);
        }
        #endregion

        #region C24
        [ObservableProperty]
        private string _DataTime_C24 = DateTime.Now.ToString("yyyyMMddHHmmss");
        [ObservableProperty]
        private string _RestartTime_C24 = DateTime.Now.ToString("yyyyMMddHHmmss");
        [ObservableProperty]
        private int _TimeOut_C24 = 120000;
        [RelayCommand]
        private async Task C24TestAsync()
        {
            if (!DateTime.TryParseExact(DataTime_C24, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var dataTime))
            {
                MessageBox.Show($"DataTime Error");
                return;
            }
            if (!DateTime.TryParseExact(RestartTime_C24, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var restartTime))
            {
                MessageBox.Show($"RestartTime Error");
                return;
            }
            try
            {
                await _gb!.UploadAcquisitionDeviceRestartTime(dataTime, restartTime, TimeOut_C24);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("请求超时");
            }
        }
        #endregion

        #region C25
        [ObservableProperty]
        private string _DataTime_C25 = DateTime.Now.ToString("yyyyMMddHHmmss");
        [ObservableProperty]
        private float _LARtd;
        [ObservableProperty]
        private int _TimeOut_C25 = 120000;
        [RelayCommand]
        private async Task C25TestAsync()
        {
            if (!DateTime.TryParseExact(DataTime_C25, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var dataTime))
            {
                MessageBox.Show($"DataTime Error");
                return;
            }
            try
            {
                await _gb!.UploadRealTimeNoiseLevel(dataTime, LARtd, TimeOut_C25);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("请求超时");
            }
        }
        #endregion

        #region C26
        [ObservableProperty]
        private string _DataTime_C26 = DateTime.Now.ToString("yyyyMMddHHmmss");
        [ObservableProperty]
        private int _TimeOut_C26 = 120000;
        [RelayCommand]
        private async Task C26TestAsync()
        {
            if (!DateTime.TryParseExact(DataTime_C26, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var dataTime))
            {
                MessageBox.Show($"DataTime Error");
                return;
            }
            try
            {
                await _gb!.UploadMinuteNoiseLevel(dataTime, NoiseLevelDatas.Select(_ => new NoiseLevelData(_.Name, _.Data)).ToList(), TimeOut_C26);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("请求超时");
            }
        }
        #endregion

        #region C27
        [ObservableProperty]
        private string _DataTime_C27 = DateTime.Now.ToString("yyyyMMddHHmmss");
        [ObservableProperty]
        private int _TimeOut_C27 = 120000;
        [RelayCommand]
        private async Task C27TestAsync()
        {
            if (!DateTime.TryParseExact(DataTime_C27, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var dataTime))
            {
                MessageBox.Show($"DataTime Error");
                return;
            }
            try
            {
                await _gb!.UploadHourNoiseLevel(dataTime, HourNoiseLevelDatas.Select(_ => new NoiseLevelData(_.Name, _.Data)).ToList(), TimeOut_C27);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("请求超时");
            }
        }
        #endregion

        #region C28
        [ObservableProperty]
        private string _DataTime_C28 = DateTime.Now.ToString("yyyyMMddHHmmss");
        [ObservableProperty]
        private int _TimeOut_C28 = 120000;
        [RelayCommand]
        private async Task C28TestAsync()
        {
            if (!DateTime.TryParseExact(DataTime_C28, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var dataTime))
            {
                MessageBox.Show($"DataTime Error");
                return;
            }
            try
            {
                await _gb!.UploadDayNoiseLevel(dataTime, DayNoiseLevelDatas.Select(_ => new NoiseLevelData_Day(_.Name, _.Data, _.DayData, _.NightData)).ToList(), TimeOut_C28);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("请求超时");
            }
        }
        #endregion

        #region C30
        [ObservableProperty]
        private bool _C30;
        partial void OnC30Changed(bool value)
        {
            if (value)
            {
                _gb!.OnCalibrate += MainViewModel_OnCalibrate;
            }
            else
            {
                _gb!.OnCalibrate -= MainViewModel_OnCalibrate;
            }
        }

        private async Task MainViewModel_OnCalibrate((string PolId, RspInfo RspInfo) objects)
        {
            MessageBox.Show($"{objects.PolId} 零点校准量程校准");
            await Task.CompletedTask;
        }
        #endregion

        #region C31
        [ObservableProperty]
        private bool _C31;
        partial void OnC31Changed(bool value)
        {
            if (value)
            {
                _gb!.OnRealTimeSampling += MainViewModel_OnRealTimeSampling;
            }
            else
            {
                _gb!.OnRealTimeSampling -= MainViewModel_OnRealTimeSampling;
            }
        }

        private async Task MainViewModel_OnRealTimeSampling((string PolId, RspInfo RspInfo) objects)
        {
            MessageBox.Show($"{objects.PolId} 即时采样");
            await Task.CompletedTask;
        }
        #endregion

        #region C32
        [ObservableProperty]
        private bool _C32;
        partial void OnC32Changed(bool value)
        {
            if (value)
            {
                _gb!.OnStartCleaningOrBlowback += MainViewModel_OnStartCleaningOrBlowback;
            }
            else
            {
                _gb!.OnStartCleaningOrBlowback -= MainViewModel_OnStartCleaningOrBlowback;
            }
        }

        private async Task MainViewModel_OnStartCleaningOrBlowback((string PolId, RspInfo RspInfo) objects)
        {
            MessageBox.Show($"{objects.PolId} 启动清洗/反吹");
            await Task.CompletedTask;
        }
        #endregion

        #region C33
        [ObservableProperty]
        private bool _C33;
        partial void OnC33Changed(bool value)
        {
            if (value)
            {
                _gb!.OnComparisonSampling += MainViewModel_OnComparisonSampling;
            }
            else
            {
                _gb!.OnComparisonSampling -= MainViewModel_OnComparisonSampling;
            }
        }

        private async Task MainViewModel_OnComparisonSampling((string PolId, RspInfo RspInfo) objects)
        {
            MessageBox.Show($"{objects.PolId} 比对采样");
            await Task.CompletedTask;
        }
        #endregion

        #region C34
        [ObservableProperty]
        private bool _C34;
        [ObservableProperty]
        private string _DataTime_C34 = DateTime.Now.ToString("yyyyMMddHHmmss");
        [ObservableProperty]
        private int _VaseNo = 1;
        partial void OnC34Changed(bool value)
        {
            if (value)
            {
                _gb!.OnOutOfStandardRetentionSample += MainViewModel_OnOutOfStandardRetentionSample;
            }
            else
            {
                _gb!.OnOutOfStandardRetentionSample -= MainViewModel_OnOutOfStandardRetentionSample;
            }
        }

        private async Task<(DateTime DataTime, int VaseNo)> MainViewModel_OnOutOfStandardRetentionSample(RspInfo objects)
        {
            if (!DateTime.TryParseExact(DataTime_C34, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var dataTime))
            {
                MessageBox.Show($"DataTime Error");
                dataTime = DateTime.Now;
            }
            return await Task.FromResult((dataTime, VaseNo));
        }
        #endregion

        #region C35
        [ObservableProperty]
        private bool _C35;
        partial void OnC35Changed(bool value)
        {
            if (value)
            {
                _gb!.OnSetSamplingPeriod += MainViewModel_OnSetSamplingPeriod;
            }
            else
            {
                _gb!.OnSetSamplingPeriod -= MainViewModel_OnSetSamplingPeriod;
            }
        }

        private async Task MainViewModel_OnSetSamplingPeriod((string PolId, TimeOnly CstartTime, int Ctime, RspInfo RspInfo) objects)
        {
            CstartTime = objects.CstartTime.ToString("HHmmss");
            Ctime = objects.Ctime;
            await Task.CompletedTask;
        }
        #endregion

        #region C36
        [ObservableProperty]
        private bool _C36;
        partial void OnC36Changed(bool value)
        {
            if (value)
            {
                _gb!.OnGetSamplingPeriod += MainViewModel_OnGetSamplingPeriod;
            }
            else
            {
                _gb!.OnGetSamplingPeriod -= MainViewModel_OnGetSamplingPeriod;
            }
        }

        private async Task<(TimeOnly CstartTime, int Ctime)> MainViewModel_OnGetSamplingPeriod((string PolId, RspInfo RspInfo) objects)
        {
            if (!TimeOnly.TryParseExact(CstartTime, "HHmmss", null, System.Globalization.DateTimeStyles.None, out var timeOnly))
            {
                MessageBox.Show($"DataTime Error");
                timeOnly = new TimeOnly(DateTime.Now.Ticks);
            }
            return await Task.FromResult((timeOnly, Ctime));
        }
        #endregion

        #region C37
        [ObservableProperty]
        private bool _C37;
        partial void OnC37Changed(bool value)
        {
            if (value)
            {
                _gb!.OnGetSampleExtractionTime += MainViewModel_OnGetSampleExtractionTime;
            }
            else
            {
                _gb!.OnGetSampleExtractionTime -= MainViewModel_OnGetSampleExtractionTime;
            }
        }

        private async Task<int> MainViewModel_OnGetSampleExtractionTime((string PolId, RspInfo RspInfo) objects)
        {
            var rs = SampleExtractionTime.FirstOrDefault(_ => _.PolId == objects.PolId);
            return await Task.FromResult(rs?.Stime ?? default);
        }
        #endregion

        #region C38
        [ObservableProperty]
        private bool _C38;
        partial void OnC38Changed(bool value)
        {
            if (value)
            {
                _gb!.OnGetSN += MainViewModel_OnGetSN;
            }
            else
            {
                _gb!.OnGetSN -= MainViewModel_OnGetSN;
            }
        }

        private async Task<string> MainViewModel_OnGetSN((string PolId, RspInfo RspInfo) objects)
        {
            var rs = SNInfos.FirstOrDefault(_ => _.PolId == objects.PolId);
            return await Task.FromResult(rs?.SN ?? "");
        }
        #endregion

        #region C39
        [ObservableProperty]
        private string _PolId_C39 = "w01018";
        [ObservableProperty]
        private string _SN_C39 = "";
        [ObservableProperty]
        private int _TimeOut_C39 = 120000;
        [RelayCommand]
        private async Task C39TestAsync()
        {
            try
            {
                await _gb!.UploadSN(DateTime.Now, PolId_C39, SN_C39, TimeOut_C39);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("请求超时");
            }
        }
        #endregion

        #region C40
        [ObservableProperty]
        private string _DataTime_C40 = DateTime.Now.ToString("yyyyMMddHHmmss");
        [ObservableProperty]
        private string? _PolId_C40;
        [ObservableProperty]
        private string _Info_C40 = "";
        [ObservableProperty]
        private int _TimeOut_C40 = 120000;
        [RelayCommand]
        private async Task C40TestAsync()
        {
            if (PolId_C40 == "") PolId_C40 = null;
            if(!DateTime.TryParseExact(DataTime_C40, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var dataTime))
            {
                MessageBox.Show($"DataTime Error");
                return;
            }
            try
            {
                await _gb!.UploadLog(dataTime, PolId_C40, Info_C40, TimeOut_C40);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("请求超时");
            }
        }
        #endregion

        #region C41
        [ObservableProperty]
        private bool _C41;
        partial void OnC41Changed(bool value)
        {
            if (value)
            {
                _gb!.OnGetLogInfos += MainViewModel_OnGetLogInfos;
            }
            else
            {
                _gb!.OnGetLogInfos -= MainViewModel_OnGetLogInfos;
            }
        }

        private async Task<List<LogInfo>> MainViewModel_OnGetLogInfos((string? PolId, DateTime BeginTime, DateTime EndTime, RspInfo RspInfo) objects)
        {
            return await Task.FromResult(LogInfos.Where(_ => _.PolId == objects.PolId && _.DataTime < objects.EndTime && _.DataTime > objects.BeginTime).Select(_ => new LogInfo(_.Info, _.DataTime) { PolId = (_.PolId == "" ? null : _.PolId) }).ToList());
        }
        #endregion

        #region C42、44
        [ObservableProperty]
        private string _DataTime_C42 = DateTime.Now.ToString("yyyyMMddHHmmss");
        [ObservableProperty]
        private string _PolId_C42 = "";
        [ObservableProperty]
        private int _TimeOut_C42 = 120000;
        [RelayCommand]
        private async Task C42TestAsync()
        {
            if (!DateTime.TryParseExact(DataTime_C42, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var dataTime))
            {
                MessageBox.Show($"DataTime Error");
                return;
            }
            try
            {
                await _gb!.UploadInfo(dataTime, PolId_C42, DeviceInfos.Select(_ => new DeviceInfo(_.InfoId, _.Info)).ToList(), TimeOut_C42);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("请求超时");
            }
        }
        #endregion

        #region C43、45
        [ObservableProperty]
        private bool _C43;
        partial void OnC43Changed(bool value)
        {
            if (value)
            {
                _gb!.OnGetInfo += MainViewModel_OnGetInfo;
            }
            else
            {
                _gb!.OnGetInfo -= MainViewModel_OnGetInfo;
            }
        }

        private async Task<(DateTime DataTime, List<DeviceInfo> DeviceInfos)> MainViewModel_OnGetInfo((string PolId, string InfoId, RspInfo RspInfo) objects)
        {
            var rs = DeviceParameterInfos.FirstOrDefault(_ => _.PolId == objects.PolId && _.InfoId == objects.InfoId);
            if (!DateTime.TryParseExact(rs?.DataTime, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var dataTime))
            {
                MessageBox.Show($"DataTime Error");
                dataTime = DateTime.Now;
            }
            return await Task.FromResult((dataTime, rs == null ? [] : new List<DeviceInfo>() { new(rs.InfoId, rs.Info) }));
        }
        #endregion

        #region C46
        [ObservableProperty]
        private bool _C46;
        [ObservableProperty]
        private string _PolId_C46 = "";
        [ObservableProperty]
        private string _InfoId_C46 = "";
        [ObservableProperty]
        private string _Info_C46 = "";
        partial void OnC46Changed(bool value)
        {
            if (value)
            {
                _gb!.OnSetInfo += MainViewModel_OnSetInfo;
            }
            else
            {
                _gb!.OnSetInfo -= MainViewModel_OnSetInfo;
            }
        }

        private async Task MainViewModel_OnSetInfo((string PolId, string InfoId, string Info, RspInfo RspInfo) objects)
        {
            PolId_C46 = objects.PolId;
            InfoId_C46 = objects.InfoId;
            Info_C46 = objects.Info;
            await Task.CompletedTask;
        }
        #endregion
    }

    internal class StatusMessage(Connection value) : ValueChangedMessage<Connection>(value) { }
}
