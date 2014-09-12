namespace Rally2Slack.Core.Rally.Models
{
    public abstract class SchedulableArtifact
    {
        public long ObjectID { get; set; }
        public string Name { get; set; }
        public string KanbanState { get; set; }
        public string Description { get; set; }
        public string FormattedID { get; set; }
        public string Owner { get; set; }
    }
}