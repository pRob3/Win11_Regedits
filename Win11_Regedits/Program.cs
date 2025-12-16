using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;

internal static class Program
{
    private const string Guid = "{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}";
    private const string ClsidPath = @"Software\Classes\CLSID\" + Guid;
    private const string InprocPath = ClsidPath + @"\InprocServer32";

    private static int Main()
    {
        Console.Title = "Win11 Context Menu Helper";

        while (true)
        {
            DrawHeader();

            Console.WriteLine("Choose an option:");
            Console.WriteLine("  [1] Disable \"Show more options\" (classic full context menu)");
            Console.WriteLine("  [2] Enable \"Show more options\" (Windows 11 default menu)");
            Console.WriteLine("  [3] Restart Explorer only");
            Console.WriteLine("  [Q] Quit");
            Console.WriteLine();

            Console.Write("Selection: ");
            var choice = (Console.ReadLine() ?? string.Empty).Trim().ToUpperInvariant();

            try
            {
                switch (choice)
                {
                    case "1":
                        DisableShowMoreOptions(); // enable classic menu
                        ApplyShellChanges();
                        RestartExplorer();
                        Info("Done: \"Show more options\" DISABLED (classic full context menu enabled).");
                        PromptForSessionAction();
                        break;

                    case "2":
                        EnableShowMoreOptions(); // restore Win11 default
                        ApplyShellChanges();
                        RestartExplorer();
                        Info("Done: \"Show more options\" ENABLED (Windows 11 default restored).");
                        PromptForSessionAction();
                        break;

                    case "3":
                        ApplyShellChanges();
                        RestartExplorer();
                        Info("Done: Explorer restarted.");
                        Pause();
                        break;

                    case "Q":
                        return 0;

                    default:
                        Warn("Unknown choice.");
                        Pause();
                        break;
                }
            }
            catch (Exception ex)
            {
                Error(ex.Message);
                Pause();
            }
        }
    }

    private static void DrawHeader()
    {
        Console.Clear();
        bool disabled = IsShowMoreOptionsDisabled();

        Console.WriteLine("===============================================================");
        Console.WriteLine($"  WIN11 CONTEXT MENU HELPER  ::  CLSID {Guid}");
        Console.WriteLine("---------------------------------------------------------------");
        Console.WriteLine($"  \"Show more options\" status: {(disabled ? "DISABLED" : "ENABLED")}");
        Console.WriteLine("===============================================================");
        Console.WriteLine();
    }

    private static bool IsShowMoreOptionsDisabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(InprocPath, writable: false);
        if (key is null) return false;

        var val = key.GetValue(null);
        return val is string s && s.Length == 0;
    }

    // Disable "Show more options" == force classic menu
    private static void DisableShowMoreOptions()
    {
        // Mirror regedit import exactly: create parent key and subkey.
        using var clsid = Registry.CurrentUser.CreateSubKey(ClsidPath, writable: true)
            ?? throw new InvalidOperationException("Could not create CLSID key.");

        using var inproc = Registry.CurrentUser.CreateSubKey(InprocPath, writable: true)
            ?? throw new InvalidOperationException("Could not create InprocServer32 key.");

        // Set (Default) value to empty string (@="")
        inproc.SetValue(null, string.Empty, RegistryValueKind.String);
    }

    // Enable "Show more options" == restore Windows 11 default menu
    private static void EnableShowMoreOptions()
    {
        // Mirror your working .reg order
        Registry.CurrentUser.DeleteSubKeyTree(InprocPath, throwOnMissingSubKey: false);
        Registry.CurrentUser.DeleteSubKeyTree(ClsidPath, throwOnMissingSubKey: false);
    }

    private static void RestartExplorer()
    {
        foreach (var p in Process.GetProcessesByName("explorer"))
        {
            try { p.Kill(entireProcessTree: true); }
            catch { /* ignore */ }
        }

        Thread.Sleep(1200);

        Process.Start(new ProcessStartInfo("explorer.exe")
        {
            UseShellExecute = true
        });
    }

    // ---- Make changes "stick" like regedit does ----

    private static void ApplyShellChanges()
    {
        // 1) Broadcast setting change (what regedit effectively triggers)
        BroadcastSettingChange("Software\\Classes");
        BroadcastSettingChange("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer");

        // 2) Shell change notify (forces shell refresh)
        SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
    }

    private static void PromptForSessionAction()
    {
        Console.WriteLine();
        Console.WriteLine("The change has been applied.");
        Console.WriteLine("If \"Show more options\" is still visible, a restart or sign-out may be required.");
        Console.WriteLine();
        Console.WriteLine("  [R] Restart Windows now");
        Console.WriteLine("  [L] Sign out now");
        Console.WriteLine("  [C] Continue");
        Console.WriteLine();

        Console.Write("Selection: ");
        var choice = (Console.ReadLine() ?? string.Empty).Trim().ToUpperInvariant();

        switch (choice)
        {
            case "R":
                Process.Start(new ProcessStartInfo("shutdown", "/r /t 0")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                break;

            case "L":
                Process.Start(new ProcessStartInfo("shutdown", "/l")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                break;

            case "C":
            default:
                // Do nothing, return to main menu
                break;
        }
    }

    private const int HWND_BROADCAST = 0xffff;
    private const int WM_SETTINGCHANGE = 0x001A;

    [Flags]
    private enum SendMessageTimeoutFlags : uint
    {
        SMTO_ABORTIFHUNG = 0x0002
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        int msg,
        IntPtr wParam,
        string lParam,
        SendMessageTimeoutFlags flags,
        uint timeout,
        out IntPtr result);

    private static void BroadcastSettingChange(string path)
    {
        _ = SendMessageTimeout(
            new IntPtr(HWND_BROADCAST),
            WM_SETTINGCHANGE,
            IntPtr.Zero,
            path,
            SendMessageTimeoutFlags.SMTO_ABORTIFHUNG,
            250,
            out _);
    }

    private const uint SHCNE_ASSOCCHANGED = 0x08000000;
    private const uint SHCNF_IDLIST = 0x0000;

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

    // ---- Console helpers ----

    private static void Pause()
    {
        Console.WriteLine();
        Console.Write("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void Info(string message) => WriteLineColored(message, ConsoleColor.Gray);
    private static void Warn(string message) => WriteLineColored("WARN: " + message, ConsoleColor.Yellow);
    private static void Error(string message) => WriteLineColored("ERROR: " + message, ConsoleColor.Red);

    private static void WriteLineColored(string message, ConsoleColor color)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = prev;
    }
}
