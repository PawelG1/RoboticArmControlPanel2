using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.Enums
{
    //TODO: this might not longer be needed, as the direction is determined by hardware
    /// <summary>
    /// DEPREACTED: This enum is no longer used, as the direction of rotation is determined by the uC
    /// </summary>
    public enum RotatingDirection
    {
        Clockwise,
        CounterClockwise,
    }
}
