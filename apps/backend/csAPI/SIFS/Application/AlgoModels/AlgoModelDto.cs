namespace SIFS.Application.AlgoModels
{
    public class AlgoModelDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public string ApiUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ReservedJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
