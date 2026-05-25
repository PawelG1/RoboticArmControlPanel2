using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.DTOs.IncomingMessages
{
    public class HeartbeatDTO
    {
        public string Type { get; set; } = "";
        public string ManipulatedObject { get; set; } = "";
        public HeartbeatValuesDTO Values { get; set; } = new();
    }

    public class HeartbeatValuesDTO
    {
        public int Uptime { get; set; }
        public int EStop { get; set; }
        public List<EncoderDTO> Encoders { get; set; } = new();
        public int SteppersEnabled { get; set; }
        public string Status { get; set; } = "";
    }

    public class EncoderDTO
    {
        public int Id { get; set; }
        public double Angle { get; set; }
    }
}
