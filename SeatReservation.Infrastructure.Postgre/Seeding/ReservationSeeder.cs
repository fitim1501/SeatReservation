using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeatReservation.Domain;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Infrastructure.Postgre.Seeding;

public class ReservationSeeder : ISeeder
{
    private readonly ReservationServiceDbContext _context;
    private readonly ILogger<ReservationSeeder> _logger;
    private readonly Random _random = new();

    public ReservationSeeder(ReservationServiceDbContext context, ILogger<ReservationSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting seeding reservation data...");

        try
        {
            await SeedData();
            
            _logger.LogInformation("Finished seeding reservation data.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding reservation data.");
            throw;
        }
    }

    private async Task SeedData()
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Отключаем автоматическое отслеживание изменений для ускорения
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
            
            // Очистка всех таблиц
            await ClearAllTables();
            
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Starting data generation at {Time}", startTime);
            
            // Генерация данных в правильном порядке (с учетом зависимостей)
            var users = await SeedUsers();
            var venues = await SeedVenues(); 
            var seats = await SeedSeats(venues);
            var events = await SeedEvents(venues);
            await SeedReservations(users, events, seats);

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;
            
            await transaction.CommitAsync();
            
            _logger.LogInformation("Successfully completed seeding in {Duration}. Users: {UsersCount}, Venues: {VenuesCount}, Seats: {SeatsCount}, Events: {EventsCount}",
                duration, users.Count, venues.Count, seats.Count, events.Count);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to seed data, transaction rolled back");
            throw;
        }
        finally
        {
            // Включаем обратно автоматическое отслеживание
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }
    }

    private async Task ClearAllTables()
    {
        _logger.LogInformation("Clearing all tables...");
        
        // Отключаем проверку внешних ключей
        await _context.Database.ExecuteSqlRawAsync("SET session_replication_role = 'replica';");
        
        // Очищаем таблицы в обратном порядке зависимостей
        await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"reservation_seats\" RESTART IDENTITY CASCADE;");
        await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"reservations\" RESTART IDENTITY CASCADE;");
        await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"event_details\" RESTART IDENTITY CASCADE;");
        await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"events\" RESTART IDENTITY CASCADE;");
        await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"seats\" RESTART IDENTITY CASCADE;");
        await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"venues\" RESTART IDENTITY CASCADE;");
        await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"users\" RESTART IDENTITY CASCADE;");
        
        // Включаем проверку внешних ключей обратно
        await _context.Database.ExecuteSqlRawAsync("SET session_replication_role = 'origin';");
        
        _logger.LogInformation("All tables cleared successfully");
    }

    private async Task<List<User>> SeedUsers()
    {
        _logger.LogInformation("Seeding {Count} users...", SeedConstants.UsersCount);
        
        var allUsers = new List<User>();
        var firstNames = new[] { "Иван", "Петр", "Сергей", "Александр", "Дмитрий", "Михаил", "Андрей", "Алексей", "Владимир", "Николай" };
        var lastNames = new[] { "Иванов", "Петров", "Сидоров", "Смирнов", "Кузнецов", "Попов", "Васильев", "Соколов", "Михайлов", "Новиков" };
        var middleNames = new[] { "Иванович", "Петрович", "Сергеевич", "Александрович", "Дмитриевич", "Михайлович", "Андреевич", "Алексеевич", "Владимирович", "Николаевич" };
        var socialNetworks = new[] { "VK", "Instagram", "Facebook", "Twitter", "LinkedIn" };

        var totalBatches = (int)Math.Ceiling((double)SeedConstants.UsersCount / SeedConstants.BatchSize);
        
        for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
        {
            var batchUsers = new List<User>();
            var batchSize = Math.Min(SeedConstants.BatchSize, SeedConstants.UsersCount - (batchIndex * SeedConstants.BatchSize));
            
            for (int i = 0; i < batchSize; i++)
            {
                var fio = $"{lastNames[_random.Next(lastNames.Length)]} {firstNames[_random.Next(firstNames.Length)]} {middleNames[_random.Next(middleNames.Length)]}";
                
                var socials = Enumerable.Range(0, _random.Next(1, 4))
                    .Select(_ => new SocialNetwork(
                        socialNetworks[_random.Next(socialNetworks.Length)],
                        $"https://{socialNetworks[_random.Next(socialNetworks.Length)]}.com/user{_random.Next(1000, 9999)}"
                    ))
                    .ToList();

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Details = new Details
                    {
                        FIO = fio,
                        Description = $"Пользователь системы бронирования. Зарегистрирован {DateTime.UtcNow.AddDays(-_random.Next(1, 365)):dd.MM.yyyy}",
                        Socials = socials
                    }
                };
                
                batchUsers.Add(user);
            }

            await _context.Users.AddRangeAsync(batchUsers);
            await _context.SaveChangesAsync();
            
            allUsers.AddRange(batchUsers);
            
            _logger.LogInformation("Users: batch {Current}/{Total} completed ({Percent}%)", 
                batchIndex + 1, totalBatches, ((batchIndex + 1) * 100 / totalBatches));
        }
        
        return allUsers;
    }

    private async Task<List<Venue>> SeedVenues()
    {
        _logger.LogInformation("Seeding {Count} venues...", SeedConstants.VenuesCount);
        
        var allVenues = new List<Venue>();
        var venuePrefixes = new[] { "КЦ", "ДК", "МЗ", "ТЦ", "СК" };
        var venueNames = new[] { "Олимпийский", "Крокус", "Космос", "Лужники", "Планета", "Звезда", "Арена", "Дворец", "Сокол", "Спартак" };

        var totalBatches = (int)Math.Ceiling((double)SeedConstants.VenuesCount / SeedConstants.BatchSize);
        
        for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
        {
            var batchVenues = new List<Venue>();
            var batchSize = Math.Min(SeedConstants.BatchSize, SeedConstants.VenuesCount - (batchIndex * SeedConstants.BatchSize));
            
            for (int i = 0; i < batchSize; i++)
            {
                var globalIndex = batchIndex * SeedConstants.BatchSize + i;
                var prefix = venuePrefixes[_random.Next(venuePrefixes.Length)];
                var name = venueNames[_random.Next(venueNames.Length)];
                var venueNameResult = VenueName.Create(prefix, $"{name}_{globalIndex + 1}");
                
                if (venueNameResult.IsFailure)
                    continue;

                var seatsLimit = _random.Next(50, 500);
                var venue = new Venue(
                    new VenueId(Guid.NewGuid()),
                    venueNameResult.Value,
                    seatsLimit
                );
                
                batchVenues.Add(venue);
            }

            await _context.Venues.AddRangeAsync(batchVenues);
            await _context.SaveChangesAsync();
            
            allVenues.AddRange(batchVenues);
            
            _logger.LogInformation("Venues: batch {Current}/{Total} completed ({Percent}%)", 
                batchIndex + 1, totalBatches, ((batchIndex + 1) * 100 / totalBatches));
        }
        
        return allVenues;
    }

    private async Task<List<Seat>> SeedSeats(List<Venue> venues)
    {
        _logger.LogInformation("Seeding seats for {Count} venues...", venues.Count);
        
        var allSeats = new List<Seat>();
        var totalSeatsToCreate = venues.Sum(v => Math.Min(SeedConstants.SeatsPerVenue, v.SeatsLimit));
        var processedSeats = 0;

        foreach (var venue in venues)
        {
            var seatsCount = Math.Min(SeedConstants.SeatsPerVenue, venue.SeatsLimit);
            var rows = _random.Next(5, 15);
            var seatsPerRow = seatsCount / rows;

            var venueSeats = new List<Seat>();
            
            for (int row = 1; row <= rows; row++)
            {
                for (int seatNum = 1; seatNum <= seatsPerRow; seatNum++)
                {
                    var seatResult = Seat.Create(venue.Id, row, seatNum);
                    if (seatResult.IsSuccess)
                    {
                        venueSeats.Add(seatResult.Value);
                    }
                }
            }

            // добавляем места в агрегат (для доменной целостности)
            venue.AddSeats(venueSeats);

            // --- ВАЖНО: явно добавляем места в DbContext, чтобы они точно были сохранены в БД ---
            if (venueSeats.Count > 0)
            {
                await _context.Seats.AddRangeAsync(venueSeats);
            }

            allSeats.AddRange(venueSeats);
            processedSeats += venueSeats.Count;
            
            // Сохраняем каждые BatchSize мест или при достижении конца площадки
            if (allSeats.Count >= SeedConstants.BatchSize || venue == venues.Last())
            {
                await _context.SaveChangesAsync();
                
                var progressPercent = (processedSeats * 100 / totalSeatsToCreate);
                _logger.LogInformation("Seats: {Processed}/{Total} completed ({Percent}%)", 
                    processedSeats, totalSeatsToCreate, progressPercent);
                
                // Очищаем ChangeTracker для освобождения памяти
                _context.ChangeTracker.Clear();
            }
        }

        // Финальное сохранение если что-то осталось
        if (_context.ChangeTracker.HasChanges())
        {
            await _context.SaveChangesAsync();
        }
        
        // Чтобы избежать FK-проблем и несогласованности используем перечень мест, гарантированно сохранённых в БД
        var persistedSeats = await _context.Seats.AsNoTracking().ToListAsync();
        return persistedSeats;
    }

    private async Task<List<Event>> SeedEvents(List<Venue> venues)
    {
        _logger.LogInformation("Seeding {Count} events...", SeedConstants.EventsCount);
        
        var allEvents = new List<Event>();
        var eventNames = new[] { "Концерт", "Конференция", "Вебинар", "Мастер-класс", "Лекция", "Презентация", "Встреча", "Семинар" };
        var performers = new[] { "Группа А", "Группа Б", "Солист В", "Оркестр Г", "Ансамбль Д" };
        var speakers = new[] { "Иванов И.И.", "Петров П.П.", "Сидоров С.С.", "Смирнов С.М.", "Кузнецов К.К." };
        var topics = new[] { "Разработка ПО", "Архитектура систем", "DevOps практики", "Agile методологии", "Machine Learning" };

        var totalBatches = (int)Math.Ceiling((double)SeedConstants.EventsCount / SeedConstants.BatchSize);
        
        for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
        {
            var batchEvents = new List<Event>();
            var batchSize = Math.Min(SeedConstants.BatchSize, SeedConstants.EventsCount - (batchIndex * SeedConstants.BatchSize));
            
            for (int i = 0; i < batchSize; i++)
            {
                var globalIndex = batchIndex * SeedConstants.BatchSize + i;
                var venue = venues[_random.Next(venues.Count)];
                var eventName = $"{eventNames[_random.Next(eventNames.Length)]} #{globalIndex + 1}";
                var eventDate = DateTime.UtcNow.AddDays(_random.Next(1, 180));
                var startDate = eventDate.AddHours(-2);
                var endDate = eventDate.AddHours(4);
                var capacity = _random.Next(50, venue.SeatsLimit);
                var description = $"Описание события {eventName}. Место проведения: {venue.Name}. Вместимость: {capacity} человек.";

                Event? eventEntity = null;
                var eventType = _random.Next(3);

                switch (eventType)
                {
                    case 0: // Concert
                        var concertResult = Event.CreateConcert(
                            venue.Id,
                            eventName,
                            eventDate,
                            startDate,
                            endDate,
                            capacity,
                            description,
                            performers[_random.Next(performers.Length)]
                        );
                        if (concertResult.IsSuccess)
                            eventEntity = concertResult.Value;
                        break;

                    case 1: // Conference
                        var conferenceResult = Event.CreateConference(
                            venue.Id,
                            eventName,
                            eventDate,
                            startDate,
                            endDate,
                            capacity,
                            description,
                            speakers[_random.Next(speakers.Length)],
                            topics[_random.Next(topics.Length)]
                        );
                        if (conferenceResult.IsSuccess)
                            eventEntity = conferenceResult.Value;
                        break;

                    case 2: // Online
                        var onlineResult = Event.CreateOnline(
                            venue.Id,
                            eventName,
                            eventDate,
                            startDate,
                            endDate,
                            capacity,
                            description,
                            $"https://stream.example.com/event{globalIndex + 1}"
                        );
                        if (onlineResult.IsSuccess)
                            eventEntity = onlineResult.Value;
                        break;
                }

                if (eventEntity != null)
                {
                    batchEvents.Add(eventEntity);
                }
            }

            await _context.Event.AddRangeAsync(batchEvents);
            await _context.SaveChangesAsync();
            
            allEvents.AddRange(batchEvents);
            
            _logger.LogInformation("Events: batch {Current}/{Total} completed ({Percent}%)", 
                batchIndex + 1, totalBatches, ((batchIndex + 1) * 100 / totalBatches));
        }
        
        return allEvents;
    }

    private async Task SeedReservations(List<User> users, List<Event> events, List<Seat> seats)
    {
        _logger.LogInformation("Seeding {Count} reservations...", SeedConstants.ReservationsCount);
        
        // Загружаем места и события из БД, чтобы гарантировать корректные FK и одну копию сущностей
        var persistedSeats = await _context.Seats.AsNoTracking().ToListAsync();
        var seatsByVenue = persistedSeats.GroupBy(s => s.VenueId).ToDictionary(g => g.Key, g => g.ToList());

        var persistedEvents = await _context.Event.AsNoTracking().ToListAsync();
        var eventsById = persistedEvents.ToDictionary(e => e.Id.Value, e => e);

        // Отслеживаем забронированные места для каждого события (EventId -> HashSet<SeatId>)
        var reservedSeatsByEvent = new Dictionary<Guid, HashSet<Guid>>();

        var totalBatches = (int)Math.Ceiling((double)SeedConstants.ReservationsCount / SeedConstants.BatchSize);
        var totalCreated = 0;
        
        for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
        {
            var batchReservations = new List<Reservation>();
            var batchSize = Math.Min(SeedConstants.BatchSize, SeedConstants.ReservationsCount - (batchIndex * SeedConstants.BatchSize));
            var attempts = 0;
            var maxAttempts = batchSize * 3; // Даем больше попыток если места заканчиваются
            
            while (batchReservations.Count < batchSize && attempts < maxAttempts)
            {
                attempts++;
                
                var user = users[_random.Next(users.Count)];
                var eventEntity = events[_random.Next(events.Count)];

                // Используем event из БД (persistent) для получения VenueId
                if (!eventsById.TryGetValue(eventEntity.Id.Value, out var persistedEvent))
                {
                    // если событие не найдено в БД (маловероятно), пропускаем
                    continue;
                }

                // Получаем места для этого venue из сохранённых в БД
                if (!seatsByVenue.TryGetValue(persistedEvent.VenueId, out var venueSeats) || venueSeats.Count == 0)
                    continue;

                // Инициализируем HashSet для этого события, если еще не создан
                if (!reservedSeatsByEvent.ContainsKey(persistedEvent.Id.Value))
                {
                    reservedSeatsByEvent[persistedEvent.Id.Value] = new HashSet<Guid>();
                }

                // Получаем доступные (еще не забронированные) места для этого события
                var availableSeats = venueSeats
                    .Where(s => !reservedSeatsByEvent[persistedEvent.Id.Value].Contains(s.Id.Value))
                    .ToList();

                if (availableSeats.Count == 0)
                {
                    // Все места уже забронированы для этого события, пробуем другое событие
                    continue;
                }

                // Выбираем случайное количество мест для бронирования
                var seatsCount = _random.Next(SeedConstants.MinSeatsPerReservation, 
                    Math.Min(SeedConstants.MaxSeatsPerReservation + 1, availableSeats.Count + 1));
                
                // Берем случайные свободные места
                var selectedSeats = availableSeats
                    .OrderBy(_ => _random.Next())
                    .Take(seatsCount)
                    .Select(s => s.Id.Value)
                    .ToList();

                if (selectedSeats.Count == 0)
                    continue;

                var reservationResult = Reservation.Create(
                    persistedEvent.Id.Value,
                    user.Id,
                    selectedSeats
                );

                if (reservationResult.IsSuccess)
                {
                    batchReservations.Add(reservationResult.Value);
                    
                    // Отмечаем выбранные места как забронированные для этого события
                    foreach (var seatId in selectedSeats)
                    {
                        reservedSeatsByEvent[persistedEvent.Id.Value].Add(seatId);
                    }
                }
            }

            if (batchReservations.Count > 0)
            {
                // Добавляем и сохраняем батч бронирований
                await _context.Reservation.AddRangeAsync(batchReservations);
                await _context.SaveChangesAsync();
                
                totalCreated += batchReservations.Count;
                
                _logger.LogInformation("Reservations: batch {Current}/{Total} completed ({Count} reservations, {Percent}%)", 
                    batchIndex + 1, totalBatches, totalCreated, ((batchIndex + 1) * 100 / totalBatches));
                
                // Очищаем ChangeTracker для освобождения памяти
                _context.ChangeTracker.Clear();
            }
        }
        
        _logger.LogInformation("Successfully created {Count} reservations", totalCreated);
    }
}