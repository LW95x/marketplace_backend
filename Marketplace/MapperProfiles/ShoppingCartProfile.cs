using AutoMapper;
using Marketplace.DataAccess.Entities;
using Marketplace.Models;

namespace Marketplace.MapperProfiles
{
    public class ShoppingCartProfile : Profile
    {
        public ShoppingCartProfile() 
        {
            CreateMap<ShoppingCart, ShoppingCartForResponseDto>()
                .ForMember(dest => dest.CartId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
            CreateMap<ShoppingCartItem, ShoppingCartItemForResponseDto>()
                .ForMember(dest => dest.CartItemId, opt => opt.MapFrom(src => src.Id));
            CreateMap<ShoppingCartItemForCreationDto, ShoppingCartItem>();
        }
    }
}
