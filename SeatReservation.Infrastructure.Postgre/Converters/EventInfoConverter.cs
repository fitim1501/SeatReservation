using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SeatReservation.Domain.Events;

namespace SeatReservation.Infrastructure.Postgre.Converters;

public class EventInfoConverter : ValueConverter<IEventInfo, string>
{
    public EventInfoConverter() : base(i => InfoToString(i), s => StringToInfo(s))
    {
        
    }
    
    private static string InfoToString(IEventInfo info) => info switch
    {
        ConcertInfo c => $"Concert:{c.Performer}",
        ConferenceInfo c => $"Conference:{c.Speaker} {c.Topic}",
        OnlineInfo c => $"Online:{c.Url}",
        _ => throw new ArgumentOutOfRangeException(nameof(info), info, null)
    };

    private static IEventInfo StringToInfo(string info)
    {
        var split = info.Split(':', 2);
        var type = split[0];
        var data = split[1];

        return type switch
        {
            "Concert" => new ConcertInfo(data),
            "Conference" => new ConferenceInfo(data.Split(' ')[0], data.Split(' ')[1]),
            "Online" => new OnlineInfo(data),
            _ => throw new ArgumentOutOfRangeException(nameof(info), info, null)
        };
    }
}