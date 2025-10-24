using Microsoft.OpenApi.Models;
using SeatReservation.Application;
using SeatReservation.Application.DataBase;
using SeatReservation.Domain;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Venues;
using SeatReservation.Infrastructure.Postgre;
using Shared;
using EventId = SeatReservation.Domain.Events.EventId;

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

builder.Services.AddScoped<IReservationServiceDbContext, ReservationServiceDbContext>(_ =>
    new ReservationServiceDbContext(builder.Configuration.GetConnectionString("ReservationServiceDb")!));

builder.Services.AddScoped<CreateVenueHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(option => option.SwaggerEndpoint("/openapi/v1.json", "SeatReservationApi")); 
}

app.MapControllers();

app.Run();