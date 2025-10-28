using AOT;
using ManagedBass;
using System;
using System.IO;
using System.Runtime.InteropServices;
using YARG;
using YARG.Audio.BASS;

public class ILBassStreamProcedures
{
    public static FileProcedures Callbacks = new FileProcedures
    {
        Close = CloseCallback,
        Length = LengthCallback,
        Read = ReadCallback,
        Seek = SeekCallback
    };

    public static IntPtr CreateUserData(Stream stream)
    {
        GCHandle handle = GCHandle.Alloc(stream);
        return GCHandle.ToIntPtr(handle);
    }

    public static void FreeUserData(IntPtr user)
    {
        if (user != IntPtr.Zero)
        {
            GCHandle handle = GCHandle.FromIntPtr(user);
            handle.Free();
        }
    }

    [MonoPInvokeCallback(typeof(FileCloseProcedure))]
    private static void CloseCallback(IntPtr user)
    {
        var stream = GetStream(user);
        stream?.Close();
        FreeUserData(user);
    }

    [MonoPInvokeCallback(typeof(FileLengthProcedure))]
    private static long LengthCallback(IntPtr user)
    {
        var stream = GetStream(user);
        return stream?.Length ?? 0;
    }

    [MonoPInvokeCallback(typeof(FileReadProcedure))]
    private static int ReadCallback(IntPtr buffer, int length, IntPtr user)
    {
        var stream = GetStream(user);
        if (stream == null) return 0;

        byte[] temp = new byte[length];
        int read;
        try
        {
            read = stream.Read(temp, 0, length);
            Marshal.Copy(temp, 0, buffer, read);
        }
        catch
        {
            read = 0;
        }

        return read;
    }

    [MonoPInvokeCallback(typeof(FileSeekProcedure))]
    private static bool SeekCallback(long offset, IntPtr user)
    {
        var stream = GetStream(user);
        if (stream == null) return false;

        try
        {
            stream.Seek(offset, SeekOrigin.Begin);
            return true;
        }
        catch
        {
            return false;
        }
    }

    [MonoPInvokeCallback(typeof(SyncProcedure))]
    public static void SongEndSyncCallback(int handle, int channel, int data, IntPtr user)
    {
        if (user == IntPtr.Zero) return;

        var gch = GCHandle.FromIntPtr(user);
        if (!(gch.Target is BassStemMixer instance))
            return;

        var end = instance._songEnd;
        if (end != null)
        {
            UnityMainThreadCallback.QueueEvent(end.Invoke);
        }
    }

    private static Stream GetStream(IntPtr user)
    {
        if (user == IntPtr.Zero) return null;
        GCHandle handle = GCHandle.FromIntPtr(user);
        return handle.Target as Stream;
    }
}
