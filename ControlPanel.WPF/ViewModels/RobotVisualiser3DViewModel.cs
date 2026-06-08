using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Entities.UrdfVisualiser;
using ControlPanel.Domain.ValueObjects.UrdfVisualiser;
using ControlPanel.Presentation.WPF.Common;
using ControlPanel.WPF.Services.Interfaces;
using HelixToolkit.Wpf;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace ControlPanel.Presentation.WPF.ViewModels
{
    public class RobotVisualiser3DViewModel : BaseViewModel
    {
        private readonly IUrdfLoader _urdfLoader;
        private readonly IForwardKinematicsService _forwardKinematicsService;
        private readonly IRobotStateService _robotStateService;
        private readonly IUserInteractionService _userInteractionService;

        private UrdfRobot? _robot;
        private readonly Dictionary<string, Model3DGroup> _linkGroups = new();

        public Model3DGroup RobotScene { get; } = new();
        
        public RobotVisualiser3DViewModel(
            IUrdfLoader urdfLoader, 
            IForwardKinematicsService forwardKinematicsService, 
            IRobotStateService robotStateService,
            IUserInteractionService userInteractionService
            ){
            
            _urdfLoader = urdfLoader;
            _forwardKinematicsService = forwardKinematicsService;
            _robotStateService = robotStateService;
            _userInteractionService = userInteractionService;

            _robotStateService.StateUpdated += OnRobotStateUpdated;

            LoadUrdfCommand = new RelayCommand(LoadUrdf);
        }
        public override void Dispose()
        {
            _robotStateService.StateUpdated -= OnRobotStateUpdated;
            base.Dispose();
        }
        public ObservableCollection<LinkDebugInfo> LinkInfos { get; } = new();

        public record LinkDebugInfo(string Name, string WorldPosition);//TODO: maybe remove
        
        public ICommand LoadUrdfCommand {  get; }

        private string _statusText = "No URDF file loaded - click to import files";
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                NotifyPropertyChanged();
            }
        }

        public void LoadUrdf(object _)
        {
            string? path = _userInteractionService.OpenFileDialog("URDF files (*.urdf)|*.urdf|All files (*.*)|*.*");
            if (path == null)
                return;
            try
            {
                _robot = _urdfLoader.Load(path);
                BuildScene();
                ComputeAndApplyTransforms();
                StatusText = $"{Path.GetFileName(path)} | has: {_robot.Links.Count} links and {CountMovable()} DOF";
            }
            catch (Exception ex)
            {
                StatusText = $"Error while loading:{ex.Message}";
                Debug.WriteLine(ex);
            }
        }

        private void BuildScene()
        {
            if (_robot == null) return;

            _linkGroups.Clear();
            RobotScene.Children.Clear();

            //add roots to scene
            foreach (string rootName in _robot.GetRootLinksNames())
            {
                if (!_robot.Links.TryGetValue(rootName, out var link)) continue;
                var group = CreateLinkGroup(link);
                _linkGroups[rootName] = group;
                RobotScene.Children.Add(group);
            }

            //add children for each joint
            foreach (UrdfJoint joint in _robot.Joints.Values) {
                if(!_robot.Links.TryGetValue(joint.ChildLinkName, out var link)) continue;
                var group = CreateLinkGroup(link);
                _linkGroups[joint.Name] = group;
                RobotScene.Children.Add(group);
            
            }
        }

        private static Model3DGroup CreateLinkGroup(UrdfLink link)
        {
            Model3DGroup? group = new Model3DGroup();
            string? path = link.Visual?.MeshFileAbsolutePath;
            if (path != null && path.Length > 0){
                var mesh = TryLoadMesh(path);
                if(mesh != null)
                    group.Children.Add(mesh);
            }
            return group;
        }

        private static Model3D? TryLoadMesh(string path)
        {
            if(!File.Exists(path)) return null;
            try
            {
                return Path.GetExtension(path).ToLowerInvariant() switch
                {
                    ".stl" => new StLReader().Read(path),
                    ".obj" => new ObjReader().Read(path),
                    _ => null,
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while trying to load mesh files: {ex.Message}");
                return null;
            }
        }

        private void OnRobotStateUpdated(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(ComputeAndApplyTransforms);
        }

        private void ComputeAndApplyTransforms()
        {
            if (_robot == null)
                return;

            //TODO: implement mapping URDF joints to RobotState joints
            JointState[]? jointStates = Array.Empty<JointState>();

            var transformations = _forwardKinematicsService.Compute(_robot, jointStates);

            foreach (var t in transformations) {
                if (_linkGroups.TryGetValue(t.VisualKey, out var group))
                    group.Transform = new MatrixTransform3D(ToMatrix3D(t.WorldMatrix));
            }
        }

        private static Matrix3D ToMatrix3D(Matrix4x4 m)
        {
            return new Matrix3D(
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44
            );
        }

        private int CountMovable()
        {
            return _robot?.Joints.Values.Count(j => j.IsMovable) ?? 0;
        }


    }
}
