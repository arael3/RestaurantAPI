namespace RestaurantAPI.Models
{
    public class PageResult<T>
    {
        public PageResult(List<T> items, int totalCount, int pageSize, int pageNumber)
        {
            Items = items;
            ItemsFrom = pageSize * (pageNumber - 1) + 1;
            ItemsTo = ItemsFrom + pageSize - 1;
            TotalItemsCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        public List<T> Items { get; set; }
        public int TotalPages { get; set; }
        public int ItemsFrom { get; set; }
        public int ItemsTo { get; set; }
        public int TotalItemsCount { get; set; }
    }
}
