using System.Runtime.InteropServices;

namespace AbxPilot.UI;

public static partial class Motion
{
    private const uint SpiGetClientAreaAnimation = 0x1042;

    public static bool Reduced
    {
        get
        {
            if (!OperatingSystem.IsWindows()) return false;
            var enabled = 1;
            return SystemParametersInfo(SpiGetClientAreaAnimation, 0, ref enabled, 0) && enabled == 0;
        }
    }

    [LibraryImport("user32.dll", EntryPoint = "SystemParametersInfoW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SystemParametersInfo(uint action, uint param, ref int value, uint winIni);
}
