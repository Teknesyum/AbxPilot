namespace AbxPilot.Tests;

internal static class RepoPaths
{
    internal static readonly string Root = FindRoot();

    internal static readonly string UiRoot = Path.Combine(Root, "src", "AbxPilot.UI");

    internal static string Relative(string path) =>
        Path.GetRelativePath(Root, path).Replace('\\', '/');

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AbxPilot.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? AppContext.BaseDirectory;
    }
}
