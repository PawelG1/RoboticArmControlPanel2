using ControlPanel.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.ValueObjects
{
    public class RobotConfig
    {
        public IReadOnlyList<Actuator> Steppers { get; }
        public IReadOnlyList<Actuator> Servos { get; }
        public IReadOnlyDictionary<string, int> ConfigurablePins { get; }
        public IEnumerable<Actuator> AllActuators => Steppers.Concat(Servos);

        public RobotConfig(
            IEnumerable<Actuator> steppers, 
            IEnumerable<Actuator> servos, 
            IReadOnlyDictionary<string, int> configurablePins
            ){
            
            Steppers = steppers.ToList().AsReadOnly();
            Servos = servos.ToList().AsReadOnly();
            ConfigurablePins = configurablePins;
        }
    }
}
