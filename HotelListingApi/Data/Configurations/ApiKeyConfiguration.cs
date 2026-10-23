using HotelListingApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListingApi.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.HasIndex(a => a.Key).IsUnique();

        builder.HasData(
            new ApiKey
            {
                Id = 1,
                AppName = "app",
                CreatedAtUtc = new DateTime(2026,01,01),
                Key = "dXNlcjFAbG9jYWxob3N0LmNvbTpQQHNzdzByZEAxMjM="
            }
        );
    }
}