using System.ComponentModel.DataAnnotations;
using HotelListingApi.DTOs.Hotel;
namespace HotelListingApi.DTOs.Country;

public class CreateCountryDto
{
    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(3)]
    public required string ShortName { get; set; }
}

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
public class UpdateCountryDto : CreateCountryDto
{
    [Required]
    public int CountryId { get; set; }
}