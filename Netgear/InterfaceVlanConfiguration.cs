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

using System.Collections.Generic;

namespace Netgear
{
    public sealed class InterfaceVlanConfiguration
    {
        public InterfaceVlanAcceptFrame AcceptFrame { get; set; } = InterfaceVlanAcceptFrame.All;
        public bool IngressFilterEnabled { get; set; } = false;
        public SortedSet<ushort> Membership { get; } = new SortedSet<ushort>(new ushort[] { 1 });
        public ushort Pvid { get; set; } = 1;
        public SortedSet<ushort> Tagging { get; } = new SortedSet<ushort>();
        public byte UntaggedPriority { get; set; } = 0;

        public bool DerivedAcceptsUntaggedFrames
        {
            get
            {
                return (AcceptFrame == InterfaceVlanAcceptFrame.All && (!IngressFilterEnabled || Membership.Contains(Pvid)));
            }
        }

        public SortedSet<ushort> DerivedEgressUntaggedVlans
        {
            get
            {
                var untaggedVlans = new SortedSet<ushort>(Membership);
                untaggedVlans.ExceptWith(Tagging);
                return untaggedVlans;
            }
        }

        public byte? DerivedIngressUntaggedPriority
        {
            get
            {
                return DerivedAcceptsUntaggedFrames ? UntaggedPriority : (byte?)null;
            }
        }

        public ushort? DerivedIngressUntaggedVlan
        {
            get
            {
                return DerivedAcceptsUntaggedFrames ? Pvid : (ushort?)null;
            }
        }
    }
}