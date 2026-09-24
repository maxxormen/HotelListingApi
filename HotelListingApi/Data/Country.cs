using HotelListingApi.DTOs.Hotel;

namespace HotelListingApi.Data;
public class Country
{
    public int CountryId { get; set; }
    public required string Name { get; set; }
    public required string ShortName { get; set; }
    public List<Hotel>? Hotels { get; set; } = [];
}