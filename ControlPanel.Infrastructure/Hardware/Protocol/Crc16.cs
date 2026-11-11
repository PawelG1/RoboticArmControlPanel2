namespace ControlPanel.Infrastructure.Hardware.Protocol
{
    /// <summary>
    /// CRC-16/CCITT-FALSE implementation.
    /// Polynomial: 0x1021, Initial: 0xFFFF, Final XOR: 0x0000, Not reflected.
    /// MIT License equivalent implementation.
    /// </summary>
    public static class Crc16
    {
        public static ushort Compute(byte[] data, int offset, int length)
        {
            ushort crc = 0xFFFF;
            for (int i = offset; i < offset + length; i++)
            {
                crc ^= (ushort)(data[i] << 8);
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    else
                        crc <<= 1;
                }
            }
            return crc;
        }
    }
}

