using System.ComponentModel.DataAnnotations;
namespace HotelListingApi.DTOs.Hotel;

public class CreateHotelDto
{
    [Required]
    public required string Name { get; set; }

    [MaxLength(100)]
    public required string Address { get; set; }

    [Range(1, 5)]
    public double Rating { get; set; }

    [Required]
    public int CountryId { get; set; }
}

public record GetHotelsDto(
    int Id,
    string Name, 
    string Address, 
    double Rating, 
    int CountryId
);

public record GetHotelDto(
    int Id,
    string Name, 
    string Address, 
    double Rating, 
    int CountryId,
    string Country
);

public record GetHotelSlimDto(
    int Id,
    string Name, 
    string Address, 
    double Rating
);

public class UpdateHotelDto : CreateHotelDto
{
    [Required]
    public int Id { get; set; }
}