using Marketplace.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Marketplace.Test.Fixtures
{
    public static class TestDataFactory
    {
        public static List<User> GetUsers()
        {
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

           return new List<User>() { user1, user2, user3 };
    }

        public static List<Product> GetProducts(List<User> users)
        {
            var products = new List<Product>();

            Product product1 = new Product("Deckchair", "Furniture", 50M, "Very swingy", 10)
            {
                Id = Guid.Parse("9B23A145-039B-4561-B59E-3DED02F712FD"),
                SellerId = users[0].Id,
                SellerName = users[0].UserName
            };
            Product product2 = new Product("Football", "Sport", 20M, "Bouncy", 25)
            {
                Id = Guid.Parse("12C9D57C-1578-4CCD-AEB5-1E082D668E70"),
                SellerId = users[0].Id,
                SellerName = users[0].UserName
            };
            Product product3 = new Product("Box of Flapjacks", "Food & Drink", 10M, "Tasty", 100)
            {
                Id = Guid.Parse("684B656C-B7C2-405B-8F73-27E1A486A715"),
                SellerId = users[1].Id,
                SellerName = users[1].UserName
            };
            Product product4 = new Product("Case of Beer", "Food & Drink", 20M, "Parched", 250)
            {
                Id = Guid.Parse("8547092F-ADE0-4450-ABEA-41E475F7C4DF"),
                SellerId = users[1].Id,
                SellerName = users[1].UserName
            };
            Product product5 = new Product("Sand Wedge", "Sport", 50M, "Very sturdy", 20)
            {
                Id = Guid.Parse("54785B42-2735-447E-83FA-E0F0227D1B2E"),
                SellerId = users[1].Id,
                SellerName = users[1].UserName
            };
            Product product6 = new Product("Table", "Furniture", 100M, "A large table", 10)
            {
                Id = Guid.Parse("8B35097D-CAC4-47CC-805B-8BB533CB722F"),
                SellerId = users[2].Id,
                SellerName = users[2].UserName
            };
            Product product7 = new Product("Winter Coat", "Clothing", 80M, "Warm and cosy", 5)
            {
                Id = Guid.Parse("71DA0C50-80C4-42BB-8DB1-22527E5E19D8"),
                SellerId = users[2].Id,
                SellerName = users[2].UserName
            };
            Product product8 = new Product("T-shirt", "Clothing", 20M, "White", 200)
            {
                Id = Guid.Parse("770FF4BD-9355-446C-854B-5C9E46978B6E"),
                SellerId = users[2].Id,
                SellerName = users[2].UserName
            };
            Product product9 = new Product("Jeans", "Clothing", 40M, "Optimal comfort", 1)
            {
                Id = Guid.Parse("07573B01-22CF-4E64-A310-E60BDFAB8CDC"),
                SellerId = users[2].Id,
                SellerName = users[2].UserName
            };

            product1.Images = new List<ProductImage>
            {
                new ProductImage("https://www.photo1.com")
                {
                    Id = Guid.NewGuid(),
                    ProductId = product1.Id
                }
            };

            product2.Images = new List<ProductImage>
            {
                new ProductImage("https://www.photo2.com")
                {
                    Id = Guid.NewGuid(),
                    ProductId = product2.Id
                }
            };

            product3.Images = new List<ProductImage>
            {
                new ProductImage("https://www.photo3.com")
                {
                    Id = Guid.NewGuid(),
                    ProductId = product3.Id
                },
            new ProductImage("https://www.photo4.com")
            {
                Id = Guid.NewGuid(),
                ProductId = product3.Id
            },
            new ProductImage("https://www.photo5.com")
            {
                Id = Guid.NewGuid(),
                ProductId = product3.Id
            }
            };

            products.AddRange([product1, product2, product3, product4, product5, product6, product7, product8, product9]);

            return products;


        }

        public static List<Order> GetOrders(List<User> users, List<Product> products)
        {
            var user1 = users[0];
            var user2 = users[1];
            var user3 = users[2];

            var deckchair = products[0];
            var football = products[1];
            var flapjacks = products[2];
            var beer = products[3];
            var sandWedge = products[4];
            var table = products[5];
            var coat = products[6];

            var orderId1 = Guid.NewGuid();
            var orderId2 = Guid.NewGuid();
            var orderId3 = Guid.NewGuid();

            var order1 = new Order
            {
                Id = orderId1,
                BuyerId = user1.Id,
                Date = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Address = "39, Blackstock Street, Greater Manchester, M13 0FZ",
                OrderItems = new List<OrderItem>()
            };


            var orderItem1 = new OrderItem(1, deckchair.Id)
            {
                Id = Guid.NewGuid(),
                Price = deckchair.Price,
                TotalPrice = deckchair.Price * 1,
                OrderId = orderId1,
            };

            var orderItem2 = new OrderItem(2, football.Id)
            {
                Id = Guid.NewGuid(),
                Price = football.Price,
                TotalPrice = football.Price * 2,
                OrderId = orderId1,
            };

            var orderItem3 = new OrderItem(3, flapjacks.Id)
            {
                Id = Guid.NewGuid(),
                Price = flapjacks.Price,
                TotalPrice = flapjacks.Price * 3,
                OrderId = orderId1,
            };

            order1.OrderItems.Add(orderItem1);
            order1.OrderItems.Add(orderItem2);
            order1.OrderItems.Add(orderItem3);
            order1.TotalPrice = order1.OrderItems.Sum(i => i.TotalPrice);

            var order2 = new Order
            {
                Id = orderId2,
                BuyerId = user2.Id,
                Date = DateTime.UtcNow,
                Status = OrderStatus.Completed,
                Address = "Bodaioch Hall, Trefeglwys, Caersws, SY17 5PN",
                OrderItems = new List<OrderItem>()
            };

            var orderItem4 = new OrderItem(5, beer.Id)
            {
                Id = Guid.NewGuid(),
                Price = beer.Price,
                TotalPrice = beer.Price * 5,
                OrderId = orderId2,
            };

            var orderItem5 = new OrderItem(1, sandWedge.Id)
            {
                Id = Guid.NewGuid(),
                Price = sandWedge.Price,
                TotalPrice = sandWedge.Price * 1,
                OrderId = orderId2,
            };

            var orderItem6 = new OrderItem(1, table.Id)
            {
                Id = Guid.NewGuid(),
                Price = table.Price,
                TotalPrice = table.Price * 1,
                OrderId = orderId2,
            };

            order2.OrderItems.Add(orderItem4);
            order2.OrderItems.Add(orderItem5);
            order2.OrderItems.Add(orderItem6);
            order2.TotalPrice = order2.OrderItems.Sum(i => i.TotalPrice);

            var order3 = new Order
            {
                Id = orderId3,
                BuyerId = user2.Id,
                Date = DateTime.UtcNow,
                Status = OrderStatus.Cancelled,
                Address = "Terrace Rd, Aberdyfi, Aberdovey, LL35 0LT",
                OrderItems = new List<OrderItem>()
            };

            var orderItem7 = new OrderItem(1, coat.Id)
            {
                Id = Guid.NewGuid(),
                Price = coat.Price,
                TotalPrice = coat.Price * 1,
                OrderId = orderId3,
            };

            order3.OrderItems.Add(orderItem7);
            order3.TotalPrice = order3.OrderItems.Sum(i => i.TotalPrice);

            return new List<Order> { order1, order2, order3 };
        }

     public static List<ShoppingCart> GetShoppingCarts(List<User> users, List<Product> products)
        {
            var user1 = users[0];
            var user2 = users[1];
            var user3 = users[2];

            var deckchair = products[0];
            var football = products[1];
            var flapjacks = products[2];
            var beer = products[3];
            var sandWedge = products[4];
            var table = products[5];

            var cartId1 = Guid.NewGuid();
            var cartId2 = Guid.NewGuid();
            var cartId3 = Guid.NewGuid();

            var cart1 = new ShoppingCart
            {
                Id = cartId1,
                BuyerId = user1.Id,
                Items = new List<ShoppingCartItem>()
            };

            var cart2 = new ShoppingCart
            {
                Id = cartId2,
                BuyerId = user2.Id,
                Items = new List<ShoppingCartItem>()
            };

            var cart3 = new ShoppingCart
            {
                Id = cartId3,
                BuyerId = user3.Id,
                Items = new List<ShoppingCartItem>()
            };

            var cartItem1 = new ShoppingCartItem(deckchair.Id, 1)
            {
                Id = Guid.NewGuid(),
                Price = deckchair.Price,
                TotalPrice = deckchair.Price * 1, 
                ShoppingCartId = cartId1
            };

            var cartItem2 = new ShoppingCartItem(football.Id, 2)
            {
                Id = Guid.NewGuid(),
                Price = football.Price,
                TotalPrice = football.Price * 2,
                ShoppingCartId = cartId1
            };

            var cartItem3 = new ShoppingCartItem(flapjacks.Id, 3)
            {
                Id = Guid.NewGuid(),
                Price = flapjacks.Price,
                TotalPrice = flapjacks.Price * 3,
                ShoppingCartId = cartId1
            };

            cart1.Items.Add(cartItem1);
            cart1.Items.Add(cartItem2);
            cart1.Items.Add(cartItem3);
            cart1.TotalPrice = cart1.Items.Sum(i => i.TotalPrice);

            var cartItem4 = new ShoppingCartItem(beer.Id, 5)
            {
                Id = Guid.NewGuid(),
                Price = beer.Price,
                TotalPrice = beer.Price * 5,
                ShoppingCartId = cartId2
            };

            var cartItem5 = new ShoppingCartItem(sandWedge.Id, 1)
            {
                Id = Guid.NewGuid(),
                Price = sandWedge.Price,
                TotalPrice = sandWedge.Price * 1,
                ShoppingCartId = cartId2
            };

            cart2.Items.Add(cartItem4);
            cart2.Items.Add(cartItem5);
            cart2.TotalPrice = cart2.Items.Sum(i => i.TotalPrice);

            var cartItem6 = new ShoppingCartItem(table.Id, 1)
            {
                Id = Guid.NewGuid(),
                Price = table.Price,
                TotalPrice = table.Price * 1,
                ShoppingCartId = cartId3
            };

            cart3.Items.Add(cartItem6);
            cart3.TotalPrice = cart3.Items.Sum(i => i.TotalPrice);

            return new List<ShoppingCart> { cart1, cart2, cart3 };
        }
    }
}
