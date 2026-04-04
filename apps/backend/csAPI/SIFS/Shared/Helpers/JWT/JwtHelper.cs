using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SIFS.Shared.Helpers.JWT
{
    public class JwtHelper
    {
        private readonly JwtSettings _settings;

        public JwtHelper(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
            if (_settings.SecretKey == null)
                throw new Exception("JwtSettings.SecretKey is NULL");
        }

        private SigningCredentials GetCredentials()
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_settings.SecretKey));

            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        private JwtSecurityToken CreateToken(IEnumerable<Claim> claims)
        {
            return new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpiresMinutes),
                signingCredentials: GetCredentials()
            );
        }

        public string UserGenerateToken(Guid uuid, string? account)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, uuid.ToString())
            };


            if (!string.IsNullOrEmpty(account))
            {
                claims.Add(new Claim("Account", account));
            }

            var token = CreateToken(claims);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
