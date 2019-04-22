using System.Globalization;
using System.Xml;
using Jellyfin.Common.Configuration;
using Jellyfin.Controller.Entities.TV;
using Jellyfin.Controller.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.XbmcMetadata.Parsers
{
    public class SeasonNfoParser : BaseNfoParser<Season>
    {
        /// <summary>
        /// Fetches the data from XML node.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="itemResult">The item result.</param>
        protected override void FetchDataFromXmlNode(XmlReader reader, MetadataResult<Season> itemResult)
        {
            var item = itemResult.Item;

            switch (reader.Name)
            {
                case "seasonnumber":
                    {
                        var number = reader.ReadElementContentAsString();

                        if (!string.IsNullOrWhiteSpace(number))
                        {
                            if (int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
                            {
                                item.IndexNumber = num;
                            }
                        }
                        break;
                    }

                default:
                    base.FetchDataFromXmlNode(reader, itemResult);
                    break;
            }
        }

        public SeasonNfoParser(ILogger logger, IConfigurationManager config, IProviderManager providerManager)
            : base(logger, config, providerManager)
        {
        }
    }
}
