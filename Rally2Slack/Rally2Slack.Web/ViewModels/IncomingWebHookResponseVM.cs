using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rally2Slack.Web.ViewModels
{
    public class IncomingWebHookResponseVM
    {
        public string username = "slackbot"; 
        public string channel = "@cheng.huang";
        public string text = "bla bla <@cheng.huang>";
    }
}