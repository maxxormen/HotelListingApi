using AutoMapper;
using HotelListingApi.Data;
using HotelListingApi.DTOs.Auth;


namespace HotelListingApi.MappingProfiles;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<ApplicationUser, RegisteredUserDto>();
        CreateMap<RegisterUserDto, ApplicationUser>()
    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
        CreateMap<LoginUserDto, ApplicationUser>().ReverseMap();
    }
}