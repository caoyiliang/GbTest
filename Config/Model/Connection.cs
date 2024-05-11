using CommunityToolkit.Mvvm.ComponentModel;
using DataPairs;
using DataPairs.Interfaces;
using System.IO.Ports;

namespace Config.Model
{
    public partial class Connection : ObservableObject
    {
        [ObservableProperty]
        private CommunicationType _Type = CommunicationType.SerialPort;
        [ObservableProperty]
        private string _PortName = "COM1";
        [ObservableProperty]
        private int _BaudRate = 9600;
        [ObservableProperty]
        private int _DataBits = 8;
        [ObservableProperty]
        private StopBits _StopBits = StopBits.One;
        [ObservableProperty]
        private Parity _Parity = Parity.None;
        [ObservableProperty]
        private bool _DTR = false;
        [ObservableProperty]
        private bool _RTS = false;

        [ObservableProperty]
        private string _HostName = "127.0.0.1";
        [ObservableProperty]
        private int _Port = 2756;

        private static readonly IDataPair<Connection> _pair = new DataPair<Connection>(nameof(Connection));

        public async Task InitAsync()
        {
            var data = await _pair.TryGetValueAsync();
            foreach (var item in data.GetType().GetProperties())
            {
                GetType().GetProperty(item.Name)!.SetValue(this, item.GetValue(data));
            }
        }

        public async Task TrySaveChangeAsync()
        {
            await _pair.TryInitOrUpdateAsync(this);
        }

        public override string ToString()
        {
            return Type switch
            {
                CommunicationType.SerialPort => $"当前[连接方式:{Type}] [串口名:{PortName}][波特率:{BaudRate}][数据位:{DataBits}][停止位:{StopBits}][校验位:{Parity}][DTR:{DTR}][RTS:{RTS}]",
                CommunicationType.TcpClient => $"当前[连接方式:{Type}] [远端Ip:{HostName}][端口:{Port}]",
                _ => throw new NotImplementedException(),
            };
        }
    }
}
