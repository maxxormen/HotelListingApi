using HotelListingApi.Contracts;
using HotelListingApi.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListingApi.Services;

public class ApiKeyValidatorService(HotelListingDbContext context) : IApiKeyValidatorService
{
    public async Task<bool> IsValidAsync(string apiKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return false;

        var apiKeyEntity = await context.ApiKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Key == apiKey, ct);

        if (apiKeyEntity is null) return false;
        return apiKeyEntity.IsActive;
    }
}