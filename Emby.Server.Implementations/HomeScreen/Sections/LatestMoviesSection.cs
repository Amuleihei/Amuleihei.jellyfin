using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Library;
using MediaBrowser.Model.Querying;

namespace Emby.Server.Implementations.HomeScreen.Sections
{
    public class LatestMoviesSection : IHomeScreenSection
    {
        public string Section => "LatestMovies";

        public string DisplayText { get; set; } = "Latest Movies";

        public int Limit => 1;

        public string Route => "movies";

        public string AdditionalData { get; set; } = "movies";

        private readonly IUserViewManager _userViewManager;
        private readonly IUserManager _userManager;
        private readonly IDtoService _dtoService;

        public LatestMoviesSection(IUserViewManager userViewManager,
            IUserManager userManager,
            IDtoService dtoService)
        {
            _userViewManager = userViewManager;
            _userManager = userManager;
            _dtoService = dtoService;
        }

        public QueryResult<BaseItemDto> GetResults(HomeScreenSectionPayload payload)
        {
            var user = _userManager.GetUserById(payload.UserId);

            var dtoOptions = new DtoOptions
            {
                Fields = new List<ItemFields>
                {
                    ItemFields.PrimaryImageAspectRatio,
                    ItemFields.BasicSyncInfo,
                    ItemFields.Path
                }
            };

            dtoOptions.ImageTypeLimit = 1;
            dtoOptions.ImageTypes = new List<ImageType>
            {
                ImageType.Primary,
                ImageType.Backdrop,
                ImageType.Thumb
            };

            MyMediaSection myMedia = new MyMediaSection(_userViewManager, _userManager, _dtoService);
            QueryResult<BaseItemDto> media = myMedia.GetResults(payload);

            Guid parentId = media.Items.FirstOrDefault(x => x.Name == payload.AdditionalData)?.Id ?? Guid.Empty;

            var list = _userViewManager.GetLatestItems(
                new LatestItemsQuery
                {
                    GroupItems = false,
                    Limit = 16,
                    ParentId = parentId,
                    UserId = payload.UserId,
                    IncludeItemTypes = new BaseItemKind[]
                    {
                        BaseItemKind.Movie
                    }
                },
                dtoOptions);

            var dtos = list.Select(i =>
            {
                var item = i.Item2[0];
                var childCount = 0;

                if (i.Item1 != null && (i.Item2.Count > 1 || i.Item1 is MusicAlbum))
                {
                    item = i.Item1;
                    childCount = i.Item2.Count;
                }

                var dto = _dtoService.GetBaseItemDto(item, dtoOptions, user);

                dto.ChildCount = childCount;

                return dto;
            });

            return new QueryResult<BaseItemDto>(dtos.ToList());
        }
    }
}
