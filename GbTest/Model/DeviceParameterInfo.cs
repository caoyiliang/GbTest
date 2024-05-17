namespace GbTest.Model
{
    /// <summary>
    /// 设备信息
    /// </summary>
    class DeviceParameterInfo
    {
        /// <summary>数据时间</summary>
        public string DataTime { get; set; } = null!;
        /// <summary>污染物编码</summary>
        public string PolId { get; set; } = null!;
        /// <summary>参数名</summary>
        public string InfoId { get; set; } = null!;
        /// <summary>参数值</summary>
        public string Info { get; set; } = null!;
    }
}
