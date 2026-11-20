using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListingApi.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = "d6c751cd-aa40-48d6-a309-438994fb75ed",
                ConcurrencyStamp = "771992a4-3f83-4118-8764-aa5c18c00933",
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            },
            new IdentityRole
            {
                Id = "80952496-2c09-4882-b9f5-b38b43e5bbe5",
                ConcurrencyStamp = "05389603-9023-4a93-9c1b-955f101b4695",
                Name = "User",
                NormalizedName = "USER"
            },
            new IdentityRole
            {
                Id = "80952496-4882-aa40-b9f5-b38b43e5994f",
                ConcurrencyStamp = "05389603-9023-4a93-9c1b-b4695955f101",
                Name = "Hotel Admin",
                NormalizedName = "HOTEL ADMIN"
            }
        );
    }
}