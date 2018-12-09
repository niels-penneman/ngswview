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

using System.Text.RegularExpressions;

namespace Netgear
{
    public sealed class InterfaceConfiguration
    {
        public InterfaceConfiguration(string id, InterfaceType interfaceType)
        {
            Description = id;
            Id = id;

            var match = Regex.Match(id, @"^([^0-9]+?)([1-9][0-9]?)$");
            if (match.Success)
            {
                SortKey = $"{match.Groups[1].Value}{int.Parse(match.Groups[2].Value):00}";
            }
            else
            {
                SortKey = id;
            }
        }

        public bool ClassOfServiceTrusted { get; set; } = true;
        public string Description { get; set; }
        public bool Ipv4DhcpServerTrusted { get; set; } = false;
        public bool Enabled { get; set; } = true;
        public GreenModeConfiguration GreenMode { get; } = new GreenModeConfiguration();
        public string Id { get; }
        public ushort Mtu { get; set; } = 1518;
        public bool SnmpLinkTrap { get; set; } = true;
        public string SortKey { get; }
        public InterfaceSpanningTreeConfiguration SpanningTree { get; } = new InterfaceSpanningTreeConfiguration();
        public InterfaceVlanConfiguration Vlan { get; } = new InterfaceVlanConfiguration();
    }
}