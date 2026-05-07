namespace SIFS.Application.Identity
{
    public class UserReadDto
    {
        public string Account { get; set; }
        public Guid Id { get; set; }
        public string Username => Account;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }
}
