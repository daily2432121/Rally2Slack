using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rally2Slack.Core.Rally.Models
{
    public  class UserStory : SchedulableArtifact, IParsable
    {
        public void FromDynamicJsonObject(dynamic source)
        {
            FormattedID = source["FormattedID"];
            ObjectID = source["ObjectID"];
            Name = source["Name"];
            KanbanState = source["c_KanbanState"]??"";
            KanbanProgress = source["c_KanbanProgress"] ?? "";
            ScheduleState = source["ScheduleState"] ?? "";
            STEPKanbanState = source["c_STEPKanban"] ?? "";
            Description = source["Description"];
            if (source["Owner"] == null)
            {
                Owner = null;
            }
            else
            {
                Owner = source["Owner"]["_refObjectName"];
            }
            

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