namespace ControlPanel.Application.DTOs.IncomingMessages
{
    /*
     * 1) CONFIG (odpowiedź na GET_CONFIG)
            {
              "Type": "CONFIG",
              "ManipulatedObject": "SYSTEM",
              "Steppers": [
                { "Id": 0, "Name": "Stepper 1 (TB6600)", "RangeStart": 0.0, "RangeEnd": 0.0 },
                { "Id": 1, "Name": "Stepper 2 (DM556)", "RangeStart": 0.0, "RangeEnd": 0.0 }
              ],
              "Servos": [
                { "Id": 2, "Name": "Servo 1", "RangeStart": 0, "RangeEnd": 180 },
                { "Id": 3, "Name": "Servo 2", "RangeStart": 0, "RangeEnd": 180 }
              ],
              "UserPins": {
                "User1": 20,
                "User2": 21,
                "User3": 22,
                "User4": 23,
                "User5": 24,
                "User6": 25
              }
            }
     */

    public class RobotConfigDTO
    {
        public string Type { get; set; } = "";
        public string ManipulatedObject { get; set; } = "";
        public List<ActuatorConfigDTO> Steppers { get; set; } = new();
        public List<ActuatorConfigDTO> Servos { get; set; } = new();
        public Dictionary<string, int> UserPins { get; set; } = new();
    }

    public class ActuatorConfigDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double RangeStart { get; set; }
        public double RangeEnd { get; set; }
    }
}
