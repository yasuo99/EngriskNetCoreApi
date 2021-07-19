namespace Application.Helper
{
    public class SubjectParams
    {
        private int MaxPageSize { get; set; } = 50;
        public int CurrentPage { get; set; } = 1;
        private int pageSize = 10;
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = (value > MaxPageSize) ? MaxPageSize : value; }
        }
    }
}