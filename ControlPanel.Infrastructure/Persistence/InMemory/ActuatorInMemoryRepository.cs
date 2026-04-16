using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Entities;
using ControlPanel.Domain.Enums;
using ControlPanel.Domain.ValueObjects;
using ControlPanel.Infrastructure.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Infrastructure.Persistence.InMemory
{

    public class ActuatorInMemoryRepository : IActuatorRepository
    {
        private readonly List<Actuator> _actuators = new List<Actuator>();

        public Task AddActuator(Actuator actuator)
        {
            if(_actuators.Any(a => a.GetId == actuator.GetId))
                throw new InvalidOperationException("Actuator with the same ID already exists.");

            _actuators.Add(actuator);
            return Task.CompletedTask;
        }

        Task<Actuator?> IActuatorRepository.GetActuatorById(int id)
        {
            Actuator? actuator = _actuators.FirstOrDefault(a => a.GetId == id);
            return Task.FromResult(actuator);
        }

        Task<IEnumerable<Actuator>> IActuatorRepository.GetAllActuators()
        {
            return Task.FromResult(_actuators.AsEnumerable());
        }

        public Task SetActuatorWorkingLimits(Actuator actuator, ActuatorWorkingLimits workingLimits)
        {
            actuator.SetWorkingLimits(workingLimits);
            return Task.CompletedTask;
        }

        public Task<ActuatorWorkingLimits> GetActuatorWorkingLimits(Actuator actuator)
        {
            return Task.FromResult(actuator.GetWorkingLimits());
        }

        public Task SetActuatorCurrentAngle(Actuator actuator, double angle)
        {
            actuator.SetCurrentAngle(angle);
            return Task.CompletedTask;
        }

        public Task<double> GetActuatorCurrentAngle(Actuator actuator)
        {
            return Task.FromResult(actuator.GetCurrentAngle);
        }

        public Task SetActuatorTargetAngle(Actuator actuator, double target)
        {
            actuator.SetTargetAngle(target);
            return Task.CompletedTask;
        }

        public Task<double> GetActuatorTargetAngle(Actuator actuator)
        {
            return Task.FromResult(actuator.GetTargetAngle());
        }

        public Task SetActuatorSpeed(Actuator actuator, double speed)
        {
            actuator.SetSpeed(speed);
            return Task.CompletedTask;
        }

        public Task<double> GetActuatorSpeed(Actuator actuator)
        {
            return Task.FromResult(actuator.GetSpeed());
        }

        public Task SetRotatingDirection(Actuator actuator, RotatingDirection direction)
        {
            actuator.SetRotatingDirection(direction);
            return Task.CompletedTask;
        }

        public Task<RotatingDirection> GetRotatingDirection(Actuator actuator)
        {
            return Task.FromResult(actuator.GetRotatingDirection);
        }

        public Task<bool> SetActuatorState(Actuator actuator, ActuatorState state)
        {
            actuator.SetState(state);
            return Task.FromResult(true);
        }

        public Task<ActuatorState> GetActuatorState(Actuator actuator)
        {
            return Task.FromResult(actuator.GetState);
        }
    }
}
