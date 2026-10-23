using System.ComponentModel.DataAnnotations;
using HotelListingApi.DTOs.Hotel;

namespace HotelListingApi.DTOs.Country;
public record GetCountriesDto(
    int CountryId,
    string Name, 
    string ShortName
);

public record GetCountryDto(
    int CountryId,
    string Name, 
    string ShortName,
    List<GetHotelSlimDto> Hotels
);