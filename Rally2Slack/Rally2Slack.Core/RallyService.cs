using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rally.RestApi;
using Rally.RestApi.Response;

namespace Rally2Slack.Core
{
    

    public class RallyService
    {
        

        private RallyConfiguration Config { get; set; }


        public RallyService(string username, string pass, string teamName, params string[] channels)
        {
            Config=new RallyConfiguration()
            {
                UserName = username,
                Password = pass,
                Team =  teamName,
                Channels = new List<string>(channels)

            };
        }

        public RallyService(RallyConfiguration config)
        {
            Config = config;
        }

        public QueryResult GetItem(string requestType,string itemId)
        {
            RallyRestApi restApi=new RallyRestApi(Config.UserName,Config.Password);
            //Request request = new Request("hierarchicalrequirement");
            Request request = new Request(requestType);
            //Request request = new Request();
            request.Fetch = new List<string>() {"Name","Description","FormattedID"};

            request.Query = new Query("FormattedID", Query.Operator.Equals, itemId);

            QueryResult queryResult = restApi.Query(request);
            return queryResult;


        }
    }


    public class RallyConfiguration
    {
        public string Team { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<string> Channels { get; set; }

        public static List<RallyConfiguration> GetAllConfigurations()
        {
            List<RallyConfiguration> configs = new List<RallyConfiguration>()
            {
                GetDEConfiguration(),
                GetEncariConfiguration(),
                GetSlackTestConfiguration()
            };
            return configs;
        }
        

        public static RallyConfiguration GetConfigurationByChannel(string channel)
        {
            var configs = GetAllConfigurations().Where(c => c.Channels.Contains(channel.ToLower())).ToList();
            if (configs.Any())
            {
                return configs.First();
            }
            return null;
        }


        public static RallyConfiguration GetDEConfiguration()
        {
            return new RallyConfiguration()
            {
                Team = "DE",
                UserName = "cheng.huang@finoconsulting.com",
                Password = "570124yaya",
                Channels = new List<string>(){"direct-energy","direct-energy-ios","de_analytics","ex-directenergy-ng","ex-directenergy-dev"}
            };
        }


        public static RallyConfiguration GetEncariConfiguration()
        {
            return new RallyConfiguration()
            {
                Team = "Encari",
                UserName = "patrick.cash@finoconsulting.com",
                Password = "570124yaya",
                Channels = new List<string>() { "encari-dev", "encari" }
            };
        }


        public static RallyConfiguration GetSlackTestConfiguration()
        {
            return new RallyConfiguration()
            {
                Team = "TEST",
                UserName = "cheng.huang@finoconsulting.com",
                Password = "570124yaya",
                Channels = new List<string>() { "slacktesting"}
            };
        }
    }
}
