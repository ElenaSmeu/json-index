﻿using System;
using System.IO;
using Lucene.Net.Store;

namespace DotJEM.Json.Index.Storage
{
    public class RamInputStream : IndexInput
    {
        private readonly RamFile file;

        internal static readonly int BUFFER_SIZE = 1024;
        private long length;
        private byte[] currentBuffer;
        private int currentBufferIndex;
        private int bufferPosition;
        private long bufferStart;
        private int bufferLength;

        public override long FilePointer
        {
            get
            {
                if (currentBufferIndex >= 0)
                    return bufferStart + bufferPosition;
                return 0L;
            }
        }

        static RamInputStream()
        {
        }

        public RamInputStream(RamFile f)
        {
            file = f;
            length = file.Length;
            if (length / BUFFER_SIZE >= int.MaxValue)
                throw new IOException("Too large RAMFile! " + length);
            currentBufferIndex = -1;
            currentBuffer = null;
        }

        protected override void Dispose(bool disposing)
        {
        }

        public override long Length()
        {
            return length;
        }

        public override byte ReadByte()
        {
            if (bufferPosition >= bufferLength)
            {
                ++currentBufferIndex;
                SwitchCurrentBuffer(true);
            }
            return currentBuffer[bufferPosition++];
        }

        public override void ReadBytes(byte[] b, int offset, int len)
        {
            while (len > 0)
            {
                if (bufferPosition >= bufferLength)
                {
                    ++currentBufferIndex;
                    SwitchCurrentBuffer(true);
                }
                int num = bufferLength - bufferPosition;
                int length = len < num ? len : num;
                Array.Copy(currentBuffer, bufferPosition, b, offset, length);
                offset += length;
                len -= length;
                bufferPosition += length;
            }
        }

        private void SwitchCurrentBuffer(bool enforceEOF)
        {
            if (currentBufferIndex >= file.NumBuffers())
            {
                if (enforceEOF)
                    throw new IOException("Read past EOF");
                --currentBufferIndex;
                bufferPosition = BUFFER_SIZE;
            }
            else
            {
                currentBuffer = file.GetBuffer(currentBufferIndex);
                bufferPosition = 0;
                bufferStart = BUFFER_SIZE * (long)currentBufferIndex;
                long num = length - bufferStart;
                bufferLength = num > (long)BUFFER_SIZE ? BUFFER_SIZE : (int)num;
            }
        }

        public override void Seek(long pos)
        {
            if (currentBuffer == null || pos < bufferStart || pos >= bufferStart + BUFFER_SIZE)
            {
                currentBufferIndex = (int)(pos / BUFFER_SIZE);
                SwitchCurrentBuffer(false);
            }
            bufferPosition = (int)(pos % BUFFER_SIZE);
        }
    }
}