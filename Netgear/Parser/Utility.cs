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
using System.Diagnostics;

namespace Netgear.Parser
{
    internal static class Utility
    {
        public static EnumT ParseEnum<EnumT>(string value, int m_lineNumber) where EnumT : struct, IConvertible
        {
            if (!typeof(EnumT).IsEnum) 
            {
                throw new ArgumentException($"{nameof(EnumT)} must be an enumerated type");
            }

            var enumT = typeof(EnumT);
            foreach (var enumValue in Enum.GetValues(enumT))
            {
                var memberInfo = enumT.GetMember(enumValue.ToString());
                Debug.Assert(memberInfo != null);
                Debug.Assert(memberInfo.Length == 1);

                var attributes = memberInfo[0].GetCustomAttributes(typeof(ConfigurationTokenAttribute), false);
                Debug.Assert(attributes.Length == 1);
                if (((ConfigurationTokenAttribute)attributes[0]).Token == value)
                {
                    return (EnumT)enumValue;
                }
            }
            
            throw new ParseException($"Unknown {enumT.Name} value: {value}", m_lineNumber);
        }
    }
}