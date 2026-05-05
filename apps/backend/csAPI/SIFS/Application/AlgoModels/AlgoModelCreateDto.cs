namespace SIFS.Application.AlgoModels
{
    public class AlgoModelCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public object? ReservedJson { get; set; }
    }
}
