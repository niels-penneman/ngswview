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
    public sealed class GS724Tv4Configuration : SwitchConfiguration
    {
        public GS724Tv4Configuration() : base("GS724Tv4")
        {
            DoSControl.IcmpFragmentEnabled = OptionalBoolean.False;
            DoSControl.Icmpv6MaxSize = 512;
            DoSControl.Icmpv6MaxSizeEnabled = OptionalBoolean.False;
            DoSControl.SourceMacEqualsDestinationMacEnabled = OptionalBoolean.False;
            DoSControl.TcpFinUrgPshEnabled = OptionalBoolean.False;
            DoSControl.TcpFlagSequenceEnabled = OptionalBoolean.False;
            DoSControl.TcpOffsetEnabled = OptionalBoolean.False;
            DoSControl.TcpSynFinEnabled = OptionalBoolean.False;

            SpanningTree.Enabled = true;
            SpanningTree.Version = SpanningTreeVersion.Rstp;

            VoiceVlan.Enabled = false;

            // Default interfaces
            for (int i = 1; i <= 26; ++i)
            {
                var id = $"g{i}";
                Interfaces.Add(id, SetInterfaceDefaults(new InterfaceConfiguration(id, InterfaceType.Physical)));
            }
            for (int i = 1; i <= 26; ++i)
            {
                var id = $"lag {i}";
                Interfaces.Add(id, SetInterfaceDefaults(new InterfaceConfiguration(id, InterfaceType.LinkAggregationGroup)));
            }

            // Default VLAN configuration
            VlanDatabase.Add(1, "Default");
            VlanDatabase.Add(2, "Auto VoIP");
            VlanDatabase.Add(3, "Auto-Video");
        }

        private InterfaceConfiguration SetInterfaceDefaults(InterfaceConfiguration interfaceConfiguration)
        {
            interfaceConfiguration.SpanningTree.AutoEdge = OptionalBoolean.True;
            interfaceConfiguration.SpanningTree.Enabled = true;
            return interfaceConfiguration;
        }
    }
}