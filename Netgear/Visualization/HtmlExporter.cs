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

using Netgear.Parser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Netgear.Visualization
{
    using SwitchTable = TableDefinition<SwitchConfiguration>;
    using SwitchCategory = TableCategoryDefinition<SwitchConfiguration>;
    using SwitchProperty = TablePropertyDefinition<SwitchConfiguration>;
    using InterfaceCategory = TableCategoryDefinition<InterfaceConfiguration>;
    using InterfaceProperty = TablePropertyDefinition<InterfaceConfiguration>;

    public sealed class HtmlExporter
    {
        private static readonly SwitchTable OverviewTable = new SwitchTable(
            new SwitchProperty(string.Empty, c => c.SnmpServerSysName), new [] {
            new SwitchCategory("General", new [] {
                new SwitchProperty("Model", c => c.Model),
                new SwitchProperty("Firmware version", c => c.FirmwareVersion),
                new SwitchProperty("Location", c => c.SnmpServerLocation)
            }),
            new SwitchCategory("Management", new [] {
                new SwitchProperty("Management VLAN", c => c.ManagementInterface.VLAN),
                new SwitchProperty("IPv4 protocol", c => c.ManagementInterface.IPv4.Protocol),
                new SwitchProperty("IPv4 address", c => c.ManagementInterface.IPv4.IPAddress),
                new SwitchProperty("IPv4 netmask", c => c.ManagementInterface.IPv4.NetMask),
                new SwitchProperty("IPv4 gateway", c => c.ManagementInterface.IPv4.DefaultGateway),
                new SwitchProperty("IPv6 enabled", c => c.ManagementInterface.IPv6.Enabled),
                new SwitchProperty("HTTP session soft timeout (minutes)", c => c.ManagementInterface.HttpSessionSoftTimeoutMinutes),
                new SwitchProperty("HTTP session hard timeout (hours)", c => c.ManagementInterface.HttpSessionHardTimeoutHours),
                new SwitchProperty("Java interface enabled", c => c.ManagementInterface.JavaEnabled)
            }),
            new SwitchCategory("DNS", new [] {
                new SwitchProperty("DNS servers", c => c.DnsServers)
            }),
            new SwitchCategory("Time", new [] {
                new SwitchProperty("Clock source", c => c.ClockSource),
                new SwitchProperty("SNTP servers", c => c.SntpServers),
                new SwitchProperty("SNTP client mode", c => c.SntpClientMode)
            }),
            new SwitchCategory("DoS control", new [] {
                new SwitchProperty("Source MAC = destination MAC", c => c.DoSControl.SourceMacEqualsDestinationMacEnabled),
                new SwitchProperty("Source IP = destination IP", c => c.DoSControl.SourceIpEqualsDestinationIpEnabled),
                new SwitchProperty("IPv4 first fragment", c => c.DoSControl.Ipv4FirstFragmentEnabled),
                new SwitchProperty("TCP fragment", c => c.DoSControl.TcpFragmentEnabled),
                new SwitchProperty("TCP flag", c => c.DoSControl.TcpFlagEnabled),
                new SwitchProperty("TCP flag / sequence", c => c.DoSControl.TcpFlagSequenceEnabled),
                new SwitchProperty("TCP FIN/URG/PSH", c => c.DoSControl.TcpFinUrgPshEnabled),
                new SwitchProperty("TCP SYN/FIN", c => c.DoSControl.TcpSynFinEnabled),
                new SwitchProperty("TCP offset", c => c.DoSControl.TcpOffsetEnabled),
                new SwitchProperty("L4 port", c => c.DoSControl.L4PortEnabled),
                new SwitchProperty("ICMPv4 maximum packet size",
                    c => (c.DoSControl.Icmpv4MaxSizeEnabled ? (object)c.DoSControl.Icmpv4MaxSize : c.DoSControl.Icmpv4MaxSizeEnabled)),
                new SwitchProperty("ICMPv6 maximum packet size",
                    c => (c.DoSControl.Icmpv6MaxSizeEnabled == OptionalBoolean.True ? (object)c.DoSControl.Icmpv6MaxSize : c.DoSControl.Icmpv6MaxSizeEnabled))
            }),
            new SwitchCategory("Green Ethernet", new [] {
                new SwitchProperty("Auto power down mode", c => c.GreenMode.EnergyDetectEnabled),
                new SwitchProperty("Short cable mode", c => c.GreenMode.ShortReachEnabled)
            }),
            new SwitchCategory("Services", new [] {
                new SwitchProperty("DHCP filtering / snooping", c => c.Ipv4DhcpFilteringEnabled)
            }),
            new SwitchCategory("Flow control", new [] {
                new SwitchProperty("Globally enabled", c => c.FlowControlEnabled)
            }),
            new SwitchCategory("Voice VLAN", new [] {
                new SwitchProperty("Enabled", c => c.VoiceVlan.Enabled),
                new SwitchProperty("ID", c => c.VoiceVlan.Id)
            }),
            new SwitchCategory("STP", new [] {
                new SwitchProperty("Enabled", c => c.SpanningTree.Enabled),
                new SwitchProperty("Operation mode", c => c.SpanningTree.Version),
                new SwitchProperty("Configuration name", c => c.SpanningTree.Name),
                new SwitchProperty("CST bridge priority", c => c.SpanningTree.CstBridgePriority)
            }),
            new SwitchCategory("QoS", new [] {
                new SwitchProperty("DiffServ enabled", c => c.DiffServEnabled)
            })
        });

        private static readonly InterfaceCategory[] InterfaceTable = new[] {
            new InterfaceCategory("Description", new [] { new InterfaceProperty(string.Empty, i => i.Description) }),
            new InterfaceCategory("Enabled", new [] { new InterfaceProperty(string.Empty, i => i.Enabled) }),
            new InterfaceCategory("MTU", new [] { new InterfaceProperty(string.Empty, i => i.Mtu) }),
            new InterfaceCategory("SNMP", new [] { new InterfaceProperty("Link trap", i => i.SnmpLinkTrap) }),
            new InterfaceCategory("Ingress", new [] {
                new InterfaceProperty("Untagged VLAN", i => i.Vlan.DerivedIngressUntaggedVlan),
                new InterfaceProperty("Untagged priorty", i => i.Vlan.DerivedIngressUntaggedPriority),
                new InterfaceProperty("VLAN filter", i => i.Vlan.IngressFilterEnabled),
                new InterfaceProperty("802.1p trusted", i => i.ClassOfServiceTrusted),
                new InterfaceProperty("DHCP trusted", i => i.Ipv4DhcpServerTrusted)
            }),
            new InterfaceCategory("VLAN membership", new [] { new InterfaceProperty(string.Empty, i => i.Vlan.Membership) }),
            new InterfaceCategory("Egress VLANs", new [] {
                new InterfaceProperty("Tagged", i => i.Vlan.Tagging),
                new InterfaceProperty("Untagged", i => i.Vlan.DerivedEgressUntaggedVlans)
            }),
            new InterfaceCategory("STP", new [] {
                new InterfaceProperty("Enabled", i => i.SpanningTree.Enabled),
                new InterfaceProperty("Auto edge", i => i.SpanningTree.AutoEdge),
                new InterfaceProperty("Fast link", i => i.SpanningTree.FastLink)
            }),
            new InterfaceCategory("Power saving", new [] {
                new InterfaceProperty("Auto off", i => i.GreenMode.EnergyDetectEnabled),
                new InterfaceProperty("Short cable", i => i.GreenMode.ShortReachEnabled)
            })
        };

        private static readonly string Stylesheet;

        static HtmlExporter()
        {
            var self = typeof(HtmlExporter);
            var assembly = Assembly.GetAssembly(self);
            var resourceName = $"{assembly.GetName().Name}.{self.Namespace}.Stylesheet.css";
            var resourceStream = assembly.GetManifestResourceStream(resourceName);
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                Stylesheet = reader.ReadToEnd();
                Stylesheet = Regex.Replace(Stylesheet, @"\/\*.+\*/", string.Empty, RegexOptions.Singleline);
                Stylesheet = Regex.Replace(Stylesheet, @"\s+", " ");
                Stylesheet = Regex.Replace(Stylesheet, @"(( (?<c>[:;,\{\}\>]))|((?<c>[:;,\{\}\>]) ))", "${c}", RegexOptions.ExplicitCapture);
            }
        }

        public HtmlExporter(IList<SwitchConfiguration> configurations)
        {
            m_configurations = configurations;
        }

        public void ExportTo(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
            {
                writer.Write("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\">"
                    + "<link href=\"https://use.fontawesome.com/releases/v5.0.2/css/all.css\" rel=\"stylesheet\">"
                    + "<title>Switch configuration overview</title>"
                    + $"<style>\n{Stylesheet}\n</style>"
                    + "</head><body>");

                writer.Write("<div id=\"title\"><h1>NETGEAR Switch Synoptical Configuration Overview</h1>");
                writer.Write($"<p>Generated on {DateTime.UtcNow:R}</p></div><div id=\"content\">");

                // Overview table
                writer.Write("<div class=\"column\"><h2>Settings overview</h2><table>");

                // Overview table header
                writer.Write("<thead><tr><th colspan=\"2\"></th>");
                foreach (var configuration in m_configurations)
                {
                    writer.Write($"<th>{OverviewTable.Header.Getter(configuration)}</th>");
                }
                writer.Write("</tr></thead>");

                // Overview table body
                writer.Write("<tbody>");
                foreach (var category in OverviewTable.Categories)
                {
                    writer.Write($"<tr><th rowspan=\"{category.Properties.Count}\">{category.Name}</th>");
                    for (int propertyIndex = 0; propertyIndex < category.Properties.Count; ++propertyIndex)
                    {
                        var property = category.Properties[propertyIndex];
                        if (propertyIndex > 0)
                        {
                            writer.Write("</tr><tr>");
                        }
                        writer.Write($"<th>{property.Name}</th>");
                        string prevCellContents = null;
                        int prevCellRepeat = 0;
                        var cells = new List<string>();
                        for (int configurationIndex = 0; configurationIndex <= m_configurations.Count; ++configurationIndex)
                        {
                            string cellContents = null;
                            // We go one past the edge of the array to ensure all cells are written
                            if (configurationIndex < m_configurations.Count)
                            {
                                cellContents = Render((dynamic)property.Getter(m_configurations[configurationIndex]));
                            }
                            if (prevCellContents != null && cellContents != null && prevCellContents == cellContents)
                            {
                                ++prevCellRepeat;
                            }
                            else
                            {
                                if (prevCellContents != null)
                                {
                                    cells.Add($"<td colspan=\"{prevCellRepeat}\">{prevCellContents}</td>");
                                }
                                prevCellContents = cellContents;
                                prevCellRepeat = 1;
                            }
                        }
                        writer.Write(string.Join(string.Empty, cells));
                    }
                    writer.Write("</tr>");
                }
                writer.Write("</tbody></table></div>");

                // Build VLAN database overview
                var vlans = new SortedDictionary<ushort, IDictionary<string, ISet<int>>>();
                {
                    Func<int, SortedSet<int>> newSet = (configurationIndex) => new SortedSet<int>(new [] { configurationIndex });
                    for (int configurationIndex = 0; configurationIndex < m_configurations.Count; ++configurationIndex)
                    {
                        var configuration = m_configurations[configurationIndex];
                        foreach (var vlan in configuration.VlanDatabase)
                        {
                            IDictionary<string, ISet<int>> vlanInfo;
                            if (vlans.TryGetValue(vlan.Key, out vlanInfo))
                            {
                                ISet<int> configurationsWithThisVlan;
                                if (vlanInfo.TryGetValue(vlan.Value, out configurationsWithThisVlan))
                                {
                                    configurationsWithThisVlan.Add(configurationIndex);
                                }
                                else
                                {
                                    vlanInfo.Add(vlan.Value, newSet(configurationIndex));
                                }
                            }
                            else
                            {
                                vlans.Add(vlan.Key, new SortedDictionary<string, ISet<int>> {{ vlan.Value, newSet(configurationIndex) }});
                            }
                        }
                    }
                }

                // VLAN database table
                writer.Write("<div><h2>VLAN database</h2>"
                    + "<table><thead><tr><th>ID</th><th>Description</th>");
                foreach (var configuration in m_configurations)
                {
                    writer.Write($"<th>{OverviewTable.Header.Getter(configuration)}</th>");
                }
                writer.Write("</tr></thead><tbody>");
                foreach (var vlanInfoById in vlans)
                {
                    bool firstRow = true;
                    writer.Write($"<tr><th class=\"numeric\" rowspan=\"{vlanInfoById.Value.Count}\">{vlanInfoById.Key}</th>");
                    foreach (var vlanDescriptionToConfigurations in vlanInfoById.Value)
                    {
                        if (!firstRow)
                        {
                            writer.Write("</tr><tr>");
                        }
                        firstRow = false;
                        writer.Write($"<td>{vlanDescriptionToConfigurations.Key}</td>");
                        string prevCellContents = null;
                        int prevCellRepeat = 0;
                        var cells = new List<string>();
                        for (int configurationIndex = 0; configurationIndex <= m_configurations.Count; ++configurationIndex)
                        {
                            string cellContents = null;
                            // We go one past the edge of the array to ensure all cells are written
                            if (configurationIndex < m_configurations.Count)
                            {
                                cellContents = Render(vlanDescriptionToConfigurations.Value.Contains(configurationIndex));
                            }
                            if (prevCellContents != null && cellContents != null && prevCellContents == cellContents)
                            {
                                ++prevCellRepeat;
                            }
                            else
                            {
                                if (prevCellContents != null)
                                {
                                    cells.Add($"<td colspan=\"{prevCellRepeat}\">{prevCellContents}</td>");
                                }
                                prevCellContents = cellContents;
                                prevCellRepeat = 1;
                            }
                        }
                        writer.Write(string.Join(string.Empty, cells));
                    }
                    writer.Write("</tr>");
                }
                writer.Write("</tbody></table></div>");

                // MAC-based VLANs table
                bool haveMacBasedVlans = false;
                foreach (var configuration in m_configurations)
                {
                    if (configuration.MacBasedVlans.Count > 0)
                    {
                        haveMacBasedVlans = true;
                        break;
                    }
                }
                if (haveMacBasedVlans)
                {
                    writer.Write("<div><h2>MAC-based VLANs</h2>"
                        + "<table><thead><tr><th>MAC Address</th><th>Switch</th><th>VLAN ID</th>");
                    writer.Write("</tr></thead><tbody>");
                    foreach (var configuration in m_configurations)
                    {
                        var switchName = OverviewTable.Header.Getter(configuration);
                        foreach (var entry in configuration.MacBasedVlans)
                        {
                            writer.Write($"<tr><td>{entry.Key}</td>"
                                + $"<td>{switchName}</td>"
                                + $"<td>{entry.Value}</td></tr>");
                        }
                    }
                    writer.Write("</tbody></table></div>");
                }

                // Build interface list
                var interfaceFields = new List<Func<InterfaceConfiguration, object>>();
                writer.Write("<div class=\"wide\"><h2>Interfaces</h2><table><thead>");
                {
                    Func<string, int, int, string> makeHeader = (string text, int colSpan, int rowSpan)
                        => $"<th colspan=\"{colSpan}\" rowspan=\"{rowSpan}\">{text}</th>";
                    writer.Write(makeHeader("Switch", 1, 2));
                    writer.Write(makeHeader("ID", 1, 2));

                    var secondRowHeaders = new List<string>();
                    foreach (var category in InterfaceTable)
                    {
                        int colSpan = category.Properties.Count;
                        int rowSpan = 1;
                        if (colSpan == 1 && string.IsNullOrEmpty(category.Properties[0].Name))
                        {
                            interfaceFields.Add(category.Properties[0].Getter);
                            rowSpan = 2;
                        }
                        else
                        {
                            foreach (var property in category.Properties)
                            {
                                interfaceFields.Add(property.Getter);
                                secondRowHeaders.Add(property.Name);
                            }
                        }
                        writer.Write(makeHeader(category.Name, colSpan, rowSpan));
                    }

                    writer.Write("</tr><tr>");

                    foreach (var secondRowHeader in secondRowHeaders)
                    {
                        writer.Write($"<th>{secondRowHeader}</th>");
                    }
                }
                writer.Write("</tr></thead><tbody>");
                foreach (var configuration in m_configurations)
                {
                    // Check for and sort enabled interfaces for this configuration
                    var switchInterfaces = new SortedList<string, InterfaceConfiguration>();
                    foreach (var switchInterface in configuration.Interfaces.Values)
                    {
                        if (switchInterface.Enabled)
                        {
                            switchInterfaces.Add(switchInterface.SortKey, switchInterface);
                        }
                    }

                    bool firstRow = true;
                    writer.Write($"<tr><th rowspan=\"{switchInterfaces.Count}\">{OverviewTable.Header.Getter(configuration)}</th>");
                    foreach (var switchInterface in switchInterfaces)
                    {
                        if (!firstRow)
                        {
                            writer.Write("</tr><tr>");
                        }
                        firstRow = false;

                        writer.Write($"<th>{switchInterface.Key}</th>");
                        foreach (var getter in interfaceFields)
                        {
                            writer.Write($"<td>{Render((dynamic)getter(switchInterface.Value))}</td>");
                        }
                    }
                    writer.Write("</tr>");
                }
                writer.Write("</tbody></table></div>");

                writer.Write("</div></body></html>");
            }
        }

        private static string Render(object value)
        {
            return value?.ToString() ?? "<i class=\"fas fa-times-circle undefined\"></i>";
        }

        private static string Render(bool value)
        {
            return Render(value ? OptionalBoolean.True : OptionalBoolean.False);
        }

        private static string Render(Enum value)
        {
            if (value == null)
            {
                return Render((object)null);
            }

            var enumType = value.GetType();
            var memberInfo = enumType.GetMember(value.ToString());
            Debug.Assert(memberInfo != null);
            Debug.Assert(memberInfo.Length == 1);

            var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length == 1)
            {
                return ((DescriptionAttribute)attributes[0]).Description;
            }
            attributes = memberInfo[0].GetCustomAttributes(typeof(ConfigurationTokenAttribute), false);
            if (attributes.Length == 1)
            {
                return ((ConfigurationTokenAttribute)attributes[0]).Token;
            }
            return value.ToString();
        }

        private static string Render<T>(ICollection<T> value)
        {
            return value == null || value.Count == 0 ? Render((object)null) : string.Join(", ", value);
        }

        private static string Render(OptionalBoolean value)
        {
            string icon;
            switch (value)
            {
                case OptionalBoolean.NotSupported:
                    icon = "minus";
                    break;
                case OptionalBoolean.False:
                    icon = "times";
                    break;
                case OptionalBoolean.True:
                    icon = "check";
                    break;
                default:
                    return Render((object)null);
            }
            return $"<i class=\"fas fa-{icon}-circle\"></i>";
        }

        private readonly IList<SwitchConfiguration> m_configurations;
    }
}