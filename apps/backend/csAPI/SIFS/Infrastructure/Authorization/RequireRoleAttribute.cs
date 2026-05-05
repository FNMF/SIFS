using Microsoft.AspNetCore.Mvc;

namespace SIFS.Infrastructure.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireRoleAttribute : TypeFilterAttribute
    {
        public RequireRoleAttribute(string roleName)
            : base(typeof(RequireRoleFilter))
        {
            Arguments = new object[] { roleName };
        }
    }
}
