using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rally2Slack.Web.Services;

namespace Rally2Slack.Web.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void PostSlackTest()
        {
            Slack2RallyService service =new Slack2RallyService();
            service.PostSlack("!", "@cheng.huang");
        }



    }
}
