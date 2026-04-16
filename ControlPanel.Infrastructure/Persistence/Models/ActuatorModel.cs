using ControlPanel.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ControlPanel.Infrastructure.Persistence.Models
{
    [Table("Actuators")]
    public class ActuatorModel
    {
        [Key] [Required]
        public int Id { get; set; }

        [Required]
        public ActuatorState State { get; set; }

        [Required]
        public double CurrentAngle { get; set; }

        [Required]
        public double TargetAngle { get; set; }

        [Required]
        public RotatingDirection RotatingDirection { get; set; }

        [Required]
        public double Speed { get; set; }

    }

}
    
