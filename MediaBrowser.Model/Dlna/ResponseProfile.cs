#pragma warning disable CS1591
#pragma warning disable CA1819 // Properties should not return arrays

using System;
using System.Xml.Serialization;

namespace MediaBrowser.Model.Dlna
{
    public class ResponseProfile
    {
        public ResponseProfile()
        {
            Conditions = Array.Empty<ProfileCondition>();
        }

        [XmlAttribute("container")]
        public string Container { get; set; }

        [XmlAttribute("audioCodec")]
        public string AudioCodec { get; set; }

        [XmlAttribute("videoCodec")]
        public string VideoCodec { get; set; }

        [XmlAttribute("type")]
        public DlnaProfileType Type { get; set; }

        [XmlAttribute("orgPn")]
        public string OrgPn { get; set; }

        [XmlAttribute("mimeType")]
        public string MimeType { get; set; }

        public ProfileCondition[] Conditions { get; set; }

        public string[] GetContainers()
        {
            return ContainerProfile.SplitValue(Container);
        }

        public string[] GetAudioCodecs()
        {
            return ContainerProfile.SplitValue(AudioCodec);
        }

        public string[] GetVideoCodecs()
        {
            return ContainerProfile.SplitValue(VideoCodec);
        }
    }
}
