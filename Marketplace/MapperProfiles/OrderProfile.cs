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
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<Order, OrderForUpdateDto>().ReverseMap();
        }
    }
}
