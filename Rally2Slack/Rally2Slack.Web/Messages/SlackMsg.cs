using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Rally2Slack.Web.Messages
{
    public class SlackMsg
    {
        public string Token { get; set; }
        public string TeamId { get; set; }
        public string ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string TeamDomain { get; set; }
        public DateTime TimeStamp { get; set; }
        public string UserId { get; set; }
        public string ServiceId { get; set; }
        public string UserName { get; set; }
        public string TriggerWord { get; set; }
        public string Text { get; set; }

        public static SlackMsg FromString(string source)
        {
            //token=zvhpiUW4HnFUACA1lPFZuQXO&team_id=T024SS9SJ&team_domain=finoconsulting&service_id=2539062323&channel_id=C029YKXUP&channel_name=slacktesting&timestamp=1410311282.000007&user_id=U024T9P3E&user_name=cheng.huang&text=rallybot%3A+US1243&trigger_word=rallybot%3A 
            SlackMsg msg = new SlackMsg();
            var entries = source.Split(new[] {'&'});
            foreach (var en in entries)
            {
                var element= en.Split(new[] {'='});
                if (en.Contains("token"))
                {
                    msg.Token = element[1];
                }
                if (en.Contains("team_id"))
                {
                    msg.TeamId = element[1];
                }
                if (en.Contains("team_domain"))
                {
                    msg.TeamDomain= element[1];
                }
                if (en.Contains("service_id"))
                {
                    msg.ServiceId = element[1];
                }
                if (en.Contains("channel_id"))
                {
                    msg.ChannelId = element[1];
                }
                if (en.Contains("channel_name"))
                {
                    msg.ChannelName = element[1];
                }
                if (en.Contains("timestamp"))
                {
                    msg.TimeStamp = Convert.ToDouble(element[1]).UnixTimeStampToDateTime();
                }
                if (en.Contains("user_id"))
                {
                    msg.UserId = element[1];
                }
                if (en.Contains("user_name"))
                {
                    msg.UserName = element[1];
                }
                if (en.Contains("trigger_word"))
                {
                    msg.TriggerWord = element[1];
                }
                if (en.Contains("text"))
                {
                    msg.Text = element[1];
                }
            }
            return msg;
        }
        
    }
}