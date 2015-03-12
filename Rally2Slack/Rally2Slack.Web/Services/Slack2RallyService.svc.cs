using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using DotNetOpenAuth.Messaging;
using Microsoft.SqlServer.Server;
using Rally2Slack.Core;
using Rally2Slack.Core.HtmlConvert.Service;
using Rally2Slack.Core.Rally.Models;
using Rally2Slack.Core.Rally.Service;
using Rally2Slack.Web.Messages;
using Rally2Slack.Web.ViewModels;
using Google.Apis.Customsearch.v1.Data;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;

namespace Rally2Slack.Web.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Slack2RallyService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Slack2RallyService.svc or Slack2RallyService.svc.cs at the Solution Explorer and start debugging.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.PerSession)]
    public class Slack2RallyService : ISlack2RallyService
    {
        
        public SlackResponseVM RequestRallyItem(Stream slackBody)
        {
            StreamReader sr = new StreamReader(slackBody);
            
            string str = sr.ReadToEnd();
            
            SlackMsg msg= SlackMsg.FromString(str);
            msg.MessageType = SlackMsgType.OutgoingWebhooks;
            if (str.ToLower().Contains("kanban"))
            {
                return GetKanban2(msg.ChannelName);
            }
            Regex regex = new Regex(@"((US|Us|uS|us)\d{1,9})|(((dE|de|De|DE)\d{1,9}))");
            Match m = regex.Match(msg.Text);
            
            if (!m.Success)
            {
                var keyword = msg.Text.Replace(msg.TriggerWord,"");
                return GetFirstGoogleImage(keyword);

            }

            string itemStr = m.Groups[0].Value;
            return GetItem(itemStr, msg.ChannelName);
        }

        public SlackResponseVM GetFirstGoogleImage(string keyword)
        {
            CustomsearchService service = new CustomsearchService(new BaseClientService.Initializer(){ApiKey = "AIzaSyAMlvbcGscP5VLDUcT1eXU0d0M1NszqWZw"});
            //https://www.googleapis.com/customsearch/v1?key=AIzaSyAMlvbcGscP5VLDUcT1eXU0d0M1NszqWZw&cx=000120523936578647521:xln_vnvdaiy&q=flower&searchType=image&fileType=jpg&imgSize=small&alt=json
            

            Google.Apis.Customsearch.v1.CseResource.ListRequest listRequest= service.Cse.List(keyword);
            
            listRequest.Cx = "000120523936578647521:xln_vnvdaiy";
            listRequest.SearchType=Google.Apis.Customsearch.v1.CseResource.ListRequest.SearchTypeEnum.Image;
            listRequest.FileType = "jpg";
            //listRequest.ImgSize = Google.Apis.Customsearch.v1.CseResource.ListRequest.ImgSizeEnum.Small;
            listRequest.Safe = Google.Apis.Customsearch.v1.CseResource.ListRequest.SafeEnum.High;
            listRequest.Alt = Google.Apis.Customsearch.v1.CseResource.ListRequest.AltEnum.Json;
            
            Search search = listRequest.Execute();
            
            if (search.Items != null && search.Items.Any())
            {
                var imgPath = search.Items[0].Link;
                return new SlackResponseVM() { text = imgPath };
            }
            return new SlackResponseVM() { text = "_Whuaaat?_" };
        }

        public SlackResponseVM RequestRallyItemBySlashCommand(Stream slackBody)
        {
            try
            {
                StreamReader sr = new StreamReader(slackBody);
                
                string str = sr.ReadToEnd();
                
                SlackMsg msg = SlackMsg.FromString(str);
                msg.MessageType = SlackMsgType.SlashCommand;
                msg.ChannelName = "slack_dev_test";
                if (msg.Text.Contains("kanban"))
                {
                    return GetKanban2(msg.ChannelName);
                }
                Regex regex = new Regex(@"((US|Us|uS|us)\d{0,9})|(((dE|de|De|DE)\d{0,9}))");
                Match m = regex.Match(msg.Text);

                if (!m.Success)
                {
                    return new SlackResponseVM() {text = "_Whuaaat?_"};
                }

                string itemStr = m.Groups[0].Value;
                var result =  GetItem(itemStr, msg.ChannelName);
                //result.text = "ab";
                PostSlack(result.text, msg.ChannelName);
                return result;

            }
            catch (Exception e)
            {
                return new SlackResponseVM() {text = e.ToString()};
            }
            
        }



        //old text kanban, obsolete for time being
        private SlackResponseVM GetKanban(string channelName)
        {
            RallyService service = new RallyService(RallyService.RallyConfiguration.GetConfigurationByChannel(channelName));
            var result = service.GetKanban().OrderBy(e=>e.KanbanState).ThenBy(e=>e.Owner).ToList();
            StringBuilder sb=new StringBuilder();
            string responseText;
            if (!result.Any())
            {
                responseText = "_Kanban is empty! I ate all the items for you_";
                return new SlackResponseVM(){text = responseText};
            }
            sb.Append("*KANBAN 看板 かんばん கான்பன்*" + Environment.NewLine);
            sb.Append("_________________________________" + Environment.NewLine);
            foreach (var sa in result)
            {
                sb.Append("*"+sa.FormattedID+"*"+Environment.NewLine);
                sb.Append("*"+sa.Name+"*"+Environment.NewLine);
                sb.Append(sa.Owner+Environment.NewLine);
                sb.Append(sa.KanbanState+Environment.NewLine);
                sb.Append("_________________________________" + Environment.NewLine);
            }
            responseText = sb.ToString();
            return new SlackResponseVM() {text = responseText};
        }

        public KanbanHtmlVM GetKanbanItems(string channelName)
        {
            var config = RallyService.RallyConfiguration.GetConfigurationByChannel(channelName);
            List<SchedulableArtifact> list = new List<SchedulableArtifact>();
            Dictionary<string, List<SchedulableArtifact>> dict ;
            RallyService service = new RallyService(config);
            var kanban = service.GetKanban();
            switch (config.KanbanSort)
            {
                case RallyService.KanbanSortCategory.CatagorizedByKanbanState:
                    list = kanban.OrderBy(e => e.KanbanState).ThenBy(e => e.Owner).ToList();
                    dict = list.GroupBy(e => e.KanbanState).ToDictionary(e => e.Key, e => e.ToList());
                    break;
                case RallyService.KanbanSortCategory.CatagorizedByKanbanProgress:
                    list = kanban.OrderBy(e => e.KanbanProgress).ThenBy(e => e.Owner).ToList();
                    dict = list.GroupBy(e => e.KanbanProgress).ToDictionary(e => e.Key, e => e.ToList());
                    break;
                case RallyService.KanbanSortCategory.CatagorizedByScheduleState:
                    list = service.GetKanban().OrderBy(e => e.ScheduleState).ThenBy(e => e.Owner).ToList();
                    dict = list.GroupBy(e => e.ScheduleState).ToDictionary(e => e.Key, e => e.ToList());
                    break;
                default:
                    list = kanban.OrderBy(e => e.ScheduleState).ThenBy(e => e.Owner).ToList();
                    dict = list.GroupBy(e => e.ScheduleState).ToDictionary(e => e.Key, e => e.ToList());
                    break;
            }
            KanbanHtmlVM result = new KanbanHtmlVM() {KanbanItems = dict, KanbanCatorgory = dict.Keys.ToList()};
            return result;
        }

        private SlackResponseVM GetKanban2(string channelName)
        {
            HtmlConvertService service=new HtmlConvertService();
            
            var kanban = GetKanbanItems(channelName);
            List<Image> images = new List<Image>();
            foreach (var state in kanban.KanbanCatorgory)
            {
                var c = BuildKanbanHtmlForOneColumn(state, kanban.KanbanItems[state]);
                var image = service.ConvertToJpeg(c);
                images.Add(image);
            }
            Bitmap bmp = images.MergeHorizontally();
            
            
            Bitmap b = new Bitmap(bmp);
            AzureService azService=new AzureService();
            var path =azService.Upload(b, "kanban");
            
            
            return new SlackResponseVM() { text = path};
        }

        private string BuildKanbanHtml(KanbanHtmlVM kanban)
        {
            StringBuilder sb=new StringBuilder();
            sb.Append("<html><head><title>Kanban</title></head><body><form id='form'><div id='board'>");
            foreach (var state in kanban.KanbanCatorgory)
            {
                var key = state;
                var values = kanban.KanbanItems[key];
                sb.Append("<div style='float:left;' >" + "<div style='text-align:center'>" + (string.IsNullOrEmpty(key)?"None":key)+"</div><div>");
                foreach (var value in values)
                {
                    sb.Append("<div style='margin:20px;width:250px;border: 2px solid;' >");

                        sb.Append("<div style='height:10px;background-color: #3B89F6;color:#3B89F6;'>" + "</div>");

                        sb.Append("<div style='padding:10px'>");
                            sb.Append("<div style='position: relative;float:left;color:#3B89F6;'>"+value.FormattedID+"</div>");
                            sb.Append("<div style='position: relative;float:right;color:#0033CC'>" + value.Owner + "</div>");
                            sb.Append("<div style='clear:both'>" + value.KanbanState + "</div>");
                            sb.Append("<div>" + value.Name + "</div>");

                        sb.Append("</div>");

                    sb.Append("</div>");
                }

                sb.Append("</div></div>");

            }
            sb.Append("</div></div></body></html>");
            return sb.ToString();
        }

        private string BuildKanbanHtmlForOneColumn(string state, List<SchedulableArtifact> items )
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>Kanban</title></head><body><form id='form'><div id='board'>");
            
            
            var key = state;
            var values = items;
            sb.Append("<div style='float:left;' >" + "<div style='text-align:center;font-weight:bold'><h3>" + (string.IsNullOrEmpty(key) ? "None" : key) + "</h3></div><div>");
            foreach (var value in values)
            {
                sb.Append("<div style='margin:20px;width:250px;border: 2px solid;' >");

                sb.Append("<div style='height:10px;background-color: #3B89F6;color:#3B89F6;'>" + "</div>");

                sb.Append("<div style='padding:10px'>");
                sb.Append("<div style='position: relative;float:left;color:#3B89F6;'>" + value.FormattedID + "</div>");
                sb.Append("<div style='position: relative;float:right;font-weight:bold;margin-top:5px;'>" + value.Owner + "</div>");
                //sb.Append("<div style='clear:both'>" + value.KanbanState + "</div>");
                sb.Append("<div style='clear:both; margin-top:5px'>" + value.Name + "</div>");

                sb.Append("</div>");

                sb.Append("</div>");
            }

            sb.Append("</div></div>");

            
            sb.Append("</div></div></body></html>");
            return sb.ToString();
        }

        private SlackResponseVM GetItem(string itemStr, string channelName, bool postBack = false)
        {
            
            string type;

            //Get defect/user story
            if (itemStr.StartsWith("DE", StringComparison.CurrentCultureIgnoreCase))
            {
                type = "defect";
            }
            else
            {
                type = "hierarchicalrequirement";
            }

            //Get item from Rally by channel and FormattedID
            var config = RallyService.RallyConfiguration.GetConfigurationByChannel(channelName);
            RallyService service = new RallyService(config);
            var result = service.GetItem(type, itemStr);
            if (result.Results == null || !result.Results.Any())
            {
                return new SlackResponseVM() { text = "_Nothing here but a wasted slack message_" };
            }
            
            //get first result
            var item = result.Results.First();
            string itemName = (item["Name"] as string);
            string itemBody = (item["Description"] as string).HtmlToPlainText();
            var images = (item["Description"] as string).GetAllImageSrcs();
            if (images != null)
            {
                AzureService aService = new AzureService();
                var firstImage = aService.Upload(images, itemName, config.UserName, config.Password);

                string imagesAsAttached = string.Join("\r\n", firstImage);

                return new SlackResponseVM() {text = "_" + GetWelcomeMsg() + "_" + "\r\n\r\n" + "*" + itemStr.ToUpper() + "*\r\n" + "*" + itemName + "*" + "\r\n" + itemBody + "\r\n" + imagesAsAttached};
            }
            else
            {
                return new SlackResponseVM() { text = "_" + GetWelcomeMsg() + "_" + "\r\n\r\n" + "*" + itemStr.ToUpper() + "*\r\n" + "*" + itemName + "*" + "\r\n" + itemBody };
            }


        }

        private string GetWelcomeMsg()
        {
            List<string> welcomes = new List<string> { "how can I help all of you slackers?", "you called?", "Wassup?", "I think I heard my name", "Yes?", "At your service" };
            Random r = new Random((int)DateTime.Now.Ticks);
            return welcomes[r.Next(0, welcomes.Count - 1)];
        }
        //not in used
        public SlackResponseVM RequestRallyKanban(Stream stream)
        {
            StreamReader sr = new StreamReader(stream);

            string str = sr.ReadToEnd();
            SlackMsg msg = SlackMsg.FromString(str);
            Regex regex = new Regex(@"kanban");
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
            RallyService service = new RallyService(RallyService.RallyConfiguration.GetConfigurationByChannel(msg.ChannelName));
            var result = service.GetItem(type, itemStr);
            if (result.Results == null || !result.Results.Any())
            {
                return new SlackResponseVM() { text = "_Nothing here but a wasted slack message_" };
            }
            List<string> welcomes = new List<string> { "how can I help all of you slackers?", "you called?", "Wassup?", "I think I heard my name", "Yes?", "At your service" };
            Random r = new Random((int)DateTime.Now.Ticks);
            ;
            //PostSlack(result.Results.First()["Description"],msg.token);
            var item = result.Results.First();
            string itemBody = (item["Description"] as string).HtmlToPlainText();
            string itemName = (item["Name"] as string);



            return new SlackResponseVM() { text = "_" + welcomes[r.Next(0, welcomes.Count - 1)] + "_" + "\r\n\r\n" + "*" + itemStr.ToUpper() + "*\r\n" + "*" + itemName + "*" + "\r\n" + itemBody };
        }

        public SlackResponseVM RequestRallyItemTest(Stream slackBody)
        {
            SlackResponseVM result;
            StreamReader sr = new StreamReader(slackBody);
            
            string str = sr.ReadToEnd();
            result = new SlackResponseVM() {text = str};
            PostSlack(str, "@cheng.huang");

            return result;

        }


        public string GetSlackPostUrl(string token=null)
        {
            
            string url = ConfigurationManager.AppSettings["SlackEndPoint"];
            if (token == null)
            {
                token = ConfigurationManager.AppSettings["SlackToken"];
  
            }
            var result = string.Format("https://{0}/services/hooks/incoming-webhook?token={1}", url, token);
            Debug.WriteLine("Watcher::SlackPostUrl: " + result);
            return result;

            
        }
        //Mz63vU2OV0yYU2hPey9FEvcC


        public bool PostSlack(string message,string channel,  string token = null)
        {
            try
            {
                IncomingWebHookResponseVM msg = new IncomingWebHookResponseVM();
                msg.text = message;
                msg.username= "rallycat";
                msg.channel= "#"+channel;
                var url2 = new Uri(GetSlackPostUrl(token));
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url2);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                request.Timeout = 1000000;
                request.UseDefaultCredentials = true;
                request.ReadWriteTimeout = 1000000;
                string jsonBody = ToJSON(msg);
                //request.ContentLength = jsonBody.Length;
                if (!string.IsNullOrEmpty(jsonBody))
                {
                    using (Stream s = request.GetRequestStream())
                    {
                        using (StreamWriter writer = new StreamWriter(s))
                        {
                            writer.Write(jsonBody);
                        }    
                    }
                    
                }
                else
                {
                    request.ContentLength = 0;
                }


                
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Debug.WriteLine("SlackPostBack::Rest:Fail to send" + response.StatusCode);
                        return false;
                    }
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
                return Encoding.UTF8.GetString(stream.ToArray());
            }

            //Else
            //    Dim s As JavaScriptSerializer = New JavaScriptSerializer()
            //    Return s.Serialize(obj)
            //End If    

        }
    }
}
