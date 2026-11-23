using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace HotelListingApi.DTOs.Booking;

public record CreateBookingDto(
    [Required] int HotelId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    [Required] [Range(minimum:1, maximum:10)] int Guests
) : IValidatableObject {
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (CheckOut <= CheckIn)
        {
            yield return new ValidationResult(
                "Check-out must be after Check-in.",
                [nameof(CheckOut), nameof(CheckIn)]);
        }
    }
}