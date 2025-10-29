namespace Component;

using Newtonsoft.Json;

/// <summary>
///   
/// </summary>
public class CalendarApiTimeOnlyConverter : JsonConverter<TimeOnly>
{
    public override void WriteJson(JsonWriter writer, TimeOnly value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString("HH:mm"));
    }

    public override TimeOnly ReadJson(JsonReader reader, Type objectType, TimeOnly existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var timeString = reader.Value?.ToString() ?? string.Empty;
        
        // if it's not missing the ':' then parse it as is
        if (timeString.Contains(':'))
            return TimeOnly.Parse(timeString);

        // if it's missing the ':' then add it in the middle and parse it
        if (timeString.Length == 4)
        {
            timeString = timeString.Insert(2, ":");
            return TimeOnly.Parse(timeString);
        }

        throw new JsonException($"Unable to parse '{timeString}' as TimeOnly");
    }
}