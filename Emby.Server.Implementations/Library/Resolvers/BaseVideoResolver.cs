#nullable disable

#pragma warning disable CS1591

using System;
using System.IO;
using System.Linq;
using DiscUtils.Udf;
using Emby.Naming.Video;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace Emby.Server.Implementations.Library.Resolvers
{
    /// <summary>
    /// Resolves a Path into a Video or Video subclass.
    /// </summary>
    /// <typeparam name="T">The type of item to resolve.</typeparam>
    public abstract class BaseVideoResolver<T> : MediaBrowser.Controller.Resolvers.ItemResolver<T>
        where T : Video, new()
    {
        protected BaseVideoResolver(ILibraryManager libraryManager)
        {
            LibraryManager = libraryManager;
        }

        protected ILibraryManager LibraryManager { get; }

        /// <summary>
        /// Resolves the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>`0.</returns>
        public override T Resolve(ItemResolveArgs args)
        {
            return ResolveVideo<T>(args, false);
        }

        /// <summary>
        /// Resolves the video.
        /// </summary>
        /// <typeparam name="TVideoType">The type of the T video type.</typeparam>
        /// <param name="args">The args.</param>
        /// <param name="parseName">if set to <c>true</c> [parse name].</param>
        /// <returns>``0.</returns>
        protected virtual TVideoType ResolveVideo<TVideoType>(ItemResolveArgs args, bool parseName)
              where TVideoType : Video, new()
        {
            var namingOptions = LibraryManager.GetNamingOptions();

            // If the path is a file check for a matching extensions
            if (args.IsDirectory)
            {
                TVideoType video = null;
                VideoFileInfo videoInfo = null;

                // Loop through each child file/folder and see if we find a video
                foreach (var child in args.FileSystemChildren)
                {
                    var filename = child.Name;

                    if (child.IsDirectory)
                    {
                        if (IsDvdDirectory(child.FullName, filename, args.DirectoryService))
                        {
                            videoInfo = VideoResolver.ResolveDirectory(args.Path, namingOptions);

                            if (videoInfo == null)
                            {
                                return null;
                            }

                            video = new TVideoType
                            {
                                Path = args.Path,
                                VideoType = VideoType.Dvd,
                                ProductionYear = videoInfo.Year
                            };
                            break;
                        }

                        if (IsBluRayDirectory(filename))
                        {
                            videoInfo = VideoResolver.ResolveDirectory(args.Path, namingOptions);

                            if (videoInfo == null)
                            {
                                return null;
                            }

                            video = new TVideoType
                            {
                                Path = args.Path,
                                VideoType = VideoType.BluRay,
                                ProductionYear = videoInfo.Year
                            };
                            break;
                        }
                    }
                    else if (IsDvdFile(filename))
                    {
                        videoInfo = VideoResolver.ResolveDirectory(args.Path, namingOptions);

                        if (videoInfo == null)
                        {
                            return null;
                        }

                        video = new TVideoType
                        {
                            Path = args.Path,
                            VideoType = VideoType.Dvd,
                            ProductionYear = videoInfo.Year
                        };
                        break;
                    }
                }

                if (video != null)
                {
                    video.Name = parseName ?
                        videoInfo.Name :
                        Path.GetFileName(args.Path);

                    Set3DFormat(video, videoInfo);
                }

                return video;
            }
            else
            {
                var videoInfo = VideoResolver.Resolve(args.Path, false, namingOptions, false);

                if (videoInfo == null)
                {
                    return null;
                }

                if (LibraryManager.IsVideoFile(args.Path) || videoInfo.IsStub)
                {
                    var path = args.Path;

                    var video = new TVideoType
                    {
                        Path = path,
                        IsInMixedFolder = true,
                        ProductionYear = videoInfo.Year
                    };

                    SetVideoType(video, videoInfo);

                    video.Name = parseName ?
                        videoInfo.Name :
                        Path.GetFileNameWithoutExtension(args.Path);

                    Set3DFormat(video, videoInfo);

                    return video;
                }
            }

            return null;
        }

        protected void SetVideoType(Video video, VideoFileInfo videoInfo)
        {
            var extension = Path.GetExtension(video.Path.AsSpan());
            video.VideoType = extension.Equals(".iso", StringComparison.OrdinalIgnoreCase)
                              || extension.Equals(".img", StringComparison.OrdinalIgnoreCase)
                ? VideoType.Iso
                : VideoType.VideoFile;

            video.IsShortcut = extension.Equals(".strm", StringComparison.OrdinalIgnoreCase);
            video.IsPlaceHolder = videoInfo.IsStub;

            if (videoInfo.IsStub)
            {
                if (string.Equals(videoInfo.StubType, "dvd", StringComparison.OrdinalIgnoreCase))
                {
                    video.VideoType = VideoType.Dvd;
                }
                else if (string.Equals(videoInfo.StubType, "bluray", StringComparison.OrdinalIgnoreCase))
                {
                    video.VideoType = VideoType.BluRay;
                }
            }

            SetIsoType(video);
        }

        protected void SetIsoType(Video video)
        {
            if (video.VideoType == VideoType.Iso)
            {
                if (video.Path.Contains("dvd", StringComparison.OrdinalIgnoreCase))
                {
                    video.IsoType = IsoType.Dvd;
                }
                else if (video.Path.Contains("bluray", StringComparison.OrdinalIgnoreCase))
                {
                    video.IsoType = IsoType.BluRay;
                }
                else
                {
                    // use disc-utils, both DVDs and BDs use UDF filesystem
                    using (var videoFileStream = File.Open(video.Path, FileMode.Open, FileAccess.Read))
                    {
                        UdfReader udfReader = new UdfReader(videoFileStream);
                        if (udfReader.DirectoryExists("VIDEO_TS"))
                        {
                            video.IsoType = IsoType.Dvd;
                        }
                        else if (udfReader.DirectoryExists("BDMV"))
                        {
                            video.IsoType = IsoType.BluRay;
                        }
                    }
                }
            }
        }

        protected void Set3DFormat(Video video, bool is3D, string format3D)
        {
            if (is3D)
            {
                if (string.Equals(format3D, "fsbs", StringComparison.OrdinalIgnoreCase))
                {
                    video.Video3DFormat = Video3DFormat.FullSideBySide;
                }
                else if (string.Equals(format3D, "ftab", StringComparison.OrdinalIgnoreCase))
                {
                    video.Video3DFormat = Video3DFormat.FullTopAndBottom;
                }
                else if (string.Equals(format3D, "hsbs", StringComparison.OrdinalIgnoreCase))
                {
                    video.Video3DFormat = Video3DFormat.HalfSideBySide;
                }
                else if (string.Equals(format3D, "htab", StringComparison.OrdinalIgnoreCase))
                {
                    video.Video3DFormat = Video3DFormat.HalfTopAndBottom;
                }
                else if (string.Equals(format3D, "sbs", StringComparison.OrdinalIgnoreCase))
                {
                    video.Video3DFormat = Video3DFormat.HalfSideBySide;
                }
                else if (string.Equals(format3D, "sbs3d", StringComparison.OrdinalIgnoreCase))
                {
                    video.Video3DFormat = Video3DFormat.HalfSideBySide;
                }
                else if (string.Equals(format3D, "tab", StringComparison.OrdinalIgnoreCase))
                {
                    video.Video3DFormat = Video3DFormat.HalfTopAndBottom;
                }
                else if (string.Equals(format3D, "mvc", StringComparison.OrdinalIgnoreCase))
                {
                    video.Video3DFormat = Video3DFormat.MVC;
                }
            }
        }

        protected void Set3DFormat(Video video, VideoFileInfo videoInfo)
        {
            Set3DFormat(video, videoInfo.Is3D, videoInfo.Format3D);
        }

        protected void Set3DFormat(Video video)
        {
            var result = Format3DParser.Parse(video.Path, LibraryManager.GetNamingOptions());

            Set3DFormat(video, result.Is3D, result.Format3D);
        }

        /// <summary>
        /// Determines whether [is DVD directory] [the specified directory name].
        /// </summary>
        /// <param name="fullPath">The full path of the directory.</param>
        /// <param name="directoryName">The name of the directory.</param>
        /// <param name="directoryService">The directory service.</param>
        /// <returns><c>true</c> if the provided directory is a DVD directory, <c>false</c> otherwise.</returns>
        protected bool IsDvdDirectory(string fullPath, string directoryName, IDirectoryService directoryService)
        {
            if (!string.Equals(directoryName, "video_ts", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return directoryService.GetFilePaths(fullPath).Any(i => string.Equals(Path.GetExtension(i), ".vob", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines whether [is DVD file] [the specified name].
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if [is DVD file] [the specified name]; otherwise, <c>false</c>.</returns>
        protected bool IsDvdFile(string name)
        {
            return string.Equals(name, "video_ts.ifo", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether [is bluray directory] [the specified directory name].
        /// </summary>
        /// <param name="directoryName">The directory name.</param>
        /// <returns>Whether the directory is a bluray directory.</returns>
        protected bool IsBluRayDirectory(string directoryName)
        {
            return string.Equals(directoryName, "bdmv", StringComparison.OrdinalIgnoreCase);
        }
    }
}
