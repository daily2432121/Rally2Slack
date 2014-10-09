using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using HtmlAgilityPack;


namespace Rally2Slack.Web.Helpers
{
    public class HtmlToText
    {
        public HtmlToText()
        {
        }

        public string ConvertFromString(string str)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(str.ToStream());

            StringWriter sw = new StringWriter();
            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        public List<string> GetAllImageSrcs(string str)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(str.ToStream());

            HtmlNodeCollection imgs = new HtmlNodeCollection(doc.DocumentNode.ParentNode);
            imgs = doc.DocumentNode.SelectNodes("//img");
            if (imgs == null || !imgs.Any())
            {
                return null;
            }
            var result = imgs.Select(i => @"https://rally1.rallydev.com"+i.Attributes[@"src"].Value).ToList();
            return result;
        }

        private int _tlevel = 0;
        private bool _bTag = false;

        private void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (HtmlNode subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText);
            }
        }

        public void ConvertTo(HtmlNode node, TextWriter outText)
        {
            string html;
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    string parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    // get text
                    html = ((HtmlTextNode)node).Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0)
                    {
                        if (_bTag)
                        {
                            outText.Write("*");
                        }
                        outText.Write(HtmlEntity.DeEntitize(html));
                        if (_bTag)
                        {
                            outText.Write("*");
                        }
                        
                    }
                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "b":
                            _bTag = true;
                            // treat paragraphs as crlf
                            break;
                        case "p":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                        case "div":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                        case "span":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                        case "ul":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            _tlevel += 1;
                            break;
                        case "li":
                            // treat paragraphs as crlf
                            
                            var ts = string.Join("", Enumerable.Repeat("\t", _tlevel));
                            outText.Write("\r\n");
                            outText.Write(">");
                            outText.Write(ts);
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText);
                    }

                    switch (node.Name)
                    {
                        case "b":
                            _bTag = false;
                            break;
                        case "ul":
                            _tlevel -= 1;
                            break;
                    }

                    break;
            }
        }
    }
}