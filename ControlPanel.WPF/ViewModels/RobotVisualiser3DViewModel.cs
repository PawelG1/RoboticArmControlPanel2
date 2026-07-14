using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Entities;
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
using System.Windows.Media;
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
        private readonly Dictionary<string, Model3DGroup> _ghostLinkGroups = new();

        public Model3DGroup RobotScene { get; } = new();
        public Model3DGroup GhostScene { get; } = new();
        
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
            _robotStateService.RobotConfigured += OnRobotConfiguredUpdated;

            LoadUrdfCommand = new RelayCommand(LoadUrdf);
        }
        public override void Dispose()
        {
            _robotStateService.StateUpdated -= OnRobotStateUpdated;
            _robotStateService.RobotConfigured -= OnRobotConfiguredUpdated;
            base.Dispose();
        }
        
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
                SetupDefaultActuatorToJointMapping();
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
            GhostScene.Children.Clear();

            //add roots to scene
            foreach (string rootName in _robot.GetRootLinksNames())
            {
                if (!_robot.Links.TryGetValue(rootName, out var link)) continue;
                
                var group = CreateLinkGroup(link);
                _linkGroups[rootName] = group;
                RobotScene.Children.Add(group);

                var ghostGroup = CreateLinkGroup(link, true);
                _ghostLinkGroups[rootName] = ghostGroup;
                GhostScene.Children.Add(ghostGroup);
            }

            //add children for each joint
            foreach (UrdfJoint joint in _robot.Joints.Values) {
                if(!_robot.Links.TryGetValue(joint.ChildLinkName, out var link)) continue;
                
                var group = CreateLinkGroup(link);
                _linkGroups[joint.Name] = group;
                RobotScene.Children.Add(group);

                var ghostGroup = CreateLinkGroup(link, true);
                _ghostLinkGroups[joint.Name] = ghostGroup;
                GhostScene.Children.Add(ghostGroup);

            }
        }

        private static Model3DGroup CreateLinkGroup(UrdfLink link, bool isGhost=false)
        {
            Model3DGroup? group = new Model3DGroup();
            string? path = link.Visual?.MeshFileAbsolutePath;
            if (path != null && path.Length > 0){
                var mesh = TryLoadMesh(path);
                if(mesh != null)
                {
                    if (isGhost)
                        ApplyGhostMaterial(mesh);
                    group.Children.Add(mesh);
                }
            }
            return group;
        }

        private static void ApplyGhostMaterial(Model3D mesh)
        {
            //if (mesh is not GeometryModel3D geo) return;
            //geo.Material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(80, 0, 150, 255)));
            //geo.BackMaterial = geo.Material;

            if (mesh is Model3DGroup group)
            {
                foreach (var child in group.Children)
                    ApplyGhostMaterial(child);
                return;
            }

            if (mesh is not GeometryModel3D geo) return;

            var brush = new SolidColorBrush(Color.FromArgb(80, 0, 150, 255));
            geo.Material = new DiffuseMaterial(brush);
            geo.BackMaterial = geo.Material;
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

        private List<Actuator> _availableActuators = new();
        public List<Actuator> AvailableActuators
        {
            get => _availableActuators;
            set
            {
                _availableActuators = value;
                NotifyPropertyChanged();
            }
        }

        private List<ActuatorAndJointNamePair> _pairedActuatorAndJoints = new();
        public List<ActuatorAndJointNamePair> PairedActuatorAndJoints {
            get => _pairedActuatorAndJoints;
            set
            {
                _pairedActuatorAndJoints = value;
                NotifyPropertyChanged();
            }
        }
        
        private void OnRobotConfiguredUpdated(object? sender, EventArgs e)
        {
            SetupDefaultActuatorToJointMapping();
        }

        private void SetupDefaultActuatorToJointMapping()
        {
            if (!_robotStateService.Robot.IsConfigured)
                return;

            AvailableActuators = [.. _robotStateService.Robot.GetAllActuators()];
            IEnumerable<string>? AvailableUrdfJoints = null;

            if (_robot != null)
                AvailableUrdfJoints = _robot.Joints.Where(j => j.Value.IsMovable).Select(j => j.Value.Name);

            PairedActuatorAndJoints = DefaultActuatorToJointMapping(AvailableActuators, AvailableUrdfJoints);
        }

        private List<ActuatorAndJointNamePair> DefaultActuatorToJointMapping(IEnumerable<Actuator> actuators, IEnumerable<string>? movableJoints)
        {
            List<ActuatorAndJointNamePair> pairs = new();

            if (movableJoints == null)
                return pairs;

            int actuatorsCount = actuators.Count();

            for(int i=0; i < movableJoints.Count(); i++)
            {
                pairs.Add(new ActuatorAndJointNamePair(
                    actuator: i < actuatorsCount ? actuators.ElementAt(i) : null,
                    jointName: movableJoints.ElementAt(i)
                    )
                );
            }
            return pairs;
        }

        private List<string> _availableJoints = new();
        public List<string> AvailableJoints
        {
            get => _availableJoints;
            set
            {
                _availableJoints = value;
                NotifyPropertyChanged();
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

            HashSet<JointState> jointStatesForActualAngle = new();
            HashSet<JointState> jointStatesForTargetAngle = new();
            foreach (ActuatorAndJointNamePair actuator in PairedActuatorAndJoints)
            {
                jointStatesForActualAngle.Add( new JointState {JointName= actuator.JointName, AngleDegrees=actuator.Acutator?.GetCurrentAngle ?? 0.0});
                jointStatesForTargetAngle.Add( new JointState {JointName= actuator.JointName, AngleDegrees=actuator.Acutator?.GetTargetAngle() ?? 0.0});
            }

            var transformations = _forwardKinematicsService.Compute(_robot, jointStatesForActualAngle.ToList());
            var transformationsForGhost = _forwardKinematicsService.Compute(_robot, jointStatesForTargetAngle.ToList());

            ApplyTransformations(transformations, _linkGroups);
            ApplyTransformations(transformationsForGhost, _ghostLinkGroups);
        }

        private void ApplyTransformations(IEnumerable<LinkTransformation> transformations, Dictionary<string, Model3DGroup> modelGroup)
        {
            foreach (var t in transformations)
            {
                if (modelGroup.TryGetValue(t.VisualKey, out var group))
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