#nullable disable
#pragma warning disable CS1591

using System;
using System.Collections.Generic;

namespace MediaBrowser.Model.Configuration
{
    public class LibraryOptions
    {
        public LibraryOptions()
        {
            TypeOptions = Array.Empty<TypeOptions>();
            DisabledSubtitleFetchers = Array.Empty<string>();
            SubtitleFetcherOrder = Array.Empty<string>();
            DisabledLocalMetadataReaders = Array.Empty<string>();

            SkipSubtitlesIfAudioTrackMatches = true;
            RequirePerfectSubtitleMatch = true;

            EnablePhotos = true;
            SaveSubtitlesWithMedia = true;
            EnableRealtimeMonitor = true;
            PathInfos = Array.Empty<MediaPathInfo>();
            EnableInternetProviders = true;
            EnableAutomaticSeriesGrouping = true;
            SeasonZeroDisplayName = "Specials";
        }

        public bool EnablePhotos { get; set; }

        public bool EnableRealtimeMonitor { get; set; }

        public bool EnableChapterImageExtraction { get; set; }

        public bool ExtractChapterImagesDuringLibraryScan { get; set; }

        public bool DownloadImagesInAdvance { get; set; }

        public IReadOnlyCollection<MediaPathInfo> PathInfos { get; set; }

        public bool SaveLocalMetadata { get; set; }

        public bool EnableInternetProviders { get; set; }

        public bool EnableAutomaticSeriesGrouping { get; set; }

        public bool EnableEmbeddedTitles { get; set; }

        public bool EnableEmbeddedEpisodeInfos { get; set; }

        public int AutomaticRefreshIntervalDays { get; set; }

        /// <summary>
        ///     Gets or sets the preferred metadata language.
        /// </summary>
        /// <value>The preferred metadata language.</value>
        public string PreferredMetadataLanguage { get; set; }

        /// <summary>
        ///     Gets or sets the metadata country code.
        /// </summary>
        /// <value>The metadata country code.</value>
        public string MetadataCountryCode { get; set; }

        public string SeasonZeroDisplayName { get; set; }

        public IReadOnlyCollection<string> MetadataSavers { get; set; }

        public IReadOnlyCollection<string> DisabledLocalMetadataReaders { get; set; }

        public IReadOnlyCollection<string> LocalMetadataReaderOrder { get; set; }

        public IReadOnlyCollection<string> DisabledSubtitleFetchers { get; set; }

        public IReadOnlyCollection<string> SubtitleFetcherOrder { get; set; }

        public bool SkipSubtitlesIfEmbeddedSubtitlesPresent { get; set; }

        public bool SkipSubtitlesIfAudioTrackMatches { get; set; }

        public IReadOnlyCollection<string> SubtitleDownloadLanguages { get; set; }

        public bool RequirePerfectSubtitleMatch { get; set; }

        public bool SaveSubtitlesWithMedia { get; set; }

        public TypeOptions[] TypeOptions { get; set; }

        public TypeOptions GetTypeOptions(string type)
        {
            foreach (var options in TypeOptions)
            {
                if (string.Equals(options.Type, type, StringComparison.OrdinalIgnoreCase))
                {
                    return options;
                }
            }

            return null;
        }
    }
}
