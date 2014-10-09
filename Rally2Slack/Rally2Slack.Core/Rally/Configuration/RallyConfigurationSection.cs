using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Rally2Slack.Core.Rally.Configuration
{
    public class RallyConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("projects")]
        public RallyProjectCollection Projects
        {
            get { return ((RallyProjectCollection) (base["projects"])); }
        }


        [ConfigurationProperty("rallyUrl")]
        public string RallyUrl
        {
            get { return (string) this["rallyUrl"]; }
        }
    }

    public class RallyProjectCollection : ConfigurationElementCollection
    {
        public ConfigurationElement this[int index]
        {
            get { return (ConfigurationElement) BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);

                BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new RallyProjectElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RallyProjectElement) (element)).Name;
        }
    }

    public class RallyProjectElement : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string) this["name"]; }
            set { this["name"] = value; }
        }
    }
}