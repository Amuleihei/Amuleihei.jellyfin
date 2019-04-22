using System;
using System.IO;
using System.Threading.Tasks;
using Jellyfin.Controller;
using Jellyfin.Controller.Net;
using Jellyfin.Model.IO;
using Jellyfin.Model.Services;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Server.Implementations
{
    public class ResourceFileManager : IResourceFileManager
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;
        private readonly IHttpResultFactory _resultFactory;

        public ResourceFileManager(
            IHttpResultFactory resultFactory,
            ILoggerFactory loggerFactory,
            IFileSystem fileSystem)
        {
            _resultFactory = resultFactory;
            _logger = loggerFactory.CreateLogger("ResourceManager");
            _fileSystem = fileSystem;
        }

        public Stream GetResourceFileStream(string basePath, string virtualPath)
        {
            return _fileSystem.GetFileStream(GetResourcePath(basePath, virtualPath), FileOpenMode.Open, FileAccessMode.Read, FileShareMode.ReadWrite, true);
        }

        public Task<object> GetStaticFileResult(IRequest request, string basePath, string virtualPath, string contentType, TimeSpan? cacheDuration)
        {
            return _resultFactory.GetStaticFileResult(request, GetResourcePath(basePath, virtualPath));
        }

        public string ReadAllText(string basePath, string virtualPath)
        {
            return File.ReadAllText(GetResourcePath(basePath, virtualPath));
        }

        private string GetResourcePath(string basePath, string virtualPath)
        {
            var fullPath = Path.Combine(basePath, virtualPath.Replace('/', Path.DirectorySeparatorChar));

            try
            {
                fullPath = Path.GetFullPath(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Path.GetFullPath");
            }

            // Don't allow file system access outside of the source folder
            if (!_fileSystem.ContainsSubPath(basePath, fullPath))
            {
                throw new SecurityException("Access denied");
            }

            return fullPath;
        }
    }
}
