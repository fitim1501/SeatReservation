using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Infrastructure.Postgre.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("seats");
        
        builder.HasKey(v => v.Id).HasName("pk_seats");

        builder.Property(v => v.Id)
            .HasConversion(v => v.Value, id => new SeatId(id))
            .HasColumnName("id");
        
        builder.Property(v => v.VenueId)
            .HasConversion(v => v.Value, id => new VenueId(id))
            .HasColumnName("venue_id");

        builder.Property(v => v.RowNumber)
            .HasColumnName("row_number");
        
        builder.Property(v => v.SeatNumber)
            .HasColumnName("seat_number");

        // builder.Property(v => v.VenueId).HasColumnName("venue_id");
    }
}