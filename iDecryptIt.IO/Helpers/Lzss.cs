﻿using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;

namespace iDecryptIt.IO.Helpers;

[PublicAPI]
internal static class Lzss
{
    private const int N = 4096; // size of ring buffer - must be a power of 2
    private const int F = 18; // upper limit for match length
    private const int THRESHOLD = 2; // encode string into position and length if match_length is greater than this

    // ReSharper disable CommentTypo
    // Based off of Haruhiko Okumura's LZSS.C which is in the public domain
    // ReSharper enable CommentTypo

    internal static byte[] Decompress(byte[] input)
    {
        List<byte> dest = new();
        using MemoryStream src = new(input);

        byte[] buf = new byte[N - 1 + F];
        Array.Fill(buf, (byte)0x20, 0, N - F); // space
        int bufPos = N - F; // "r" in the original C

        uint flags = 0;
        while (true)
        {
            if (((flags >>= 1) & 0x100) == 0)
            {
                int b = src.ReadByte();
                if (b is -1)
                    break;

                flags = (uint)b | 0xFF00;
            }

            if ((flags & 1) != 0)
            {
                int b = src.ReadByte();
                if (b is -1)
                    break;

                dest.Add((byte)b);
                buf[bufPos++] = (byte)b;
                bufPos &= N - 1;
            }
            else
            {
                int i = src.ReadByte();
                int j = src.ReadByte();
                if (i is -1 || j is -1)
                    break;

                i |= (j & 0xF0) << 4;
                j = (j & 0x0F) + THRESHOLD;
                for (int k = 0; k <= j; k++)
                {
                    byte b = buf[(i + k) & (N - 1)];
                    dest.Add(b);
                    buf[bufPos++] = b;
                    bufPos &= N - 1;
                }
            }
        }

        return dest.ToArray();
    }
}
