namespace SIFS.Shared.Results
{
    public class APIResult<T> : Result<T>
    {
        public long ElapsedMilliseconds { get; set; }  // 接口耗时
        public string TraceId { get; set; }            // 请求跟踪ID（用于日志链路）
    }
}
