using System.ComponentModel.DataAnnotations;

namespace HotelListingApi.DTOs.Country;

public class CreateCountryDto
{
    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(3)]
    public required string ShortName { get; set; }
}
public class UpdateCountryDto : CreateCountryDto
{
    [Required]
    public int CountryId { get; set; }
}