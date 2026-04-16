namespace SIFS.Application.DetectionTaskApp
{
    public class DetectionTaskDetailDto
    {
        public Guid Guid { get; set; }

        public List<string> ImageUrls { get; set; } = new();

        public string PreviewImageUrl { get; set; } = string.Empty;

        public int ImageCount { get; set; }

        public int SubTaskCount { get; set; }

        public int CompletedSubTaskCount { get; set; }

        public decimal Completion { get; set; }

        public int? Level { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<AlgoReadDto> AlgoTasks { get; set; } = new();
    }
}
