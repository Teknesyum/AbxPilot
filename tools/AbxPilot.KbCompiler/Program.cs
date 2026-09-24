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

        return KbCompilation.Run(args[0], args[1], Console.Out, Console.Error);
    }
}
