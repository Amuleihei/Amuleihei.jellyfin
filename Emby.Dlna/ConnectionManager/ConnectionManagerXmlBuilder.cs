#pragma warning disable CS1591
#nullable enable
using System.Collections.Generic;
using Emby.Dlna.Common;
using Emby.Dlna.Service;

namespace Emby.Dlna.ConnectionManager
{
    public static class ConnectionManagerXmlBuilder
    {
        public static string GetXml()
        {
            return ServiceXmlBuilder.GetXml(ServiceActionListBuilder.GetActions(), GetStateVariables());
        }

        private static IEnumerable<StateVariable> GetStateVariables()
        {
            var list = new List<StateVariable>
            {
                new StateVariable
                {
                    Name = "SourceProtocolInfo",
                    DataType = "string",
                    SendsEvents = true
                },

                new StateVariable
                {
                    Name = "SinkProtocolInfo",
                    DataType = "string",
                    SendsEvents = true
                },

                new StateVariable
                {
                    Name = "CurrentConnectionIDs",
                    DataType = "string",
                    SendsEvents = true
                },

                new StateVariable
                {
                    Name = "A_ARG_TYPE_ConnectionStatus",
                    DataType = "string",
                    SendsEvents = false,

                    AllowedValues = new string[]
                {
                    "OK",
                    "ContentFormatMismatch",
                    "InsufficientBandwidth",
                    "UnreliableChannel",
                    "Unknown"
                }
                },

                new StateVariable
                {
                    Name = "A_ARG_TYPE_ConnectionManager",
                    DataType = "string",
                    SendsEvents = false
                },

                new StateVariable
                {
                    Name = "A_ARG_TYPE_Direction",
                    DataType = "string",
                    SendsEvents = false,

                    AllowedValues = new string[]
                {
                    "Output",
                    "Input"
                }
                },

                new StateVariable
                {
                    Name = "A_ARG_TYPE_ProtocolInfo",
                    DataType = "string",
                    SendsEvents = false
                },

                new StateVariable
                {
                    Name = "A_ARG_TYPE_ConnectionID",
                    DataType = "ui4",
                    SendsEvents = false
                },

                new StateVariable
                {
                    Name = "A_ARG_TYPE_AVTransportID",
                    DataType = "ui4",
                    SendsEvents = false
                },

                new StateVariable
                {
                    Name = "A_ARG_TYPE_RcsID",
                    DataType = "ui4",
                    SendsEvents = false
                }
            };

            return list;
        }
    }
}
