using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
            HtmlToText h=new HtmlToText();
            return h.ConvertFromString(htmlString);
        }

        public static List<string> GetAllImageSrcs(this string htmlString)
        {
            HtmlToText h = new HtmlToText();
            return  h.GetAllImageSrcs(htmlString);

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



        public static Bitmap MergeHorizontally(this List<Image> images)
        {

            int outputImageWidth = images.Sum(i => i.Width);
            int outputImageHeight = images.Max(i => i.Height);

            

            Bitmap outputImage = new Bitmap(outputImageWidth, outputImageHeight);
            
            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                graphics.Clear(Color.White);  
                int totalWidth = 0;
                for (var i=0;i<images.Count;i++)
                {
                    graphics.DrawImage(images[i], new Rectangle(new Point(totalWidth, 0), images[i].Size),new Rectangle(new Point(), images[i].Size), GraphicsUnit.Pixel);
                    totalWidth += images[i].Width;
                    images[i].Dispose();
                }
                
            }
            
            return outputImage;
        }
    }
}