#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1819 // Properties should not return arrays

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Jellyfin.Data.Enums;
using Jellyfin.Extensions;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Model.Session;

namespace MediaBrowser.Model.Dlna
{
    /// <summary>
    /// Class StreamInfo.
    /// </summary>
    public class StreamInfo
    {
        public StreamInfo()
        {
            AudioCodecs = Array.Empty<string>();
            VideoCodecs = Array.Empty<string>();
            SubtitleCodecs = Array.Empty<string>();
            StreamOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            DeviceProfile = new DeviceProfile();
        }

        public Guid ItemId { get; set; }

        public PlayMethod PlayMethod { get; set; }

        public EncodingContext Context { get; set; }

        public DlnaProfileType MediaType { get; set; }

        public string? Container { get; set; }

        public MediaStreamProtocol SubProtocol { get; set; }

        public long StartPositionTicks { get; set; }

        public int? SegmentLength { get; set; }

        public int? MinSegments { get; set; }

        public bool BreakOnNonKeyFrames { get; set; }

        public bool RequireAvc { get; set; }

        public bool RequireNonAnamorphic { get; set; }

        public bool CopyTimestamps { get; set; }

        public bool EnableMpegtsM2TsMode { get; set; }

        public bool EnableSubtitlesInManifest { get; set; }

        public string[] AudioCodecs { get; set; }

        public string[] VideoCodecs { get; set; }

        public int? AudioStreamIndex { get; set; }

        public int? SubtitleStreamIndex { get; set; }

        public int? TranscodingMaxAudioChannels { get; set; }

        public int? GlobalMaxAudioChannels { get; set; }

        public int? AudioBitrate { get; set; }

        public int? AudioSampleRate { get; set; }

        public int? VideoBitrate { get; set; }

        public int? MaxWidth { get; set; }

        public int? MaxHeight { get; set; }

        public float? MaxFramerate { get; set; }

        public required DeviceProfile DeviceProfile { get; set; }

        public string? DeviceProfileId { get; set; }

        public string? DeviceId { get; set; }

        public long? RunTimeTicks { get; set; }

        public TranscodeSeekInfo TranscodeSeekInfo { get; set; }

        public bool EstimateContentLength { get; set; }

        public MediaSourceInfo? MediaSource { get; set; }

        public string[] SubtitleCodecs { get; set; }

        public SubtitleDeliveryMethod SubtitleDeliveryMethod { get; set; }

        public string? SubtitleFormat { get; set; }

        public string? PlaySessionId { get; set; }

        public TranscodeReason TranscodeReasons { get; set; }

        public Dictionary<string, string> StreamOptions { get; private set; }

        public string? MediaSourceId => MediaSource?.Id;

        public bool EnableAudioVbrEncoding { get; set; }

        public bool IsDirectStream => MediaSource?.VideoType is not (VideoType.Dvd or VideoType.BluRay)
            && PlayMethod is PlayMethod.DirectStream or PlayMethod.DirectPlay;

        /// <summary>
        /// Gets the audio stream that will be used.
        /// </summary>
        public MediaStream? TargetAudioStream => MediaSource?.GetDefaultAudioStream(AudioStreamIndex);

        /// <summary>
        /// Gets the video stream that will be used.
        /// </summary>
        public MediaStream? TargetVideoStream => MediaSource?.VideoStream;

        /// <summary>
        /// Gets the audio sample rate that will be in the output stream.
        /// </summary>
        public int? TargetAudioSampleRate
        {
            get
            {
                var stream = TargetAudioStream;
                return AudioSampleRate.HasValue && !IsDirectStream
                    ? AudioSampleRate
                    : stream?.SampleRate;
            }
        }

        /// <summary>
        /// Gets the audio sample rate that will be in the output stream.
        /// </summary>
        public int? TargetAudioBitDepth
        {
            get
            {
                if (IsDirectStream)
                {
                    return TargetAudioStream?.BitDepth;
                }

                var targetAudioCodecs = TargetAudioCodec;
                var audioCodec = targetAudioCodecs.Length == 0 ? null : targetAudioCodecs[0];
                if (!string.IsNullOrEmpty(audioCodec))
                {
                    return GetTargetAudioBitDepth(audioCodec);
                }

                return TargetAudioStream?.BitDepth;
            }
        }

        /// <summary>
        /// Gets the audio sample rate that will be in the output stream.
        /// </summary>
        public int? TargetVideoBitDepth
        {
            get
            {
                if (IsDirectStream)
                {
                    return TargetVideoStream?.BitDepth;
                }

                var targetVideoCodecs = TargetVideoCodec;
                var videoCodec = targetVideoCodecs.Length == 0 ? null : targetVideoCodecs[0];
                if (!string.IsNullOrEmpty(videoCodec))
                {
                    return GetTargetVideoBitDepth(videoCodec);
                }

                return TargetVideoStream?.BitDepth;
            }
        }

        /// <summary>
        /// Gets the target reference frames.
        /// </summary>
        /// <value>The target reference frames.</value>
        public int? TargetRefFrames
        {
            get
            {
                if (IsDirectStream)
                {
                    return TargetVideoStream?.RefFrames;
                }

                var targetVideoCodecs = TargetVideoCodec;
                var videoCodec = targetVideoCodecs.Length == 0 ? null : targetVideoCodecs[0];
                if (!string.IsNullOrEmpty(videoCodec))
                {
                    return GetTargetRefFrames(videoCodec);
                }

                return TargetVideoStream?.RefFrames;
            }
        }

        /// <summary>
        /// Gets the audio sample rate that will be in the output stream.
        /// </summary>
        public float? TargetFramerate
        {
            get
            {
                var stream = TargetVideoStream;
                return MaxFramerate.HasValue && !IsDirectStream
                    ? MaxFramerate
                    : stream?.ReferenceFrameRate;
            }
        }

        /// <summary>
        /// Gets the audio sample rate that will be in the output stream.
        /// </summary>
        public double? TargetVideoLevel
        {
            get
            {
                if (IsDirectStream)
                {
                    return TargetVideoStream?.Level;
                }

                var targetVideoCodecs = TargetVideoCodec;
                var videoCodec = targetVideoCodecs.Length == 0 ? null : targetVideoCodecs[0];
                if (!string.IsNullOrEmpty(videoCodec))
                {
                    return GetTargetVideoLevel(videoCodec);
                }

                return TargetVideoStream?.Level;
            }
        }

        /// <summary>
        /// Gets the audio sample rate that will be in the output stream.
        /// </summary>
        public int? TargetPacketLength
        {
            get
            {
                var stream = TargetVideoStream;
                return !IsDirectStream
                    ? null
                    : stream?.PacketLength;
            }
        }

        /// <summary>
        /// Gets the audio sample rate that will be in the output stream.
        /// </summary>
        public string? TargetVideoProfile
        {
            get
            {
                if (IsDirectStream)
                {
                    return TargetVideoStream?.Profile;
                }

                var targetVideoCodecs = TargetVideoCodec;
                var videoCodec = targetVideoCodecs.Length == 0 ? null : targetVideoCodecs[0];
                if (!string.IsNullOrEmpty(videoCodec))
                {
                    return GetOption(videoCodec, "profile");
                }

                return TargetVideoStream?.Profile;
            }
        }

        /// <summary>
        /// Gets the target video range type that will be in the output stream.
        /// </summary>
        public VideoRangeType TargetVideoRangeType
        {
            get
            {
                if (IsDirectStream)
                {
                    return TargetVideoStream?.VideoRangeType ?? VideoRangeType.Unknown;
                }

                var targetVideoCodecs = TargetVideoCodec;
                var videoCodec = targetVideoCodecs.Length == 0 ? null : targetVideoCodecs[0];
                if (!string.IsNullOrEmpty(videoCodec)
                    && Enum.TryParse(GetOption(videoCodec, "rangetype"), true, out VideoRangeType videoRangeType))
                {
                    return videoRangeType;
                }

                return TargetVideoStream?.VideoRangeType ?? VideoRangeType.Unknown;
            }
        }

        /// <summary>
        /// Gets the target video codec tag.
        /// </summary>
        /// <value>The target video codec tag.</value>
        public string? TargetVideoCodecTag
        {
            get
            {
                var stream = TargetVideoStream;
                return !IsDirectStream
                    ? null
                    : stream?.CodecTag;
            }
        }

        /// <summary>
        /// Gets the audio bitrate that will be in the output stream.
        /// </summary>
        public int? TargetAudioBitrate
        {
            get
            {
                var stream = TargetAudioStream;
                return AudioBitrate.HasValue && !IsDirectStream
                    ? AudioBitrate
                    : stream?.BitRate;
            }
        }

        /// <summary>
        /// Gets the audio channels that will be in the output stream.
        /// </summary>
        public int? TargetAudioChannels
        {
            get
            {
                if (IsDirectStream)
                {
                    return TargetAudioStream?.Channels;
                }

                var targetAudioCodecs = TargetAudioCodec;
                var codec = targetAudioCodecs.Length == 0 ? null : targetAudioCodecs[0];
                if (!string.IsNullOrEmpty(codec))
                {
                    return GetTargetRefFrames(codec);
                }

                return TargetAudioStream?.Channels;
            }
        }

        /// <summary>
        /// Gets the audio codec that will be in the output stream.
        /// </summary>
        public string[] TargetAudioCodec
        {
            get
            {
                var stream = TargetAudioStream;

                string? inputCodec = stream?.Codec;

                if (IsDirectStream)
                {
                    return string.IsNullOrEmpty(inputCodec) ? Array.Empty<string>() : new[] { inputCodec };
                }

                foreach (string codec in AudioCodecs)
                {
                    if (string.Equals(codec, inputCodec, StringComparison.OrdinalIgnoreCase))
                    {
                        return string.IsNullOrEmpty(codec) ? Array.Empty<string>() : new[] { codec };
                    }
                }

                return AudioCodecs;
            }
        }

        public string[] TargetVideoCodec
        {
            get
            {
                var stream = TargetVideoStream;

                string? inputCodec = stream?.Codec;

                if (IsDirectStream)
                {
                    return string.IsNullOrEmpty(inputCodec) ? Array.Empty<string>() : new[] { inputCodec };
                }

                foreach (string codec in VideoCodecs)
                {
                    if (string.Equals(codec, inputCodec, StringComparison.OrdinalIgnoreCase))
                    {
                        return string.IsNullOrEmpty(codec) ? Array.Empty<string>() : new[] { codec };
                    }
                }

                return VideoCodecs;
            }
        }

        /// <summary>
        /// Gets the audio channels that will be in the output stream.
        /// </summary>
        public long? TargetSize
        {
            get
            {
                if (IsDirectStream)
                {
                    return MediaSource?.Size;
                }

                if (RunTimeTicks.HasValue)
                {
                    int? totalBitrate = TargetTotalBitrate;

                    double totalSeconds = RunTimeTicks.Value;
                    // Convert to ms
                    totalSeconds /= 10000;
                    // Convert to seconds
                    totalSeconds /= 1000;

                    return totalBitrate.HasValue ?
                        Convert.ToInt64(totalBitrate.Value * totalSeconds) :
                        null;
                }

                return null;
            }
        }

        public int? TargetVideoBitrate
        {
            get
            {
                var stream = TargetVideoStream;

                return VideoBitrate.HasValue && !IsDirectStream
                    ? VideoBitrate
                    : stream?.BitRate;
            }
        }

        public TransportStreamTimestamp TargetTimestamp
        {
            get
            {
                var defaultValue = string.Equals(Container, "m2ts", StringComparison.OrdinalIgnoreCase)
                    ? TransportStreamTimestamp.Valid
                    : TransportStreamTimestamp.None;

                return !IsDirectStream
                    ? defaultValue
                    : MediaSource is null ? defaultValue : MediaSource.Timestamp ?? TransportStreamTimestamp.None;
            }
        }

        public int? TargetTotalBitrate => (TargetAudioBitrate ?? 0) + (TargetVideoBitrate ?? 0);

        public bool? IsTargetAnamorphic
        {
            get
            {
                if (IsDirectStream)
                {
                    return TargetVideoStream?.IsAnamorphic;
                }

                return false;
            }
        }

        public bool? IsTargetInterlaced
        {
            get
            {
                if (IsDirectStream)
                {
                    return TargetVideoStream?.IsInterlaced;
                }

                var targetVideoCodecs = TargetVideoCodec;
                var videoCodec = targetVideoCodecs.Length == 0 ? null : targetVideoCodecs[0];
                if (!string.IsNullOrEmpty(videoCodec))
                {
                    if (string.Equals(GetOption(videoCodec, "deinterlace"), "true", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                return TargetVideoStream?.IsInterlaced;
            }
        }

        public bool? IsTargetAVC
        {
            get
            {
                if (IsDirectStream)
                {
                    return TargetVideoStream?.IsAVC;
                }

                return true;
            }
        }

        public int? TargetWidth
        {
            get
            {
                var videoStream = TargetVideoStream;

                if (videoStream is not null && videoStream.Width.HasValue && videoStream.Height.HasValue)
                {
                    ImageDimensions size = new ImageDimensions(videoStream.Width.Value, videoStream.Height.Value);

                    size = DrawingUtils.Resize(size, 0, 0, MaxWidth ?? 0, MaxHeight ?? 0);

                    return size.Width;
                }

                return MaxWidth;
            }
        }

        public int? TargetHeight
        {
            get
            {
                var videoStream = TargetVideoStream;

                if (videoStream is not null && videoStream.Width.HasValue && videoStream.Height.HasValue)
                {
                    ImageDimensions size = new ImageDimensions(videoStream.Width.Value, videoStream.Height.Value);

                    size = DrawingUtils.Resize(size, 0, 0, MaxWidth ?? 0, MaxHeight ?? 0);

                    return size.Height;
                }

                return MaxHeight;
            }
        }

        public int? TargetVideoStreamCount
        {
            get
            {
                if (IsDirectStream)
                {
                    return GetMediaStreamCount(MediaStreamType.Video, int.MaxValue);
                }

                return GetMediaStreamCount(MediaStreamType.Video, 1);
            }
        }

        public int? TargetAudioStreamCount
        {
            get
            {
                if (IsDirectStream)
                {
                    return GetMediaStreamCount(MediaStreamType.Audio, int.MaxValue);
                }

                return GetMediaStreamCount(MediaStreamType.Audio, 1);
            }
        }

        public void SetOption(string? qualifier, string name, string value)
        {
            if (string.IsNullOrEmpty(qualifier))
            {
                SetOption(name, value);
            }
            else
            {
                SetOption(qualifier + "-" + name, value);
            }
        }

        public void SetOption(string name, string value)
        {
            StreamOptions[name] = value;
        }

        public string? GetOption(string? qualifier, string name)
        {
            var value = GetOption(qualifier + "-" + name);

            if (string.IsNullOrEmpty(value))
            {
                value = GetOption(name);
            }

            return value;
        }

        public string? GetOption(string name)
        {
            if (StreamOptions.TryGetValue(name, out var value))
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// Returns this class as a url.
        /// </summary>
        /// <param name="baseUrl">The base Url.</param>
        /// <param name="accessToken">The access Token.</param>
        /// <param name="query">Optional extra query.</param>
        /// <returns>A querystring representation of this object.</returns>
        public string ToUrl(string? baseUrl, string? accessToken, string? query)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(baseUrl))
            {
                sb.Append(baseUrl.TrimEnd('/'));
            }

            if (MediaType == DlnaProfileType.Audio)
            {
                sb.Append("/audio/");
            }
            else
            {
                sb.Append("/videos/");
            }

            sb.Append(ItemId);

            if (SubProtocol == MediaStreamProtocol.hls)
            {
                sb.Append("/master.m3u8?");
            }
            else
            {
                sb.Append("/stream");

                if (!string.IsNullOrEmpty(Container))
                {
                    sb.Append('.');
                    sb.Append(Container);
                }

                sb.Append('?');
            }

            if (!string.IsNullOrEmpty(DeviceProfileId))
            {
                sb.Append("&DeviceProfileId=");
                sb.Append(DeviceProfileId);
            }

            if (!string.IsNullOrEmpty(DeviceId))
            {
                sb.Append("&DeviceId=");
                sb.Append(DeviceId);
            }

            if (!string.IsNullOrEmpty(MediaSourceId))
            {
                sb.Append("&MediaSourceId=");
                sb.Append(MediaSourceId);
            }

            // default true so don't store.
            if (IsDirectStream)
            {
                sb.Append("&Static=true");
            }

            if (VideoCodecs.Length != 0)
            {
                sb.Append("&VideoCodec=");
                sb.AppendJoin(',', VideoCodecs);
            }

            if (AudioCodecs.Length != 0)
            {
                sb.Append("&AudioCodec=");
                sb.AppendJoin(',', AudioCodecs);
            }

            if (AudioStreamIndex.HasValue)
            {
                sb.Append("&AudioStreamIndex=");
                sb.Append(AudioStreamIndex.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (SubtitleStreamIndex.HasValue && SubtitleDeliveryMethod != SubtitleDeliveryMethod.External && SubtitleStreamIndex != -1)
            {
                sb.Append("&SubtitleStreamIndex=");
                sb.Append(SubtitleStreamIndex.Value.ToString(CultureInfo.InvariantCulture));
                sb.Append("&SubtitleMethod=");
                sb.Append(SubtitleDeliveryMethod.ToString());
            }

            if (VideoBitrate.HasValue)
            {
                sb.Append("&VideoBitrate=");
                sb.Append(VideoBitrate.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (AudioBitrate.HasValue)
            {
                sb.Append("&AudioBitrate=");
                sb.Append(AudioBitrate.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (AudioSampleRate.HasValue)
            {
                sb.Append("&AudioSampleRate=");
                sb.Append(AudioSampleRate.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (MaxFramerate.HasValue)
            {
                sb.Append("&MaxFramerate=");
                sb.Append(MaxFramerate.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (MaxWidth.HasValue)
            {
                sb.Append("&MaxWidth=");
                sb.Append(MaxWidth.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (MaxHeight.HasValue)
            {
                sb.Append("&MaxHeight=");
                sb.Append(MaxHeight.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (SubProtocol == MediaStreamProtocol.hls)
            {
                if (!string.IsNullOrEmpty(Container))
                {
                    sb.Append("&SegmentContainer=");
                    sb.Append(Container);
                }

                if (SegmentLength.HasValue)
                {
                    sb.Append("&SegmentLength=");
                    sb.Append(SegmentLength.Value.ToString(CultureInfo.InvariantCulture));
                }

                if (MinSegments.HasValue)
                {
                    sb.Append("&MinSegments=");
                    sb.Append(MinSegments.Value.ToString(CultureInfo.InvariantCulture));
                }

                sb.Append("&BreakOnNonKeyFrames=");
                sb.Append(BreakOnNonKeyFrames.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                if (StartPositionTicks != 0)
                {
                    sb.Append("&StartTimeTicks=");
                    sb.Append(StartPositionTicks.ToString(CultureInfo.InvariantCulture));
                }
            }

            if (!string.IsNullOrEmpty(PlaySessionId))
            {
                sb.Append("&PlaySessionId=");
                sb.Append(PlaySessionId);
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                sb.Append("&api_key=");
                sb.Append(accessToken);
            }

            var liveStreamId = MediaSource?.LiveStreamId;
            if (!string.IsNullOrEmpty(liveStreamId))
            {
                sb.Append("&LiveStreamId=");
                sb.Append(liveStreamId);
            }

            if (!IsDirectStream)
            {
                if (RequireNonAnamorphic)
                {
                    sb.Append("&RequireNonAnamorphic=");
                    sb.Append(RequireNonAnamorphic.ToString(CultureInfo.InvariantCulture));
                }

                if (TranscodingMaxAudioChannels.HasValue)
                {
                    sb.Append("&TranscodingMaxAudioChannels=");
                    sb.Append(TranscodingMaxAudioChannels.Value.ToString(CultureInfo.InvariantCulture));
                }

                if (EnableSubtitlesInManifest)
                {
                    sb.Append("&EnableSubtitlesInManifest=");
                    sb.Append(EnableSubtitlesInManifest.ToString(CultureInfo.InvariantCulture));
                }

                if (EnableMpegtsM2TsMode)
                {
                    sb.Append("&EnableMpegtsM2TsMode=");
                    sb.Append(EnableMpegtsM2TsMode.ToString(CultureInfo.InvariantCulture));
                }

                if (EstimateContentLength)
                {
                    sb.Append("&EstimateContentLength=");
                    sb.Append(EstimateContentLength.ToString(CultureInfo.InvariantCulture));
                }

                if (TranscodeSeekInfo != TranscodeSeekInfo.Auto)
                {
                    sb.Append("&TranscodeSeekInfo=");
                    sb.Append(TranscodeSeekInfo.ToString());
                }

                if (CopyTimestamps)
                {
                    sb.Append("&CopyTimestamps=");
                    sb.Append(CopyTimestamps.ToString(CultureInfo.InvariantCulture));
                }

                if (RequireAvc)
                {
                    sb.Append("&RequireAvc=");
                    sb.Append(RequireAvc.ToString(CultureInfo.InvariantCulture));
                }

                if (EnableAudioVbrEncoding)
                {
                    sb.Append("EnableAudioVbrEncoding=");
                    sb.Append(EnableAudioVbrEncoding.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
                }
            }

            var etag = MediaSource?.ETag;
            if (!string.IsNullOrEmpty(etag))
            {
                sb.Append("&Tag=");
                sb.Append(etag);
            }

            if (SubtitleStreamIndex.HasValue && SubtitleDeliveryMethod != SubtitleDeliveryMethod.External)
            {
                sb.Append("&SubtitleMethod=");
                sb.AppendJoin(',', SubtitleDeliveryMethod);
            }

            if (SubtitleStreamIndex.HasValue && SubtitleDeliveryMethod == SubtitleDeliveryMethod.Embed && SubtitleCodecs.Length != 0)
            {
                sb.Append("&SubtitleCodec=");
                sb.AppendJoin(',', SubtitleCodecs);
            }

            foreach (var pair in StreamOptions)
            {
                if (string.IsNullOrEmpty(pair.Value))
                {
                    continue;
                }

                // Strip spaces to avoid having to encode h264 profile names
                sb.Append('&');
                sb.Append(pair.Key);
                sb.Append('=');
                sb.Append(pair.Value.Replace(" ", string.Empty, StringComparison.Ordinal));
            }

            var transcodeReasonsValues = TranscodeReasons.GetUniqueFlags().ToArray();
            if (!IsDirectStream && transcodeReasonsValues.Length > 0)
            {
                sb.Append("&TranscodeReasons=");
                sb.AppendJoin(',', transcodeReasonsValues);
            }

            if (!string.IsNullOrEmpty(query))
            {
                sb.Append(query);
            }

            return sb.ToString();
        }

        public IEnumerable<SubtitleStreamInfo> GetSubtitleProfiles(ITranscoderSupport transcoderSupport, bool includeSelectedTrackOnly, string baseUrl, string? accessToken)
        {
            return GetSubtitleProfiles(transcoderSupport, includeSelectedTrackOnly, false, baseUrl, accessToken);
        }

        public IEnumerable<SubtitleStreamInfo> GetSubtitleProfiles(ITranscoderSupport transcoderSupport, bool includeSelectedTrackOnly, bool enableAllProfiles, string baseUrl, string? accessToken)
        {
            if (MediaSource is null)
            {
                return Enumerable.Empty<SubtitleStreamInfo>();
            }

            var list = new List<SubtitleStreamInfo>();

            // HLS will preserve timestamps so we can just grab the full subtitle stream
            long startPositionTicks = SubProtocol == MediaStreamProtocol.hls
                ? 0
                : (PlayMethod == PlayMethod.Transcode && !CopyTimestamps ? StartPositionTicks : 0);

            // First add the selected track
            if (SubtitleStreamIndex.HasValue)
            {
                foreach (var stream in MediaSource.MediaStreams)
                {
                    if (stream.Type == MediaStreamType.Subtitle && stream.Index == SubtitleStreamIndex.Value)
                    {
                        AddSubtitleProfiles(list, stream, transcoderSupport, enableAllProfiles, baseUrl, accessToken, startPositionTicks);
                    }
                }
            }

            if (!includeSelectedTrackOnly)
            {
                foreach (var stream in MediaSource.MediaStreams)
                {
                    if (stream.Type == MediaStreamType.Subtitle && (!SubtitleStreamIndex.HasValue || stream.Index != SubtitleStreamIndex.Value))
                    {
                        AddSubtitleProfiles(list, stream, transcoderSupport, enableAllProfiles, baseUrl, accessToken, startPositionTicks);
                    }
                }
            }

            return list;
        }

        private void AddSubtitleProfiles(List<SubtitleStreamInfo> list, MediaStream stream, ITranscoderSupport transcoderSupport, bool enableAllProfiles, string baseUrl, string? accessToken, long startPositionTicks)
        {
            if (enableAllProfiles)
            {
                foreach (var profile in DeviceProfile.SubtitleProfiles)
                {
                    var info = GetSubtitleStreamInfo(stream, baseUrl, accessToken, startPositionTicks, new[] { profile }, transcoderSupport);
                    if (info is not null)
                    {
                        list.Add(info);
                    }
                }
            }
            else
            {
                var info = GetSubtitleStreamInfo(stream, baseUrl, accessToken, startPositionTicks, DeviceProfile.SubtitleProfiles, transcoderSupport);
                if (info is not null)
                {
                    list.Add(info);
                }
            }
        }

        private SubtitleStreamInfo? GetSubtitleStreamInfo(MediaStream stream, string baseUrl, string? accessToken, long startPositionTicks, SubtitleProfile[] subtitleProfiles, ITranscoderSupport transcoderSupport)
        {
            if (MediaSource is null)
            {
                return null;
            }

            var subtitleProfile = StreamBuilder.GetSubtitleProfile(MediaSource, stream, subtitleProfiles, PlayMethod, transcoderSupport, Container, SubProtocol);
            var info = new SubtitleStreamInfo
            {
                IsForced = stream.IsForced,
                Language = stream.Language,
                Name = stream.Language ?? "Unknown",
                Format = subtitleProfile.Format,
                Index = stream.Index,
                DeliveryMethod = subtitleProfile.Method,
                DisplayTitle = stream.DisplayTitle
            };

            if (info.DeliveryMethod == SubtitleDeliveryMethod.External)
            {
                if (MediaSource.Protocol == MediaProtocol.File || !string.Equals(stream.Codec, subtitleProfile.Format, StringComparison.OrdinalIgnoreCase) || !stream.IsExternal)
                {
                    info.Url = string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}/Videos/{1}/{2}/Subtitles/{3}/{4}/Stream.{5}",
                        baseUrl,
                        ItemId,
                        MediaSourceId,
                        stream.Index.ToString(CultureInfo.InvariantCulture),
                        startPositionTicks.ToString(CultureInfo.InvariantCulture),
                        subtitleProfile.Format);

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        info.Url += "?api_key=" + accessToken;
                    }

                    info.IsExternalUrl = false;
                }
                else
                {
                    info.Url = stream.Path;
                    info.IsExternalUrl = true;
                }
            }

            return info;
        }

        public int? GetTargetVideoBitDepth(string? codec)
        {
            var value = GetOption(codec, "videobitdepth");

            if (int.TryParse(value, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return null;
        }

        public int? GetTargetAudioBitDepth(string? codec)
        {
            var value = GetOption(codec, "audiobitdepth");

            if (int.TryParse(value, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return null;
        }

        public double? GetTargetVideoLevel(string? codec)
        {
            var value = GetOption(codec, "level");

            if (double.TryParse(value, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return null;
        }

        public int? GetTargetRefFrames(string? codec)
        {
            var value = GetOption(codec, "maxrefframes");

            if (int.TryParse(value, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return null;
        }

        public int? GetTargetAudioChannels(string? codec)
        {
            var defaultValue = GlobalMaxAudioChannels ?? TranscodingMaxAudioChannels;

            var value = GetOption(codec, "audiochannels");
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            {
                return Math.Min(result, defaultValue ?? result);
            }

            return defaultValue;
        }

        private int? GetMediaStreamCount(MediaStreamType type, int limit)
        {
            var count = MediaSource?.GetStreamCount(type);

            if (count.HasValue)
            {
                count = Math.Min(count.Value, limit);
            }

            return count;
        }
    }
}
