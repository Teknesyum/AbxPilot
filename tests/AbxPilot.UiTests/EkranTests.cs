using System.Collections;
using System.Diagnostics;
using System.Reflection;
using AbxPilot.Core.Knowledge;
using AbxPilot.Data;
using AbxPilot.UI;
using AbxPilot.UI.Kontrast;
using AbxPilot.UI.Settings;
using AbxPilot.UI.ViewModels;
using AbxPilot.UI.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace AbxPilot.UiTests;

public class EkranTests
{
    static readonly string Klasor = Hazirla();

    [AvaloniaFact]
    public void CapturesTheDesignedStates()
    {
        var olcumler = Olcumler();

        var kapi = new ManualResetEventSlim();
        var (yukleme, yvm) = Ac(1280, 800, () => { kapi.Wait(); return KbResources.Knowledge(); });
        Kaydet(yukleme, "a3-yukleniyor");
        kapi.Set();
        Bitir(yvm.Idle);
        Kaydet(yukleme, "a3-bos");
        Olc(yukleme, "bos", olcumler);
        yukleme.Close();

        var (hata, _) = Ac(1280, 800, () => throw new InvalidDataException("kb"));
        Kaydet(hata, "a3-hata");
        Olc(hata, "hata", olcumler);
        hata.Close();

        var (pencere, vm) = Ac(1280, 800);
        Bitir(vm.SelectSyndromeAsync("cap"));
        Assert.True(vm.IsReady);
        Assert.True(vm.EvaluatedOffUiThread);
        Kaydet(pencere, "a3-tkp-varsayilan");
        Olc(pencere, "tkp", olcumler);

        Kaydet(pencere, "a4-iz-0-once");
        Kareler(pencere, vm.AnswerAsync("setting", "icu"), "a4-iz");
        Bitir(vm.AnswerAsync("prior_mrsa", "yes"));
        Kaydet(pencere, "a3-yogun-bakim-mrsa");
        Olc(pencere, "icu-mrsa", olcumler);

        vm.ShowScore = true;
        vm.ToggleSettingsCommand.Execute(null);
        Bekle();
        Kaydet(pencere, "a3-ayarlar-puan");
        Olc(pencere, "ayarlar", olcumler);
        vm.CloseSettingsCommand.Execute(null);
        vm.ShowScore = false;
        Bekle();

        var satir = pencere.GetVisualDescendants().OfType<Border>().First(b => b.Classes.Contains("alt") && ToolTip.GetServiceEnabled(b));
        ToolTip.SetPlacement(satir, PlacementMode.Bottom);
        satir.BringIntoView();
        Bekle();
        ToolTip.SetIsOpen(satir, true);
        Bekle();
        KaydetIpucu(pencere, satir, "a3-puan-ipucu");
        ToolTip.SetIsOpen(satir, false);
        pencere.Close();

        var (alerji, avm) = Ac(1280, 800);
        Bitir(avm.SelectSyndromeAsync("cap"));
        Bitir(avm.AnswerAsync("setting", "ward"));
        Kaydet(alerji, "a4-eleme-0-once");
        Kareler(alerji, avm.AnswerAsync("pen_allergy", "ige"), "a4-eleme");
        Kaydet(alerji, "a3-penisilin-ige");
        Olc(alerji, "ige", olcumler);
        Assert.NotEmpty(avm.Excluded);

        Bitir(avm.AnswerAsync("qt_risk", "yes"));
        Bitir(avm.AnswerAsync("pregnancy", "yes"));
        Kaydet(alerji, "a3-uzmana-danisin");
        Olc(alerji, "danis", olcumler);
        Assert.True(avm.IsConsult, avm.State.ToString());
        alerji.Close();

        var (kucuk, kvm) = Ac(Sayi("WindowMinWidth"), Sayi("WindowMinHeight"));
        Bitir(kvm.SelectSyndromeAsync("cap"));
        Kaydet(kucuk, "a3-en-kucuk");
        Olc(kucuk, "en-kucuk", olcumler);
        kucuk.Close();

        var tvm = new MainViewModel(KbResources.Knowledge, new MemorySettingsStore());
        var telefon = new Window { Width = 390, Height = 844, Content = new MainView { DataContext = tvm } };
        telefon.Show();
        Bitir(tvm.Idle);
        Bitir(tvm.SelectSyndromeAsync("cap"));
        Assert.True(tvm.IsCompact);
        Kaydet(telefon, "a3-telefon");
        Olc(telefon, "telefon", olcumler);
        tvm.ToggleDrawerCommand.Execute(null);
        Bekle();
        Kaydet(telefon, "a3-telefon-cekmece");
        Olc(telefon, "cekmece", olcumler);
        telefon.Close();

        var hatalar = Hatalar(olcumler);
        Assert.True(hatalar.Count == 0, string.Join("\n", hatalar));
    }

    [AvaloniaFact]
    public void CapturesTheNewSyndromes()
    {
        var olcumler = Olcumler();

        var (pencere, vm) = Ac(1280, 800);
        Bitir(vm.SelectSyndromeAsync("cap"));
        Assert.Equal("ttd-2021", vm.GuidelineSet);

        Bitir(vm.SelectSyndromeAsync("ssti"));
        Assert.Equal("idsa-2014", vm.GuidelineSet);
        Assert.Equal(["idsa-2014", "nice-ng141-2019"], vm.GuidelineSets.Select(item => item.Code));
        Assert.True(vm.IsReady, vm.State.ToString());
        Kaydet(pencere, "a5-ssti-varsayilan");
        Olc(pencere, "ssti", olcumler);

        Bitir(vm.AnswerAsync("ssti_type", "purulent"));
        Assert.True(vm.IsNoAntibiotic, vm.State.ToString());
        Kaydet(pencere, "a5-ssti-antibiyotik-yok");
        Olc(pencere, "antibiyotik-yok", olcumler);

        Bitir(vm.ClearAsync("ssti_type"));
        Bitir(vm.AnswerAsync("necrotizing_signs", "yes"));
        Bitir(vm.AnswerAsync("systemic_signs", "yes"));
        Assert.True(vm.IsReferral, vm.State.ToString());
        Assert.True(vm.HasFirstChoice);
        Kaydet(pencere, "a5-ssti-sevk");
        Olc(pencere, "sevk", olcumler);

        vm.SelectGuidelineSetCommand.Execute(vm.GuidelineSets.First(item => item.Code == "nice-ng141-2019"));
        Bitir(vm.Idle);
        Bitir(vm.SelectSyndromeAsync("uti"));
        Assert.Equal("eau-2026", vm.GuidelineSet);
        Assert.True(vm.IsReady, vm.State.ToString());
        Kaydet(pencere, "a6-uti-varsayilan");
        Olc(pencere, "uti", olcumler);

        Bitir(vm.SelectSyndromeAsync("iai"));
        Assert.Equal("ekmud-2016", vm.GuidelineSet);
        Assert.True(vm.IsReady, vm.State.ToString());
        Kaydet(pencere, "a6-iai-varsayilan");
        Olc(pencere, "iai", olcumler);

        Bitir(vm.AnswerAsync("sepsis", "yes"));
        Assert.True(vm.IsReferral, vm.State.ToString());
        Assert.False(vm.HasFirstChoice);
        Kaydet(pencere, "a6-iai-sevk-rejimsiz");
        Olc(pencere, "sevk-rejimsiz", olcumler);

        vm.ToggleSettingsCommand.Execute(null);
        Bekle();
        Kaydet(pencere, "a6-ayarlar-set");
        Olc(pencere, "ayarlar-set", olcumler);
        vm.CloseSettingsCommand.Execute(null);

        Bitir(vm.SelectSyndromeAsync("ssti"));
        Assert.Equal("nice-ng141-2019", vm.GuidelineSet);
        pencere.Close();

        var tvm = new MainViewModel(KbResources.Knowledge, new MemorySettingsStore());
        var telefon = new Window { Width = 390, Height = 844, Content = new MainView { DataContext = tvm } };
        telefon.Show();
        Bitir(tvm.Idle);
        Assert.True(tvm.IsEmpty);
        Kaydet(telefon, "a5-telefon-bos");
        Olc(telefon, "telefon-bos", olcumler);
        telefon.Close();

        var hatalar = Hatalar(olcumler);
        Assert.True(hatalar.Count == 0, string.Join("\n", hatalar));
    }

    [AvaloniaFact]
    public void EvaluationLeavesTheUiThreadFree()
    {
        var vm = new MainViewModel(KbResources.Knowledge, new MemorySettingsStore());
        Bitir(vm.Idle);
        var gorev = vm.SelectSyndromeAsync("cap");
        Assert.False(gorev.IsCompleted && !vm.EvaluatedOffUiThread);
        Bitir(gorev);
        Assert.True(vm.EvaluatedOffUiThread);
        Bitir(vm.AnswerAsync("setting", "icu"));
        Assert.True(vm.EvaluatedOffUiThread);
    }

    static (Window, MainViewModel) Ac(double en, double boy, Func<KnowledgeBase>? yukle = null)
    {
        var vm = new MainViewModel(yukle ?? KbResources.Knowledge, new MemorySettingsStore());
        var pencere = new MainWindow { DataContext = vm, Width = en, Height = boy };
        pencere.Show();
        Bekle();
        if (yukle is null) Bitir(vm.Idle);
        return (pencere, vm);
    }

    static void Kareler(Window pencere, Task gorev, string ad)
    {
        var saat = Stopwatch.StartNew();
        while (!gorev.IsCompleted && saat.Elapsed < TimeSpan.FromSeconds(10))
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(2);
        }
        Dispatcher.UIThread.RunJobs();
        foreach (var (kare, bekle) in new[] { (1, 20), (2, 90) })
        {
            Thread.Sleep(bekle);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            Dispatcher.UIThread.RunJobs();
            Kaydet(pencere, ad + "-" + kare + "-ara");
        }
        Bitir(gorev);
        Kaydet(pencere, ad + "-3-sonra");
    }

    static void Bitir(Task gorev)
    {
        var saat = Stopwatch.StartNew();
        while (!gorev.IsCompleted && saat.Elapsed < TimeSpan.FromSeconds(20))
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(2);
        }
        gorev.GetAwaiter().GetResult();
        Bekle();
    }

    static void Bekle()
    {
        var saat = Stopwatch.StartNew();
        while (saat.ElapsedMilliseconds < 700)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            Thread.Sleep(8);
        }
        Dispatcher.UIThread.RunJobs();
    }

    static void Kaydet(Window pencere, string ad) =>
        pencere.CaptureRenderedFrame()?.Save(Path.Combine(Klasor, ad + ".png"));

    static void KaydetIpucu(Window pencere, Control hedef, string ad)
    {
        var ana = pencere.CaptureRenderedFrame();
        if (ana is null) return;
        var ipucu = ToolTip.GetTip(hedef) as Control;
        var kok = ipucu is null ? null : TopLevel.GetTopLevel(ipucu);
        var kare = kok is not null && kok != pencere ? kok.CaptureRenderedFrame() : null;
        var yer = hedef.TranslatePoint(new Point(0, hedef.Bounds.Height), pencere) ?? default;
        var boyut = ana.PixelSize;
        using var tuval = new RenderTargetBitmap(boyut);
        using (var ctx = tuval.CreateDrawingContext())
        {
            ctx.DrawImage(ana, new Rect(0, 0, boyut.Width, boyut.Height));
            if (kare is not null)
            {
                var alan = new Rect(yer.X, yer.Y, kare.PixelSize.Width, kare.PixelSize.Height);
                ctx.DrawImage(kare, alan);
            }
        }
        tuval.Save(Path.Combine(Klasor, ad + ".png"));
    }

    static double Sayi(string anahtar) =>
        Application.Current!.TryGetResource(anahtar, null, out var deger) && deger is double sayi ? sayi : 0;

    static IList Olcumler()
    {
        var tur = typeof(KontrastTests).GetNestedType("Olcum", BindingFlags.NonPublic)!;
        return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(tur))!;
    }

    static void Olc(Visual kok, string durum, IList olcumler) =>
        typeof(KontrastTests).GetMethod("Yuru", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, [kok, durum, olcumler]);

    static List<string> Hatalar(IList olcumler)
    {
        var sonuc = new List<string>();
        foreach (var o in olcumler)
        {
            var t = o.GetType();
            var edilgen = (bool)t.GetProperty("Edilgen")!.GetValue(o)!;
            var kesin = (bool)t.GetProperty("Kesin")!.GetValue(o)!;
            var oran = (double)t.GetProperty("Oran")!.GetValue(o)!;
            if (!edilgen && (!kesin || oran < 7.0)) sonuc.Add(o.ToString()!);
        }
        return sonuc;
    }

    static string Hazirla()
    {
        var dizin = new DirectoryInfo(AppContext.BaseDirectory);
        while (dizin is not null && !File.Exists(Path.Combine(dizin.FullName, "AbxPilot.sln"))) dizin = dizin.Parent;
        var klasor = Path.Combine(dizin?.FullName ?? AppContext.BaseDirectory, "docs", "ui-denetim", "2026-09-24");
        Directory.CreateDirectory(klasor);
        return klasor;
    }
}
