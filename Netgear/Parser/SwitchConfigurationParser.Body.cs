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
using System.Net;

namespace Netgear.Parser
{
    using static Netgear.Parser.Utility;

    partial class SwitchConfigurationParser
    {
        private void ParseBodyConfigure()
        {
            Parse("configure", new LineMatcher[] {
                new ExactMatcher("authentication login \"defaultList\"  local", () => {}), /* Ignored */
                new ExactMatcher("clock source SNTP", () => {
                    m_configuration.ClockSource = ClockSource.Sntp;
                }),
                new ExactMatcher("dos-control firstfrag", () => {
                    m_configuration.DoSControl.Ipv4FirstFragmentEnabled = true;
                }),
                new RegexMatcher("^dos-control icmp(v4)?$", _ => {
                    m_configuration.DoSControl.Icmpv4MaxSizeEnabled = true;
                }),
                new ExactMatcher("dos-control icmpfrag", () => {
                    m_configuration.DoSControl.IcmpFragmentEnabled = OptionalBoolean.True;
                }),
                new ExactMatcher("dos-control icmpv6", () => {
                    m_configuration.DoSControl.Icmpv6MaxSizeEnabled = OptionalBoolean.True;
                }),
                new ExactMatcher("dos-control sipdip", () => {
                    m_configuration.DoSControl.SourceIpEqualsDestinationIpEnabled = true;
                }),
                new ExactMatcher("dos-control smacdmac", () => {
                    m_configuration.DoSControl.SourceMacEqualsDestinationMacEnabled = OptionalBoolean.True;
                }),
                new ExactMatcher("dos-control tcpfinurgpsh", () => {
                    m_configuration.DoSControl.TcpFinUrgPshEnabled = OptionalBoolean.True;
                }),
                new ExactMatcher("dos-control tcpflag", () => {
                    m_configuration.DoSControl.TcpFlagEnabled = OptionalBoolean.True;
                }),
                new ExactMatcher("dos-control tcpflagseq", () => {
                    m_configuration.DoSControl.TcpFlagSequenceEnabled = OptionalBoolean.True;
                }),
                new ExactMatcher("dos-control tcpfrag", () => {
                    m_configuration.DoSControl.TcpFragmentEnabled = true;
                }),
                new ExactMatcher("dos-control tcpoffset", () => {
                    m_configuration.DoSControl.TcpOffsetEnabled = OptionalBoolean.True;
                }),
                new ExactMatcher("dos-control tcpsynfin", () => {
                    m_configuration.DoSControl.TcpSynFinEnabled = OptionalBoolean.True;
                }),
                new ExactMatcher("exit", null),
                new ExactMatcher("flowcontrol", () => {
                    m_configuration.FlowControlEnabled = true; // GS724Tv4 only ...
                }),
                new ExactMatcher("green-mode energy-detect", () => {
                    m_configuration.GreenMode.EnergyDetectEnabled = true;
                }),
                new RegexMatcher("^green-mode ((eee)|(short-reach auto))$", _ => {
                    m_configuration.GreenMode.ShortReachEnabled = true;
                }),
                new RegexMatcher(@"^interface (?<id>(([03])\/([1-8])|(g[0-9]{1,2})|(lag [0-9]{1,2})))$", groups => {
                    var id = groups["id"].Value;
                    if (!m_configuration.Interfaces.ContainsKey(id))
                    {
                        throw new ParseException($"Invalid interface ID: {id}", m_lineNumber);
                    }
                    ParseBodyInterface(m_configuration.Interfaces[id]);
                }),
                new RegexMatcher("^ip dhcp (filtering|snooping)$", _ => {
                    m_configuration.Ipv4DhcpFilteringEnabled = true;
                }),
                new RegexMatcher(@"^ip name server(?<servers>( [^\s]+)+)$", groups => {
                    m_configuration.DnsServers = groups["servers"].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                }),
                new ExactMatcher("lineconfig", ParseBodyLineConfig),
                new ExactMatcher("line console", ParseBodyLineConsole),
                new ExactMatcher("line telnet", ParseBodyLineTelnet),
                new RegexMatcher(@"^logging host [^\s]+ ipv4 [0-9]{1,5} info$", _ => {}), /* Ignore */
                new ExactMatcher("logging syslog", () => {}), /* Ignore */
                new ExactMatcher("no diffserv", () => {
                    m_configuration.DiffServEnabled = false;
                }),
                new ExactMatcher("no ip dhcp snooping verify mac-address", () => {}), /* Ignored */
                new RegexMatcher("^no port-channel linktrap lag [0-9]{1,2}$", _ => {}), /* Ignored */
                new RegexMatcher("^no snmp-server community (mode )?(private|public)$", _ => {}), /* Ignored */
                new RegexMatcher("^no snmp-server enable traps( (linkmode|mac|stpmode))?$", _ => {}), /* Ignored */
                new ExactMatcher("no voip status", () => {
                    m_configuration.VoiceVlan.Enabled = false;
                }),
                new RegexMatcher("^snmp-server sysname \"(?<name>[^\"]*)\"$", groups => {
                    m_configuration.SnmpServerSysName = groups["name"].Value;
                }),
                new RegexMatcher("^sntp client mode (?<mode>(broad|uni)cast)$", groups => {
                    m_configuration.SntpClientMode = ParseEnum<SntpClientMode>(groups["mode"].Value, m_lineNumber);
                }),
                new RegexMatcher("^sntp server \"?(?<server>[^\\s\"]+)\"?$", groups => {
                    m_configuration.SntpServers.Add(groups["server"].Value);
                }),
                new ExactMatcher("spanning-tree", () => {
                    m_configuration.SpanningTree.Enabled = true;
                }),
                new RegexMatcher("^spanning-tree configuration name \"(?<name>[^\\s]+)\"$", groups => {
                    m_configuration.SpanningTree.Name = groups["name"].Value;
                }),
                new RegexMatcher(@"^spanning-tree forceversion (?<version>802\.1[dsw])$", groups => {
                    m_configuration.SpanningTree.Version = ParseEnum<SpanningTreeVersion>(groups["version"].Value, m_lineNumber);
                }),
                new RegexMatcher("^spanning-tree mst priority 0 (?<priority>[0-9]+)$", groups => {
                    m_configuration.SpanningTree.CstBridgePriority = ushort.Parse(groups["priority"].Value);
                }),
                new ExactMatcher("storm-control flowcontrol", () => {
                    m_configuration.FlowControlEnabled = true; // GS108Tv2 only ...
                }),
                new RegexMatcher("^users passwd \"admin\" encrypted [0-9a-f]+$", _ => {}), /* Ignored */
                new ExactMatcher("users snmpv3 authentication admin sha", () => {}), /* Ignored */
                new RegexMatcher(@"^voip oui (?:[0-9A-F]{2}:){2}[0-9A-F]{2} desc [^\s]+$", _ => {}) /* Ignored */
            });
        }

        private void ParseBodyInterface(InterfaceConfiguration interfaceConfiguration)
        {
            Parse("interface", new LineMatcher[] {
                new ExactMatcher("classofservice trust untrusted", () => {
                    interfaceConfiguration.ClassOfServiceTrusted = false;
                }),
                new RegexMatcher("description '(?<description>[^']*)'", groups => {
                    interfaceConfiguration.Description = groups["description"].Value;
                }),
                new ExactMatcher("exit", null),
                new ExactMatcher("flowcontrol", () => {}), /* GS724Tv4 only; web interface does not allow per-interface configuration */
                new ExactMatcher("green-mode energy-detect", () => {
                    interfaceConfiguration.GreenMode.EnergyDetectEnabled = true;
                }),
                new RegexMatcher("^green-mode ((eee)|(short-reach auto))$", _ => {
                    interfaceConfiguration.GreenMode.ShortReachEnabled = true;
                }),
                new RegexMatcher("^ip dhcp (filtering|snooping) trust$", _ => {
                    interfaceConfiguration.Ipv4DhcpServerTrusted = true;
                }),
                new ExactMatcher("lacp collector max-delay 0", () => {}), /* Ignore */
                new RegexMatcher("^mtu (?<mtu>[0-9]+)$", groups => {
                    interfaceConfiguration.Mtu = ushort.Parse(groups["mtu"].Value);
                }),
                new ExactMatcher("no adminmode", () => {}), /* Ignore */
                new RegexMatcher("^no lldp ", _ => {}), /* Ignore */
                new ExactMatcher("no snmp trap link-status", () => {
                    interfaceConfiguration.SnmpLinkTrap = false;
                }),
                new ExactMatcher("no spanning-tree auto-edge", () => {
                    interfaceConfiguration.SpanningTree.AutoEdge = OptionalBoolean.False;
                }),
                new ExactMatcher("shutdown", () => {
                    interfaceConfiguration.Enabled = false;
                }),
                new ExactMatcher("spanning-tree edgeport", () => {
                    interfaceConfiguration.SpanningTree.FastLink = true;
                }),
                new ExactMatcher("spanning-tree port mode", () => {
                    interfaceConfiguration.SpanningTree.Enabled = true;
                }),
                new RegexMatcher("^vlan acceptframe (?<mode>(all|vlanonly))$", groups => {
                    interfaceConfiguration.Vlan.AcceptFrame = ParseEnum<InterfaceVlanAcceptFrame>(groups["mode"].Value, m_lineNumber);
                }),
                new ExactMatcher("vlan ingressfilter", () => {
                    interfaceConfiguration.Vlan.IngressFilterEnabled = true;
                }),
                new ExactMatcher("vlan participation auto 1", () => {
                    interfaceConfiguration.Vlan.Membership.Remove(1);
                }),
                new RegexMatcher("^vlan participation include (?<ids>[0-9]{1,4}(?:,[0-9]{1,4})*)$", groups => {
                    var ids = groups["ids"].Value.Split(',');
                    foreach (var id in ids)
                    {
                        interfaceConfiguration.Vlan.Membership.Add(ushort.Parse(id));
                    }
                }),
                new RegexMatcher("^vlan priority (?<priority>[0-7])$", groups => {
                    interfaceConfiguration.Vlan.UntaggedPriority = byte.Parse(groups["priority"].Value);
                }),
                new RegexMatcher("^vlan pvid (?<id>[0-9]{1,4})$", groups => {
                    interfaceConfiguration.Vlan.Pvid = ushort.Parse(groups["id"].Value);
                }),
                new RegexMatcher("^vlan tagging (?<ids>[0-9]{1,4}(?:,[0-9]{1,4})*)$", groups => {
                    var ids = groups["ids"].Value.Split(',');
                    foreach (var idString in ids)
                    {
                        var id = ushort.Parse(idString);
                        if (!interfaceConfiguration.Vlan.Membership.Contains(id))
                        {
                            throw new ParseException($"Interface {interfaceConfiguration.Id} is not a member of VLAN {id}", m_lineNumber);
                        }
                        interfaceConfiguration.Vlan.Tagging.Add(id);
                    }
                })
            });
        }

        private void ParseBodyLineConfig()
        {
            Parse("lineconfig", new LineMatcher[] {
                new ExactMatcher("exit", null)
            });
        }

        private void ParseBodyLineConsole()
        {
            Parse("line console", new LineMatcher[] {
                new ExactMatcher("exit", null),
                new ExactMatcher("serial timeout 0", () => {}) /* Ignore */
            });
        }

        private void ParseBodyLineTelnet()
        {
            Parse("line telnet", new LineMatcher[] {
                new ExactMatcher("exit", null)
            });
        }

        private void ParseBodyVLANDatabase()
        {
            Parse("vlan database", new LineMatcher[] {
                new ExactMatcher("exit", null),
                new RegexMatcher("^vlan (?<ids>[0-9]{1,4}(?:,[0-9]{1,4})*)$", groups => {
                    foreach (var id in groups["ids"].Value.Split(','))
                    {
                        m_configuration.VlanDatabase.Add(ushort.Parse(id), string.Empty);
                    }
                }),
                new RegexMatcher("^vlan name (?<id>[0-9]+) \"(?<name>[^\"]*)\"$", groups => {
                    var id = ushort.Parse(groups["id"].Value);
                    var name = groups["name"].Value;
                    if (!m_configuration.VlanDatabase.ContainsKey(id))
                    {
                        throw new ParseException($"Got name for unknown VLAN {id}: {name}", m_lineNumber);
                    }
                    m_configuration.VlanDatabase[id] = name;
                })
            });
        }

        private void ParseBody()
        {
            Parse(null, new LineMatcher[] {
                new ExactMatcher("configure", ParseBodyConfigure),
                new ExactMatcher("ip http secure-protocol TLS1", () => {}), /* Ignore */
                new RegexMatcher("^ip http session hard-timeout (?<timeout>[0-9]{1,3})$", groups => {
                    m_configuration.ManagementInterface.HttpSessionHardTimeoutHours = int.Parse(groups["timeout"].Value);
                }),
                new RegexMatcher("^ip http session soft-timeout (?<timeout>[0-9]{1,2})$", groups => {
                    m_configuration.ManagementInterface.HttpSessionSoftTimeoutMinutes = int.Parse(groups["timeout"].Value);
                }),
                new RegexMatcher("^network mgmt_vlan (?<vlan>[0-9]+)$", groups => {
                    m_configuration.ManagementInterface.VLAN = ushort.Parse(groups["vlan"].Value);
                }),
                new RegexMatcher(@"^network parms (?<address>[^\s]+) (?<mask>[^\s]+) (?<gateway>[^\s]+)$", groups => {
                    m_configuration.ManagementInterface.IPv4.IPAddress = IPAddress.Parse(groups["address"].Value);
                    m_configuration.ManagementInterface.IPv4.NetMask = IPAddress.Parse(groups["mask"].Value);
                    m_configuration.ManagementInterface.IPv4.DefaultGateway = IPAddress.Parse(groups["gateway"].Value);
                }),
                new RegexMatcher(@"^network protocol (?<protocol>(bootp|dhcp|none))$", groups => {
                    m_configuration.ManagementInterface.IPv4.Protocol = ParseEnum<IPv4AddressingProtocol>(groups["protocol"].Value, m_lineNumber);
                }),
                new ExactMatcher("no ip http java", () => {
                    m_configuration.ManagementInterface.JavaEnabled = false;
                }),
                new ExactMatcher("no network ipv6 enable", () => {
                    m_configuration.ManagementInterface.IPv6.Enabled = false;
                }),
                new ExactMatcher("vlan database", ParseBodyVLANDatabase),
            });
        }
    }
}