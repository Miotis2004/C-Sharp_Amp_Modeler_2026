using System;
using AmpModeler.Core;

namespace AmpModeler.Engine
{
    /// <summary>
    /// Adapts variable-sized audio buffers from the host to fixed-sized blocks for the internal processor.
    /// </summary>
    public class BlockAdaptor : IAudioProcessor
    {
        private readonly IAudioProcessor _internalProcessor;
        private readonly int _fixedBlockSize;
        private readonly CircularBuffer[] _inputBuffers;
        private readonly CircularBuffer[] _outputBuffers;
        private readonly AudioBuffer _processingBuffer;
        private int _channels;

        public BlockAdaptor(IAudioProcessor internalProcessor, int fixedBlockSize)
        {
            _internalProcessor = internalProcessor ?? throw new ArgumentNullException(nameof(internalProcessor));
            if (fixedBlockSize <= 0) throw new ArgumentException("Block size must be positive", nameof(fixedBlockSize));

            _fixedBlockSize = fixedBlockSize;

            // We don't know the channel count yet, will initialize in PrepareToPlay
            // But we need to handle the case where PrepareToPlay is called multiple times.
            _inputBuffers = new CircularBuffer[0];
            _outputBuffers = new CircularBuffer[0];
            _processingBuffer = new AudioBuffer(1, fixedBlockSize); // Dummy
        }

        public void PrepareToPlay(double sampleRate, int estimatedBlockSize)
        {
             // We can assume stereo for now or pass channel count in PrepareToPlay in the future interface update.
             // For now, IAudioProcessor.PrepareToPlay signature in Core/IAudioProcessor.cs only has sampleRate and estimatedBlockSize.
             // We will lazy-initialize on first Process or assume a default.
             // However, to be safe, let's update IAudioProcessor to include channels later or infer it.
             // The AudioBuffer passed to Process() has channel count.
             // We will initialize buffers on the first Process call if needed, or update the interface.
             // Updating the interface is cleaner.

             _internalProcessor.PrepareToPlay(sampleRate, _fixedBlockSize);
        }

        private CircularBuffer[] _inRings;
        private CircularBuffer[] _outRings;
        private AudioBuffer _internalBlock;

        public void Process(AudioBuffer buffer)
        {
            int channels = buffer.ChannelCount;
            int inputLength = buffer.Length;

            // Initialize or re-initialize if channel count changes
            if (_inRings == null || _inRings.Length != channels)
            {
                _inRings = new CircularBuffer[channels];
                _outRings = new CircularBuffer[channels];
                // Capacity: 3x block size + input size to be safe against jitter
                int capacity = Math.Max(_fixedBlockSize * 4, inputLength * 4);

                for(int i=0; i<channels; i++)
                {
                    _inRings[i] = new CircularBuffer(capacity);
                    _outRings[i] = new CircularBuffer(capacity);
                    // Pre-fill output rings with silence to mitigate initial underrun latency
                    // Logic: we need to output something immediately.
                    // If we don't pre-fill, we output silence until we process a block.
                }
                _internalBlock = new AudioBuffer(channels, _fixedBlockSize);
            }

            // 1. Push input to input rings
            for (int ch = 0; ch < channels; ch++)
            {
                _inRings[ch].Write(buffer.GetChannel(ch));
            }

            // 2. Process as many fixed blocks as possible
            while (_inRings[0].Count >= _fixedBlockSize)
            {
                // Pull from input rings to internal block
                for (int ch = 0; ch < channels; ch++)
                {
                    _inRings[ch].Read(_internalBlock.GetChannel(ch));
                }

                // Process
                _internalProcessor.Process(_internalBlock);

                // Push to output rings
                for (int ch = 0; ch < channels; ch++)
                {
                    _outRings[ch].Write(_internalBlock.GetChannel(ch));
                }
            }

            // 3. Pull output from output rings to buffer
            // Note: If not enough data, we output silence (underrun)
            for (int ch = 0; ch < channels; ch++)
            {
                var dest = buffer.GetChannel(ch);
                int read = _outRings[ch].Read(dest);

                if (read < dest.Length)
                {
                    // Underrun: Clear the rest
                    dest.Slice(read).Fill(0);
                }
            }
        }
    }
}
