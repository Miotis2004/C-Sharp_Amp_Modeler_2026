using Xunit;
using AmpModeler.Core;
using System;

namespace AmpModeler.Tests
{
    public class CircularBufferTests
    {
        [Fact]
        public void Capacity_IsCorrect()
        {
            var buf = new CircularBuffer(100);
            Assert.Equal(100, buf.Capacity);
        }

        [Fact]
        public void Write_And_Read_FullBuffer()
        {
            var buf = new CircularBuffer(5);
            float[] input = { 1, 2, 3, 4, 5 };
            float[] output = new float[5];

            int written = buf.Write(input);
            Assert.Equal(5, written);
            Assert.Equal(5, buf.Count);

            int read = buf.Read(output);
            Assert.Equal(5, read);
            Assert.Equal(input, output);
            Assert.Equal(0, buf.Count);
        }

        [Fact]
        public void WrapAround_Works()
        {
            var buf = new CircularBuffer(5);
            float[] input1 = { 1, 2, 3 };
            float[] output1 = new float[3];

            buf.Write(input1);
            buf.Read(output1); // Buffer empty, indices moved

            float[] input2 = { 4, 5, 6, 7 }; // Should wrap: 4, 5 at end, 6, 7 at start
            int written = buf.Write(input2);
            Assert.Equal(4, written);

            float[] output2 = new float[4];
            int read = buf.Read(output2);

            Assert.Equal(4, read);
            Assert.Equal(input2, output2);
        }

        [Fact]
        public void Overflow_OnlyWritesCapacity()
        {
            var buf = new CircularBuffer(3);
            float[] input = { 1, 2, 3, 4, 5 };

            int written = buf.Write(input);
            Assert.Equal(3, written);
            Assert.Equal(3, buf.Count);

            float[] output = new float[3];
            buf.Read(output);
            Assert.Equal(new float[]{ 1, 2, 3}, output);
        }
    }
}
