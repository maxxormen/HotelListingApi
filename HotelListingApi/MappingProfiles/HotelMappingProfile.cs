using AutoMapper;
using HotelListingApi.Data;
using HotelListingApi.DTOs.Country;
using HotelListingApi.DTOs.Hotel;


namespace HotelListingApi.MappingProfiles;

public class HotelMappingProfile : Profile
{
    public HotelMappingProfile()
    {
        CreateMap<Hotel, GetHotelDto>()
            .ForMember(dest => dest.Country,
            opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : null));
        CreateMap<Hotel, GetHotelSlimDto>();
        CreateMap<CreateHotelDto, Hotel>();
        CreateMap<UpdateHotelDto, Hotel>();
    }
}
