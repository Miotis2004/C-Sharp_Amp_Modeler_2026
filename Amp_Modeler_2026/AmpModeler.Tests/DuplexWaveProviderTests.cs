using Xunit;
using AmpModeler.Audio.Wasapi;
using AmpModeler.Core;
using NAudio.Wave;
using System;

namespace AmpModeler.Tests
{
    class MockProcessor : IAudioProcessor
    {
        public bool ProcessCalled { get; private set; }

        public void PrepareToPlay(double sampleRate, int estimatedBlockSize) { }

        public void Process(AudioBuffer buffer)
        {
            ProcessCalled = true;
            // Passthrough for verification
            // Just copy some known pattern or leave it (identity if input matches)
        }
    }

    public class DuplexWaveProviderTests
    {
        [Fact]
        public void Read_WithInsufficientInput_ReturnsSilence()
        {
            var format = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
            var proc = new MockProcessor();
            var provider = new DuplexWaveProvider(proc, format);

            byte[] buffer = new byte[100];
            // Pre-fill with noise to verify it gets cleared
            for(int i=0; i<100; i++) buffer[i] = 0xFF;

            int read = provider.Read(buffer, 0, 100);

            Assert.Equal(100, read);
            Assert.Equal(0, buffer[0]); // Should be cleared to 0
            Assert.False(proc.ProcessCalled);
        }

        [Fact]
        public void Read_WithSufficientInput_CallsProcess()
        {
            var format = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
            var proc = new MockProcessor();
            var provider = new DuplexWaveProvider(proc, format);

            // Create some fake input (silence is fine, just need bytes)
            byte[] input = new byte[400]; // enough bytes
            provider.AddInput(input, 400);

            byte[] output = new byte[100];
            int read = provider.Read(output, 0, 100);

            Assert.Equal(100, read);
            Assert.True(proc.ProcessCalled);
        }
    }
}
