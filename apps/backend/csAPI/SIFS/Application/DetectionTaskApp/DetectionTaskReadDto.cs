namespace SIFS.Application.DetectionTaskApp
{
    public class DetectionTaskReadDto
    {
        public Guid Guid { get; set; }

        public int SubTaskCount { get; set; }

        public int CompletedSubTaskCount { get; set; }

        public decimal Completion { get; set; }
        public string Url { get; set; } = string.Empty;
        public int? Level { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
