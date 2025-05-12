using AutoMapper;
using Marketplace.DataAccess.Entities;
using Marketplace.Models;

namespace Marketplace.MapperProfiles
{
    public class SavedItemProfile : Profile
    {
        public SavedItemProfile() 
        {
            CreateMap<SavedItem, SavedItemForResponseDto>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Product.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Product.Description))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Product.Quantity))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Product.Category))
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Product.SellerName))
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.Product.Images.Select(i => i.Url)));
        }
    }
}
