namespace SIFS.Shared.Results
{
    public enum ResultCode
    {
        Success = 200,              //成功
        Unauthorized = 401,         //未授权
        Forbidden = 403,            //权限不足
        NotFound = 404,             //未找到资源
        ValidationError = 422,      //参数验证失败
        ServerError = 500,          //服务器内部错误

        BusinessError = 600,        //自定义业务异常
        LoginVerifyError = 601,     //用户名或密码错误
        NotExist = 602,             //用户不存在
        InfoExpire = 603,           //信息过期
        InfoExist = 604,            //信息已存在
        TokenInvalid = 605,         //Token无效
        InvalidInput = 606,         //输入不合法
    }
}
