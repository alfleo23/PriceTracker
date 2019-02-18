namespace PriceTracker.ScrapeEngine
{
    public class Retailer
    {
        /*public Retailer(string url, string headingSelector, string priceSelector)
        {
            _searchURLSelector = url;
            _productHeadingSelector = headingSelector;
            _productPriceSelector = priceSelector;
        }*/

        protected Retailer()
        {
            
        }

        protected string SearchUrlSelector { get; set; }

        protected string ProductHeadingSelector { get; set; }

        protected string ProductPriceSelector { get; set; }
    }
}