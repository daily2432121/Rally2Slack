using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rally.RestApi;

namespace Rally2Slack.Core.Rally.Models
{
    internal class Iteration : IParsable
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public void FromDynamicJsonObject(dynamic source)
        {
            Name = source["Name"];
            StartDate = source["StartDate"]==null? DateTime.MinValue:DateTime.Parse(source["StartDate"]);
            EndDate = source["EndDate"] == null ? DateTime.MaxValue : DateTime.Parse(source["EndDate"]);
        }

        public Iteration()
        {
        }

        public Iteration(dynamic source)
        {
            FromDynamicJsonObject(source);
        }
    }
}