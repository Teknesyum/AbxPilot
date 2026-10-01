using System.Globalization;
using System.Net;
using System.Text;
using AbxPilot.UI.Choreography;
using Avalonia;
using Avalonia.Media;

namespace AbxPilot.UI.Printing;

public static class PrintSheet
{
    private static readonly TimeSpan KeepFor = TimeSpan.FromDays(1);

    public static string Folder => Path.Combine(Path.GetTempPath(), "AbxPilot");

    public static string Html(IReadOnlyList<SummaryPart> parts, string language, DateTime printedAt)
    {
        var ink = Hex(Brush("Surface", Colors.Black));
        var paper = Hex(Brush("TextBody", Colors.White));
        var line = Px(Resource("BorderWidth", new Thickness(1)).Top);
        var size = (string key) => Px(Tokens.Number(key));
        var title = parts.FirstOrDefault(part => part.Kind == SummaryKind.Title)?.Text ?? "AbxPilot";
        var html = new StringBuilder();
        html.Append("<!doctype html><html lang=\"").Append(Encode(language)).Append("\"><head><meta charset=\"utf-8\">");
        html.Append("<title>").Append(Encode(title)).Append("</title><style>");
        html.Append("@page{margin:").Append(size("Space5")).Append("}@media screen{body{padding:").Append(size("Space5")).Append("}}");
        html.Append("body{margin:0;background:").Append(paper).Append(";color:").Append(ink)
            .Append(";font-family:'Atkinson Hyperlegible Next','Segoe UI',system-ui,sans-serif;font-size:")
            .Append(size("FontSize2")).Append(";line-height:").Append(Tokens.Number("LineHeightBody").ToString(CultureInfo.InvariantCulture)).Append('}');
        html.Append("h1,h2{line-height:").Append(Tokens.Number("LineHeightHeading").ToString(CultureInfo.InvariantCulture)).Append("}h1{font-size:").Append(size("FontSize4")).Append(";margin:0 0 ").Append(size("Space2")).Append('}');
        html.Append("h2{font-size:").Append(size("FontSize3")).Append(";margin:0 0 ").Append(size("Space4")).Append('}');
        html.Append("h3{font-size:").Append(size("FontSize2")).Append(";margin:").Append(size("Space4")).Append(" 0 ").Append(size("Space1")).Append('}');
        html.Append("p,li{margin:0 0 ").Append(size("Space1")).Append('}');
        html.Append("ul{margin:0 0 ").Append(size("Space3")).Append(";padding-left:").Append(size("Space5")).Append('}');
        html.Append(".dose{font-weight:600}");
        html.Append(".warn{border-left:").Append(size("Space1")).Append(" solid ").Append(ink).Append(";padding-left:").Append(size("Space2")).Append('}');
        html.Append(".meta,.foot{font-size:").Append(size("FontSize1")).Append('}');
        html.Append(".foot{border-top:").Append(line).Append(" solid ").Append(ink).Append(";margin-top:").Append(size("Space5")).Append(";padding-top:").Append(size("Space2")).Append('}');
        html.Append("</style></head><body>");
        SummaryKind? list = null;
        foreach (var part in parts)
        {
            var listed = part.Kind is SummaryKind.Dose or SummaryKind.Answer ? part.Kind : (SummaryKind?)null;
            if (list != listed)
            {
                if (list is not null) html.Append("</ul>");
                if (listed is not null) html.Append("<ul>");
                list = listed;
            }
            var text = Encode(part.Text);
            html.Append(part.Kind switch
            {
                SummaryKind.Title => $"<h1>{text}</h1>",
                SummaryKind.Headline => $"<h2>{text}</h2>",
                SummaryKind.Dose => $"<li class=\"dose\">{text}</li>",
                SummaryKind.Answer => $"<li>{text}</li>",
                SummaryKind.Warning => $"<p class=\"warn\">! {text}</p>",
                SummaryKind.Heading => $"<h3>{text}</h3>",
                SummaryKind.Meta => $"<p class=\"meta\">{text}</p>",
                SummaryKind.Disclaimer => $"<p class=\"foot\">{text}</p>",
                _ => $"<p>{text}</p>"
            });
        }
        if (list is not null) html.Append("</ul>");
        html.Append("<p class=\"meta\">").Append(Encode(printedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture))).Append("</p>");
        html.Append("<script>addEventListener('load',function(){print()})</script></body></html>");
        return html.ToString();
    }

    public static FileInfo Write(IReadOnlyList<SummaryPart> parts, string language)
    {
        var now = DateTime.Now;
        Directory.CreateDirectory(Folder);
        foreach (var old in new DirectoryInfo(Folder).GetFiles("ozet-*.html"))
            if (now - old.LastWriteTime > KeepFor)
                try { old.Delete(); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        var file = new FileInfo(Path.Combine(Folder, $"ozet-{now:yyyyMMdd-HHmmss}.html"));
        File.WriteAllText(file.FullName, Html(parts, language, now), new UTF8Encoding(false));
        return file;
    }

    private static Color Brush(string key, Color fallback) =>
        Resource<ISolidColorBrush?>(key, null) is { } brush ? brush.Color : fallback;

    private static T Resource<T>(string key, T fallback) =>
        Application.Current is { } app && app.TryGetResource(key, app.ActualThemeVariant, out var value) && value is T typed
            ? typed
            : fallback;

    private static string Hex(Color color) => $"#{color.R:x2}{color.G:x2}{color.B:x2}";

    private static string Px(double value) => value.ToString(CultureInfo.InvariantCulture) + "px";

    private static string Encode(string text) => WebUtility.HtmlEncode(text);
}
