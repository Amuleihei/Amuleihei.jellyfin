﻿#pragma warning disable SA1402
#pragma warning disable CA1819

using System;
using System.Xml.Serialization;

namespace Jellyfin.NfoMetadata.Models
{
    /// <summary>
    /// The fileinfo nfo tag.
    /// </summary>
    public class FileInfoNfo
    {
        /// <summary>
        /// Gets or sets the streamdetails nfo tag.
        /// </summary>
        [XmlElement("streamdetails")]
        public StreamDetailsNfo? StreamDetails { get; set; }
    }

    /// <summary>
    /// The streamdetails nfo tag.
    /// </summary>
    public class StreamDetailsNfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamDetailsNfo"/> class.
        /// </summary>
        public StreamDetailsNfo()
        {
            Video = Array.Empty<VideoStreamNfo>();
            Audio = Array.Empty<AudioStreamNfo>();
            Subtitle = Array.Empty<SubtitleStreamNfo>();
        }

        /// <summary>
        /// Gets or sets the video nfo tag.
        /// </summary>
        [XmlElement("video")]
        public VideoStreamNfo[] Video { get; set; }

        /// <summary>
        /// Gets or sets the audio nfo tag.
        /// </summary>
        [XmlElement("audio")]
        public AudioStreamNfo[] Audio { get; set; }

        /// <summary>
        /// Gets or sets the subtitle nfo tag.
        /// </summary>
        [XmlElement("subtitle")]
        public SubtitleStreamNfo[] Subtitle { get; set; }
    }

    /// <summary>
    /// The video nfo tag.
    /// </summary>
    public class VideoStreamNfo
    {
        /// <summary>
        /// Gets or sets the audio codec.
        /// </summary>
        [XmlElement("codec")]
        public string? Codec { get; set; }

        /// <summary>
        /// Gets or sets the 3d format.
        /// </summary>
        [XmlElement("format3d")]
        public string? Format3D { get; set; }

        /// <summary>
        /// Gets or sets the video aspect ratio.
        /// </summary>
        [XmlElement("aspect")]
        public string? AspectRatio { get; set; }

        /// <summary>
        /// Gets or sets the video width.
        /// </summary>
        [XmlElement("width")]
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the video height.
        /// </summary>
        [XmlElement("height")]
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets the video duration in minutes.
        /// </summary>
        [XmlElement("duration")]
        public double? Duration { get; set; }

        /// <summary>
        /// Gets or sets the video duration in seconds.
        /// </summary>
        [XmlElement("durationinseconds")]
        public double? DurationInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the video framerate.
        /// </summary>
        [XmlElement("framerate")]
        public float? Framerate { get; set; }

        /// <summary>
        /// Gets or sets the video bitrate.
        /// </summary>
        [XmlElement("bitrate")]
        public int? Bitrate { get; set; }

        /// <summary>
        /// Gets or sets the scan type (interlaced or progressive).
        /// </summary>
        [XmlElement("scantype")]
        public string? Scantype { get; set; }
    }

    /// <summary>
    /// The audio nfo tag.
    /// </summary>
    public class AudioStreamNfo
    {
        /// <summary>
        /// Gets or sets the audio codec.
        /// </summary>
        [XmlElement("codec")]
        public string? Codec { get; set; }

        /// <summary>
        /// Gets or sets the audio channels.
        /// </summary>
        [XmlElement("channels")]
        public int? Channels { get; set; }

        /// <summary>
        /// Gets or sets the language code.
        /// </summary>
        [XmlElement("language")]
        public string? Language { get; set; }

        /// <summary>
        /// Gets or sets the audio sampling rate.
        /// </summary>
        [XmlElement("samplingrate")]
        public int? SamplingRate { get; set; }

        /// <summary>
        /// Gets or sets the audio bitrate.
        /// </summary>
        [XmlElement("bitrate")]
        public int? Bitrate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this stream is the default.
        /// Can't be a bool because old Jellyfin version wrote this value as "True" and the XmlSerializer doesn't recognize that.
        /// </summary>
        [XmlElement("default")]
        public string? Default { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this stream is forced.
        /// Can't be a bool because old Jellyfin version wrote this value as "True" and the XmlSerializer doesn't recognize that.
        /// </summary>
        [XmlElement("forced")]
        public string? Forced { get; set; }
    }

    /// <summary>
    /// The subtitle nfo tag.
    /// </summary>
    public class SubtitleStreamNfo
    {
        /// <summary>
        /// Gets or sets the language code.
        /// </summary>
        [XmlElement("language")]
        public string? Language { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this stream is the default.
        /// Can't be a bool because old Jellyfin version wrote this value as "True" and the XmlSerializer doesn't recognize that.
        /// </summary>
        [XmlElement("default")]
        public string? Default { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this stream is forced.
        /// Can't be a bool because old Jellyfin version wrote this value as "True" and the XmlSerializer doesn't recognize that.
        /// </summary>
        [XmlElement("forced")]
        public string? Forced { get; set; }
    }
}
