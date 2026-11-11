using System;
using System.Collections.Generic;

namespace ControlPanel.Infrastructure.Hardware.Protocol
{
    /// <summary>
    /// Consistent Overhead Byte Stuffing (COBS) algorithm.
    /// MIT License equivalent implementation.
    /// </summary>
    public static class CobsCodec
    {
        public static byte[] Encode(byte[] data)
        {
            var encoded = new List<byte>();
            int codeIdx = 0;
            encoded.Add(0); // Placeholder for the first code
            byte code = 1;

            foreach (byte b in data)
            {
                if (b == 0)
                {
                    encoded[codeIdx] = code;
                    codeIdx = encoded.Count;
                    encoded.Add(0); // Placeholder for next code
                    code = 1;
                }
                else
                {
                    encoded.Add(b);
                    code++;
                    if (code == 0xFF)
                    {
                        encoded[codeIdx] = code;
                        codeIdx = encoded.Count;
                        encoded.Add(0);
                        code = 1;
                    }
                }
            }
            encoded[codeIdx] = code;
            encoded.Add(0x00); // Separator added directly by codec
            return encoded.ToArray();
        }

        public static byte[] Decode(byte[] encoded)
        {
            if (encoded.Length == 0) return Array.Empty<byte>();

            var decoded = new List<byte>();
            int readIdx = 0;

            while (readIdx < encoded.Length)
            {
                byte code = encoded[readIdx++];
                
                // Should not happen in well-formed COBS, but just in case
                if (code == 0) break; 
                
                for (int i = 1; i < code; i++)
                {
                    if (readIdx < encoded.Length)
                    {
                        decoded.Add(encoded[readIdx++]);
                    }
                    else
                    {
                        // Error: reached end of encoded sequence prematurely
                        return Array.Empty<byte>();
                    }
                }
                if (code < 0xFF && readIdx < encoded.Length)
                {
                    decoded.Add(0);
                }
            }

            return decoded.ToArray();
        }
    }
}

