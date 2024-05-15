namespace GbTest.Model
{
    /// <summary>
    /// 噪声日数据
    /// </summary>
    internal class NoiseLevelData_Day
    {
        /// <summary>污染物名称</summary>
        public string Name { get; set; } = null!;
        /// <summary>昼夜等效升级</summary>
        public string Data { get; set; } = null!;
        /// <summary>昼间等效升级</summary>
        public string DayData { get; set; } = null!;
        /// <summary>夜间等效升级</summary>
        public string NightData { get; set; } = null!;
    }
}
