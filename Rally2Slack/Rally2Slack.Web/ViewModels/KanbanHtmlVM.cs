using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rally2Slack.Core.Rally.Models;

namespace Rally2Slack.Web.ViewModels
{
    public class KanbanHtmlVM
    {
        public Dictionary<string, List<SchedulableArtifact>> KanbanItems { get; set; }
        public List<string> KanbanCatorgory { get; set; }

    }
}