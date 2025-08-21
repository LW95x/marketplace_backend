using AutoMapper;
using Marketplace.DataAccess.Entities;
using Marketplace.Models;

namespace Marketplace.MapperProfiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile() 
        {
            CreateMap<OrderItem, OrderItemForResponseDto>()
                .ForMember(dest => dest.OrderItemId, opt => opt.MapFrom(src => src.Id));
            CreateMap<Order, OrderForResponseDto>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));
            CreateMap<ShoppingCartItem, OrderItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<ShoppingCart, Order>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.Items))
                .ForMember(d => d.BuyerId, o => o.MapFrom(s => s.BuyerId))
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<Order, OrderForUpdateDto>().ReverseMap();
            CreateMap<OrderItem, SoldItemForResponseDto>()
                    .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Product.Id))
                    .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Product.Title))
                    .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Product.Category))
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Product.Description))
                    .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Order.Address))
                    .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                    .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                    .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Quantity * src.Price))
                    .ForMember(dest => dest.BuyerName, opt => opt.MapFrom(src => src.Order.Buyer.UserName))
                    .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.Order.Date))
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Order.Status));
        }
    }
}
