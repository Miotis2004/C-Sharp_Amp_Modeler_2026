using System;
using System.Threading.Tasks;
using AmpModeler.Audio.Abstractions;
using AmpModeler.Core;

namespace AmpModeler.Engine
{
    public class AudioEngine : IDisposable
    {
        private IAudioHost _host;
        private readonly BlockAdaptor _blockAdaptor;
        private readonly IAudioProcessor _dspGraph; // This will eventually be the full graph

        public bool IsRunning => _host?.IsRunning ?? false;

        public AudioEngine()
        {
            // For now, we use a simple passthrough as the "DSP Graph"
            _dspGraph = new PassthroughProcessor();

            // Wrap it in the block adaptor (e.g., 256 samples fixed)
            _blockAdaptor = new BlockAdaptor(_dspGraph, 256);
        }

        public void SetHost(IAudioHost host)
        {
            if (_host != null)
            {
                // Detach from old host if necessary
                 _host.StopAsync().Wait(); // simplified for now
            }

            _host = host;
            _host.SetProcessor(_blockAdaptor);
        }

        public async Task StartAsync()
        {
            if (_host == null) throw new InvalidOperationException("No audio host set.");

            // Prepare DSP
            // Note: We need to know sample rate.
            // If host is initialized, it might have a sample rate.
            // Typically host calls PrepareToPlay on the processor when it starts.

            await _host.StartAsync();
        }

        public async Task StopAsync()
        {
            if (_host != null)
            {
                await _host.StopAsync();
            }
        }

        public void Dispose()
        {
            StopAsync().Wait();
        }
    }

    // Temporary processor for testing
    public class PassthroughProcessor : IAudioProcessor
    {
        public void PrepareToPlay(double sampleRate, int estimatedBlockSize)
        {
            // No-op
        }

        public void Process(AudioBuffer buffer)
        {
            // Identity: Input is already in the buffer, so we do nothing.
            // If we wanted to silence it: buffer.Clear();

            // Let's lower the volume slightly to prove we are touching it (safety)
            for (int ch = 0; ch < buffer.ChannelCount; ch++)
            {
                var samples = buffer.GetChannel(ch);
                for (int i = 0; i < samples.Length; i++)
                {
                    samples[i] *= 0.9f;
                }
            }
        }
    }
}
