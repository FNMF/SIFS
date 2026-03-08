namespace SIFS.Shared.Results
{
    public class Result<T> : Result
    {
        public T Data { get; set; }

        public static Result<T> Success(T data, string message = "成功") =>
            new Result<T> { Code = ResultCode.Success, Message = message, Data = data };

        public static new Result<T> Fail(ResultCode code, string message) =>
            new Result<T> { Code = code, Message = message };
    }
}
