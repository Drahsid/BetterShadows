using Dalamud.Logging;
using Dalamud.Memory;
using System;
using System.Runtime.InteropServices;

namespace BetterShadows;

internal class CodeManager
{
    private static IntPtr Text_ShadowCascade0 = IntPtr.Zero;
    private static IntPtr Text_ShadowCascade1 = IntPtr.Zero;
    private static IntPtr Text_ShadowCascade2 = IntPtr.Zero;
    private static IntPtr Text_ShadowCascade3 = IntPtr.Zero;
    private static IntPtr Text_ShadowmapResolution = IntPtr.Zero;
    private static byte[] OriginalBytes_ShadowCascade0 = new byte[8];
    private static byte[] OriginalBytes_ShadowCascade1 = new byte[8];
    private static byte[] OriginalBytes_ShadowCascade2 = new byte[8];
    private static byte[] OriginalBytes_ShadowCascade3 = new byte[8];
    private static byte[] OriginalBytes_ShadowmapResolution = new byte[8];
    private static bool HacksWasEnabled = false;
    private static bool ShadowmapWasEnabled = false;

    public static void ReadWriteCode(IntPtr addr, ref byte[] originalBytes, int byteCount = 5)
    {
        const byte NOP = 0x90;
        if (addr != IntPtr.Zero)
        {
            MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteReadWrite);
            for (int index = 0; index < byteCount; index++)
            {
                originalBytes[index] = Marshal.ReadByte(addr + index);
                Marshal.WriteByte(addr + index, NOP);
            }
            MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteRead);
        }
    }

    public static void ReadWriteShadowmapCode(IntPtr addr, ref byte[] originalBytes, int byteCount = 5)
    {
        if (addr != IntPtr.Zero)
        {
            MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteReadWrite);
            for (int index = 0; index < byteCount; index++)
            {
                originalBytes[index] = Marshal.ReadByte(addr + index);
            }
            Marshal.WriteInt32(addr + 1, 0x00001000);
            MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteRead);
        }
    }

    public static void RestoreCode(IntPtr addr, byte[] originalBytes, int byteCount = 5)
    {
        if (addr != IntPtr.Zero)
        {
            MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteReadWrite);
            for (int index = 0; index < byteCount; index++)
            {
                Marshal.WriteByte(addr + index, originalBytes[index]);
            }
            MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteRead);
        }
    }

    public static unsafe void RestoreShadowmapCode(IntPtr addr, byte[] originalBytes)
    {
        ShadowManager* shadowManager = ShadowManager.Instance();

        if (addr != IntPtr.Zero)
        {
            RestoreCode(addr, originalBytes);
            shadowManager->Unk_Bitfield |= 1;
        }
    }

    public static unsafe void DoEnableHacks()
    {
        ShadowManager* shadowManager = ShadowManager.Instance();

        if (shadowManager == null)
        {
            PluginLog.Error("shadowManager is null!");
            return;
        }

        // if regalloc ever changes, these will fail; may be better to hijack the whole function
        Text_ShadowCascade0 = Service.SigScanner.ScanText("F3 0F 11 4F 44 F3 44 0F 5C");
        Text_ShadowCascade1 = Service.SigScanner.ScanText("F3 0F 11 47 48 F3 41 0F 58");
        Text_ShadowCascade2 = Service.SigScanner.ScanText("F3 0F 11 5F 4C 48 8D 9F 18");
        Text_ShadowCascade3 = Service.SigScanner.ScanText("F3 44 0F 11 6F 50 48 8B 05");

        ReadWriteCode(Text_ShadowCascade0, ref OriginalBytes_ShadowCascade0);
        ReadWriteCode(Text_ShadowCascade1, ref OriginalBytes_ShadowCascade1);
        ReadWriteCode(Text_ShadowCascade2, ref OriginalBytes_ShadowCascade2);
        ReadWriteCode(Text_ShadowCascade3, ref OriginalBytes_ShadowCascade3, 6);

        HacksWasEnabled = true;
    }

    public static unsafe void DoEnableShadowmap()
    {
        ShadowManager* shadowManager = ShadowManager.Instance();

        if (shadowManager == null)
        {
            PluginLog.Error("shadowManager is null!");
            return;
        }

        Text_ShadowmapResolution = Service.SigScanner.ScanText("BA ?? ?? ?? ?? EB 0C BA 00 04");

        ReadWriteShadowmapCode(Text_ShadowmapResolution, ref OriginalBytes_ShadowmapResolution);
        if (shadowManager != null)
        {
            shadowManager->Unk_Bitfield |= 1;
        }
        
        ShadowmapWasEnabled = true;
    }

    public static void DoDisableHacks()
    {
        if (HacksWasEnabled) {
            RestoreCode(Text_ShadowCascade0, OriginalBytes_ShadowCascade0);
            RestoreCode(Text_ShadowCascade1, OriginalBytes_ShadowCascade1);
            RestoreCode(Text_ShadowCascade2, OriginalBytes_ShadowCascade2);
            RestoreCode(Text_ShadowCascade3, OriginalBytes_ShadowCascade3, 6);

            Text_ShadowCascade0 = IntPtr.Zero;
            Text_ShadowCascade1 = IntPtr.Zero;
            Text_ShadowCascade2 = IntPtr.Zero;
            Text_ShadowCascade3 = IntPtr.Zero;
        }

        HacksWasEnabled = false;
    }

    public static void DoDisableShadowmap()
    {
        if (ShadowmapWasEnabled) {
            RestoreShadowmapCode(Text_ShadowmapResolution, OriginalBytes_ShadowmapResolution);
            Text_ShadowmapResolution = IntPtr.Zero;
        }
        ShadowmapWasEnabled = false;
    }
}
