using ControlPanel.Application.DTOs;
using ControlPanel.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.Interfaces
{
    public interface IRobotStateService
    {
        Robot Robot { get; }
        event EventHandler RobotConfigured;
        event EventHandler StateUpdated;
        void StartListening();
        void StopListening();
        public RobotStateDTO GetRobotState();
    }
}
