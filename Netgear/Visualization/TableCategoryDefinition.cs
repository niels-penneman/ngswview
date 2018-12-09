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

namespace Netgear.Visualization
{
    public sealed class TableCategoryDefinition<T>
    {
        public TableCategoryDefinition(string name, IList<TablePropertyDefinition<T>> properties)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public string Name { get; }
        public IList<TablePropertyDefinition<T>> Properties { get; }
    }
}