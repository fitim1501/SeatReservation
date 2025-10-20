using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeatReservation.Domain;
using Shared;

namespace SeatReservation.Infrastructure.Postgre.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        
        builder.HasKey(v => v.Id).HasName("pk_users");

        builder.Property(v => v.Id)
            .HasColumnName("id");

        builder.OwnsOne(u => u.Details, db =>
        {
            db.ToJson("details");

            db.OwnsMany(d => d.Socials, sb =>
            {
                sb
                    .Property(u => u.Link)
                    .IsRequired()
                    .HasMaxLength(LengthConstants.LENGTH500)
                    .HasColumnName("link");
            
                sb
                    .Property(u => u.Name)
                    .IsRequired()
                    .HasMaxLength(LengthConstants.LENGTH500)
                    .HasColumnName("name");
            });
            
            db
                .Property(u => u.Description)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH500)
                .HasColumnName("description");
        });
    }
}