using ControlPanel.Application.Interfaces;
using ControlPanel.Presentation.WPF.Common;
using ControlPanel.WPF;
using ControlPanel.WPF.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ControlPanel.Presentation.WPF.ViewModels
{
    public class ConfigurationViewModel : BaseViewModel
    {
        private ISerialCommunication _serialCommunication;
        private IUserInteractionService _userInteractionService;
        public ConfigurationViewModel(IUserInteractionService userInteractionService,ISerialCommunication serialCommunication) {
            _serialCommunication = serialCommunication;
            _userInteractionService = userInteractionService;
            _title = "Configuration Window";

            SetUpDefaultConnection();

            GetAvailableComPortsCommand = new RelayCommand(GetAvailableComPorts);
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);

            _serialCommunication.MessageReceived += OnMessageReceived;
        }
        public override void Dispose()
        {
            _serialCommunication.MessageReceived -= OnMessageReceived;
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

        public void Connect(object _)
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
            SelectedComPortName = AvailableComPorts.FirstOrDefault() ?? "";

        }

        private void OnMessageReceived(object? sender, string message)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                CommunicationLog += $"{message}\n";
            });
        }

    }
}
