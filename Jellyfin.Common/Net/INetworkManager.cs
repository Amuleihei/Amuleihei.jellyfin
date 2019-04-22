using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.Model.IO;
using Jellyfin.Model.Net;

namespace Jellyfin.Common.Net
{
    public interface INetworkManager
    {
        event EventHandler NetworkChanged;

        /// <summary>
        /// Gets a random port number that is currently available
        /// </summary>
        /// <returns>System.Int32.</returns>
        int GetRandomUnusedTcpPort();

        int GetRandomUnusedUdpPort();

        Func<string[]> LocalSubnetsFn { get; set; }

        /// <summary>
        /// Returns MAC Address from first Network Card in Computer
        /// </summary>
        /// <returns>[string] MAC Address</returns>
        List<string> GetMacAddresses();

        /// <summary>
        /// Determines whether [is in private address space] [the specified endpoint].
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <returns><c>true</c> if [is in private address space] [the specified endpoint]; otherwise, <c>false</c>.</returns>
        bool IsInPrivateAddressSpace(string endpoint);

        /// <summary>
        /// Gets the network shares.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>IEnumerable{NetworkShare}.</returns>
        IEnumerable<NetworkShare> GetNetworkShares(string path);

        /// <summary>
        /// Gets available devices within the domain
        /// </summary>
        /// <returns>PC's in the Domain</returns>
        IEnumerable<FileSystemEntryInfo> GetNetworkDevices();

        /// <summary>
        /// Determines whether [is in local network] [the specified endpoint].
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <returns><c>true</c> if [is in local network] [the specified endpoint]; otherwise, <c>false</c>.</returns>
        bool IsInLocalNetwork(string endpoint);

        IpAddressInfo[] GetLocalIpAddresses(bool ignoreVirtualInterface);

        IpAddressInfo ParseIpAddress(string ipAddress);

        bool TryParseIpAddress(string ipAddress, out IpAddressInfo ipAddressInfo);

        Task<IpAddressInfo[]> GetHostAddressesAsync(string host);

        bool IsAddressInSubnets(string addressString, string[] subnets);

        bool IsInSameSubnet(IpAddressInfo address1, IpAddressInfo address2, IpAddressInfo subnetMask);
        IpAddressInfo GetLocalIpSubnetMask(IpAddressInfo address);
    }
}
