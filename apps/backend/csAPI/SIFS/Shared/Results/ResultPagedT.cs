namespace SIFS.Shared.Results
{
    public class ResultPaged<T> : Result
    {
        public Paged<T> Page { get; set; }

        public static ResultPaged<T> Success(Paged<T> page, string message = "成功") =>
        new ResultPaged<T> { Code = ResultCode.Success, Message = message, Page = page };
    }
}
