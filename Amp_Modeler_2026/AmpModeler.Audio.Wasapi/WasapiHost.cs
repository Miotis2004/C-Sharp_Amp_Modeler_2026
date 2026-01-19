using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmpModeler.Audio.Abstractions;
using AmpModeler.Core;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace AmpModeler.Audio.Wasapi
{
    public class WasapiHost : IAudioHost, IDisposable
    {
        private MMDeviceEnumerator _enumerator;
        private WasapiCapture _capture;
        private WasapiOut _render;
        private IAudioProcessor _processor;

        private MMDevice _inputDevice;
        private MMDevice _outputDevice;

        private DuplexWaveProvider _duplexProvider;
        private bool _isRunning;

        public bool IsRunning => _isRunning;

        public double SampleRate => _render?.OutputWaveFormat.SampleRate ?? 48000; // Default or actual

        public WasapiHost()
        {
            _enumerator = new MMDeviceEnumerator();
        }

        public IEnumerable<AudioDeviceDescriptor> GetInputDevices()
        {
            return _enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
                .Select(d => new AudioDeviceDescriptor(d.ID, d.FriendlyName, d.ID == _enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia).ID));
        }

        public IEnumerable<AudioDeviceDescriptor> GetOutputDevices()
        {
            return _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                .Select(d => new AudioDeviceDescriptor(d.ID, d.FriendlyName, d.ID == _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).ID));
        }

        public void SetInputDevice(AudioDeviceDescriptor device)
        {
            _inputDevice = _enumerator.GetDevice(device.Id);
        }

        public void SetOutputDevice(AudioDeviceDescriptor device)
        {
            _outputDevice = _enumerator.GetDevice(device.Id);
        }

        public void SetProcessor(IAudioProcessor processor)
        {
            _processor = processor;
        }

        public async Task StartAsync()
        {
            if (_isRunning) return;
            if (_processor == null) throw new InvalidOperationException("Processor not set");

            // 1. Initialize Devices
            // Use defaults if not set
            if (_outputDevice == null)
                _outputDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (_inputDevice == null)
                _inputDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);

            // 2. Setup Render (Output)
            // Shared mode: Latency ~ 100ms default. We try to request lower if possible,
            // but WasapiOut (Shared) doesn't give much control over buffer size in constructor.
            _render = new WasapiOut(_outputDevice, AudioClientShareMode.Shared, true, 20); // 20ms latency goal

            // 3. Setup Capture (Input)
            // We need to match the format of the output if possible, or resample.
            // For MVP, we hope they match or NAudio handles it.
            // WASAPI Shared mode enforces the device's mix format.
            _capture = new WasapiCapture(_inputDevice, true, 20); // 20ms

            // 4. Verify Formats
            // In a real app, we need to handle format conversion (resampling).
            // Here we assume 44.1/48kHz match or close enough for testing logic.
            var format = _render.OutputWaveFormat;

            // 5. Initialize Processor
            _processor.PrepareToPlay(format.SampleRate, format.AverageBytesPerSecond / 100); // approx block size

            // 6. Setup Pipeline
            _duplexProvider = new DuplexWaveProvider(_processor, format);

            _capture.DataAvailable += (s, e) =>
            {
                _duplexProvider.AddInput(e.Buffer, e.BytesRecorded);
            };

            _capture.StartRecording();

            _render.Init(_duplexProvider);
            _render.Play();

            _isRunning = true;
            await Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            if (!_isRunning) return;

            _render?.Stop();
            _render?.Dispose();
            _render = null;

            _capture?.StopRecording();
            _capture?.Dispose();
            _capture = null;

            _isRunning = false;
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            StopAsync().Wait();
            _enumerator?.Dispose();
        }
    }
}
