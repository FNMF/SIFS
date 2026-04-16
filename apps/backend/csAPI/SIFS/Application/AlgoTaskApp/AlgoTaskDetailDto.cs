namespace SIFS.Application.AlgoTaskApp
{
    public class AlgoTaskDetailDto
    {
        public Guid Guid { get; set; }

        public string OriginImageUrl { get; set; } = string.Empty;

        public string MaskUrl { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public int Status { get; set; }

        public string StatusText { get; set; } = string.Empty;

        public int? Level { get; set; }

        public bool? IsFake { get; set; }

        public decimal? Confidence { get; set; }

        public Dictionary<string, object>? ExtraParams { get; set; }
    }
}
