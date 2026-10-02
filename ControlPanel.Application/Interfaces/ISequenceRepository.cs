using ControlPanel.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.Interfaces
{
    public interface ISequenceRepository
    {
        public List<Step> ImportSequenceFile(string filePath);
        public void ExportSequenceFile(List<Step> sequence, string filePath);
        public void SaveSequenceFile();
    }
}
