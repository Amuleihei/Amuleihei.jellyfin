#pragma warning disable CS1591

using System.Collections.Generic;
using Emby.Dlna.Configuration;
using MediaBrowser.Common.Configuration;

namespace Emby.Dlna
{
    public class DlnaConfigurationFactory : IConfigurationFactory
    {
        public IEnumerable<ConfigurationStore> GetConfigurations()
        {
            return new ConfigurationStore[]
            {
                new ConfigurationStore
                {
                    Key = "dlna",
                    ConfigurationType = typeof(DlnaOptions)
                }
            };
        }
    }
}
