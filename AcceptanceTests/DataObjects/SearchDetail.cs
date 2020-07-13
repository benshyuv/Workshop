
namespace AcceptanceTests.DataObjects
{
    public class SearchDetail
    {
        public double? FilterItemRank { get; set; }
        public double? FilterMinPrice { get; set; }
        public double? FilterMaxPrice { get; set; }
        public double? FilterStoreRank { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string[] Keywords { get; set; }

        public SearchDetail(double? filterItemRank, double? filterMinPrice, double? filterMaxPrice,
            double? filterStoreRank, string name, string category, string[] keywords)
        {
            FilterItemRank = filterItemRank;
            FilterMinPrice = filterMinPrice;
            FilterMaxPrice = filterMaxPrice;
            FilterStoreRank = filterStoreRank;
            Name = name;
            Category = category;
            Keywords = keywords;
        }


    }
}
