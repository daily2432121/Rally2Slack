using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using Rally.RestApi;
using Rally.RestApi.Response;
using Rally2Slack.Core.Rally.Models;

namespace Rally2Slack.Core.Rally.Service
{


    public class RallyService
    {


        private RallyConfiguration Config { get; set; }


        public RallyService(string username, string pass, string teamName, params string[] channels)
        {
            Config = new RallyConfiguration()
            {
                UserName = username,
                Password = pass,
                Team = teamName,
                Channels = new List<string>(channels)

            };
        }

        public RallyService(RallyConfiguration config)
        {
            Config = config;
        }

        public QueryResult GetItem(string requestType, string itemId)
        {
            RallyRestApi restApi = new RallyRestApi(Config.UserName, Config.Password);
            //Request request = new Request("hierarchicalrequirement");

            Request request = new Request(requestType);
            request.Project = "/project/" + Config.ProjectID;
            request.Workspace = "/workspace/" + Config.WorkSpaceID;
            //Request request = new Request();
            request.Fetch = new List<string>() {"Name", "Description", "FormattedID"};

            request.Query = new Query("FormattedID", Query.Operator.Equals, itemId);

            QueryResult queryResult = restApi.Query(request);
            return queryResult;


        }


        public List<SchedulableArtifact> GetKanban()
        {
            RallyRestApi restApi = new RallyRestApi(Config.UserName, Config.Password);
            Request request = new Request("Iteration");
            request.Fetch = new List<string>() {"Name", "StartDate", "Project", "EndDate"};
            //request.Query = new Query(string.Format("((StartDate <= {0}) AND (Project.OID = {1}))",DateTime.Now.ToString("o"), projectId));
            request.Query = new Query(string.Format("(StartDate <= {0})", DateTime.Now.ToString("o")));
            request.Project = "/project/" + Config.ProjectID;
            request.Workspace = "/workspace/" + Config.WorkSpaceID;
            QueryResult queryResult = restApi.Query(request);
            string iterationName;

            if (queryResult.Results.Any())
            {
                var iter = queryResult.Results.Select(e => new Iteration(e)).Where(r => r.StartDate <= DateTime.Now).OrderByDescending(r => r.StartDate).First();
                iterationName = iter.Name;

            }
            else
            {
                return new List<SchedulableArtifact>();
            }

            request = new Request("defect");
            request.Project = "/project/" + Config.ProjectID;
            request.Workspace = "/workspace/" + Config.WorkSpaceID;
            request.Fetch = new List<string>() {"Name", "ObjectID", "FormattedID", "c_KanbanState","c_STEPKanban", "c_KanbanProgress","Description", "ScheduleState", "Owner"};
            request.Query = new Query("Iteration.Name", Query.Operator.Equals, iterationName);
            queryResult = restApi.Query(request);
            List<SchedulableArtifact> results = new List<SchedulableArtifact>();
            if (queryResult.Results.Any())
            {
                results.AddRange(queryResult.Results.Select(e => new Defect(e)).ToList());
            }

            request = new Request("hierarchicalrequirement");
            request.Project = "/project/" + Config.ProjectID;
            request.Workspace = "/workspace/" + Config.WorkSpaceID;
            request.Fetch = new List<string>() { "Name", "ObjectID", "FormattedID", "c_KanbanState", "c_STEPKanban","c_KanbanProgress", "Description", "ScheduleState", "Owner" };
            request.Query = new Query("Iteration.Name", Query.Operator.Equals, iterationName);
            queryResult = restApi.Query(request);
            if (queryResult.Results.Any())
            {
                results.AddRange(queryResult.Results.Select(e => new UserStory(e)).ToList());
            }
            return results;

        }




        public enum KanbanSortCategory
        {
            CatagorizedByKanbanState,
            CatagorizedByKanbanProgress,
            CatagorizedByScheduleState,
            CatagorizedBySTEPKanbanState
        }

        public class RallyConfiguration
        {
            public string Team { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public List<string> Channels { get; set; }
            public long ProjectID { get; set; }
            public long WorkSpaceID { get; set; }
            public KanbanSortCategory KanbanSort { get;set;}
            public static List<RallyConfiguration> GetAllConfigurations()
            {
                List<RallyConfiguration> configs = new List<RallyConfiguration>()
                {
                    GetDEConfiguration(),
                    GetDEAnalyticsConfiguration(),
                    GetDeSTEPConfiguration(),
                    GetEncariConfiguration(),
                    GetFinoMarketingConfiguration(),
                    GetSlackTestConfiguration(),
                    GetApricotConfiguration(),
                    GetPearsonAnalyticsConfiguration()
                    //GetSlackTestForMarketingConfiguration()

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
                    ProjectID = 13073500360,
                    WorkSpaceID = 13073262940,
                    KanbanSort = KanbanSortCategory.CatagorizedByKanbanState,
                    Channels = new List<string>() {"directenergy", "directenergy-ios", "ex-directenergy-ng", "ex-directenergy-dev","directenergy-support","capacity-models"}
                };
            }



            public static RallyConfiguration GetDEAnalyticsConfiguration()
            {
                return new RallyConfiguration()
                {
                    Team = "DE",
                    UserName = "cheng.huang@finoconsulting.com",
                    Password = "570124yaya",
                    ProjectID = 20523617147,
                    WorkSpaceID = 13073262940,
                    KanbanSort = KanbanSortCategory.CatagorizedByScheduleState,
                    Channels = new List<string>() {  "de_analytics" ,"de-analytics-private"}
                };
            }

            public static RallyConfiguration GetEncariConfiguration()
            {
                return new RallyConfiguration()
                {
                    Team = "Encari",
                    UserName = "cheng.huang@finoconsulting.com",
                    Password = "570124yaya",
                    ProjectID = 18985605563,
                    WorkSpaceID = 13073262940,
                    KanbanSort = KanbanSortCategory.CatagorizedByKanbanProgress,
                    Channels = new List<string>() {"encari-dev", "encari"}
                };
            }


            public static RallyConfiguration GetSlackTestConfiguration()
            {
                return new RallyConfiguration()
                {
                    Team = "TEST",
                    UserName = "cheng.huang@finoconsulting.com",
                    Password = "570124yaya",
                    ProjectID = 13073500360,
                    WorkSpaceID = 13073262940,
                    KanbanSort = KanbanSortCategory.CatagorizedByKanbanState,
                    Channels = new List<string>() {"slacktesting","slack_dev_test"}
                };
            }


            public static RallyConfiguration GetSlackTestForMarketingConfiguration()
            {
                return new RallyConfiguration()
                {
                    Team = "TEST",
                    UserName = "cheng.huang@finoconsulting.com",
                    Password = "570124yaya",
                    ProjectID = 21395328391,
                    WorkSpaceID = 13073262940,
                    KanbanSort = KanbanSortCategory.CatagorizedByScheduleState,
                    Channels = new List<string>() { "slacktesting" }
                };
            }



            public static RallyConfiguration GetFinoMarketingConfiguration()
            {
                return new RallyConfiguration()
                {
                    Team = "FinoMarketing",
                    UserName = "cheng.huang@finoconsulting.com",
                    Password = "570124yaya",
                    ProjectID = 21395328391,
                    WorkSpaceID = 13073262940,
                    KanbanSort = KanbanSortCategory.CatagorizedByScheduleState,
                    Channels = new List<string>() { "marketing" }
                };
            }



            public static RallyConfiguration GetApricotConfiguration()
            {
                return new RallyConfiguration()
                {
                    Team = "Apricot",
                    UserName = "cheng.huang@finoconsulting.com",
                    Password = "570124yaya",
                    ProjectID = 16835562806,
                    WorkSpaceID = 13073262940,
                    KanbanSort = KanbanSortCategory.CatagorizedByKanbanProgress,
                    Channels = new List<string>() { "apricot", "apricot-dev" }
                };
            }

            public static RallyConfiguration GetPearsonAnalyticsConfiguration()
            {
                return new RallyConfiguration()
                {
                    Team = "PearsonAnalytics",
                    UserName = "cheng.huang@finoconsulting.com",
                    Password = "570124yaya",
                    ProjectID = 25094138731,
                    WorkSpaceID = 13073262940,
                    KanbanSort = KanbanSortCategory.CatagorizedByScheduleState,
                    Channels = new List<string>() { "pearson_analytics" }
                };
            }

            public static RallyConfiguration GetDeSTEPConfiguration()
            {
                return new RallyConfiguration()
                {
                    Team = "DE",
                    UserName = "cheng.huang@finoconsulting.com",
                    Password = "570124yaya",
                    ProjectID = 25951496746,
                    WorkSpaceID = 13073262940,
                    KanbanSort = KanbanSortCategory.CatagorizedBySTEPKanbanState,
                    Channels = new List<string>() { "de-step","de-step-git","de-step-scrum","de-step-scrum-dev" }
                };
            }
        }
    }
}
