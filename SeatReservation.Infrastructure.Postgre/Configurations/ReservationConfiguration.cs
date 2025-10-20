using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeatReservation.Domain.Reservations;

namespace SeatReservation.Infrastructure.Postgre.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");
        
        builder.HasKey(v => v.Id).HasName("pk_reservation");

        builder.Property(v => v.Id)
            .HasConversion(v => v.Value, id => new ReservationId(id))
            .HasColumnName("id");

        // builder
        //     .HasMany(r => r.ReservedSeats)
        //     .WithOne(rs => rs.Reservation)
        //     .HasForeignKey("reservation_id")
        //     .IsRequired()
        //     .OnDelete(DeleteBehavior.Cascade);




    }
}