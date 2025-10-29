namespace Component;

using Newtonsoft.Json;

/// <summary>
///   This class serves as a custom JSON converter for deserializing TimeOnly objects that are returned from the Alpaca
///   Calendar trading days API endpoint. Some of their TimeOnly value strings are missing the colon in the middle,
///   i.e. "2000" instead of "20:00". So this checks if that is the case and adds the colon so it can be parsed into
///   a TimeOnly object.
/// </summary>
public class CalendarApiTimeOnlyConverter : JsonConverter<TimeOnly>
{
    /// <summary>
    ///   WriteJson override that will be used if a TimeOnly property is serialized. 
    /// </summary>
    /// <param name="writer">JsonWriter that is writing the TimeOnly json</param>
    /// <param name="value">The value of the TimeOnly object</param>
    /// <param name="serializer">The JsonSerializer, not currently used</param>
    public override void WriteJson(JsonWriter writer, TimeOnly value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString("HH:mm"));
    }

    /// <summary>
    ///   ReadJson override that handles the custom logic for parsing the API TimeOnly fields that are missing the
    ///   ':' character. If one is present, it is parsed normally. If one is missing, and it's 4 chars long, add the
    ///   ':' char in the middle of the string and then parse it. If somehow neither of those are true throw a
    ///   JsonException.
    /// </summary>
    /// <param name="reader">The JsonReader that is reading the json string value</param>
    /// <param name="objectType">Unused parameter needed for ReadJson overrides</param>
    /// <param name="existingValue">Unused parameter needed for ReadJson overrides</param>
    /// <param name="hasExistingValue">Unused parameter needed for ReadJson overrides</param>
    /// <param name="serializer">Unused parameter needed for ReadJson overrides</param>
    /// <returns>The json string parsed into a TimeOnly object</returns>
    /// <exception cref="JsonException">If something unexpected went wrong while parsing the json</exception>
    public override TimeOnly ReadJson(JsonReader reader, Type objectType, TimeOnly existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var timeString = reader.Value?.ToString() ?? string.Empty;
        
        // if it's not missing the ':' then parse it as is
        if (timeString.Contains(':'))
            return TimeOnly.Parse(timeString);

        // if it's missing the ':' then add it in the middle and parse it
        if (timeString.Length == 4)
            return TimeOnly.Parse(timeString.Insert(2, ":"));

        throw new JsonException($"Unable to parse '{timeString}' as TimeOnly");
    }
}