using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Rally2Slack.Core.Rally.Models
{
    public interface IParsable
    {
        void FromDynamicJsonObject(dynamic source);
    }

    public class Defect : SchedulableArtifact,IParsable
    {
        public void FromDynamicJsonObject(dynamic source)
        {
            FormattedID = source["FormattedID"];
            ObjectID = source["ObjectID"];
            Name = source["Name"];
            KanbanState = source["c_KanbanState"];
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

        public Defect()
        {
            
        }

        public Defect(dynamic source)
        {
            FromDynamicJsonObject(source);
        }
    }
}
