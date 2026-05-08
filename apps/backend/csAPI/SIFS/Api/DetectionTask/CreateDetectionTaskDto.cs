namespace SIFS.Api.DetectionTask
{
    public class CreateDetectionTaskDto
    {
        public List<ImageInputDto> Images { get; set; } = new();
        public List<int> AlgoModelIds { get; set; } = new();
        public int? Level { get; set; }

        public class ImageInputDto
        {
            public int Order { get; set; }
            public IFormFile File { get; set; } = null!;
        }
    }
}
