using System.Text.Json;

namespace ControlPanel.Application.Serialization;

/// <summary> (DTO) record to  JSON string </summary>
public static class JsonCommandSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
    };

    public static string ToJson<T>(T value) =>
        JsonSerializer.Serialize(value, Options);
}
