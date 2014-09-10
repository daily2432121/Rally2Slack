using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Xsl;
using HtmlAgilityPack;
using Rally2Slack.Web.Helpers;

namespace Rally2Slack.Web
{
    public static class Extensions
    {
        public static DateTime UnixTimeStampToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static string HtmlToMarkdown(this string htmlString)
        {
            string text = string.Empty;

            XmlDocument xmlDoc = new XmlDocument();
            XmlDocument xsl = new XmlDocument();
            htmlString = "<html>" + htmlString + "</html>";
            htmlString=HttpUtility.HtmlDecode(htmlString);
            xmlDoc.LoadXml(htmlString);
            xsl.CreateEntityReference("nbsp");
            xsl.Load(System.Web.HttpContext.Current.Server.MapPath("/Tools/Html2Markdown.xslt"));

            //creating xslt
            XslTransform xslt = new XslTransform();
            xslt.Load(xsl, null, null);

            //creating stringwriter
            StringWriter writer = new System.IO.StringWriter();

            //Transform the xml.
            xslt.Transform(xmlDoc, null, writer, null);

            //return string
            text = writer.ToString();
            writer.Close();
            text = text.Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", "");
            return text;
        }

        public static string HtmlToPlainText(this string htmlString)
        {
            //var output = new StringBuilder();
            //HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //doc.Load(htmlString.ToStream());
            //foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//*"))
            //{
            //    output.AppendLine(node.InnerText.ToString() + Environment.NewLine);
            //}
            //return output.ToString();
            HtmlToText h=new HtmlToText();
            return h.ConvertFromString(htmlString);
        }

        public static Stream ToStream(this string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}