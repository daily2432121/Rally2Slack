using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rally2Slack.Web.Messages
{
    public class SlackMsg
    {
        public string token { get; set; }
        public string team_id { get; set; }
        public string channel_id { get; set; }
        public string channel_name { get; set; }
        public double timestamp { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string trigger_word { get; set; }
        public string text { get; set; }
        
    }
}