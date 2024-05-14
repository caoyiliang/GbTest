namespace GbTest.Model
{
    /// <summary>
    /// 统计数据
    /// </summary>
    /// <param name="name">污染物名称</param>
    internal class StatisticsData
    {
        /// <summary>污染物名称</summary>
        public string Name { get; set; } = null!;
        /// <summary>污染物指定时间内累计值</summary>
        public float? Cou { get; set; }
        /// <summary>污染物指定时间内最小值</summary>
        public float Min { get; set; }
        /// <summary>污染物指定时间内平均值</summary>
        public float Avg { get; set; }
        /// <summary>污染物指定时间内最大值</summary>
        public float Max { get; set; }
        /// <summary>监测仪器数据标记</summary>
        public string? Flag { get; set; }
    }
}
