namespace SIFS.Infrastructure.External
{
    public class DetectionResult
    {
        public string? Type { get; set; }
        public bool? IsFake { get; set; }
        public double? Confidence { get; set; }
        public string? MaskUrl { get; set; }
    }
}
