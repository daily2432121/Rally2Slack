using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using Rally2Slack.Web.Messages;
using Rally2Slack.Web.ViewModels;

namespace Rally2Slack.Web.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISlack2RallyService" in both code and config file together.
    [ServiceContract]
    public interface ISlack2RallyService
    {
        
        [OperationContract()]
        [WebInvoke(UriTemplate = "Rally/Details",Method = "POST")]
        SlackResponseVM RequestRallyItem(SlackMsg slackBoday);



        [OperationContract()]
        [WebInvoke(UriTemplate = "Rally/Test", Method = "POST")]
        string RequestRallyItem(Stream slackBoday);
    
    }
}
