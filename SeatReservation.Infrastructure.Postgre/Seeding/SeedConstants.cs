namespace SeatReservation.Infrastructure.Postgre.Seeding;

/// <summary>
/// Константы для управления количеством генерируемых данных при сидировании
/// </summary>
public static class SeedConstants
{
    /// <summary>
    /// Количество пользователей (достаточно для тестирования)
    /// </summary>
    public const int UsersCount = 20_000;
    
    /// <summary>
    /// Количество площадок (venues) - разнообразие площадок
    /// </summary>
    public const int VenuesCount = 1_000;
    
    /// <summary>
    /// Среднее количество мест на площадку (от малых залов до средних арен)
    /// </summary>
    public const int SeatsPerVenue = 400;
    
    /// <summary>
    /// Количество событий (достаточно для тестирования производительности)
    /// </summary>
    public const int EventsCount = 10_000;
    
    /// <summary>
    /// Количество бронирований (активные + завершенные + отмененные)
    /// </summary>
    public const int ReservationsCount = 100_000;
    
    /// <summary>
    /// Минимальное количество мест в одном бронировании
    /// </summary>
    public const int MinSeatsPerReservation = 1;
    
    /// <summary>
    /// Максимальное количество мест в одном бронировании
    /// </summary>
    public const int MaxSeatsPerReservation = 10;
    
    /// <summary>
    /// Размер батча для вставки данных (количество записей за один SaveChanges)
    /// Оптимизировано для скорости и памяти
    /// </summary>
    public const int BatchSize = 5000;
}