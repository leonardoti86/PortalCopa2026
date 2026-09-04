using System.Text.Json;
using System.Text.Json.Serialization;

namespace PortalCopa2026.Data;

internal class TeamRankingSeedDto
{
    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("points")]
    public double Points { get; set; }
}

internal class TeamSeedDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("group")]
    public string Group { get; set; } = string.Empty;

    [JsonPropertyName("coach")]
    public string Coach { get; set; } = string.Empty;

    [JsonPropertyName("ranking")]
    public TeamRankingSeedDto? Ranking { get; set; }
}

internal class GroupSeedDto
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("teams")]
    public List<string> Teams { get; set; } = new();
}

internal class PlayerSeedDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonPropertyName("position")]
    public string Position { get; set; } = string.Empty;

    [JsonPropertyName("goals")]
    public int Goals { get; set; }
}

internal class MatchSeedDto
{
    [JsonPropertyName("phase")]
    public string Phase { get; set; } = string.Empty;

    [JsonPropertyName("group")]
    public string? Group { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("home")]
    public JsonElement Home { get; set; }

    [JsonPropertyName("away")]
    public JsonElement Away { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("stadium")]
    public string Stadium { get; set; } = string.Empty;

    /// <summary>
    /// Retorna o nome da seleção quando já definida, ou nulo quando o confronto
    /// ainda depende do resultado de outro jogo (ex.: "vencedor do jogo X").
    /// </summary>
    public static string? ResolveTeamName(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return element.GetString();
        }

        if (element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty("type", out var type)
            && type.GetString() == "team"
            && element.TryGetProperty("name", out var name))
        {
            return name.GetString();
        }

        return null;
    }
}
