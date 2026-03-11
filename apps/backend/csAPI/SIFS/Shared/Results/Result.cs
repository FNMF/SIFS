namespace SIFS.Shared.Results
{
    public class Result
    {
        public ResultCode Code { get; set; }
        public string Message { get; set; }

        public bool IsSuccess => Code == ResultCode.Success;

        public static Result Success(string message = "成功") =>
            new Result { Code = ResultCode.Success, Message = message };

        public static Result Fail(ResultCode code, string message) =>
            new Result { Code = code, Message = message };
    }
}
