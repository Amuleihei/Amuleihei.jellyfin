﻿namespace Jellyfin.Api.Constants
{
    /// <summary>
    /// Internal claim types for authorization.
    /// </summary>
    public static class InternalClaimTypes
    {
        /// <summary>
        /// User Id.
        /// </summary>
        public const string UserId = "Jellyfin-UserId";

        /// <summary>
        /// Device Id.
        /// </summary>
        public const string DeviceId = "Jellyfin-DeviceId";

        /// <summary>
        /// Device.
        /// </summary>
        public const string Device = "Jellyfin-Device";

        /// <summary>
        /// Client.
        /// </summary>
        public const string Client = "Jellyfin-Client";

        /// <summary>
        /// Version.
        /// </summary>
        public const string Version = "Jellyfin-Version";

        /// <summary>
        /// Token.
        /// </summary>
        public const string Token = "Jellyfin-Token";

        /// <summary>
        /// IP Address.
        /// </summary>
        public const string IPAddress = "Jellyfin-IP";
    }
}
