using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zodiark;
using EasyHook;
using System.Diagnostics;
using System.IO;

namespace NoKill
{
    internal class Program : IEntryPoint
    {
        static Process FFXIV;
        static ZodiarkProcess ZodiarkFFXIV;
        static public IntPtr LobbyErrorHandler { get; private set; }
        static public IntPtr StartHandler { get; private set; }
        static public IntPtr LoginHandler { get; private set; }

        private delegate Int64 StartHandlerDelegate(Int64 a1, Int64 a2);
        private delegate Int64 LoginHandlerDelegate(Int64 a1, Int64 a2);
        private delegate char LobbyErrorHandlerDelegate(Int64 a1, Int64 a2, Int64 a3);

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                FFXIV = Process.GetProcessesByName("ffxiv_dx11").FirstOrDefault();
            } else
            {
                FFXIV = Process.GetProcessById(int.Parse(args[0]));
            }
            ZodiarkFFXIV = new ZodiarkProcess(FFXIV);
            LobbyErrorHandler = ZodiarkFFXIV.Scanner.ScanText("40 53 48 83 EC 30 48 8B D9 49 8B C8 E8 ?? ?? ?? ?? 8B D0");
            try
            {
                StartHandler = ZodiarkFFXIV.Scanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? B2 01 49 8B CC");
            }
            catch (Exception)
            {
                StartHandler = ZodiarkFFXIV.Scanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? B2 01 49 8B CD");
            }
            LoginHandler = ZodiarkFFXIV.Scanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B6 81 ?? ?? ?? ?? 40 32 FF");
            Console.WriteLine($"LobbyErrorHandler {LobbyErrorHandler:X}");
            Console.WriteLine($"StartHandler {StartHandler:X}");
            Console.WriteLine($"LoginHandler {LoginHandler:X}");
            var exePath = Process.GetCurrentProcess().MainModule.FileName;
            var dllPath = Path.Combine(Path.GetDirectoryName(exePath), "NKCore.dll");
            Console.WriteLine($"Current Process Name: {Process.GetCurrentProcess().ProcessName}");
            Console.WriteLine($"Current Process Dir: {Process.GetCurrentProcess().MainModule.FileName}");
            Console.WriteLine($"Dll path: {dllPath}");
            var dllDir = Path.GetDirectoryName(dllPath);
            Config.HelperLibraryLocation = dllDir;
            Config.DependencyPath = dllDir;
            RemoteHooking.Inject(
                FFXIV.Id,
                InjectionOptions.DoNotRequireStrongName,
                dllPath,
                dllPath,
                string.Empty
            );
        }
    }
}
