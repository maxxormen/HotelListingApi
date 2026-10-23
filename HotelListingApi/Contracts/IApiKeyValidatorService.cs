namespace HotelListingApi.Contracts;

public interface IApiKeyValidatorService
{
    Task<bool> IsValidAsync(string apikey, CancellationToken ct = default);
}