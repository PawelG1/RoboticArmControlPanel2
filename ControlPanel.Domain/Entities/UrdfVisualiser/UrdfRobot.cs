using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.Entities.UrdfVisualiser
{
    public class UrdfRobot
    {
        public string Name { get; init; } = "";
        public IReadOnlyDictionary<string, UrdfLink> Links { get; init; } = new Dictionary<string, UrdfLink>();
        public IReadOnlyDictionary<string, UrdfJoint> Joints { get; init; } = new Dictionary<string, UrdfJoint>();

        public UrdfRobot(string name, IReadOnlyDictionary<string, UrdfLink> links, IReadOnlyDictionary<string, UrdfJoint> joints)
        {
            Name = name;
            Links = links;
            Joints = joints;
        }

        public IEnumerable<UrdfJoint> GetChildJoints(string ParentLinkName)
        {
            foreach (var joint in Joints.Values)
            {
                if (joint.ParentLinkName == ParentLinkName)
                {
                    yield return joint;
                }
            }
        }

        public IReadOnlyList<string> GetRootLinksNames()
        {
            var childLinkNames = new HashSet<string>(Joints.Values.Select(j => j.ChildLinkName));
            var rootLinkNames = Links.Keys.Where(linkName => !childLinkNames.Contains(linkName));
            return rootLinkNames.ToList();
        }
    }
}
