using AutoMapper;
using Marketplace.DataAccess.Entities;
using Marketplace.Models;

namespace Marketplace.MapperProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserForResponseDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));
            CreateMap<UserForCreationDto, User>();
            CreateMap<User, UserForUpdateDto>().ReverseMap();
        }
    }
}
