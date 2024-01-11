using Dalamud.Memory;
using System;
using System.Runtime.InteropServices;

namespace BetterShadows;

public static class BytecodeHelper {
    public static void ReadWriteNops(IntPtr addr, ref byte[] originalBytes, int byteCount) {
        const byte NOP = 0x90;
        if (addr != IntPtr.Zero) {
            MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteReadWrite);
            for (int index = 0; index < byteCount; index++) {
                originalBytes[index] = Marshal.ReadByte(addr + index);
                Marshal.WriteByte(addr + index, NOP);
            }
            MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteRead);
        }
    }

    public static void RestoreNops(IntPtr addr, byte[] originalBytes, int byteCount) {
        if (addr != IntPtr.Zero) {
            MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteReadWrite);
            for (int index = 0; index < byteCount; index++) {
                Marshal.WriteByte(addr + index, originalBytes[index]);
            }
            MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteRead);
        }
    }

    private static byte?[] ParseCodePattern(string codePattern) {
        var parts = codePattern.Split(' ');
        var bytes = new byte?[parts.Length];
    
        for (int index = 0; index < parts.Length; index++) {
            if (parts[index] == "??") {
                bytes[index] = null;
            } else {
                bytes[index] = Convert.ToByte(parts[index], 16);
            }
        }
    
        return bytes;
    }

    public static void ReadWriteCode(IntPtr addr, ref byte[] originalBytes, string codePattern) {
        if (addr == IntPtr.Zero) {
            return;
        }
    
        var codeBytes = ParseCodePattern(codePattern);
        int byteCount = codeBytes.Length;
        originalBytes = new byte[byteCount];
    
        MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteReadWrite);
    
        for (int index = 0; index < byteCount; index++) {
            originalBytes[index] = Marshal.ReadByte(addr + index);
            if (codeBytes[index] != null) {
                Marshal.WriteByte(addr + index, codeBytes[index].Value);
            }
        }
    
        MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteRead);
    }

    public static void RestoreCode(IntPtr addr, byte[] originalBytes) {
        if (addr == IntPtr.Zero) {
            return;
        }
    
        int byteCount = originalBytes.Length;
        MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteReadWrite);
    
        for (int index = 0; index < byteCount; index++) {
            Marshal.WriteByte(addr + index, originalBytes[index]);
        }
    
        MemoryHelper.ChangePermission(addr, byteCount, MemoryProtection.ExecuteRead);
    }
}
