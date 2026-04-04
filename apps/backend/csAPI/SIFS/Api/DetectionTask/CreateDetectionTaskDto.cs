namespace SIFS.Api.DetectionTask
{
    public class CreateDetectionTaskDto
    {
        public List<ImageInputDto> Images { get; set; }
        public class ImageInputDto
        {
            public int Order { get; set; }
            public IFormFile File { get; set; }
        }
        
    }
}
