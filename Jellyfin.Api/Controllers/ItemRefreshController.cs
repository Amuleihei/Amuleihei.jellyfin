#nullable enable

using System.ComponentModel;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Api.Controllers
{
    /// <summary>
    /// Item Refresh Controller.
    /// </summary>
    /// [Authenticated]
    [Route("/Items")]
    [Authorize]
    public class ItemRefreshController : BaseJellyfinApiController
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IProviderManager _providerManager;
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemRefreshController"/> class.
        /// </summary>
        /// <param name="libraryManager">Instance of <see cref="ILibraryManager"/> interface.</param>
        /// <param name="providerManager">Instance of <see cref="IProviderManager"/> interface.</param>
        /// <param name="fileSystem">Instance of <see cref="IFileSystem"/> interface.</param>
        public ItemRefreshController(
            ILibraryManager libraryManager,
            IProviderManager providerManager,
            IFileSystem fileSystem)
        {
            _libraryManager = libraryManager;
            _providerManager = providerManager;
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// Refreshes metadata for an item.
        /// </summary>
        /// <param name="id">Item id.</param>
        /// <param name="metadataRefreshMode">(Optional) Specifies the metadata refresh mode.</param>
        /// <param name="imageRefreshMode">(Optional) Specifies the image refresh mode.</param>
        /// <param name="replaceAllMetadata">(Optional) Determines if metadata should be replaced. Only applicable if mode is FullRefresh.</param>
        /// <param name="replaceAllImages">(Optional) Determines if images should be replaced. Only applicable if mode is FullRefresh.</param>
        /// <param name="recursive">(Unused) Indicates if the refresh should occur recursively.</param>
        /// <response code="200">Item metadata refresh queued.</response>
        /// <response code="404">Item to refresh not found.</response>
        /// <returns>An <see cref="OkResult"/> on success, or a <see cref="NotFoundResult"/> if the item could not be found.</returns>
        [HttpPost("{Id}/Refresh")]
        [Description("Refreshes metadata for an item.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Post(
            [FromRoute] string id,
            [FromQuery] MetadataRefreshMode metadataRefreshMode = MetadataRefreshMode.None,
            [FromQuery] MetadataRefreshMode imageRefreshMode = MetadataRefreshMode.None,
            [FromQuery] bool replaceAllMetadata = false,
            [FromQuery] bool replaceAllImages = false,
            [FromQuery] bool recursive = false)
        {
            var item = _libraryManager.GetItemById(id);
            if (item == null)
            {
                return NotFound();
            }

            var refreshOptions = new MetadataRefreshOptions(new DirectoryService(_fileSystem))
            {
                MetadataRefreshMode = metadataRefreshMode,
                ImageRefreshMode = imageRefreshMode,
                ReplaceAllImages = replaceAllImages,
                ReplaceAllMetadata = replaceAllMetadata,
                ForceSave = metadataRefreshMode == MetadataRefreshMode.FullRefresh ||
                            imageRefreshMode == MetadataRefreshMode.FullRefresh ||
                            replaceAllImages || replaceAllMetadata,
                IsAutomated = false
            };

            _providerManager.QueueRefresh(item.Id, refreshOptions, RefreshPriority.High);
            return Ok();
        }
    }
}
