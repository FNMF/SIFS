namespace SIFS.Application.Identity
{
    public class LoginTokenResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public UserReadDto UserReadDto { get; set; }
    }
}
