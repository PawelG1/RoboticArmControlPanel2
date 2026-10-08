namespace ControlPanel.Domain.Constants
{
    public static class ProtocolMessageTypes
    {
        // PC -> ESP
        public const byte STOP = 0x01;
        public const byte GET = 0x02;
        public const byte GET_CONFIG = 0x03;
        public const byte MOVE = 0x04;
        public const byte SET_LIMITS = 0x05;
        public const byte CALIBRATE = 0x06;
        public const byte IO_SET = 0x07;
        public const byte IO_GET = 0x08;
        public const byte BUZZER = 0x09;

        // ESP -> PC
        public const byte ACK = 0x80;
        public const byte ERROR = 0x81;
        public const byte INFO = 0x82;
        public const byte CONFIG = 0x83;
        public const byte HEARTBEAT = 0x84;
    }

    public static class ActuatorIds
    {
        public const byte Stepper1 = 0;
        public const byte Stepper2 = 1;
        public const byte Servo1 = 2;
        public const byte Servo2 = 3;
        public const byte Servo3 = 4;
        public const byte All = 0xFF;
    }
}

