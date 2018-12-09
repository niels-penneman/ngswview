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

using System;
using System.Collections.Generic;

namespace Netgear
{
    public abstract class SwitchConfiguration
    {
        protected SwitchConfiguration(string model)
        {
            Model = model;
        }

        public ClockSource ClockSource = ClockSource.Local; // For the GS724T this doesn't seem to be part of the config other than in header comments, or its turned on by setting client mode / configuring a server
        public bool DiffServEnabled { get; set; } = true;
        public IEnumerable<string> DnsServers { get; set; } = null;
        public DoSControlConfiguration DoSControl { get; } = new DoSControlConfiguration();
        public Version FirmwareVersion { get; set; } = null;
        public bool FlowControlEnabled { get; set; } = false;
        public GreenModeConfiguration GreenMode { get; } = new GreenModeConfiguration();
        public IDictionary<string, InterfaceConfiguration> Interfaces { get; } = new Dictionary<string, InterfaceConfiguration>();
        public bool Ipv4DhcpFilteringEnabled { get; set; } = false;
        public ManagementInterfaceConfiguration ManagementInterface { get; } = new ManagementInterfaceConfiguration();
        public string Model { get; }
        public string SnmpServerSysName { get; set; }
        public SntpClientMode SntpClientMode { get; set; } /* DEFAULT?? */
        public IList<string> SntpServers { get; } = new List<String>();
        public SpanningTreeConfiguration SpanningTree { get; } = new SpanningTreeConfiguration();
        public Dictionary<ushort, string> VlanDatabase { get; } = new Dictionary<ushort, string>();
        public VoiceVlanConfiguration VoiceVlan { get; } = new VoiceVlanConfiguration();
    }
}