using SeatReservation.Application.DataBase;
using SeatReservation.Domain;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Venues;
using SeatReservation.Infrastructure.Postgre;
using EventId = SeatReservation.Domain.Events.EventId;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddScoped<IReservationServiceDbContext, ReservertionServiceDbContext>(_ => 
    new ReservertionServiceDbContext(builder.Configuration.GetConnectionString("ReservationServiceDb")!));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(option => option.SwaggerEndpoint("/openapi/v1.json", "SeatReservationApi")); 
}

app.Run();