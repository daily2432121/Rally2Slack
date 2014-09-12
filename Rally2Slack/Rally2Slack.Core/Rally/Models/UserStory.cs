using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rally2Slack.Core.Rally.Models
{
    internal class UserStory : SchedulableArtifact, IParsable
    {
        public void FromDynamicJsonObject(dynamic source)
        {
            FormattedID = source["FormattedID"];
            ObjectID = source["ObjectID"];
            Name = source["Name"];
            KanbanState = source["c_KanbanState"];
            Description = source["Description"];
            Owner = source["Owner"]["_refObjectName"];
            

        }

        public UserStory()
        {
        }

        public UserStory(dynamic source)
        {
            FromDynamicJsonObject(source);
        }
    }
}