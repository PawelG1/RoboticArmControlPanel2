using ControlPanel.Domain.Entities;
using ControlPanel.Domain.Enums;
using ControlPanel.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.Interfaces
{
    public interface IActuatorRepository
    {
        Task AddActuator(Actuator actuator);
        Task<Actuator?> GetActuatorById(int id);
        Task<IEnumerable<Actuator>> GetAllActuators();
        Task SetActuatorWorkingLimits(Actuator actuator, ActuatorWorkingLimits workingLimits);
        Task<ActuatorWorkingLimits> GetActuatorWorkingLimits(Actuator actuator);
        Task SetActuatorCurrentAngle(Actuator actuator, double angle);
        Task<double> GetActuatorCurrentAngle(Actuator actuator);
        Task SetActuatorTargetAngle(Actuator actuator, double target);
        Task<double> GetActuatorTargetAngle(Actuator actuator);
        Task SetActuatorSpeed(Actuator actuator, double speed);
        Task<double> GetActuatorSpeed(Actuator actuator);
        Task SetRotatingDirection(Actuator actuator, RotatingDirection direction);
        Task<RotatingDirection> GetRotatingDirection(Actuator actuator);
        Task<bool> SetActuatorState(Actuator actuator, ActuatorState state);
        Task<ActuatorState> GetActuatorState(Actuator actuator);
    }
}
