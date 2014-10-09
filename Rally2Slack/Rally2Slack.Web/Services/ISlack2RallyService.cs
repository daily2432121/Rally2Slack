using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using Rally2Slack.Core.Rally.Models;
using Rally2Slack.Web.Messages;
using Rally2Slack.Web.ViewModels;

namespace Rally2Slack.Web.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISlack2RallyService" in both code and config file together.
    [ServiceContract]
    public interface ISlack2RallyService
    {
        
        [OperationContract()]
        [WebInvoke(UriTemplate = "Rally/Details", Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        SlackResponseVM RequestRallyItem(Stream slackBody);

        [OperationContract()]
        [WebInvoke(UriTemplate = "Rally/DetailsByCmd", Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        SlackResponseVM RequestRallyItemBySlashCommand(Stream slackBody);


        //For testting
        [OperationContract()]
        [WebInvoke(UriTemplate = "Rally/Test", Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        SlackResponseVM RequestRallyItemTest(Stream slackBoday);

        //For testting
        [OperationContract()]
        [WebGet(UriTemplate = "Rally/KanBanData/{channelName}", ResponseFormat = WebMessageFormat.Json)]
        KanbanHtmlVM GetKanbanItems(string channelName);
    }
}
