using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.ValueObjects;
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

        public List<Step> ImportSequenceFile(string filePath)
        {
            List<Step> deserializedSequence = new();
            foreach (string line in File.ReadLines(filePath))
            {
                Step step = JsonSerializer.Deserialize<Step>(line);
                deserializedSequence.Add(step);
            }
            return deserializedSequence;
        }

        public void SaveSequenceFile()
        {
            throw new NotImplementedException();
        }
    }
}
