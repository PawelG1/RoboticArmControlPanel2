using System;
using System.IO;

namespace ControlPanel.Infrastructure.Hardware.Protocol
{
    public class BinaryFrame
    {
        public byte Version { get; set; } = 2;
        public byte MessageType { get; set; }
        public byte Flags { get; set; } = 0;
        public ushort Sequence { get; set; }
        public byte[] Payload { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Creates a raw byte array (header + payload + crc16) suitable for COBS encoding.
        /// </summary>
        public byte[] ToBytes()
        {
            int totalLength = 1 + 1 + 1 + 2 + 2 + Payload.Length + 2; 
            var ms = new MemoryStream(totalLength);
            using var writer = new BinaryWriter(ms);
            
            writer.Write(Version);
            writer.Write(MessageType);
            writer.Write(Flags);
            writer.Write(Sequence); //BinaryWriter writes little-endian by default
            writer.Write((ushort)Payload.Length);
            writer.Write(Payload);
            
            byte[] frameBytesWithoutCrc = ms.ToArray();
            ushort crc = Crc16.Compute(frameBytesWithoutCrc, 0, frameBytesWithoutCrc.Length);
            
            writer.Write(crc);
            return ms.ToArray();
        }

        /// <summary>
        /// Parses a raw decoded COBS byte array into a BinaryFrame object.
        /// Returns null if validation (e.g. CRC16, length, version) fails.
        /// </summary>
        public static BinaryFrame? Parse(byte[] decodedBytes)
        {
            if (decodedBytes.Length < 9) return null;

            ushort crcExpected = Crc16.Compute(decodedBytes, 0, decodedBytes.Length - 2);
            ushort crcActual = BitConverter.ToUInt16(decodedBytes, decodedBytes.Length - 2);
            if (crcExpected != crcActual) return null;

            var frame = new BinaryFrame();
            frame.Version = decodedBytes[0];
            if (frame.Version != 2) return null;

            frame.MessageType = decodedBytes[1];
            frame.Flags = decodedBytes[2];
            frame.Sequence = BitConverter.ToUInt16(decodedBytes, 3);
            ushort payloadLength = BitConverter.ToUInt16(decodedBytes, 5);

            if (payloadLength > 128 || decodedBytes.Length != 9 + payloadLength) return null;

            frame.Payload = new byte[payloadLength];
            Array.Copy(decodedBytes, 7, frame.Payload, 0, payloadLength);

            return frame;
        }
    }
}

