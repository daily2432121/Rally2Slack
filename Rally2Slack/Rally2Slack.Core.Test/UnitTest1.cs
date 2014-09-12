using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rally2Slack.Core.Rally;
using Rally2Slack.Core.Rally.Service;
using Rally2Slack.Core.Rally.Models;

namespace Rally2Slack.Core.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GetKanban()
        {
            RallyService service = new RallyService(RallyConfiguration.GetSlackTestConfiguration());
            var result = service.GetKanban();
            foreach (var r in result)
            {
                Debug.WriteLine(r.FormattedID);
                Debug.WriteLine(r.Name);
                Debug.WriteLine(r.Owner);
                Debug.WriteLine(r.KanbanState);
                //Debug.WriteLine(r.Description);
            }
            
        }
    }
}
