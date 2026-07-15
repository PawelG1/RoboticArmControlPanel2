using ControlPanel.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.Interfaces
{
    public interface ISequenceRepository
    {
        public List<Step> ImportSequenceFile();
        public void ExportSequenceFile(List<Step> sequence, string filePath);
        public void SaveSequenceFile();
    }
}
