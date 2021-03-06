﻿//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

#if !NETCORE
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Dse.Compression
{
    internal class LZ4Compressor : IFrameCompressor
    {
        public Stream Decompress(Stream stream)
        {
            var buffer = Utils.ReadAllBytes(stream, 0);
            var outputLengthBytes = new byte[4];
            Buffer.BlockCopy(buffer, 0, outputLengthBytes, 0, 4);
            Array.Reverse(outputLengthBytes);
            var outputLength = BitConverter.ToInt32(outputLengthBytes, 0);
            var decompressStream = new MemoryStream(LZ4.LZ4Codec.Decode(buffer, 4, buffer.Length - 4, outputLength), 0, outputLength, false, true);
            return decompressStream;
        }
    }
}
#endif