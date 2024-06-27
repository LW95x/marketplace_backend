using Marketplace.DataAccess.Entities;

namespace Marketplace.DataAccess.DbContexts
{
    public static class DatabaseSeed
    {
        public void Seed(MarketplaceContext context)
        {
            context.Products.AddRange(
                new Product("Deckchair", "Furniture", 50M, "Very swingy", 10)
        );
        }
    }
}
