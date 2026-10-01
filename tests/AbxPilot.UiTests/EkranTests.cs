using System.Collections;
using System.Diagnostics;
using System.Reflection;
using AbxPilot.Core.Knowledge;
using AbxPilot.Data;
using AbxPilot.UI;
using AbxPilot.UI.Controls;
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
        Kaydet(yukleme, "b3-yukleniyor");
        kapi.Set();
        Bitir(yvm.Idle);
        Assert.Equal("pharyngitis", yvm.SelectedSyndrome?.Id);
        Assert.True(yvm.IsReady, yvm.State.ToString());
        Assert.True(yvm.HasFirstChoice);
        Assert.All(yvm.Questions.Where(item => item.IsVisible && !item.IsMulti), item => Assert.Contains(item.Options, option => option.IsSelected));
        Kaydet(yukleme, "b3-baslangic-varsayilan");
        Olc(yukleme, "baslangic", olcumler);
        yukleme.Close();

        var (hata, _) = Ac(1280, 800, () => throw new InvalidDataException("kb"));
        Kaydet(hata, "b3-hata");
        Olc(hata, "hata", olcumler);
        hata.Close();

        var (pencere, vm) = Ac(1280, 800);
        Bitir(vm.SelectSyndromeAsync("cap"));
        Assert.True(vm.IsReady);
        Assert.True(vm.EvaluatedOffUiThread);
        Kaydet(pencere, "b3-tkp-varsayilan");
        Olc(pencere, "tkp", olcumler);

        Kaydet(pencere, "b3-iz-0-once");
        Kareler(pencere, vm.AnswerAsync("setting", "icu"), "b3-iz");
        Bitir(vm.AnswerAsync("prior_mrsa", "yes"));
        Kaydet(pencere, "b3-secimler");
        Olc(pencere, "icu-mrsa", olcumler);

        vm.ShowScore = true;
        vm.ToggleSettingsCommand.Execute(null);
        Bekle();
        Kaydet(pencere, "b3-ayarlar-puan");
        Olc(pencere, "ayarlar", olcumler);
        vm.CloseSettingsCommand.Execute(null);
        vm.ShowScore = false;
        Bekle();

        var satir = pencere.GetVisualDescendants().OfType<Border>().First(b => b.Classes.Contains("alt") && ToolTip.GetServiceEnabled(b));
        satir.BringIntoView();
        Bekle();
        ToolTip.SetIsOpen(satir, true);
        Bekle();
        KaydetIpucu(pencere, satir, "b3-puan-ipucu");
        ToolTip.SetIsOpen(satir, false);
        pencere.Close();

        var (alerji, avm) = Ac(1280, 800);
        Bitir(avm.SelectSyndromeAsync("cap"));
        Bitir(avm.AnswerAsync("setting", "ward"));
        Kaydet(alerji, "b3-eleme-0-once");
        Kareler(alerji, avm.AnswerAsync("pen_allergy", "ige"), "b3-eleme");
        Kaydet(alerji, "b3-penisilin-ige");
        Olc(alerji, "ige", olcumler);
        Assert.NotEmpty(avm.Excluded);

        Bitir(avm.AnswerAsync("qt_risk", "yes"));
        Bitir(avm.AnswerAsync("pregnancy", "yes"));
        Kaydet(alerji, "b3-uzmana-danisin");
        Olc(alerji, "danis", olcumler);
        Assert.True(avm.IsConsult, avm.State.ToString());
        alerji.Close();

        var (kucuk, kvm) = Ac(Sayi("WindowMinWidth"), Sayi("WindowMinHeight"));
        Bitir(kvm.SelectSyndromeAsync("cap"));
        Kaydet(kucuk, "b3-en-kucuk");
        SpektrumKaymaz(kucuk);
        Olc(kucuk, "en-kucuk", olcumler);
        kucuk.Close();

        var tvm = new MainViewModel(KbResources.Knowledge, new MemorySettingsStore());
        var telefon = new Window { Width = 390, Height = 844, Content = new MainView { DataContext = tvm } };
        telefon.Show();
        Bitir(tvm.Idle);
        Bitir(tvm.SelectSyndromeAsync("cap"));
        Assert.True(tvm.IsCompact);
        Kaydet(telefon, "b3-telefon");
        Olc(telefon, "telefon", olcumler);
        tvm.ToggleDrawerCommand.Execute(null);
        Bekle();
        Kaydet(telefon, "b3-telefon-cekmece");
        Olc(telefon, "cekmece", olcumler);
        telefon.Close();

        var hatalar = Hatalar(olcumler);
        Assert.True(hatalar.Count == 0, string.Join("\n", hatalar));
    }

    [AvaloniaFact]
    public void QuestionsAreSingleRowsThatMoveWhenChanged()
    {
        var olcumler = Olcumler();
        var (pencere, vm) = Ac(1280, 800);
        Bitir(vm.SelectSyndromeAsync("cap"));
        Assert.False(vm.CanReset);
        Assert.False(vm.ResetAnswersCommand.CanExecute(null));
        Assert.Empty(vm.ChosenQuestions);
        Assert.DoesNotContain(vm.OpenQuestions, item => item.Id is "severe_vasopressor" or "severe_ventilation" or "severe_minor");
        Assert.Equal("Amoksisilin", vm.Headline);
        Kaydet(pencere, "b3-acilis");
        SpektrumKaymaz(pencere);
        var satirlar = Satirlar(pencere);
        Assert.Equal(vm.OpenQuestions.Count, satirlar.Count);
        Assert.All(satirlar.Where(satir => satir.DataContext is QuestionCard { IsBoolean: true } or QuestionCard { Id: "setting" }),
            satir => Assert.False(((QuestionRow)satir.Child!).IsStacked, ((QuestionCard)satir.DataContext!).Id));
        var sinir = Sayi("InputHeight") + 2 * Sayi("Space2") + 2;
        Assert.All(satirlar.Where(satir => satir.DataContext is QuestionCard { IsBoolean: true }),
            satir => Assert.True(satir.Child!.DesiredSize.Height <= sinir, ((QuestionCard)satir.DataContext!).Id));
        Olc(pencere, "acilis", olcumler);

        var eslik = vm.Questions.First(item => item.Id == "comorbidity");
        vm.ChooseCommand.Execute(eslik.Options.First(option => option.Code == "heart"));
        Bitir(vm.Idle);
        Assert.Equal("Sefuroksim aksetil + Azitromisin", vm.Headline);
        Assert.Contains(eslik, vm.ChosenQuestions);
        Assert.DoesNotContain(eslik, vm.OpenQuestions);
        Assert.Contains("birinci seçenek değişti", vm.ChangeText);
        Assert.True(vm.CanReset);
        Kaydet(pencere, "b3-eslik-eden-hastalik");
        Olc(pencere, "eslik", olcumler);

        vm.ChooseCommand.Execute(eslik.Options.First(option => option.Code == "heart"));
        Bitir(vm.Idle);
        Assert.Equal("Amoksisilin", vm.Headline);
        Assert.DoesNotContain(eslik, vm.ChosenQuestions);
        Assert.False(vm.CanReset);

        var ortam = vm.Questions.First(item => item.Id == "setting");
        vm.ChooseCommand.Execute(ortam.Options.First(option => option.Code == "ward"));
        Bitir(vm.Idle);
        Assert.Equal("Ortam → birinci seçenek değişti", vm.ChangeText.Replace(ortam.Label, "Ortam"));
        Assert.Contains(vm.OpenQuestions, item => item.Id == "severe_vasopressor");
        Kaydet(pencere, "b3-degisim-etiketi");
        Olc(pencere, "degisim", olcumler);

        var etkisiz = vm.Questions.First(item => item.Id == "severe_minor");
        vm.ChooseCommand.Execute(etkisiz.Options.First(option => option.Code != "lt3"));
        Bitir(vm.Idle);
        Assert.True(vm.NoEffectText.Length > 0 != vm.ChangeText.Length > 0);

        foreach (var id in new[] { "severe_vasopressor", "prior_mrsa" })
        {
            vm.ToggleQuestionCommand.Execute(vm.Questions.First(item => item.Id == id));
            Bitir(vm.Idle);
        }
        Assert.Contains(vm.ChosenQuestions, item => item.Id == "severe_vasopressor" && item.IsOn);
        Assert.Contains(vm.ChosenQuestions, item => item.Id == "prior_mrsa" && item.IsOn);
        Kaydet(pencere, "b3-secilenler");
        pencere.GetVisualDescendants().OfType<ScrollViewer>().First(item => item.Name == "LowerScroll").ScrollToEnd();
        Bekle();
        Kaydet(pencere, "b3-secilenler-alt");
        SpektrumKaymaz(pencere);
        Olc(pencere, "secilenler", olcumler);

        vm.ToggleQuestionCommand.Execute(vm.Questions.First(item => item.Id == "prior_mrsa"));
        Bitir(vm.Idle);
        Assert.Contains(vm.OpenQuestions, item => item.Id == "prior_mrsa" && !item.IsOn);

        vm.ResetAnswersCommand.Execute(null);
        Bitir(vm.Idle);
        Assert.Empty(vm.ChosenQuestions);
        Assert.False(vm.CanReset);
        Assert.Equal("Amoksisilin", vm.Headline);
        pencere.Close();

        var hatalar = Hatalar(olcumler);
        Assert.True(hatalar.Count == 0, string.Join("\n", hatalar));
    }

    static void SpektrumKaymaz(Window pencere)
    {
        var kaydirma = pencere.GetVisualDescendants().OfType<ScrollViewer>().First(item => item.Name == "SpectrumScroll");
        Assert.True(kaydirma.Extent.Height <= kaydirma.Viewport.Height + 1,
            $"spektrum kayıyor: {kaydirma.Extent.Height} > {kaydirma.Viewport.Height}");
    }

    static List<Border> Satirlar(Window pencere) =>
        pencere.GetVisualDescendants().OfType<Border>().Where(b => b.Classes.Contains("qcard") && b.IsEffectivelyVisible).ToList();

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
        Kaydet(pencere, "b3-ssti-varsayilan");
        Olc(pencere, "ssti", olcumler);

        Bitir(vm.AnswerAsync("ssti_type", "purulent"));
        Assert.True(vm.IsNoAntibiotic, vm.State.ToString());
        Kaydet(pencere, "b3-ssti-antibiyotik-yok");
        Olc(pencere, "antibiyotik-yok", olcumler);

        Bitir(vm.ClearAsync("ssti_type"));
        Bitir(vm.AnswerAsync("necrotizing_signs", "yes"));
        Bitir(vm.AnswerAsync("systemic_signs", "yes"));
        Assert.True(vm.IsReferral, vm.State.ToString());
        Assert.True(vm.HasFirstChoice);
        Kaydet(pencere, "b3-ssti-sevk");
        Olc(pencere, "sevk", olcumler);

        vm.SelectGuidelineSetCommand.Execute(vm.GuidelineSets.First(item => item.Code == "nice-ng141-2019"));
        Bitir(vm.Idle);
        Bitir(vm.SelectSyndromeAsync("uti"));
        Assert.Equal("eau-2026", vm.GuidelineSet);
        Assert.True(vm.IsReady, vm.State.ToString());
        Kaydet(pencere, "b3-uti-varsayilan");
        Olc(pencere, "uti", olcumler);

        Bitir(vm.SelectSyndromeAsync("iai"));
        Assert.Equal("ekmud-2016", vm.GuidelineSet);
        Assert.True(vm.IsReady, vm.State.ToString());
        Kaydet(pencere, "b3-iai-varsayilan");
        SpektrumKaymaz(pencere);
        Olc(pencere, "iai", olcumler);

        Bitir(vm.AnswerAsync("sepsis", "yes"));
        Assert.True(vm.IsReferral, vm.State.ToString());
        Assert.False(vm.HasFirstChoice);
        Kaydet(pencere, "b3-iai-sevk-rejimsiz");
        Olc(pencere, "sevk-rejimsiz", olcumler);

        vm.ToggleSettingsCommand.Execute(null);
        Bekle();
        Kaydet(pencere, "b3-ayarlar-set");
        Olc(pencere, "ayarlar-set", olcumler);
        vm.CloseSettingsCommand.Execute(null);

        Bitir(vm.SelectSyndromeAsync("ssti"));
        Assert.Equal("nice-ng141-2019", vm.GuidelineSet);
        pencere.Close();

        var tvm = new MainViewModel(KbResources.Knowledge, new MemorySettingsStore());
        var telefon = new Window { Width = 390, Height = 844, Content = new MainView { DataContext = tvm } };
        telefon.Show();
        Bitir(tvm.Idle);
        Assert.True(tvm.IsReady, tvm.State.ToString());
        Kaydet(telefon, "b3-telefon-baslangic");
        Olc(telefon, "telefon-baslangic", olcumler);
        telefon.Close();

        var hatalar = Hatalar(olcumler);
        Assert.True(hatalar.Count == 0, string.Join("\n", hatalar));
    }

    [AvaloniaFact]
    public void CapturesTheRegionLayer()
    {
        var olcumler = Olcumler();
        var (pencere, vm) = Ac(1280, 800);
        try
        {
            Assert.Equal(["tr", "eu", "us", "other"], vm.Regions.Select(item => item.Code));
            Bitir(vm.SelectSyndromeAsync("cap"));
            Assert.Equal("ttd-2021", vm.GuidelineSet);
            Assert.False(vm.NoRegionData);

            Bolge(vm, "eu");
            Assert.Equal("idsa-ats-2019", vm.GuidelineSet);
            Bitir(vm.AnswerAsync("setting", "outpatient"));
            Assert.True(vm.IsReady, vm.State.ToString());
            Assert.False(vm.NoRegionData);
            Assert.DoesNotContain(vm.Current!.Excluded, item => item.RegimenId == "azm_po");
            Kaydet(pencere, "b3-bolge-eu");
            Olc(pencere, "bolge-eu", olcumler);

            Bolge(vm, "other");
            Assert.True(vm.NoRegionData);
            Assert.Contains(vm.Current!.Excluded, item => item.RegimenId == "azm_po");
            Kaydet(pencere, "b3-bolge-diger");
            Olc(pencere, "bolge-diger", olcumler);

            vm.ToggleSettingsCommand.Execute(null);
            Bekle();
            Kaydet(pencere, "b3-ayarlar-bolge");
            Olc(pencere, "ayarlar-bolge", olcumler);
            vm.CloseSettingsCommand.Execute(null);

            Bolge(vm, "us");
            vm.SelectLanguageCommand.Execute("en");
            Bekle();
            Assert.Equal("Licence (United States)", vm.LicensesTitle);
            Kaydet(pencere, "b3-en");
            Olc(pencere, "en", olcumler);
        }
        finally
        {
            vm.SelectLanguageCommand.Execute("tr");
            Bolge(vm, "tr");
            pencere.Close();
        }

        var hatalar = Hatalar(olcumler);
        Assert.True(hatalar.Count == 0, string.Join("\n", hatalar));
    }

    private static void Bolge(MainViewModel vm, string code)
    {
        vm.SelectRegionCommand.Execute(vm.Regions.First(item => item.Code == code));
        Bitir(vm.Idle);
    }

    [AvaloniaFact]
    public void OpensOnPharyngitisWithPenicillinV()
    {
        var olcumler = Olcumler();
        var (pencere, vm) = Ac(1280, 800);
        Assert.Equal("pharyngitis", vm.Syndromes[0].Id);
        Assert.Equal("pharyngitis", vm.SelectedSyndrome?.Id);
        Assert.True(vm.IsReady, vm.State.ToString());
        Assert.Equal("penicillin_v", vm.Slots[0].DrugId);
        SpektrumKaymaz(pencere);
        Kaydet(pencere, "farenjit-acilis");
        Olc(pencere, "farenjit", olcumler);

        Bitir(vm.AnswerAsync("pen_allergy", "ige"));
        Assert.True(vm.IsReady, vm.State.ToString());
        Assert.Equal("clindamycin", vm.Slots[0].DrugId);
        Assert.NotEmpty(vm.Excluded);
        SpektrumKaymaz(pencere);
        Kaydet(pencere, "farenjit-alerji");
        Olc(pencere, "farenjit-alerji", olcumler);
        pencere.Close();

        var hatalar = Hatalar(olcumler);
        Assert.True(hatalar.Count == 0, string.Join("\n", hatalar));
    }

    [AvaloniaFact]
    public void OpensTheLastSyndromeWithDefaults()
    {
        var ayar = new MemorySettingsStore(new AppSettings { LastSyndrome = "uti" });
        var vm = new MainViewModel(KbResources.Knowledge, ayar);
        Bitir(vm.Idle);
        Assert.Equal("uti", vm.SelectedSyndrome?.Id);
        Assert.True(vm.IsReady, vm.State.ToString());
        Bitir(vm.SelectSyndromeAsync("cap"));
        Assert.Equal("cap", ayar.Current.LastSyndrome);
    }

    [AvaloniaFact]
    public void RemembersAnswersSearchesDrugsAndCopiesTheSummary()
    {
        var ayar = new MemorySettingsStore();
        var vm = new MainViewModel(KbResources.Knowledge, ayar);
        Bitir(vm.Idle);
        Bitir(vm.SelectSyndromeAsync("cap"));
        Bitir(vm.AnswerAsync("setting", "icu"));
        Assert.True(vm.HasHistory);

        var ozet = vm.BuildSummary();
        Assert.Contains(vm.Headline, ozet);
        Assert.Contains(vm.SourceText, ozet);
        Assert.Contains(vm.Questions.First(kart => kart.Id == "setting").Label, ozet);

        var sayfa = AbxPilot.UI.Printing.PrintSheet.Html(vm.SummaryParts(), "tr", new DateTime(2026, 10, 1, 9, 30, 0));
        Assert.StartsWith("<!doctype html>", sayfa);
        Assert.Contains(System.Net.WebUtility.HtmlEncode(vm.Headline), sayfa);
        Assert.Contains("<li class=\"dose\">", sayfa);
        Assert.Contains("2026-10-01 09:30", sayfa);
        Assert.Contains("color:#000000", sayfa);
        Assert.Contains("background:#ffffff", sayfa);
        Assert.Contains("print()", sayfa);
        File.WriteAllText(Path.Combine(Klasor, "yazdir-ozet.html"), sayfa);

        Bitir(vm.SelectSyndromeAsync("uti"));
        Assert.False(vm.HasHistory);
        Bitir(vm.SelectSyndromeAsync("cap"));
        var ortam = vm.Questions.First(kart => kart.Id == "setting");
        Assert.True(ortam.Options.First(secenek => secenek.Code == "icu").IsSelected);

        vm.SearchText = "nitrofurantoin";
        Assert.True(vm.Syndromes.First(tablo => tablo.Id == "uti").IsMatch);
        Assert.False(vm.Syndromes.First(tablo => tablo.Id == "pharyngitis").IsMatch);
        vm.SearchText = "";

        Assert.True(vm.ShowTip);
        vm.DismissTipCommand.Execute(null);
        Assert.False(vm.ShowTip);
        Assert.True(ayar.Current.TipSeen);
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
        var yer = hedef.TranslatePoint(new Point(hedef.Bounds.Width + ToolTip.GetHorizontalOffset(hedef), 0), pencere) ?? default;
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
        var klasor = Path.Combine(dizin?.FullName ?? AppContext.BaseDirectory, "docs", "ui-denetim", "2026-09-27-uc020");
        Directory.CreateDirectory(klasor);
        return klasor;
    }
}
