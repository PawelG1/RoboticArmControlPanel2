using System;

namespace ControlPanel.Domain.Enums
{
    [Flags]
    public enum CalibrationOptions : byte
    {
        None = 0,
        SetZero = 1 << 0,          // 0x01
        InvertDir = 1 << 1,        // 0x02
        WriteInvertDir = 1 << 2    // 0x04
    }
}

