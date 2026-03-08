namespace SIFS.Shared.Results
{
    public class Paged<T>
    {
        public List<T> Data { get; set; } = new();
        public int Total { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int TotalPages =>
            PageSize == 0 ? 0 : (int)Math.Ceiling((double)Total / PageSize);
    }
}
