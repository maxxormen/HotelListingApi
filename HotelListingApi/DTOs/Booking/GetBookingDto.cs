namespace HotelListingApi.DTOs.Booking;

public record GetBookingDto(
    int Id,
    int HotelId,
    string UserId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Guests,
    decimal TotalPrice,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    string Status
);