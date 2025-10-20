using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeatReservation.Domain.Venues;
using Shared;

namespace SeatReservation.Infrastructure.Postgre.Configurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("venues");
        
        builder.HasKey(v => v.Id).HasName("pk_venues");

        builder.Property(v => v.Id)
            .HasConversion(v => v.Value, id => new VenueId(id))
            .HasColumnName("id");

        builder.ComplexProperty(v => v.Name, np =>
        {
            np
                .Property(v => v.Prefix)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH50)
                .HasColumnName("prefix");
            
            np
                .Property(v => v.Name)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH500)
                .HasColumnName("name");
        });

        builder
            .HasMany(v => v.Seats)
            .WithOne(s => s.Venue)
            .HasForeignKey(s => s.VenueId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}