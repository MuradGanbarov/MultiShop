namespace MultiShop.Areas.MultiShopAdmin.ViewModels
{
    public class PaginationVM<T> where T : class, new()
    {
        public int CurrentPage { get; set; }
        public double TotalPage { get; set; }
        public List<T> Items { get; set; }
    }
}
