using ControlPanel.Application.Interfaces;
using System;

namespace ControlPanel.Application.UseCases
{
    public class GetConfigUseCase
    {
        private readonly ISerialCommunicationService _serialCommunicationService;

        public GetConfigUseCase(ISerialCommunicationService serialCommunicationService)
        {
            _serialCommunicationService = serialCommunicationService;
        }

        public async Task Execute()
        {
            await _serialCommunicationService.SendCommandAsync(ControlPanel.Domain.Constants.ProtocolMessageTypes.GET_CONFIG, Array.Empty<byte>());
        }
    }
}
