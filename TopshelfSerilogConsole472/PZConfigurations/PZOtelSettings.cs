using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZConfigurations
{
    public class PZOtelSettings : ConfigurationSection
    {
        [ConfigurationProperty("pzOtelSettings")]
        public KeyValueConfigurationCollection pzOtelSettings
        {
            get { return (KeyValueConfigurationCollection)this["pzOtelSettings"]; }
        }
    }
}
