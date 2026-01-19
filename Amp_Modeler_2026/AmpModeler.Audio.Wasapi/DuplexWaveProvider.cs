using System;
using System.Runtime.InteropServices;
using NAudio.Wave;
using AmpModeler.Core;

namespace AmpModeler.Audio.Wasapi
{
    /// <summary>
    /// A WaveProvider that buffers input, runs it through an IAudioProcessor, and provides it for output.
    /// </summary>
    public class DuplexWaveProvider : IWaveProvider
    {
        private readonly IAudioProcessor _processor;
        private readonly WaveFormat _waveFormat;
        private readonly BufferedWaveProvider _inputBuffer; // Thread-safe buffer from NAudio

        // Internal buffers
        private readonly byte[] _byteBuffer;
        private readonly float[][] _floatChannels;
        private readonly int _maxFrames;

        public WaveFormat WaveFormat => _waveFormat;

        public DuplexWaveProvider(IAudioProcessor processor, WaveFormat format)
        {
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _waveFormat = format ?? throw new ArgumentNullException(nameof(format));

            if (format.Encoding != WaveFormatEncoding.IeeeFloat && format.Encoding != WaveFormatEncoding.Pcm)
            {
                // Only basic support for now
                 // Ideally we'd throw or support conversion.
            }

            // Buffer enough input to handle jitter.
            _inputBuffer = new BufferedWaveProvider(format)
            {
                DiscardOnBufferOverflow = true,
                BufferDuration = TimeSpan.FromMilliseconds(500)
            };

            // Allocate max buffer sizes
            int maxBytes = format.AverageBytesPerSecond / 5; // ~200ms max chunk
            _byteBuffer = new byte[maxBytes];

            _maxFrames = maxBytes / (format.BitsPerSample / 8) / format.Channels;

            // Allocate backing arrays for the AudioBuffer reuse
            _floatChannels = new float[format.Channels][];
            for(int i=0; i<format.Channels; i++)
            {
                _floatChannels[i] = new float[_maxFrames];
            }
        }

        public void AddInput(byte[] buffer, int count)
        {
            _inputBuffer.AddSamples(buffer, 0, count);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            // 1. Check if we have enough input
            if (_inputBuffer.BufferedBytes < count)
            {
                Array.Clear(buffer, offset, count);
                return count;
            }

            // 2. Read from input buffer
            // We use our internal byte buffer to avoid unsafe checks on the passed buffer (though could do directly)
            int readBytes = _inputBuffer.Read(_byteBuffer, 0, count);
            if (readBytes == 0) return 0;

            int bytesPerSample = _waveFormat.BitsPerSample / 8;
            int frames = readBytes / bytesPerSample / _waveFormat.Channels;

            // 3. Convert Bytes -> Float
            // Zero-allocation conversion using Spans
            ConvertBytesToFloat(_byteBuffer.AsSpan(0, readBytes), frames);

            // 4. Process
            // Create a lightweight wrapper around our existing arrays with the EXACT frame count
            // This is crucial: we only want the processor to see 'frames' amount of data.
            // The constructor we added to AudioBuffer is cheap (reference assignment).
            var wrapper = new AudioBuffer(_floatChannels, frames);

            _processor.Process(wrapper);

            // 5. Convert Float -> Bytes
            ConvertFloatToBytes(_byteBuffer.AsSpan(0, readBytes), frames);

            // 6. Copy to output
            _byteBuffer.AsSpan(0, readBytes).CopyTo(buffer.AsSpan(offset, count));

            return readBytes;
        }

        private void ConvertBytesToFloat(ReadOnlySpan<byte> source, int frames)
        {
            int channels = _waveFormat.Channels;

            // Optimized path for IEEE Float 32-bit
            if (_waveFormat.Encoding == WaveFormatEncoding.IeeeFloat && _waveFormat.BitsPerSample == 32)
            {
                // Source is Interleaved: L R L R
                // Dest is Planar: LLL RRR

                // Reinterpret bytes as floats
                var sourceFloats = MemoryMarshal.Cast<byte, float>(source);

                for (int ch = 0; ch < channels; ch++)
                {
                    var dest = _floatChannels[ch].AsSpan(0, frames);
                    for (int i = 0; i < frames; i++)
                    {
                        dest[i] = sourceFloats[(i * channels) + ch];
                    }
                }
            }
            else if (_waveFormat.Encoding == WaveFormatEncoding.Pcm && _waveFormat.BitsPerSample == 16)
            {
                var sourceShorts = MemoryMarshal.Cast<byte, short>(source);
                const float scalar = 1.0f / 32768.0f;

                for (int ch = 0; ch < channels; ch++)
                {
                     var dest = _floatChannels[ch].AsSpan(0, frames);
                     for(int i=0; i<frames; i++)
                     {
                         dest[i] = sourceShorts[(i * channels) + ch] * scalar;
                     }
                }
            }
        }

        private void ConvertFloatToBytes(Span<byte> dest, int frames)
        {
            int channels = _waveFormat.Channels;

            if (_waveFormat.Encoding == WaveFormatEncoding.IeeeFloat && _waveFormat.BitsPerSample == 32)
            {
                 var destFloats = MemoryMarshal.Cast<byte, float>(dest);

                 for (int ch = 0; ch < channels; ch++)
                 {
                     var src = _floatChannels[ch].AsSpan(0, frames);
                     for (int i = 0; i < frames; i++)
                     {
                         destFloats[(i * channels) + ch] = src[i];
                     }
                 }
            }
            else if (_waveFormat.Encoding == WaveFormatEncoding.Pcm && _waveFormat.BitsPerSample == 16)
            {
                var destShorts = MemoryMarshal.Cast<byte, short>(dest);

                for (int ch = 0; ch < channels; ch++)
                {
                    var src = _floatChannels[ch].AsSpan(0, frames);
                    for (int i = 0; i < frames; i++)
                    {
                        float sample = src[i];
                        // Hard clip
                        if (sample > 1.0f) sample = 1.0f;
                        if (sample < -1.0f) sample = -1.0f;

                        destShorts[(i * channels) + ch] = (short)(sample * 32767.0f);
                    }
                }
            }
        }
    }
}
