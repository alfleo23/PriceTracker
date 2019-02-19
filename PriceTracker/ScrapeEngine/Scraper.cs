namespace PriceTracker.ScrapeEngine
{
    public class Scraper
    {
        protected Scraper()
        {
            
        }

        protected string SearchUrlSelector { get; set; }

        protected string ProductHeadingSelector { get; set; }

        protected string ProductPriceSelector { get; set; }
        
        protected string ProductLinkSelector { get; set; }
    }
}