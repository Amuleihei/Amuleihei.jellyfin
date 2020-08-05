#nullable enable

using System;
using System.Net;
using System.Net.Sockets;

namespace MediaBrowser.Common.Networking
{
    /// <summary>
    /// An object that holds and IP address and subnet mask.
    /// </summary>
    public class IPNetAddress : IPObject
    {
        /// <summary>
        /// Represents an IPNetAddress that has no value.
        /// </summary>
        public static readonly IPNetAddress None = new IPNetAddress(IPAddress.None);

        /// <summary>
        /// Object's IP address.
        /// </summary>
        private IPAddress _address;

        /// <summary>
        /// Initializes a new instance of the <see cref="IPNetAddress"/> class.
        /// </summary>
        /// <param name="address">Address to assign.</param>
        public IPNetAddress(IPAddress address)
        {
            _address = address ?? throw new ArgumentNullException(nameof(address));
            Mask = IPAddress.Any;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPNetAddress"/> class.
        /// </summary>
        /// <param name="address">Address to assign.</param>
        /// <param name="subnet">Mask to assign.</param>
        public IPNetAddress(IPAddress address, IPAddress subnet)
        {
            _address = address ?? throw new ArgumentNullException(nameof(address));
            Mask = subnet ?? throw new ArgumentNullException(nameof(subnet));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPNetAddress"/> class.
        /// </summary>
        /// <param name="address">IP Address.</param>
        /// <param name="cidr">Mask as a CIDR.</param>
        public IPNetAddress(IPAddress address, byte cidr)
        {
            _address = address ?? throw new ArgumentNullException(nameof(address));
            if (Address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                Mask = IPAddress.Any;
            }
            else
            {
                Mask = CidrToMask(cidr);
            }
        }

        /// <summary>
        /// Gets or sets the object's IP address.
        /// </summary>
        public override IPAddress Address
        {
            get
            {
                return _address;
            }

            set
            {
                _address = value ?? IPAddress.None;
            }
        }

        /// <summary>
        /// Gets the subnet mask of this object.
        /// </summary>
        public override IPAddress Mask { get; }

        /// <summary>
        /// Try to parse the address and subnet strings into an IPNetAddress object.
        /// </summary>
        /// <param name="addr">IP address to parse. Can be CIDR or X.X.X.X notation.</param>
        /// <param name="ip">Resultant object.</param>
        /// <returns>True if the values parsed successfully. False if not, resulting in the IP being null.</returns>
        public static bool TryParse(string addr, out IPNetAddress ip)
        {
            if (!string.IsNullOrEmpty(addr))
            {
                addr = addr.Trim();

                // Try to parse it as is.
                if (IPAddress.TryParse(addr, out IPAddress res))
                {
                    ip = new IPNetAddress(res, 32);
                    return true;
                }

                // Is it a network?
                string[] tokens = addr.Split("/");

                if (tokens.Length == 2)
                {
                    tokens[0] = tokens[0].TrimEnd();
                    tokens[1] = tokens[1].TrimStart();

                    if (IPAddress.TryParse(tokens[0], out res))
                    {
                        if (byte.TryParse(tokens[1], out byte cidr))
                        {
                            ip = new IPNetAddress(res, CidrToMask(cidr));
                            return true;
                        }

                        if (IPAddress.TryParse(tokens[1], out IPAddress mask))
                        {
                            ip = new IPNetAddress(res, mask);
                            return true;
                        }
                    }
                }
            }

            ip = IPNetAddress.None;
            return false;
        }

        /// <summary>
        /// Parses the string provided, throwing an exception if it is badly formed.
        /// </summary>
        /// <param name="addr">String to parse.</param>
        /// <returns>IPNetAddress object.</returns>
        public static IPNetAddress Parse(string addr)
        {
            if (TryParse(addr, out IPNetAddress o))
            {
                return o;
            }

            throw new ArgumentException("Unable to recognise object :" + addr);
        }

        /// <summary>
        /// Compares the address in this object and the address in the object passed as a parameter.
        /// </summary>
        /// <param name="address">Object's IP address to compare to.</param>
        /// <returns>Comparison result.</returns>
        public override bool Contains(IPAddress address)
        {
            IPAddress nwAdd1 = IPObject.NetworkAddress(Address, Mask);
            IPAddress nwAdd2 = IPObject.NetworkAddress(address, Mask);

            return nwAdd1.Equals(nwAdd2) && !nwAdd1.Equals(IPAddress.None);
        }

        /// <summary>
        /// Compares the address in this object and the address in the object passed as a parameter.
        /// </summary>
        /// <param name="address">Object's IP address to compare to.</param>
        /// <returns>Comparison result.</returns>
        public override bool Contains(IPObject address)
        {
            if (address is IPHost addressObj && addressObj.HasAddress)
            {
                foreach (IPAddress addr in addressObj.GetAddresses())
                {
                    if (Contains(addr))
                    {
                        return true;
                    }
                }
            }
            else if (address is IPNetAddress netaddrObj)
            {
                return Contains(netaddrObj.Address);
            }

            return false;
        }

        /// <inheritdoc/>
        public override bool Equals(IPObject other)
        {
            if (other is IPNetAddress otherObj && !Address.Equals(IPAddress.None) && !otherObj.Address.Equals(IPAddress.None))
            {
                if (Address.AddressFamily == otherObj.Address.AddressFamily)
                {
                    // Compare only the address for IPv6, but both Address and Mask for IPv4.

                    if (Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (!Mask.Equals(IPAddress.Any))
                        {
                            // Return true if ipObj is a host and we're a network and the host matches ours.
                            return Address.Equals(otherObj.Address) && Mask.Equals(otherObj.Mask);
                        }

                        return Address.Equals(otherObj.Address);
                    }

                    if (Address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        return Address.Equals(otherObj.Address);
                    }
                }
                else if (Address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    // Is one an ipv4 to ipv6 mapping?
                    return string.Equals(
                        Address.ToString().Replace("::ffff:", string.Empty, StringComparison.OrdinalIgnoreCase),
                        otherObj.ToString(),
                        StringComparison.OrdinalIgnoreCase);
                }
                else if (Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    // Is one an ipv4 to ipv6 mapping?
                    return string.Equals(
                        otherObj.ToString().Replace("::ffff:", string.Empty, StringComparison.OrdinalIgnoreCase),
                        Address.ToString(),
                        StringComparison.OrdinalIgnoreCase);
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public override bool Equals(IPAddress address)
        {
            if (address != null && !address.Equals(IPAddress.None) && !Address.Equals(IPAddress.None))
            {
                if (Address.AddressFamily == address.AddressFamily)
                {
                    return address.Equals(Address);
                }

                if (Address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    // Is one an ipv4 to ipv6 mapping?
                    return string.Equals(
                        Address.ToString().Replace("::ffff:", string.Empty, StringComparison.OrdinalIgnoreCase),
                        address.ToString(),
                        StringComparison.OrdinalIgnoreCase);
                }

                if (Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    // Is one an ipv4 to ipv6 mapping?
                    return string.Equals(
                        address.ToString().Replace("::ffff:", string.Empty, StringComparison.OrdinalIgnoreCase),
                        Address.ToString(),
                        StringComparison.OrdinalIgnoreCase);
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public override bool Exists(IPAddress address)
        {
            if (address != null && Address.Equals(IPAddress.None))
            {
                return Address.Equals(address);
            }

            return false;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString(false);
        }

        /// <summary>
        /// Returns a textual representation of this object.
        /// </summary>
        /// <param name="shortVersion">Set to true, if the subnet is to be included as part of the address.</param>
        /// <returns>String representation of this object.</returns>
        public string ToString(bool shortVersion)
        {
            if (!Address.Equals(IPAddress.None))
            {
                if (Address.Equals(IPAddress.Any))
                {
                    return "Any IP4 Address";
                }

                if (Address.Equals(IPAddress.IPv6Any))
                {
                    return "Any IP6 Address";
                }

                if (Address.Equals(IPAddress.Broadcast))
                {
                    return "All Addreses";
                }

                if (shortVersion)
                {
                    return Address.ToString();
                }

                if (!Mask.Equals(IPAddress.Any))
                {
                    return $"{Address}/" + IPObject.MaskToCidr(Mask);
                }

                return Address.ToString();
            }

            return string.Empty;
        }
    }
}
