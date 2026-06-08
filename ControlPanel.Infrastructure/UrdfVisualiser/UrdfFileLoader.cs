using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Entities.UrdfVisualiser;
using ControlPanel.Domain.Enums.UrdfVisualiser;
using ControlPanel.Domain.ValueObjects.UrdfVisualiser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Xml.Linq;

namespace ControlPanel.Infrastructure.UrdfVisualiser
{
    public class UrdfFileLoader : IUrdfLoader
    {
        UrdfRobot IUrdfLoader.Load(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);
            XElement root = doc.Root ?? throw new InvalidDataException("Empty URDF file");
            string urdfDir = Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? "";

            var links = ParseLinks(root, urdfDir);
            var joints = ParseJoints(root);

            return new UrdfRobot(
                name: root.Attribute("name")?.Value ?? "robot",
                links: links,
                joints: joints
            );
        }

        private static IReadOnlyDictionary<string, UrdfLink> ParseLinks(XElement root, string urdfDir)
        {
            Dictionary<string, UrdfLink> links = new();

            foreach(XElement elem in root.Elements("link"))
            {
                string name = elem.Attribute("name")?.Value ?? "";
                if (string.IsNullOrEmpty(name)) continue;

                UrdfLinkVisual? visual = null;
                XElement? visualElement = elem.Element("visual");
                if(visualElement != null) 
                    visual = ParseVisual(visualElement, urdfDir);

                string uniqueName = name;
                int suffix = 1;
                while (links.ContainsKey(uniqueName))
                {
                    uniqueName = $"{name}_{suffix++}";//Fusion360 extension for URDF export, might create duplicated link names
                }
                links[uniqueName] = new UrdfLink(uniqueName, visual);
            }
            return links;
        }

        private static UrdfLinkVisual? ParseVisual(XElement visualElem, string urdfDir)
        {
            XElement? meshElement = visualElem.Element("geometry")?.Element("mesh"); //xmlElem with path to mesh stl file and scale
            if (meshElement == null)
                return null;

            string rawPath = meshElement.Attribute("filename")?.Value ?? "";
            string absPath = ResolveMeshPath(rawPath, urdfDir);

            XElement? originElement = visualElem.Element("origin");
            UrdfOrigin origin = originElement != null ? ParseOrigin(originElement) : UrdfOrigin.Default;

            return new UrdfLinkVisual {MeshFileAbsolutePath = absPath, Origin = origin };
        }

        private static string ResolveMeshPath(string rawPath, string UrdfDir)
        {
            return Path.GetFullPath(Path.Combine(UrdfDir, rawPath));
        }

        private static UrdfOrigin ParseOrigin(XElement originElement)
        {
            const double M_TO_MM = 1000.0; //urdf has scale in Meters, which is being scaled to mm bc our scene is in mm
            Vector3 xyz = ParseVector3(originElement.Attribute("xyz")?.Value ?? "0 0 0");
            Vector3 rpy = ParseVector3(originElement.Attribute("rpy")?.Value ?? "0 0 0");

            return new UrdfOrigin
            {
                X = xyz.X * M_TO_MM,
                Y = xyz.Y * M_TO_MM,
                Z = xyz.Z * M_TO_MM,

                Roll = rpy[0], //in radians
                Pitch = rpy[1],
                Yaw = rpy[2],
            };
        }

        private static Vector3 ParseVector3(string vectorAsText)
        {
            string[] valuesAsText = vectorAsText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (valuesAsText.Length < 3)
                throw new InvalidDataException($"Given Vector values are in incorrect format: {vectorAsText}\n they should be like 0.0 2.0 6.0");
            float[] values = new float[3];
            for(int i=0; i<=2; i++)
            {
                if (!float.TryParse(valuesAsText[i], NumberStyles.Float, CultureInfo.InvariantCulture, out values[i]))
                    throw new InvalidDataException($"Given Vector values are in incorrect format: {vectorAsText}\n they should be like 0.0 2.0 6.0");
            }
            return new Vector3(values[0], values[1], values[2]);
        }

        private static IReadOnlyDictionary<string, UrdfJoint> ParseJoints(XElement root)
        {
            Dictionary<string, UrdfJoint> joints = new();

            foreach (XElement jointElem in root.Elements("joint")){
                string name = jointElem.Attribute("name")?.Value ?? "";
                JointType type = ParseJointType(jointElem.Attribute("type")?.Value ?? "fixed");
                string parent = jointElem.Element("parent")?.Attribute("link")?.Value ?? "";
                string child = jointElem.Element("child")?.Attribute("link")?.Value ?? "";

                UrdfOrigin origin = jointElem.Element("origin") is { } originElem ? ParseOrigin(originElem) : UrdfOrigin.Default;

                XElement? axisElement = jointElem.Element("axis");
                Vector3 axisVector = axisElement != null ? ParseVector3(axisElement.Attribute("xyz")?.Value ?? "0 0 1") : new Vector3(0, 0, 1);
                JointAxis axis = new JointAxis(axisVector[0], axisVector[1], axisVector[2]);
                
                UrdfJoint joint = new UrdfJoint(name, type, parent, child, origin, axis);

                XElement? limitElement = jointElem.Element("limit");
                if (limitElement != null) {
                    const double RAD_TO_DEG = 180.0 / Math.PI;
                    joint.LimitLowerDeg = double.Parse(limitElement.Attribute("lower")?.Value ?? "0", CultureInfo.InvariantCulture) * RAD_TO_DEG;
                    joint.LimitUpperDeg = double.Parse(limitElement.Attribute("upper")?.Value ?? "0", CultureInfo.InvariantCulture) * RAD_TO_DEG;
                }

                if(!string.IsNullOrEmpty(name))
                    joints[name] = joint;
            }
            return joints;
        }

        private static JointType ParseJointType(string jointTypeAsText)
        {
            return jointTypeAsText switch
            {
                "revolute"      => JointType.Revolute,
                "continuous"    => JointType.Continuous,
                "prismatic"     => JointType.Prismatic,
                "fixed"         => JointType.Fixed,
                _ => JointType.Fixed,
            };

        }

    }
}
