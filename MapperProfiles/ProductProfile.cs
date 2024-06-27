using AutoMapper;
using Marketplace.DataAccess.Entities;
using Marketplace.Models;

namespace Marketplace.MapperProfiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile() 
        {
            CreateMap<Product, ProductForResponseDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.Images.Select(img => img.Url)));
            CreateMap<ProductForCreationDto, Product>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ImageUrls.Select(url => new ProductImage(url)).ToList()));
            CreateMap<Product, ProductForUpdateDto>().ReverseMap();
        }
    }
}
