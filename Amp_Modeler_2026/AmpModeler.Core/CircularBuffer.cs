using System;

namespace AmpModeler.Core
{
    /// <summary>
    /// A fixed-size circular buffer for float data.
    /// This class is not thread-safe. Synchronization is the responsibility of the caller.
    /// </summary>
    public class CircularBuffer
    {
        private readonly float[] _buffer;
        private int _writeIndex;
        private int _readIndex;
        private int _count;

        /// <summary>
        /// Gets the maximum capacity of the buffer.
        /// </summary>
        public int Capacity => _buffer.Length;

        /// <summary>
        /// Gets the number of elements currently in the buffer.
        /// </summary>
        public int Count => _count;

        public CircularBuffer(int capacity)
        {
            if (capacity <= 0) throw new ArgumentException("Capacity must be positive", nameof(capacity));
            _buffer = new float[capacity];
        }

        /// <summary>
        /// Writes data to the buffer.
        /// </summary>
        public int Write(ReadOnlySpan<float> data)
        {
            int writable = Capacity - _count;
            int toWrite = Math.Min(data.Length, writable);

            if (toWrite == 0) return 0;

            int firstChunk = Math.Min(toWrite, Capacity - _writeIndex);
            data.Slice(0, firstChunk).CopyTo(_buffer.AsSpan(_writeIndex));

            int secondChunk = toWrite - firstChunk;
            if (secondChunk > 0)
            {
                data.Slice(firstChunk, secondChunk).CopyTo(_buffer.AsSpan(0));
            }

            _writeIndex = (_writeIndex + toWrite) % Capacity;
            _count += toWrite;

            return toWrite;
        }

        /// <summary>
        /// Reads data from the buffer.
        /// </summary>
        public int Read(Span<float> destination)
        {
            int toRead = Math.Min(destination.Length, _count);

            if (toRead == 0) return 0;

            int firstChunk = Math.Min(toRead, Capacity - _readIndex);
            _buffer.AsSpan(_readIndex, firstChunk).CopyTo(destination.Slice(0, firstChunk));

            int secondChunk = toRead - firstChunk;
            if (secondChunk > 0)
            {
                _buffer.AsSpan(0, secondChunk).CopyTo(destination.Slice(firstChunk, secondChunk));
            }

            _readIndex = (_readIndex + toRead) % Capacity;
            _count -= toRead;

            return toRead;
        }

        /// <summary>
        /// Resets the buffer to empty state.
        /// </summary>
        public void Clear()
        {
            _writeIndex = 0;
            _readIndex = 0;
            _count = 0;
            Array.Clear(_buffer, 0, _buffer.Length);
        }
    }
}
