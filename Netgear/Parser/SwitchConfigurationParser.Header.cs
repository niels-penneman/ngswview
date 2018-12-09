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
using System.Text.RegularExpressions;

namespace Netgear.Parser
{
    partial class SwitchConfigurationParser
    {
        private const string HeaderBegin = "0x4e470x010x00";
        private const string HeaderModelRegex = "GS(108Tv2|7XXT)";
        private const string HeaderVersionRegex = @"[0-9]{1,2}\.[0-9]{1,2}\.[0-9]{1,2}\.[0-9]{1,2}";
        private const string HeaderEnd = "0x000000000x00000000000000";
        private static readonly Regex HeaderRegex = new Regex($"^{HeaderBegin}(?<model>{HeaderModelRegex})( +)(?<version>{HeaderVersionRegex})( )+{HeaderEnd}$", RegexOptions.ExplicitCapture);

        private const string SystemDescription = "!System Description";
        private static readonly Dictionary<string, (string, Type)> SystemDescriptionModels = new Dictionary<string, (string, Type)> {
            { "GS108Tv2", ("GS108Tv2", typeof(GS108Tv2Configuration)) },
            { "GS724Tv4", ("GS7XXT", typeof(GS724Tv4Configuration)) }
        };
        private static readonly string SystemDescriptionModelRegex = string.Join("|", SystemDescriptionModels.Keys);
        private static readonly Regex SystemDescriptionRegex = new Regex($"^{SystemDescription} \"(?<model>{SystemDescriptionModelRegex})( [^\"]+)?\"$");
        

        private const string SystemSoftwareVersion = "!System Software Version";
        private static readonly Regex SystemSoftwareVersionRegex = new Regex($"^{SystemSoftwareVersion} \"(?<version>{HeaderVersionRegex})\"$", RegexOptions.ExplicitCapture);


        private void ParseHeader()
        {
            if (!NextLine(false))
            {
                throw new ParseException("EOF before NSDP Text Configuration header", m_lineNumber);
            }

            var match = HeaderRegex.Match(ConsumeLine());
            if (!match.Success)
            {
                throw new ParseException("NSDP Text Configuration header not found or invalid", m_lineNumber);
            }

            var headerModelName = match.Groups["model"].Value;
            var headerFirmwareVersion = match.Groups["version"].Value;
            var systemDescriptionModelName = ParseSystemDescription();
            var systemSoftwareVersion = ParseSystemSoftwareVersion();

            var (genericModelName, configurationType) = SystemDescriptionModels[systemDescriptionModelName];
            if (genericModelName != headerModelName)
            {
                throw new ParseException("System description model name does not match NSDP Text Configuration header", m_lineNumber);
            }

            if (systemSoftwareVersion != headerFirmwareVersion)
            {
                throw new ParseException("System software version does not match NSDP Text Configuration header", m_lineNumber);
            }

            m_configuration = (SwitchConfiguration)Activator.CreateInstance(configurationType);
            m_configuration.FirmwareVersion = new Version(systemSoftwareVersion);
        }

        private Match ParseSystemComment(Regex regex)
        {
            while (NextLine(false))
            {
                if (m_line[0] != CommentChar)
                {
                    break;
                }
                var match = regex.Match(ConsumeLine());
                if (match.Success)
                {
                    return match;
                }
            }
            return null;
        }

        private string ParseSystemDescription()
        {
            var match = ParseSystemComment(SystemDescriptionRegex) ?? throw new ParseException("System description not found", m_lineNumber);
            return match.Groups["model"].Value;
        }

        private string ParseSystemSoftwareVersion()
        {
            var match = ParseSystemComment(SystemSoftwareVersionRegex) ?? throw new ParseException("System software version not found", m_lineNumber);
            return match.Groups["version"].Value;
        }
    }
}