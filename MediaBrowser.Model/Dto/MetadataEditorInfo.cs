#pragma warning disable CS1591
#pragma warning disable CA1819 // Properties should not return arrays

using System;

using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Providers;

namespace MediaBrowser.Model.Dto
{
    public class MetadataEditorInfo
    {
        public MetadataEditorInfo()
        {
            ParentalRatingOptions = Array.Empty<ParentalRating>();
            Countries = Array.Empty<CountryInfo>();
            Cultures = Array.Empty<CultureDto>();
            ExternalIdInfos = Array.Empty<ExternalIdInfo>();
            ContentTypeOptions = Array.Empty<NameValuePair>();
        }

        public ParentalRating[] ParentalRatingOptions { get; set; }

        public CountryInfo[] Countries { get; set; }

        public CultureDto[] Cultures { get; set; }

        public ExternalIdInfo[] ExternalIdInfos { get; set; }

        public string ContentType { get; set; }

        public NameValuePair[] ContentTypeOptions { get; set; }
    }
}
