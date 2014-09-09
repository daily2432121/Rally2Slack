using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Text.RegularExpressions;
using Rally2Slack.Core;
using Rally2Slack.Web.Messages;
using Rally2Slack.Web.ViewModels;

namespace Rally2Slack.Web.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Slack2RallyService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Slack2RallyService.svc or Slack2RallyService.svc.cs at the Solution Explorer and start debugging.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Slack2RallyService : ISlack2RallyService
    {
        
        public SlackResponseVM RequestRallyItem(SlackMsg msg)
        {
            string text = msg.text;
            Regex regex = new Regex(@"((US|Us|uS|us)\d{0,9})|(((dE|de|De|DE)\d{0,9}))");
            Match m = regex.Match(text);
            
            if (!m.Success)
            {
                return new SlackResponseVM() { text = "Didn't recognize this item :(" };
            }
            string itemStr = m.Groups[0].Value;
            RallyService service =new RallyService();
            var result = service.GetItem(itemStr);
            if (result.Results == null || !result.Results.Any())
            {
                return new SlackResponseVM() {text = "Nothing here but love"};
            }
            PostSlack(result.Results.First()["Description"],msg.token);
            return new SlackResponseVM() { text = "Got it" };
        }

        public string RequestRallyItem(Stream slackBody)
        {
            using (StreamReader sr = new StreamReader(slackBody))
            {
                string str = sr.ReadToEnd();
                throw new Exception(str);
            }
            
        }


        public string GetSlackPostUrl(string token)
        {
            
            string url = ConfigurationManager.AppSettings["SlackEndPoint"];
            var result = string.Format("https://{0}/services/hooks/incoming-webhook?token={1}", url, token);
            Debug.WriteLine("Watcher::SlackPostUrl: " + result);
            return result;

            
        }

        private bool PostSlack(string message,string token)
        {
            try
            {
                SlackResponseVM msg = new SlackResponseVM();
                msg.text = message;
                

                var url2 = new Uri(GetSlackPostUrl(token));
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url2);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                request.Timeout = 1000000;
                request.UseDefaultCredentials = true;
                request.ReadWriteTimeout = 1000000;
                string jsonBody = ToJSON(msg);
                request.ContentLength = jsonBody.Length;
                if (!string.IsNullOrEmpty(jsonBody))
                {
                    using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                    {
                        writer.Write(jsonBody);
                    }
                }
                else
                {
                    request.ContentLength = 0;
                }



                var result = (HttpWebResponse)request.GetResponse();

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    Debug.WriteLine("SlackPostBack::Rest:Fail to send" + result.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SlackPostBack::Rest:Fail to send" + ex.ToString());
                return false;
            }
            return true;
        }




        private string ToJSON<T>(T obj)
        {
            DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                s.WriteObject(stream, obj);
                return Encoding.Default.GetString(stream.ToArray());
            }

            //Else
            //    Dim s As JavaScriptSerializer = New JavaScriptSerializer()
            //    Return s.Serialize(obj)
            //End If    

        }
    }
}
