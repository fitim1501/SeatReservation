using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SeatReservation.Domain.Departments;
using SeatReservation.Domain.Events;
using Path = SeatReservation.Domain.Departments.Path;

namespace SeatReservation.Infrastructure.Postgre.Configurations;

public class DepartmentConfiguration: IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        
        builder.HasKey(d => d.Id).HasName("pk_department");

        builder.Property(d => d.Id)
            .HasConversion(d => d.Value, id => new  DepartmentId(id))
            .HasColumnName("id");

        builder.Property(d => d.Name)
            .HasColumnName("name")
            .HasMaxLength(500)
            .IsRequired()
            .HasConversion(
                value => value.Value,
                value => DepartmentName.Create(value));
        
        builder.Property(d => d.Identifier)
            .HasColumnName("identifier")
            .HasMaxLength(100)
            .IsRequired()
            .HasConversion(
                value => value.Value,
                value => Identifier.Create(value));
        
        builder.Property(d => d.ParentId)
            .HasColumnName("parent_id")
            .HasMaxLength(100)
            .HasConversion(
                value => value!.Value,
                value => new DepartmentId(value));
        
        builder.Property(d => d.Path)
            .HasColumnName("path")
            .HasColumnType("ltree")
            .HasMaxLength(100)
            .IsRequired()
            .HasConversion(
                value => value.Value,
                value => Path.Create(value));

        builder.Property(d => d.Depth)
            .IsRequired()
            .HasColumnName("depth");
        
        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasColumnName("is_active");
        
        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");
        
        builder.Property(d => d.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.HasMany<Department>()
            .WithOne(x => x.Parent)
            .IsRequired(false)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}