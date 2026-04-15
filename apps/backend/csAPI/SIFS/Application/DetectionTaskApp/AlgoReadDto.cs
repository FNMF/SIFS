using SIFS.Domain.Enum;

namespace SIFS.Application.DetectionTaskApp
{
    public class AlgoReadDto
    {
        public Guid Guid { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Status { get; set; }
        public int? Level { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
