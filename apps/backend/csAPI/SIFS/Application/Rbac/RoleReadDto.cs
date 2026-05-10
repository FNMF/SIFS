namespace SIFS.Application.Rbac
{
    public class RoleReadDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
