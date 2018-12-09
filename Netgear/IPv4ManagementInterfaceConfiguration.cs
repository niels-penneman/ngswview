/*
 * ngswview: NETGEAR(R) Switch Synoptical Configuration Overview Builder
 * Copyright (C) 2018  Niels Penneman
 *
 * This file is part of ngswview.
 *
 * ngswview is free software: you can redistribute it and/or modify it under the
 * terms of the GNU Affero General Public License as published by the Free
 * Software Foundation, either version 3 of the License, or (at your option) any
 * later version.
 *
 * ngswview is distributed in the hope that it will be useful, but WITHOUT ANY
 * WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR
 * A PARTICULAR PURPOSE. See the GNU Affero General Public License for more
 * details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with ngswview. If not, see <https://www.gnu.org/licenses/>.
 *
 * NETGEAR and ProSAFE are registered trademarks of NETGEAR, Inc. and/or its
 * subsidiaries in the United States and/or other countries.
 */

using System.Net;

namespace Netgear
{
    public sealed class IPv4ManagementInterfaceConfiguration
    {
        public IPAddress DefaultGateway { get; set; } = IPAddress.Parse("0.0.0.0");
        public IPAddress IPAddress { get; set; } = IPAddress.Parse("192.168.0.239");
        public IPAddress NetMask { get; set; } = IPAddress.Parse("255.255.255.0");
        public IPv4AddressingProtocol Protocol { get; set; } = IPv4AddressingProtocol.Dhcp;
    }
}