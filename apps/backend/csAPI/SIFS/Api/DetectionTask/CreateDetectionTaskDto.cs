using SIFS.Infrastructure.External;

namespace SIFS.Api.DetectionTask
{
    public class CreateDetectionTaskDto
    {
        public List<ImageInputDto> Images { get; set; }
        public List<AiServiceType> Types { get; set; }
        public int? Level { get; set; }
        public class ImageInputDto
        {
            public int Order { get; set; }
            public IFormFile File { get; set; }
        }

    }
}
