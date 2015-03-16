using System.Runtime.Serialization;
using System.Text;

namespace Rally2Slack.Core.Rally.Models
{
    [DataContract]
    [KnownType(typeof(Defect))]
    [KnownType(typeof(UserStory))]
    public abstract class SchedulableArtifact
    {
        [DataMember]
        public long ObjectID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string KanbanState { get; set; }
        [DataMember]
        public string KanbanProgress { get; set; }
        [DataMember]
        public string ScheduleState { get; set; }
        [DataMember]
        public string STEPKanbanState { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string FormattedID { get; set; }
        [DataMember]
        public string Owner { get; set; }

        public string BuildHtml(string templatePath)
        {
            StringBuilder sb=new StringBuilder();
            return null;
        }
    }
}