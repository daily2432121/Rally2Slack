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
        
        public SlackResponseVM RequestRallyItem(Stream stream)
        {
            StreamReader sr = new StreamReader(stream);

            string str = sr.ReadToEnd();
            SlackMsg msg= SlackMsg.FromString(str);
            Regex regex = new Regex(@"((US|Us|uS|us)\d{0,9})|(((dE|de|De|DE)\d{0,9}))");
            Match m = regex.Match(msg.Text);
            
            if (!m.Success)
            {
                return new SlackResponseVM() { text = "_Whuaaat?_" };
            }
            string itemStr = m.Groups[0].Value;
            string type;
            if (itemStr.StartsWith("DE", StringComparison.CurrentCultureIgnoreCase))
            {
                type = "defect";
            }
            else
            {
                type = "hierarchicalrequirement";
            }
            RallyService service =new RallyService();
            var result = service.GetItem(type, itemStr);
            if (result.Results == null || !result.Results.Any())
            {
                return new SlackResponseVM() {text = "_Nothing here but a wasted slack message_"};
            }
            List<string> welcomes =new List<string>{"how can I help all of you slackers?","you called?","Wassup?","I think I heard my name","Yes?","At your service"};
            Random r = new Random((int)DateTime.Now.Ticks);
            ;
            //PostSlack(result.Results.First()["Description"],msg.token);
            string t = (result.Results.First()["Description"] as string).HtmlToPlainText();


            return new SlackResponseVM() { text = "_"+welcomes[r.Next(0,welcomes.Count-1)]+"_"+"\r\n\r\n" + "*" + itemStr.ToUpper() + "*\r\n" + t };
        }

        public SlackResponseVM RequestRallyItemTest(Stream slackBody)
        {
            SlackResponseVM result;
            StreamReader sr = new StreamReader(slackBody);
            
            string str = sr.ReadToEnd();
            result = new SlackResponseVM() {text = str};
            

            return result;

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
