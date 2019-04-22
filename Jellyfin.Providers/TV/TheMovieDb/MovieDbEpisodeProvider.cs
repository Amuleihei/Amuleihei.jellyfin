using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Common.Net;
using Jellyfin.Controller.Configuration;
using Jellyfin.Controller.Entities;
using Jellyfin.Controller.Entities.TV;
using Jellyfin.Controller.Providers;
using Jellyfin.Model.Entities;
using Jellyfin.Model.Globalization;
using Jellyfin.Model.IO;
using Jellyfin.Model.Net;
using Jellyfin.Model.Providers;
using Jellyfin.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Providers.TV.TheMovieDb
{
    public class MovieDbEpisodeProvider :
            MovieDbProviderBase,
            IRemoteMetadataProvider<Episode, EpisodeInfo>,
            IHasOrder
    {
        public MovieDbEpisodeProvider(IHttpClient httpClient, IServerConfigurationManager configurationManager, IJsonSerializer jsonSerializer, IFileSystem fileSystem, ILocalizationManager localization, ILoggerFactory loggerFactory)
            : base(httpClient, configurationManager, jsonSerializer, fileSystem, localization, loggerFactory)
        { }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(EpisodeInfo searchInfo, CancellationToken cancellationToken)
        {
            var list = new List<RemoteSearchResult>();

            // The search query must either provide an episode number or date
            if (!searchInfo.IndexNumber.HasValue || !searchInfo.ParentIndexNumber.HasValue)
            {
                return list;
            }

            var metadataResult = await GetMetadata(searchInfo, cancellationToken);

            if (metadataResult.HasMetadata)
            {
                var item = metadataResult.Item;

                list.Add(new RemoteSearchResult
                {
                    IndexNumber = item.IndexNumber,
                    Name = item.Name,
                    ParentIndexNumber = item.ParentIndexNumber,
                    PremiereDate = item.PremiereDate,
                    ProductionYear = item.ProductionYear,
                    ProviderIds = item.ProviderIds,
                    SearchProviderName = Name,
                    IndexNumberEnd = item.IndexNumberEnd
                });
            }

            return list;
        }

        public async Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Episode>();

            // Allowing this will dramatically increase scan times
            if (info.IsMissingEpisode)
            {
                return result;
            }

            info.SeriesProviderIds.TryGetValue(MetadataProviders.Tmdb.ToString(), out string seriesTmdbId);

            if (string.IsNullOrEmpty(seriesTmdbId))
            {
                return result;
            }

            var seasonNumber = info.ParentIndexNumber;
            var episodeNumber = info.IndexNumber;

            if (!seasonNumber.HasValue || !episodeNumber.HasValue)
            {
                return result;
            }

            try
            {
                var response = await GetEpisodeInfo(seriesTmdbId, seasonNumber.Value, episodeNumber.Value, info.MetadataLanguage, cancellationToken).ConfigureAwait(false);

                result.HasMetadata = true;
                result.QueriedById = true;

                if (!string.IsNullOrEmpty(response.overview))
                {
                    // if overview is non-empty, we can assume that localized data was returned
                    result.ResultLanguage = info.MetadataLanguage;
                }

                var item = new Episode();
                result.Item = item;

                item.Name = info.Name;
                item.IndexNumber = info.IndexNumber;
                item.ParentIndexNumber = info.ParentIndexNumber;
                item.IndexNumberEnd = info.IndexNumberEnd;

                if (response.external_ids.tvdb_id > 0)
                {
                    item.SetProviderId(MetadataProviders.Tvdb, response.external_ids.tvdb_id.ToString(CultureInfo.InvariantCulture));
                }

                item.PremiereDate = response.air_date;
                item.ProductionYear = result.Item.PremiereDate.Value.Year;

                item.Name = response.name;
                item.Overview = response.overview;

                item.CommunityRating = (float)response.vote_average;
                //item.VoteCount = response.vote_count;

                if (response.videos != null && response.videos.results != null)
                {
                    foreach (var video in response.videos.results)
                    {
                        if (video.type.Equals("trailer", System.StringComparison.OrdinalIgnoreCase)
                            || video.type.Equals("clip", System.StringComparison.OrdinalIgnoreCase))
                        {
                            if (video.site.Equals("youtube", System.StringComparison.OrdinalIgnoreCase))
                            {
                                var videoUrl = string.Format("http://www.youtube.com/watch?v={0}", video.key);
                                item.AddTrailerUrl(videoUrl);
                            }
                        }
                    }
                }

                result.ResetPeople();

                var credits = response.credits;
                if (credits != null)
                {
                    //Actors, Directors, Writers - all in People
                    //actors come from cast
                    if (credits.cast != null)
                    {
                        foreach (var actor in credits.cast.OrderBy(a => a.order))
                        {
                            result.AddPerson(new PersonInfo { Name = actor.name.Trim(), Role = actor.character, Type = PersonType.Actor, SortOrder = actor.order });
                        }
                    }

                    // guest stars
                    if (credits.guest_stars != null)
                    {
                        foreach (var guest in credits.guest_stars.OrderBy(a => a.order))
                        {
                            result.AddPerson(new PersonInfo { Name = guest.name.Trim(), Role = guest.character, Type = PersonType.GuestStar, SortOrder = guest.order });
                        }
                    }

                    //and the rest from crew
                    if (credits.crew != null)
                    {
                        var keepTypes = new[]
                        {
                            PersonType.Director,
                            //PersonType.Writer,
                            //PersonType.Producer
                        };

                        foreach (var person in credits.crew)
                        {
                            // Normalize this
                            var type = person.department;
                            if (string.Equals(type, "writing", StringComparison.OrdinalIgnoreCase))
                            {
                                type = PersonType.Writer;
                            }

                            if (!keepTypes.Contains(type ?? string.Empty, StringComparer.OrdinalIgnoreCase) &&
                                !keepTypes.Contains(person.job ?? string.Empty, StringComparer.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            result.AddPerson(new PersonInfo { Name = person.name.Trim(), Role = person.job, Type = type });
                        }
                    }
                }
            }
            catch (HttpException ex)
            {
                if (ex.StatusCode.HasValue && ex.StatusCode.Value == HttpStatusCode.NotFound)
                {
                    return result;
                }

                throw;
            }

            return result;
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return GetResponse(url, cancellationToken);
        }
        // After TheTvDb
        public int Order => 1;

        public string Name => "TheMovieDb";
    }
}
