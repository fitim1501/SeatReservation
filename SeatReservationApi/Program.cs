// using Microsoft.OpenApi.Models;
using SeatReservation.Application;
using SeatReservation.Application.DataBase;
using SeatReservation.Application.Events;
using SeatReservation.Application.Events.Queries;
using SeatReservation.Application.Reservations;
using SeatReservation.Application.Reservations.Commands;
using SeatReservation.Application.Seats;
using SeatReservation.Application.Venues;
using SeatReservation.Application.Venues.Commands;
using SeatReservation.Infrastructure.Postgre;
using SeatReservation.Infrastructure.Postgre.Database;
using SeatReservation.Infrastructure.Postgre.Repositories;
using SeatReservation.Infrastructure.Postgre.Seeding;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddOpenApi(options =>
// {
//     options.AddSchemaTransformer((schema, context, _) =>
//     {
//         if (context.JsonTypeInfo.Type == typeof(Envelope<Errors>))
//         {
//             if (schema.Properties.TryGetValue("errors", out var errorsProp))
//             {
//                 errorsProp.Items.Reference = new OpenApiReference
//                 {
//                     Type = ReferenceType.Schema,
//                     Id = "Error"
//                 };
//             }
//         }
//     
//         return Task.CompletedTask;
//     });
// });ъ

builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddScoped<ReservationServiceDbContext>(_ =>
    new ReservationServiceDbContext(builder.Configuration.GetConnectionString("ReservationServiceDb")!));

builder.Services.AddScoped<IReadDbContext, ReservationServiceDbContext>(_ =>
    new ReservationServiceDbContext(builder.Configuration.GetConnectionString("ReservationServiceDb")!));

builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;


builder.Services.AddScoped<ITransactionManager, TransactionManager>();

// builder.Services.AddScoped<IVenuesRepository, NpgSqlVenuesRepository>();
builder.Services.AddScoped<IVenuesRepository, VenuesRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IReservationsRepository, ReservationsesRepository>();
builder.Services.AddScoped<ISeatsRepository, SeatsRepository>();

builder.Services.AddScoped<CreateVenueHandler>();
builder.Services.AddScoped<UpdateVenueNameHandler>();
builder.Services.AddScoped<UpdateVenueNameByPrefixHandler>();
builder.Services.AddScoped<UpdateVenueSeatsHandler>();
builder.Services.AddScoped<ReserverHandler>();
builder.Services.AddScoped<ReserveAdjacentSeatsHandler>();

builder.Services.AddScoped<GetEventByIdHandler>();
builder.Services.AddScoped<GetEventByIdHandlerDapper>();

builder.Services.AddScoped<ISeeder, ReservationSeeder>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(option => option.SwaggerEndpoint("/openapi/v1.json", "SeatReservationApi"));

    if (args.Contains("--seeding"))
    {
        //await app.Services.RunSeeding();
    }
}

app.MapControllers();

app.Run();