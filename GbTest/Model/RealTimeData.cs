namespace GbTest.Model
{
    /// <summary>
    /// 污染物实时数据包
    /// </summary>
    internal class RealTimeData
    {
        /// <summary>污染物名称</summary>
        public string Name { get; set; } = null!;
        /// <summary>污染物实时数据</summary>
        public float Rtd { get; set; }
        /// <summary>污染物实时数据标记</summary>
        public string? Flag { get; set; }
        /// <summary>污染物实时数据采样时间</summary>
        public string? SampleTime { get; set; }
        /// <summary>污染物对应在线监控（监测）仪器仪表的设备标志</summary>
        public string? EFlag { get; set; }
    }
}
