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
    private static IntPtr Text_ShadowmapResolution0 = IntPtr.Zero;
    private static IntPtr Text_ShadowmapResolution1 = IntPtr.Zero;
    private static IntPtr Text_ShadowmapResolution2 = IntPtr.Zero;
    private static IntPtr Text_ShadowmapResolution3 = IntPtr.Zero;
    private static byte[] OriginalBytes_ShadowCascade0 = new byte[32];
    private static byte[] OriginalBytes_ShadowCascade1 = new byte[32];
    private static byte[] OriginalBytes_ShadowCascade2 = new byte[32];
    private static byte[] OriginalBytes_ShadowCascade3 = new byte[32];
    private static byte[] OriginalBytes_ShadowmapResolution0 = new byte[32];
    private static byte[] OriginalBytes_ShadowmapResolution1 = new byte[32];
    private static byte[] OriginalBytes_ShadowmapResolution2 = new byte[32];
    private static byte[] OriginalBytes_ShadowmapResolution3 = new byte[32];
    private static bool HacksWasEnabled = false;
    private static bool ShadowmapWasEnabled = false;
    private static int NewShadowmapResolution = 0x00001000;

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
            Marshal.WriteInt32(addr + 1, NewShadowmapResolution);
            MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteRead);
        }
    }

    public static void ReadWriteShadowmapCode2(IntPtr addr, ref byte[] originalBytes, int byteCount = 16) {
        if (addr != IntPtr.Zero) {
            MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteReadWrite);
            for (int index = 0; index < byteCount; index++) {
                originalBytes[index] = Marshal.ReadByte(addr + index);
            }
            Marshal.WriteInt32(addr + 4, NewShadowmapResolution);
            Marshal.WriteInt32(addr + 8 + 4, NewShadowmapResolution);
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

    public static unsafe void RestoreShadowmapCode(IntPtr addr, byte[] originalBytes, int byteCount = 5)
    {
        ShadowManager* shadowManager = ShadowManager.Instance();

        if (addr != IntPtr.Zero)
        {
            RestoreCode(addr, originalBytes, byteCount);
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
        Text_ShadowCascade0 = Service.SigScanner.ScanText("F3 0F 11 ?? ?? F3 44 0F 5C EC");
        Text_ShadowCascade1 = Service.SigScanner.ScanText("F3 0F 11 ?? ?? F3 41 0F 58 D8 F3 0F 11 57 ??");
        Text_ShadowCascade2 = Service.SigScanner.ScanText("F3 0F 11 ?? ?? 48 8D 9F ?? ?? ?? ?? ?? ?? 00 00 00");
        Text_ShadowCascade3 = Service.SigScanner.ScanText("F3 44 0F 11 6F ?? 48 8B 05");

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

        Text_ShadowmapResolution0 = Service.SigScanner.ScanText("BA 00 08 00 00 EB ?? ?? 00 04 00 00 EB ?? BA 00 02 00 00"); // BA will be different if regalloc changes again
        Text_ShadowmapResolution1 = Service.SigScanner.ScanText("?? B8 00 08 00 00 EB ?? ?? ?? 00 04 00 00 EB ?? ?? ?? 00 02 00 00") + 1; // B8 will be different if regalloc changes again
        Text_ShadowmapResolution2 = Service.SigScanner.ScanText("BD 00 08 00 00 EB ?? ?? 00 04 00 00 EB ?? ?? 00 02 00 00"); // BD will be different if regalloc changes again
        Text_ShadowmapResolution3 = Service.SigScanner.ScanText("C7 44 24 ?? 00 02 00 00 C7 44 24 ?? 00 02 00 00"); // wildcard struct shift

        ReadWriteShadowmapCode(Text_ShadowmapResolution0, ref OriginalBytes_ShadowmapResolution0);
        ReadWriteShadowmapCode(Text_ShadowmapResolution1, ref OriginalBytes_ShadowmapResolution1);
        ReadWriteShadowmapCode(Text_ShadowmapResolution2, ref OriginalBytes_ShadowmapResolution2);
        ReadWriteShadowmapCode2(Text_ShadowmapResolution3, ref OriginalBytes_ShadowmapResolution3);
        shadowManager->Unk_Bitfield |= 1;
        
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
            RestoreShadowmapCode(Text_ShadowmapResolution0, OriginalBytes_ShadowmapResolution0);
            RestoreShadowmapCode(Text_ShadowmapResolution1, OriginalBytes_ShadowmapResolution1);
            RestoreShadowmapCode(Text_ShadowmapResolution2, OriginalBytes_ShadowmapResolution2);
            RestoreShadowmapCode(Text_ShadowmapResolution3, OriginalBytes_ShadowmapResolution3, 16);
            Text_ShadowmapResolution0 = IntPtr.Zero;
            Text_ShadowmapResolution1 = IntPtr.Zero;
            Text_ShadowmapResolution2 = IntPtr.Zero;
            Text_ShadowmapResolution3 = IntPtr.Zero;
        }
        ShadowmapWasEnabled = false;
    }
}
