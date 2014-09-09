using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;

namespace Rally2Slack.Web.ViewModels
{
    public class RallyItemVM
    {
        public string ItemId { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
    }
}