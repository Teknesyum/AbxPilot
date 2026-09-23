namespace AbxPilot.KbCompiler;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("Usage: AbxPilot.KbCompiler <kb-dir> <out-dir>");
            return 2;
        }

        Console.Error.WriteLine("Not implemented yet: the compiler arrives in stage A1.");
        return 1;
    }
}
