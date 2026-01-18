using System;

namespace AmpModeler.Core
{
    /// <summary>
    /// Represents a multichannel block of audio samples.
    /// </summary>
    public class AudioBuffer
    {
        private readonly float[][] _channels;

        /// <summary>
        /// Gets the number of channels in the buffer.
        /// </summary>
        public int ChannelCount => _channels.Length;

        /// <summary>
        /// Gets the number of samples per channel.
        /// </summary>
        public int Length { get; }

        public AudioBuffer(int channelCount, int length)
        {
            if (channelCount < 1) throw new ArgumentException("Must have at least one channel", nameof(channelCount));
            if (length < 1) throw new ArgumentException("Length must be positive", nameof(length));

            _channels = new float[channelCount][];
            for (int i = 0; i < channelCount; i++)
            {
                _channels[i] = new float[length];
            }
            Length = length;
        }

        /// <summary>
        /// Gets a span representing the samples for a specific channel.
        /// </summary>
        public Span<float> GetChannel(int channelIndex)
        {
            return _channels[channelIndex];
        }

        /// <summary>
        /// Clears all channels to silence.
        /// </summary>
        public void Clear()
        {
            foreach (var channel in _channels)
            {
                Array.Clear(channel, 0, channel.Length);
            }
        }
    }
}
