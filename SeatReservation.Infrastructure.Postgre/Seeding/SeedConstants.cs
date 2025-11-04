namespace SeatReservation.Infrastructure.Postgre.Seeding;

/// <summary>
/// Константы для управления количеством генерируемых данных при сидировании
/// </summary>
public static class SeedConstants
{
    /// <summary>
    /// Количество пользователей
    /// </summary>
    public const int UsersCount = 1000;
    
    /// <summary>
    /// Количество площадок (venues)
    /// </summary>
    public const int VenuesCount = 1000;
    
    /// <summary>
    /// Среднее количество мест на площадку
    /// </summary>
    public const int SeatsPerVenue = 500;
    
    /// <summary>
    /// Количество событий
    /// </summary>
    public const int EventsCount = 5000;
    
    /// <summary>
    /// Количество бронирований
    /// </summary>
    public const int ReservationsCount = 2000;
    
    /// <summary>
    /// Минимальное количество мест в одном бронировании
    /// </summary>
    public const int MinSeatsPerReservation = 1;
    
    /// <summary>
    /// Максимальное количество мест в одном бронировании
    /// </summary>
    public const int MaxSeatsPerReservation = 5;
    
    /// <summary>
    /// Размер батча для вставки данных (количество записей за один SaveChanges)
    /// </summary>
    public const int BatchSize = 1000;
}

