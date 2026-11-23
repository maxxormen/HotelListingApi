using System.ComponentModel.DataAnnotations;

namespace HotelListingApi.DTOs.Booking;

public record UpdateBookingDto(
    int HotelId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Guests
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