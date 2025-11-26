using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Smartbook.Converters;

public class DateTimeNullableConverter : JsonConverter<DateTime?>
{
    private readonly string[] _formats = new[]
    {
        "yyyy-MM-dd",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss.fffZ",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy/MM/dd",
        "dd/MM/yyyy",
        "MM/dd/yyyy"
    };

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return null;
            }

            // Intentar parsear con diferentes formatos
            foreach (var format in _formats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    return date;
                }
            }

            // Si no funciona con formatos específicos, intentar parseo general
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return parsedDate;
            }

            throw new JsonException($"No se pudo convertir '{dateString}' a DateTime. Formatos aceptados: yyyy-MM-dd, yyyy-MM-ddTHH:mm:ss, yyyy-MM-ddTHH:mm:ssZ");
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            // Si viene como número (timestamp Unix)
            var timestamp = reader.GetInt64();
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }

        throw new JsonException($"No se pudo convertir el token {reader.TokenType} a DateTime.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

