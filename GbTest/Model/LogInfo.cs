namespace GbTest.Model
{
    class LogInfo
    {
        /// <summary>污染物ID</summary>
        public string? PolId { get; set; }
        /// <summary>时间</summary>
        public DateTime DataTime { get; set; }
        /// <summary>日志内容</summary>
        public string Info { get; set; } = null!;
    }
}
