using AbxPilot.Data;

namespace AbxPilot.Tests;

public sealed class LocalizationTests
{
    [Fact]
    public void EveryLanguageHasTheSameKeys()
    {
        var reference = KbResources.Strings("tr").Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();
        Assert.NotEmpty(reference);

        foreach (var language in KbResources.Languages)
            Assert.Equal(reference, KbResources.Strings(language).Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void DisclaimerIsExact()
    {
        Assert.Equal(
            "Klinik değerlendirme gerektirir. Bu program bir kılavuz gezgini ve eğitim aracıdır.",
            KbResources.Strings("tr")["footer.disclaimer"]);
    }

    [Fact]
    public void ManifestLoads()
    {
        var manifest = KbResources.Manifest();
        Assert.Equal(1, manifest.Schema);
        Assert.Empty(manifest.Syndromes);
    }
}
