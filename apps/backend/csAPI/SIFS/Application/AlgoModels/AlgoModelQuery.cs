namespace SIFS.Application.AlgoModels
{
    public class AlgoModelQuery
    {
        public string? Name { get; set; }
        public bool? Enabled { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
