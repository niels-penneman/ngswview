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
    public sealed class GS108Tv2Configuration : SwitchConfiguration
    {
        public GS108Tv2Configuration() : base("GS108Tv2")
        {
            DoSControl.L4PortEnabled = OptionalBoolean.False;
            DoSControl.TcpFlagEnabled = OptionalBoolean.False;

            SpanningTree.Enabled = false;
            SpanningTree.Version = SpanningTreeVersion.Mstp; // todo check

            VoiceVlan.Enabled = true;
            
            // Default interfaces
            for (int i = 1; i <= 8; ++i)
            {
                var id = $"0/{i}";
                Interfaces.Add(id, new InterfaceConfiguration(id, InterfaceType.Physical));
            }
            for (int i = 1; i <= 4; ++i)
            {
                var id = $"3/{i}";
                Interfaces.Add(id, new InterfaceConfiguration(id, InterfaceType.LinkAggregationGroup));
            }

            // Default VLAN configuration
            VlanDatabase.Add(1, "Default");
            VlanDatabase.Add(2, "Voice VLAN");
            VlanDatabase.Add(3, "Auto-Video");
        }
    }
}