using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using EasyHook;
using static EasyHook.RemoteHooking;

namespace NKCore
{
    public class Main : IEntryPoint
    {
        Process FFXIV;
        public IntPtr LobbyErrorHandler { get; private set; }
        public IntPtr StartHandler { get; private set; }
        public IntPtr LoginHandler { get; private set; }

        private delegate Int64 StartHandlerDelegate(Int64 a1, Int64 a2);
        private delegate Int64 LoginHandlerDelegate(Int64 a1, Int64 a2);
        private delegate char LobbyErrorHandlerDelegate(Int64 a1, Int64 a2, Int64 a3);

        private LobbyErrorHandlerDelegate LobbyErrorHookOriginal;
        private StartHandlerDelegate StartHookOriginal;
        private LoginHandlerDelegate LoginHookOriginal;

        private SigScanner Scanner;

        public Main(IContext InContext, String ChannelName)
        {
            FFXIV = Process.GetCurrentProcess();
            Scanner = new SigScanner(FFXIV.MainModule);
        }

        public void Run(IContext InContext, String ChannelName)
        {

            LobbyErrorHandler = Scanner.ScanText("40 53 48 83 EC 30 48 8B D9 49 8B C8 E8 ?? ?? ?? ?? 8B D0");
            try
            {
                StartHandler = Scanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? B2 01 49 8B CC");
            }
            catch (Exception)
            {
                StartHandler = Scanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? B2 01 49 8B CD");
            }
            if (StartHandler == IntPtr.Zero)
            {
                StartHandler = Scanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? B2 01 49 8B CD");
            }
            LoginHandler = Scanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B6 81 ?? ?? ?? ?? 40 32 FF");

            var LobbyErrorHook = LocalHook.Create(LobbyErrorHandler, new LobbyErrorHandlerDelegate(LobbyErrorHandlerDetour), null);
            var StartHook = LocalHook.Create(StartHandler, new StartHandlerDelegate(StartHandlerDetour), null);
            var LoginHook = LocalHook.Create(LoginHandler, new LoginHandlerDelegate(LoginHandlerDetour), null);

            LobbyErrorHookOriginal = Marshal.GetDelegateForFunctionPointer<LobbyErrorHandlerDelegate>(LobbyErrorHook.HookBypassAddress);
            StartHookOriginal = Marshal.GetDelegateForFunctionPointer<StartHandlerDelegate>(StartHook.HookBypassAddress);   
            LoginHookOriginal = Marshal.GetDelegateForFunctionPointer<LoginHandlerDelegate>(LoginHook.HookBypassAddress);

            LobbyErrorHook.ThreadACL.SetExclusiveACL(null);
            StartHook.ThreadACL.SetExclusiveACL(null);
            LoginHook.ThreadACL.SetExclusiveACL(null);

            while (true)
            {
                Thread.Sleep(100);
            }
        }


        public Int64 StartHandlerDetour(Int64 a1, Int64 a2)
        {
            var a1_88 = (UInt16)Marshal.ReadInt16(new IntPtr(a1 + 88));
            var a1_456 = Marshal.ReadInt32(new IntPtr(a1 + 456));
            if (a1_456 != 0)
            {
                Marshal.WriteInt32(new IntPtr(a1 + 456), 0);
            }
            return (Int64)StartHookOriginal(a1, a2);
        }

        public Int64 LoginHandlerDetour(Int64 a1, Int64 a2)
        {
            var a1_2165 = Marshal.ReadByte(new IntPtr(a1 + 2165));
            if (a1_2165 != 0)
            {
                Marshal.WriteByte(new IntPtr(a1 + 2165), 0);
            }
            return (Int64)LoginHookOriginal(a1, a2);
        }

        public char LobbyErrorHandlerDetour(Int64 a1, Int64 a2, Int64 a3)
        {
            IntPtr p3 = new IntPtr(a3);
            var t1 = Marshal.ReadByte(p3);
            var v4 = ((t1 & 0xF) > 0) ? (uint)Marshal.ReadInt32(p3 + 8) : 0;
            UInt16 v4_16 = (UInt16)(v4);
            if (v4 > 0)
            {
                if (v4_16 == 0x332C) // Auth failed
                {
                }
                else
                {
                    Marshal.WriteInt64(p3 + 8, 0x3E80); // server connection lost
                    // 0x3390: maintenance
                    v4 = ((t1 & 0xF) > 0) ? (uint)Marshal.ReadInt32(p3 + 8) : 0;
                    v4_16 = (UInt16)(v4);
                }
            }
            return (char)LobbyErrorHookOriginal(a1, a2, a3);
        }
    }
}
