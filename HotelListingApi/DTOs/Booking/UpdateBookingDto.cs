namespace HotelListingApi.DTOs.Booking;

public record UpdateBookingDto(
    int HotelId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Guests
);