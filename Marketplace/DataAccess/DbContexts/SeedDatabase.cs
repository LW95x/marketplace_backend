using Marketplace.DataAccess.DbContexts;
using Marketplace.DataAccess.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.X86;

namespace Marketplace.DataAccess.DbContexts
{
    public static class SeedDatabase
    {
        public static void Seed(MarketplaceContext context, IServiceProvider serviceProvider)
        {
            // Users
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            User user1 = new User()
            {
                Id = "7523CFD7-498F-444B-A8E4-BD29DB3FA9CC",
                UserName = "Billy",
                Email = "billybob@outlook.com"
            };

            User user2 = new User()
            {
                Id = "A90AF71F-9225-407D-A0E7-E8CE41B54412",
                UserName = "Robert",
                Email = "robertjones@hotmail.co.uk"
            };

            User user3 = new User()
            {
                Id = "741A19E7-2241-4B77-826A-B67924DDA8EE",
                UserName = "Jane",
                Email = "janedoe@live.com"
            };

            var addUser1 = userManager.CreateAsync(user1, "Billybob-123").Result;
            if (!addUser1.Succeeded)
            {
                throw new Exception("Failed to create user #1 during seeding.");
            }

            var addUser2 = userManager.CreateAsync(user2, "Roberto-123").Result;
            if (!addUser2.Succeeded)
            {
                throw new Exception("Failed to create user #2 during seeding.");
            }

            var addUser3 = userManager.CreateAsync(user3, "Janespass-123").Result;
            if (!addUser3.Succeeded)
            {
                throw new Exception("Failed to create user #3 during seeding.");
            }

            // Products

            Guid productId1 = new Guid("9B23A145-039B-4561-B59E-3DED02F712FD");
            Guid productId2 = new Guid("12C9D57C-1578-4CCD-AEB5-1E082D668E70");
            Guid productId3 = new Guid("684B656C-B7C2-405B-8F73-27E1A486A715");
            Guid productId4 = new Guid("8547092F-ADE0-4450-ABEA-41E475F7C4DF");
            Guid productId5 = new Guid("54785B42-2735-447E-83FA-E0F0227D1B2E");
            Guid productId6 = new Guid("8B35097D-CAC4-47CC-805B-8BB533CB722F");
            Guid productId7 = new Guid("71DA0C50-80C4-42BB-8DB1-22527E5E19D8");
            Guid productId8 = new Guid("770FF4BD-9355-446C-854B-5C9E46978B6E");
            Guid productId9 = new Guid("07573B01-22CF-4E64-A310-E60BDFAB8CDC");
            Guid productId10 = new Guid("1CA45E8E-030C-4E9B-A1C4-BEF6D1863593");
            

            Product product1 = new Product("Deckchair", "Furniture", 50M, "Very swingy", 10)
            {
                Id = productId1,
                SellerId = user1.Id,
                SellerName = user1.UserName
            };
            Product product2 = new Product("Football", "Sport", 20M, "Bouncy", 25)
            {
                Id = productId2,
                SellerId = user1.Id,
                SellerName = user1.UserName
            };
            Product product3 = new Product("Box of Flapjacks", "Food & Drink", 10M, "Tasty", 100)
            {
                Id = productId3,
                SellerId = user2.Id,
                SellerName = user2.UserName
            };
            Product product4 = new Product("Case of Beer", "Food & Drink", 20M, "Parched", 250)
            {
                Id = productId4,
                SellerId = user2.Id,
                SellerName = user2.UserName
            };
            Product product5 = new Product("Sand Wedge", "Sport", 50M, "Very sturdy", 20)
            {
                Id = productId5,
                SellerId = user2.Id,
                SellerName = user2.UserName
            };
            Product product6 = new Product("Table", "Furniture", 100M, "A large table", 10)
            {
                Id = productId6,
                SellerId = user3.Id,
                SellerName = user3.UserName
            };
            Product product7 = new Product("Winter Coat", "Clothing", 80M, "Warm and cosy", 5)
            {
                Id = productId7,
                SellerId = user3.Id,
                SellerName = user3.UserName
            };
            Product product8 = new Product("T-shirt", "Clothing", 20M, "White", 200)
            {
                Id = productId8,
                SellerId = user3.Id,
                SellerName = user3.UserName
            };
            Product product9 = new Product("Jeans", "Clothing", 40M, "Optimal comfort", 1)
            {
                Id = productId9,
                SellerId = user3.Id,
                SellerName = user3.UserName
            };

            var products = new List<Product> { product1, product2, product3, product4, product5, product6, product7, product8, product9 };
            context.Products.AddRange(products);
            context.SaveChanges();

            // Product Images

            var productImages = new List<ProductImage>
            {
                new ProductImage("https://www.photo1.com")
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId1
                },
                new ProductImage("https://www.photo2.com")
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId2
                },
                new ProductImage("https://www.photo3.com")
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId3
                },
                new ProductImage("https://www.photo4.com")
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId3
                },
                new ProductImage("https://www.photo5.com")
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId3
                }
            };

            context.ProductImages.AddRange(productImages);
            context.SaveChanges();

            // Shopping Cart Items

            var cartItem1 = new ShoppingCartItem(productId1, 5)
            {
                Price = product1.Price,
                TotalPrice = RoundPrice(product1.Price * 5),
                ShoppingCartId = user1.ShoppingCart.Id,
            };
            var cartItem2 = new ShoppingCartItem(productId2, 10)
            {
                Price = product2.Price,
                TotalPrice = RoundPrice(product2.Price * 10),
                ShoppingCartId = user1.ShoppingCart.Id,
            };
            var cartItem3 = new ShoppingCartItem(productId3, 50)
            {
                Price = product3.Price,
                TotalPrice = RoundPrice(product3.Price * 50),
                ShoppingCartId = user2.ShoppingCart.Id,
            };
            var cartItem4 = new ShoppingCartItem(productId4, 25)
            {
                Price = product4.Price,
                TotalPrice = RoundPrice(product4.Price * 25),
                ShoppingCartId = user2.ShoppingCart.Id,
            };
            var cartItems = new List<ShoppingCartItem> { cartItem1, cartItem2, cartItem3, cartItem4 };
            context.ShoppingCartItems.AddRange(cartItems);
            context.SaveChanges();
            UpdateAllShoppingCartTotalPrices(context);
        }
        private static void UpdateAllShoppingCartTotalPrices(MarketplaceContext context)
        {
            var carts = context.ShoppingCarts.Include(c => c.Items).ToList();
            foreach (var cart in carts)
            {
                cart.TotalPrice = cart.Items.Sum(i => i.TotalPrice);
            }
            context.SaveChanges();
        }

        private static decimal RoundPrice(decimal price)
        {
            return Math.Round(price, 2);
        }
    }
}
