using ControlPanel.Application.Interfaces;
using ControlPanel.Application.Services;
using ControlPanel.Domain.Entities;
using ControlPanel.Presentation.WPF.Common;
using ControlPanel.WPF;
using ControlPanel.WPF.Services.Interfaces;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

namespace ControlPanel.Presentation.WPF.ViewModels
{
    public class ConfigurationViewModel : BaseViewModel
    {
        private ISerialCommunicationService _serialCommunication;
        private IUserInteractionService _userInteractionService;
        private IRobotStateService _robotStateService;
        public ConfigurationViewModel(IUserInteractionService userInteractionService,ISerialCommunicationService serialCommunication, IRobotStateService robotStateService) {
            _serialCommunication = serialCommunication;
            _userInteractionService = userInteractionService;
            _robotStateService = robotStateService;
            _title = "Configuration Window";

            SetUpDefaultConnection();

            GetAvailableComPortsCommand = new RelayCommand(GetAvailableComPorts);
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);

            _serialCommunication.FrameReceived += OnFrameReceived;
        }
        public override void Dispose()
        {
            _serialCommunication.FrameReceived -= OnFrameReceived;
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                NotifyPropertyChanged();
            }
        }

        private List<string> _availableComPorts=new List<string>();
        public List<string> AvailableComPorts
        {
            get => _availableComPorts;
            set
            {
                _availableComPorts = value;
                NotifyPropertyChanged();
            }
        }

        private string _selectedComPortName="";
        public string SelectedComPortName {
            get => _selectedComPortName;
            set
            {
                _selectedComPortName = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isRawLogVisible = false;
        public bool IsRawLogVisible
        {
            get => _isRawLogVisible;
            set
            {
                _isRawLogVisible = value;
                NotifyPropertyChanged();
            }
        }

        private string _listOfActuatorsAsText = "";
        public string ListOfActuatorsAsText
        {
            get => _listOfActuatorsAsText;
            set
            {
                _listOfActuatorsAsText = value;
                NotifyPropertyChanged();
            }
        }

        private string _communicationLog = "";
        public string CommunicationLog
        {
            get => _communicationLog;
            set
            {
                _communicationLog = value;
                NotifyPropertyChanged();
            }
        }

        private string _translatedCommunicationLog = "";
        public string TranslatedCommunicationLog
        {
            get => _translatedCommunicationLog;
            set
            {
                _translatedCommunicationLog = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand GetAvailableComPortsCommand { get; set; }

        public void GetAvailableComPorts(object _)
        {
            GetAvailableComPorts();
        }

        public void GetAvailableComPorts()
        {
            string[] ports = _serialCommunication.GetAvailableSerialPorts();
            AvailableComPorts = ports.ToList();
        }

        public ICommand ConnectCommand { get; set; }

        public async void Connect(object _)
        {
            try
            {
                _serialCommunication.ConfigureConnection(SelectedComPortName);
                _serialCommunication.Connect();

            }catch(Exception e)
            {
                _userInteractionService.ShowError("Failed to connect to the selected COM port. :" + e.Message);
            }
        }

        public ICommand DisconnectCommand { get; set; } = new RelayCommand(_ => { });
        public void Disconnect(object _)
        {
            try
            {
                _serialCommunication.Disconnect();
                CommunicationLog = String.Empty;
            }
            catch (Exception e)
            {
                _userInteractionService.ShowError("Failed to disconnect from the COM port. :" + e.Message);
            }
        }

        private void SetUpDefaultConnection()
        {
            GetAvailableComPorts();
            string configuredPortName = _serialCommunication.GetSelectedPortName();
            
            SelectedComPortName = string.IsNullOrWhiteSpace(configuredPortName) ? AvailableComPorts.First() : configuredPortName;
        }

        private void OnFrameReceived(object? sender, ControlPanel.Application.DTOs.SerialCommands.BinaryMessage frame)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                CommunicationLog += $"Type: 0x{frame.MessageType:X2}, Payload: {BitConverter.ToString(frame.Payload)}\n";
                TranslatedCommunicationLog += TranslateBinaryFrame(frame) + "\n";
                ListOfActuatorsAsText = GetAllActuatorsNamesAndValuesAsText();
            });
        }

        private string TranslateBinaryFrame(ControlPanel.Application.DTOs.SerialCommands.BinaryMessage frame)
        {
            if (frame.MessageType == 0x84 && frame.Payload.Length >= 38)
            {
                uint uptime = BitConverter.ToUInt32(frame.Payload, 0);
                byte eStop = frame.Payload[4];
                byte status = frame.Payload[5];
                double stp0Angle = BitConverter.ToInt32(frame.Payload, 6) / 100.0;
                double stp1Angle = BitConverter.ToInt32(frame.Payload, 14) / 100.0;
                return $"HEARTBEAT: Uptime={uptime}ms, EStop={eStop}, Status={status}, Stp0={stp0Angle}°, Stp1={stp1Angle}°";
            }
            else if (frame.MessageType == 0x81 && frame.Payload.Length >= 3)
            {
                byte objId = frame.Payload[0];
                ushort errCode = BitConverter.ToUInt16(frame.Payload, 1);
                return $"ERROR: ObjId=0x{objId:X2}, Code=0x{errCode:X4}";
            }
            else if (frame.MessageType == 0x80 && frame.Payload.Length >= 3)
            {
                byte cmd = frame.Payload[0];
                byte res = frame.Payload[1];
                byte actId = frame.Payload[2];
                return $"ACK: Cmd=0x{cmd:X2}, Result={res}, ActId={actId}";
            }
            
            return $"UNKNOWN (Type 0x{frame.MessageType:X2})";
        }

        private string GetAllActuatorsNamesAndValuesAsText()
        {
            StringBuilder stringBuilder = new();
            var actuators = _robotStateService.Robot.GetAllActuators();
            if (actuators == null)
                return "";
            if (actuators.Count() != 0)
                stringBuilder.Append("Actuators:\n");
            foreach(var actuator in actuators)
            {
                stringBuilder.Append($"\tId: {actuator.GetId} Current Angle: {actuator.GetCurrentAngle}\n");
            }
            return stringBuilder.ToString();
        }

    }
}
