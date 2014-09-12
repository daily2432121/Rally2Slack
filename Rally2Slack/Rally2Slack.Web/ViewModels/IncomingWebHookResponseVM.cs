using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rally2Slack.Web.ViewModels
{
    public class IncomingWebHookResponseVM
    {
        public string slack_sender = "slackbot"; 
        public string slack_channel = "@cheng.huang";
        public string slack_message = "bla bla <@cheng.huang>";
    }
}