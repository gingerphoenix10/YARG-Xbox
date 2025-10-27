#if UNITY_WSA && !UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using YARG.Audio.PitchDetection;
using YARG.Core.Audio;
using YARG.Core.Logging;
using YARG.Input;
using YARG.Settings;

namespace YARG.Audio.UWP
{
    public sealed class UWPMicDevice : MicDevice
    {
        private AudioGraph _graph;
        private AudioDeviceInputNode _inputNode;
        private AudioDeviceOutputNode _outputNode;
        private AudioFrameOutputNode _frameOutput;

        private readonly ConcurrentQueue<MicOutputFrame> _frameQueue = new();
        private readonly PitchTracker _pitchDetector = new();

        private float? _lastPitch;
        private float? _lastAmplitude;
        private const float MIC_HIT_INPUT_THRESHOLD = 25f;

        public static async Task<UWPMicDevice> CreateAsync(string deviceName)
        {
            var mic = new UWPMicDevice(deviceName);
            await mic.InitializeGraph();
            return mic;
        }

        private UWPMicDevice(string name) : base(name) { }

        private async Task InitializeGraph()
        {
            var settings = new AudioGraphSettings(AudioRenderCategory.Media);
            var result = await AudioGraph.CreateAsync(settings);
            if (result.Status != AudioGraphCreationStatus.Success)
                throw new Exception("Failed to create AudioGraph");

            _graph = result.Graph;

            var inputResult = await _graph.CreateDeviceInputNodeAsync(MediaCategory.Other);
            if (inputResult.Status != AudioDeviceNodeCreationStatus.Success)
                throw new Exception("Failed to create input node");

            _inputNode = inputResult.DeviceInputNode;

            var outputResult = await _graph.CreateDeviceOutputNodeAsync();
            if (outputResult.Status == AudioDeviceNodeCreationStatus.Success)
            {
                _outputNode = outputResult.DeviceOutputNode;
                _inputNode.AddOutgoingConnection(_outputNode);
            }

            _frameOutput = _graph.CreateFrameOutputNode();
            _inputNode.AddOutgoingConnection(_frameOutput);

            _graph.QuantumStarted += OnQuantumStarted;
            _graph.Start();
        }

        private unsafe void OnQuantumStarted(AudioGraph sender, object args)
        {
            AudioFrame frame = _frameOutput.GetFrame();
            using (var buffer = frame.LockBuffer(AudioBufferAccessMode.Read))
            using (var reference = buffer.CreateReference())
            {
                ((IMemoryBufferByteAccess) reference).GetBuffer(out byte* dataInBytes, out uint capacity);
                int sampleCount = (int) capacity / sizeof(float);
                float* floatPtr = (float*) dataInBytes;
                Span<float> samples = new Span<float>(floatPtr, sampleCount);
                ProcessSamples(samples);
            }
        }

        private void ProcessSamples(ReadOnlySpan<float> samples)
        {
            // Calculate amplitude (RMS)
            float sum = 0;
            for (int i = 0; i < samples.Length; i += 4)
                sum += samples[i] * samples[i];
            sum = Mathf.Sqrt(sum / (samples.Length / 4));
            float amplitude = 20f * Mathf.Log10(sum * 180f);
            if (amplitude < -160f)
                amplitude = -160f;

            // Detect hits
            if (_lastAmplitude != null && amplitude > _lastAmplitude &&
                Mathf.Abs(amplitude - _lastAmplitude.Value) >= MIC_HIT_INPUT_THRESHOLD)
            {
                _frameQueue.Enqueue(new MicOutputFrame(InputManager.CurrentInputTime, true, -1f, -1f));
            }

            _lastAmplitude = amplitude;

            // Skip pitch if silent
            if (amplitude < SettingsManager.Settings.MicrophoneSensitivity.Value)
            {
                _lastPitch = null;
                return;
            }

            var pitchOutput = _pitchDetector.ProcessBuffer(samples);
            if (pitchOutput.HasValue)
                _lastPitch = pitchOutput;

            if (_lastPitch.HasValue)
                _frameQueue.Enqueue(new MicOutputFrame(InputManager.CurrentInputTime, false, _lastPitch.Value, amplitude));
        }

        public override bool DequeueOutputFrame(out MicOutputFrame frame) =>
            _frameQueue.TryDequeue(out frame);

        public override void ClearOutputQueue() => _frameQueue.Clear();

        public override void SetMonitoringLevel(float volume)
        {
            if (_outputNode != null)
                _outputNode.OutgoingGain = volume;
        }

        protected override void DisposeUnmanagedResources()
        {
            _graph?.Stop();
            _inputNode?.Dispose();
            _outputNode?.Dispose();
            _frameOutput?.Dispose();
            _graph?.Dispose();
        }

        public override SerializedMic Serialize() { return new SerializedMic(DisplayName); }

        public override int Reset()
        {
            try
            {
                // 1. Clear pending mic data
                _frameQueue.Clear();
                _lastPitch = null;
                _lastAmplitude = null;

                // 2. Restart the AudioGraph to effectively flush buffers
                if (_graph != null)
                {
                    bool wasRunning = _graph.CompletedQuantumCount > 0;
                    if (wasRunning)
                    {
                        _graph.Stop();
                        _graph.ResetAllNodes(); // Clears internal graph buffers
                        _graph.Start();
                    }
                }

                // 3. Reset internal counters
                _pitchDetector.Reset();
                return 0; // success
            }
            catch (Exception ex)
            {
                YargLogger.LogFormatError("Failed to reset mic: {0}", ex.Message);
                return -1;
            }
        }

    }

    // Helper for IMemoryBufferByteAccess
    [System.Runtime.InteropServices.Guid("5B0D3235-4DBA-4D44-8659-0D6C0F8EAE8C")]
    [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
    interface IMemoryBufferByteAccess
    {
        unsafe void GetBuffer(out byte* buffer, out uint capacity);
    }

}
#endif