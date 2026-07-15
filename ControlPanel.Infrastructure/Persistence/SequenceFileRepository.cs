using ControlPanel.Application.Interfaces;
using ControlPanel.Application.Services;
using System.Text;
using System.Text.Json;

namespace ControlPanel.Infrastructure.Persistence
{
    public class SequenceFileRepository : ISequenceRepository
    {
        public void ExportSequenceFile(List<Step> sequence, string filePath)
        {
            List<string> serializedSteps = new();
            foreach(Step step in sequence)
            {
                serializedSteps.Add(JsonSerializer.Serialize(step));
            }
            File.WriteAllLines(filePath, serializedSteps);
        }

        public List<Step> ImportSequenceFile()
        {
            throw new NotImplementedException();
        }

        public void SaveSequenceFile()
        {
            throw new NotImplementedException();
        }
    }
}
