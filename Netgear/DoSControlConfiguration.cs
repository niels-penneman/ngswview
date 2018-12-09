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
    public sealed class DoSControlConfiguration
    {
        public OptionalBoolean IcmpFragmentEnabled { get; set; } = OptionalBoolean.NotSupported;
        public int Icmpv4MaxSize = 512; // todo check for gs108tv2
        public bool Icmpv4MaxSizeEnabled { get; set; } = false;
        public int? Icmpv6MaxSize = null;
        public OptionalBoolean Icmpv6MaxSizeEnabled { get; set; } = OptionalBoolean.NotSupported;
        public bool Ipv4FirstFragmentEnabled { get; set; } = false;
        public OptionalBoolean L4PortEnabled { get; set; } = OptionalBoolean.NotSupported;
        public bool SourceIpEqualsDestinationIpEnabled { get; set; } = false;
        public OptionalBoolean SourceMacEqualsDestinationMacEnabled { get; set; } = OptionalBoolean.NotSupported;
        public OptionalBoolean TcpFinUrgPshEnabled { get; set; } = OptionalBoolean.NotSupported;
        public OptionalBoolean TcpFlagEnabled { get; set; } = OptionalBoolean.NotSupported;
        public OptionalBoolean TcpFlagSequenceEnabled { get; set; } = OptionalBoolean.NotSupported;
        public bool TcpFragmentEnabled { get; set; } = false;
        public OptionalBoolean TcpOffsetEnabled { get; set; } = OptionalBoolean.NotSupported;
        public OptionalBoolean TcpSynFinEnabled { get; set; } = OptionalBoolean.NotSupported;
    }
}