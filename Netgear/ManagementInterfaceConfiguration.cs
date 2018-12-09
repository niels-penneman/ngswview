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

namespace Netgear
{
    public sealed class ManagementInterfaceConfiguration
    {
        public int HttpSessionHardTimeoutHours = 24; // not checked gs108tv2
        public int HttpSessionSoftTimeoutMinutes = 5; // not checked gs108tv2
        public bool JavaEnabled { get; set; } = true;
        public ushort VLAN { get; set; } = 1;
        public IPv4ManagementInterfaceConfiguration IPv4 { get; } = new IPv4ManagementInterfaceConfiguration();
        public IPv6ManagementInterfaceConfiguration IPv6 { get; } = new IPv6ManagementInterfaceConfiguration();
    }
}