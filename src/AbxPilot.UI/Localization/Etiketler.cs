using System.Text.Json;

namespace AbxPilot.UI.Localization;

public static class Etiketler
{
    private const string Onek = "Etiketler.labels.";

    public static IReadOnlyDictionary<string, string> Oku(string dil)
    {
        var sozluk = new Dictionary<string, string>(StringComparer.Ordinal);
        using var akis = typeof(Etiketler).Assembly.GetManifestResourceStream(Onek + dil + ".json")
                         ?? typeof(Etiketler).Assembly.GetManifestResourceStream(Onek + Localizer.DefaultLanguage + ".json");
        if (akis is null) return sozluk;
        using var belge = JsonDocument.Parse(akis);
        foreach (var alan in belge.RootElement.EnumerateObject())
            if (alan.Value.ValueKind == JsonValueKind.String)
                sozluk[alan.Name] = alan.Value.GetString()!;
        return sozluk;
    }
}
