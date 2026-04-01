using System.Collections.Generic;
using System.Security.Cryptography;

namespace SIFS.Shared.Helpers
{
    public class HashHelper
    {
        static private int saltSize = 16; //盐的大小
        static private int hashSize = 32; //加密字段大小
        static private int iterations = 3; //sha256迭代次数（过大消耗性能）

        static public HashSalt HashandSalt(string password) //首次创建账户或修改密码时，对密码进行sha256加盐加密
        {
            var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[saltSize];
            rng.GetBytes(salt);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(hashSize);

            var hashed = new HashSalt();
            hashed.Hash = Convert.ToBase64String(hash);
            hashed.Salt = Convert.ToBase64String(salt);

            return hashed;
        }

        static public string Hashing(string password, string salt) //验证密码是否正确时，对密码进行hash处理
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(hashSize);

            return Convert.ToBase64String(hash);
        }
    }
}
