<<<<<<< HEAD
﻿#nullable enable
using System;
using YARG.Core.Audio;
=======
﻿using AOT;
using ManagedBass;
using ManagedBass.Fx;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using UnityEngine;
using YARG.Audio.PitchDetection;
using YARG.Core.Audio;
using YARG.Core.IO;
>>>>>>> cba5bd20 (Vocals works)
using YARG.Core.Logging;
using YARG.Input;

namespace YARG.Audio.BASS
{
<<<<<<< HEAD
    /// <summary>
    ///     Unified player microphone device for both ASIO and Shared Audio backends.
    ///     Manages timed input frames for vocal gameplay scoring and coordinates live monitoring.
    /// </summary>
=======
    internal class MonitorPlaybackHandle : IDisposable
    {
        private static readonly ReverbParameters REVERB_PARAMETERS = new()
        {
            fDryMix = 0.3f, fWetMix = 1f, fRoomSize = 0.4f, fDamp = 0.7f
        };

#nullable enable
        public static MonitorPlaybackHandle? Create()
#nullable disable
        {
            // Set up monitoring stream
            int monitorPlaybackHandle = Bass.CreateStream(44100, 1, BassFlags.Default, StreamProcedureType.Push);
            if (monitorPlaybackHandle == 0)
            {
                YargLogger.LogFormatError("Failed to create monitor stream: {0}!", Bass.LastError);
                return null;
            }

            // Add reverb to the monitor playback
            int reverbHandle = BassHelpers.FXAddParameters(monitorPlaybackHandle, EffectType.Freeverb, REVERB_PARAMETERS, 1);
            if (reverbHandle == 0)
            {
                YargLogger.LogError("Failed to add reverb to monitor stream!");
                Bass.StreamFree(monitorPlaybackHandle);
                return null;
            }

            // Apply gain to the playback
            int applyGain = Bass.ChannelSetDSP(monitorPlaybackHandle, ApplyGain);
            if (applyGain == 0)
            {
                YargLogger.LogFormatError("Failed to add gain to monitor stream: {0}!", Bass.LastError);
                Bass.StreamFree(monitorPlaybackHandle);
                return null;
            }

            // Start monitoring
            if (!Bass.ChannelPlay(monitorPlaybackHandle))
            {
                YargLogger.LogFormatError("Failed to start monitor stream: {0}!", Bass.LastError);
                Bass.StreamFree(applyGain);
                Bass.StreamFree(monitorPlaybackHandle);
                return null;
            }

            return new MonitorPlaybackHandle(monitorPlaybackHandle, reverbHandle, applyGain);
        }

        public readonly int Handle;
        private readonly int _reverbHandle;
        private readonly int _applyGain;

        private bool _disposed;

        private MonitorPlaybackHandle(int handle, int reverb, int applyGain)
        {
            Handle = handle;
            _reverbHandle = reverb;
            _applyGain = applyGain;
        }

        [MonoPInvokeCallback(typeof(SyncProcedure))]
        private static void ApplyGain(int handle, int channel, IntPtr buffer, int length, IntPtr user)
        {
            BassHelpers.ApplyGain(1.3f, buffer, length);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                Bass.StreamFree(Handle);
                Bass.StreamFree(_applyGain);
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MonitorPlaybackHandle()
        {
            Dispose(false);
        }
    }

    internal class RecordingHandle : IDisposable
    {
#nullable enable
        public static RecordingHandle? CreateRecordingHandle(BassMicDevice device)
        {
            var devPeriod = Bass.GetConfig(Configuration.DevicePeriod);

            // Keep a GCHandle so IL2CPP can track the instance
            var gcHandle = GCHandle.Alloc(device);

            int handle = Bass.RecordStart(
                44100,
                1,
                BassFlags.Default,
                devPeriod,
                BassMicDevice.ProcessRecordDataStatic,
                (IntPtr) gcHandle
            );

            if (handle == 0)
            {
                YargLogger.LogFormatError("Failed to start clean recording: {0}!", Bass.LastError);
                gcHandle.Free();
                return null;
            }

            int processedHandle = Bass.CreateStream(44100, 1, BassFlags.Decode, StreamProcedureType.Push);
            if (processedHandle == 0)
            {
                YargLogger.LogFormatError("Failed to create processed recording stream: {0}!", Bass.LastError);
                Bass.StreamFree(handle);
                gcHandle.Free();
                return null;
            }

            return new RecordingHandle(handle, processedHandle, devPeriod, gcHandle);
        }

        public readonly int Handle;
        public readonly int ProcessedHandle;

        public readonly int RecordPeriod;

        public readonly GCHandle GcHandle;

        private bool _disposed;

        private RecordingHandle(int handle, int processedHandle, int period, GCHandle gcHandle)
        {
            Handle = handle;
            ProcessedHandle = processedHandle;
            RecordPeriod = period;
            GcHandle = gcHandle;
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (GcHandle.IsAllocated) GcHandle.Free();
                Bass.ChannelStop(Handle);
                Bass.StreamFree(Handle);

                Bass.ChannelStop(ProcessedHandle);
                Bass.StreamFree(ProcessedHandle);
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RecordingHandle()
        {
            Dispose(false);
        }
    }

>>>>>>> cba5bd20 (Vocals works)
    public sealed class BassMicDevice : MicDevice
    {
        private readonly IBassMicSource  _source;
        private          BassMicAnalyzer _analyzer;
        private readonly object          _lifecycleLock = new();
        private          bool                 _disposed;

        internal BassMicDevice(IBassMicSource source) : base(source.DisplayName)
        {
<<<<<<< HEAD
            _source = source;
            _analyzer = CreateAnalyzer();
            _source.InputChanged += RecreateAnalyzer;
=======
            // Must initialise device before recording
            if (!Bass.RecordInit(deviceId))
            {
                if (Bass.LastError != Errors.Already)
                {
                    YargLogger.LogFormatError("Failed to initialize recording device: {0}!", Bass.LastError);
                    return null;
                }
                Bass.CurrentRecordingDevice = deviceId;
            }

            var monitorPlayback = MonitorPlaybackHandle.Create();
            if (monitorPlayback == null)
            {
                return null;
            }

            var device = new BassMicDevice(deviceId, name, monitorPlayback);
            device._recordHandle = RecordingHandle.CreateRecordingHandle(device);
            if (device._recordHandle == null)
            {
                // Not device.Dispose() as to not free resources that we may want to keep around
                // i.e, the record-enabled device
                monitorPlayback.Dispose();
                return null;
            }

            int lowEqHandle = BassHelpers.AddEqToChannel(device._recordHandle.ProcessedHandle, _lowEqParameters);
            int highEqHandle = BassHelpers.AddEqToChannel(device._recordHandle.ProcessedHandle, _highEqParameters);
            if (lowEqHandle == 0 || highEqHandle == 0)
            {
                YargLogger.LogFormatError("Failed to add EQ to processed recording stream: {0}!", Bass.LastError);
                device.Dispose();
                return null;
            }
            return device;
>>>>>>> cba5bd20 (Vocals works)
        }

        internal static BassMicDevice? Create(IBassMicSource source)
        {
            try
            {
                return new BassMicDevice(source);
            }
            catch (Exception exception)
            {
                YargLogger.LogException(exception, $"Failed to initialize microphone '{source.DisplayName}'");
                source.Dispose();
                return null;
            }
        }

        public bool TryCreateRecordingChannel(bool withEffects, out int handle, out int sampleRate)
            => _source.TryCreateRecordingChannel(withEffects, out handle, out sampleRate);

        public void ReleaseRecordingChannel(int handle) => _source.ReleaseRecordingChannel(handle);

        private BassMicAnalyzer CreateAnalyzer() =>
            new(_source, () => IsRecordingOutput, () => InputManager.CurrentInputTime);

        private void RecreateAnalyzer()
        {
            lock (_lifecycleLock)
            {
                if (_disposed)
                {
                    return;
                }

                _analyzer.Dispose();
                try
                {
                    _analyzer = CreateAnalyzer();
                }
                catch (Exception exception)
                {
                    YargLogger.LogException(exception, $"Failed to recreate analyzer for '{DisplayName}'");
                    return;
                }
            }
        }

        public override int Reset()
        {
            lock (_lifecycleLock)
            {
                bool sourceReset = _source.Reset();
                bool analyzerReset = _analyzer.Reset();
                return sourceReset && analyzerReset ? 0 : -1;
            }
        }

        public override bool DequeueOutputFrame(out MicOutputFrame frame)
        {
            lock (_lifecycleLock)
            {
                return _analyzer.DequeueOutputFrame(out frame);
            }
        }

        public override void ClearOutputQueue()
        {
            lock (_lifecycleLock)
            {
                _analyzer.ClearOutputQueue();
            }
        }

        public override void SetMonitoringLevel(float volume) => _source.SetMonitoringLevel(volume);

        public override void SetReverbLevel(float wet) => _source.SetReverbLevel(wet);

<<<<<<< HEAD
        public override SerializedMic Serialize() => new(_source.BaseName, _source.Channel);
=======
        [MonoPInvokeCallback(typeof(RecordProcedure))]
        public static bool ProcessRecordDataStatic(int handle, IntPtr buffer, int length, IntPtr user)
        {
            if (user == IntPtr.Zero) return true; // safety
            var gcHandle = GCHandle.FromIntPtr(user);
            var device = (BassMicDevice) gcHandle.Target;
            return device.ProcessRecordDataInstance(handle, buffer, length);
        }

        private bool ProcessRecordDataInstance(int handle, IntPtr buffer, int length)
        {
            // Copies the data from the recording buffer to the monitor playback buffer.
            if (Bass.StreamPutData(_monitorHandle.Handle, buffer, length) == -1)
            {
                YargLogger.LogFormatError("Error pushing data to monitor stream: {0}", Bass.LastError);
            }
>>>>>>> cba5bd20 (Vocals works)

        public override MicBufferInfo? GetBufferInfo() => _source.GetBufferInfo();

        protected override void DisposeUnmanagedResources()
        {
            BassMicAnalyzer analyzer;
            lock (_lifecycleLock)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _source.InputChanged -= RecreateAnalyzer;
                analyzer = _analyzer;
            }

            analyzer.Dispose();
            _source.Dispose();
        }
    }
}
