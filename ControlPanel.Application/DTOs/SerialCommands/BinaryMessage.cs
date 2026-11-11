namespace ControlPanel.Application.DTOs.SerialCommands
{
    public readonly struct BinaryMessage
    {
        public byte MessageType { get; }
        public byte[] Payload { get; }
        
        public BinaryMessage(byte messageType, byte[] payload)
        {
            MessageType = messageType;
            Payload = payload;
        }
    }
}

