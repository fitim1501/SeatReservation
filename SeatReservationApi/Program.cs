using Microsoft.OpenApi.Models;
using SeatReservation.Application;
using SeatReservation.Application.DataBase;
using SeatReservation.Application.Venues;
using SeatReservation.Infrastructure.Postgre;
using SeatReservation.Infrastructure.Postgre.Database;
using SeatReservation.Infrastructure.Postgre.Repositories;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer((schema, context, _) =>
    {
        if (context.JsonTypeInfo.Type == typeof(Envelope<Errors>))
        {
            if (schema.Properties.TryGetValue("errors", out var errorsProp))
            {
                errorsProp.Items.Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = "Error"
                };
            }
        }
    
        return Task.CompletedTask;
    });
});

builder.Services.AddControllers();

builder.Services.AddScoped<ReservationServiceDbContext>(_ =>
    new ReservationServiceDbContext(builder.Configuration.GetConnectionString("ReservationServiceDb")!));

builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();

// builder.Services.AddScoped<IVenuesRepository, NpgSqlVenuesRepository>();
builder.Services.AddScoped<IVenuesRepository, EfCoreVenuesRepository>();

builder.Services.AddScoped<CreateVenueHandler>();
builder.Services.AddScoped<UpdateVenueNameHandler>();
builder.Services.AddScoped<UpdateVenueNameByPrefixHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(option => option.SwaggerEndpoint("/openapi/v1.json", "SeatReservationApi")); 
}

app.MapControllers();

app.Run();