using Xunit;
using AmpModeler.Engine;
using AmpModeler.Core;
using System;

namespace AmpModeler.Tests
{
    // A mock processor that doubles the input
    class MockDoubler : IAudioProcessor
    {
        public void PrepareToPlay(double sampleRate, int estimatedBlockSize) { }
        public void Process(AudioBuffer buffer)
        {
            for(int c=0; c<buffer.ChannelCount; c++)
            {
                var s = buffer.GetChannel(c);
                for(int i=0; i<s.Length; i++) s[i] *= 2.0f;
            }
        }
    }

    public class BlockAdaptorTests
    {
        [Fact]
        public void Process_WithExactBlockSize_PassesThrough()
        {
            int blockSize = 10;
            var adaptor = new BlockAdaptor(new MockDoubler(), blockSize);

            var buffer = new AudioBuffer(1, blockSize);
            var span = buffer.GetChannel(0);
            for(int i=0; i<10; i++) span[i] = 1.0f;

            adaptor.Process(buffer);

            // Since latency is involved (input ring -> internal block -> output ring),
            // The first block output might be silence (latency = 1 block usually if logic is strictly sequential buffering)
            // Let's trace the logic:
            // 1. Write 10 samples to input ring. Count = 10.
            // 2. While (Count >= 10):
            //    Read 10. Process (become 2.0). Write 10 to output ring.
            // 3. Read 10 from output ring.

            // So latency should be 0 blocks *if* the processing happens in the same call.
            var outSpan = buffer.GetChannel(0);

            // Should be 2.0f
            Assert.Equal(2.0f, outSpan[0]);
        }

        [Fact]
        public void Process_WithSmallerChunks_Accumulates()
        {
            int blockSize = 10;
            var adaptor = new BlockAdaptor(new MockDoubler(), blockSize);

            var buffer = new AudioBuffer(1, 5); // Half block
            buffer.GetChannel(0).Fill(1.0f);

            // First call: Writes 5. Total 5. Not enough to process.
            // Reads 5 from output (which is empty/silence).
            adaptor.Process(buffer);

            // Output should be silence (underrun handled by 0 padding)
            // Note: The implementation logic:
            // read = _outRings[ch].Read(dest); -> returns 0
            // dest.Slice(read).Fill(0); -> Fills with 0
            Assert.Equal(0.0f, buffer.GetChannel(0)[0]);

            // Second call: Writes 5. Total 10.
            // Process runs on the 10 samples. Output ring gets 10 samples (value 2.0).
            // Reads 5 from output ring.
            buffer.GetChannel(0).Fill(1.0f);
            adaptor.Process(buffer);

            Assert.Equal(2.0f, buffer.GetChannel(0)[0]);

            // Note: There are still 5 samples (value 2.0) left in output ring.
        }
    }
}
