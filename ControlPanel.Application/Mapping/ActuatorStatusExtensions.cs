using ControlPanel.Application.DTOs.IncomingMessages;
using ControlPanel.Domain.Enums;

namespace ControlPanel.Application.Mapping
{
    public static class ActuatorStatusExtensions
    {
        public static ActuatorState ToDomain(this ActuatorStatusWire wire) => wire switch
        {
            ActuatorStatusWire.MOVING    => ActuatorState.Moving,
            ActuatorStatusWire.IDLE      => ActuatorState.Idle,
            ActuatorStatusWire.FORBIDDEN => ActuatorState.Forbidden,
            ActuatorStatusWire.ESTOP     => ActuatorState.EStop,
            _                            => ActuatorState.Idle
        };

        public static ActuatorStatusWire ToWire(this ActuatorState domain) => domain switch
        {
            ActuatorState.Moving    => ActuatorStatusWire.MOVING,
            ActuatorState.Idle      => ActuatorStatusWire.IDLE,
            ActuatorState.Forbidden => ActuatorStatusWire.FORBIDDEN,
            ActuatorState.EStop     => ActuatorStatusWire.ESTOP,
            ActuatorState.Error     => ActuatorStatusWire.ESTOP,
            _                       => ActuatorStatusWire.IDLE
        };
    }
}
