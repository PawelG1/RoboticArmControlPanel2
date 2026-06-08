using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Entities.UrdfVisualiser;
using ControlPanel.Domain.ValueObjects.UrdfVisualiser;
using System.Numerics;

namespace ControlPanel.Application.Services
{
    public class ForwardKinematicsService : IForwardKinematicsService
    {

        public IReadOnlyList<LinkTransformation> Compute(UrdfRobot urdfRobot, IReadOnlyList<JointState> jointStates)
        {
            //T_child = T_parent * T_joint_origin * T_joint_rotation
            //where T_parent for root is identity matrix
            List<LinkTransformation> results = [];
            var RootLinksNames = urdfRobot.GetRootLinksNames();
            foreach (var rootLinkName in RootLinksNames)
            {
                AddLinkResult(results, rootLinkName, rootLinkName, urdfRobot, Matrix4x4.Identity);
                Traverse(rootLinkName, new List<string>(), urdfRobot, jointStates, Matrix4x4.Identity, results);
            }
            return results;

        }

        private static Matrix4x4 ComputeChildMatrix(Matrix4x4 parentMatrix, Matrix4x4 jointOrigin, Matrix4x4 jointRotation)
        {
            return jointRotation * jointOrigin * parentMatrix;
        }

        private static Matrix4x4 ConvertUrdfOriginToMatrix(UrdfOrigin origin)
        {
            //R - rotation, T - translation of origin 
            //Transformation = R*T
            Matrix4x4 R = Matrix4x4.CreateRotationX((float)origin.Roll)
                    * Matrix4x4.CreateRotationY((float)origin.Pitch)
                    * Matrix4x4.CreateRotationZ((float)origin.Yaw);
            Matrix4x4 T = Matrix4x4.CreateTranslation(
                new Vector3((float)origin.X, (float)origin.Y, (float)origin.Z)
            );
            return R * T;
        }

        private static Matrix4x4 GetJointRotationMatrix(JointAxis jointAxis, float angleDegrees)
        {

            Vector3 axisVector = new Vector3((float)jointAxis.X, (float)jointAxis.Y, (float)jointAxis.Z);
            float angleInRadians = angleDegrees * MathF.PI / 180f;
            return Matrix4x4.CreateFromAxisAngle(axisVector, angleInRadians);
        }

        private void AddLinkResult(List<LinkTransformation> results, string visualKey, string linkName, UrdfRobot robot, Matrix4x4 worldMatrix)
        {
            robot.Links.TryGetValue(linkName, out var link);
            if (link == null)
                return;

            Matrix4x4 T_final = worldMatrix;
            if (link.Visual != null)
            {
                T_final = ConvertUrdfOriginToMatrix(link.Visual.Origin) * worldMatrix;
            }
            results.Add(new LinkTransformation { VisualKey = visualKey, LinkName = linkName, WorldMatrix = T_final });
        }

        private void Traverse(
            string entryLinkName, 
            List<string> visitedJointNames, 
            UrdfRobot urdfRobot, 
            IReadOnlyList<JointState> jointStates, 
            Matrix4x4 parentMatrix, 
            List<LinkTransformation> results
            ){
            ///<Summary>
            ///Uses DFS - Depth First Algorithm to traverse through all Links and compute transformations
            /// </Summary>
            var availableJoints = urdfRobot.GetChildJoints(entryLinkName);
            foreach (var availableJoint in availableJoints)
            {
                if (visitedJointNames.Contains(availableJoint.Name))
                    continue;

                visitedJointNames.Add(availableJoint.Name);
                
                var t_origin = ConvertUrdfOriginToMatrix(availableJoint.Origin);
                var jointAngleDegrees = jointStates.Where(j => j.JointName == availableJoint.Name).FirstOrDefault()?.AngleDegrees ?? 0;
                var t_rotation = GetJointRotationMatrix(availableJoint.JointAxis, (float)jointAngleDegrees);
                
                Matrix4x4 t_child = ComputeChildMatrix(parentMatrix, t_origin, t_rotation);         
                
                AddLinkResult(results, availableJoint.Name, availableJoint.ChildLinkName, urdfRobot, t_child);
                Traverse(availableJoint.ChildLinkName, visitedJointNames, urdfRobot, jointStates, t_child, results);
            }

        }
    }
}
