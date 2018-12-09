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
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using ElapsedEventArgs = System.Timers.ElapsedEventArgs;
using Timer = System.Timers.Timer;

namespace Netgear
{
    static class Program
    {
        private const string ConfigurationFilePattern = "*.conf";

        private static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: exename CONFIGS_DIR OUT_FILE");
                return 1;
            }

            var configsDir = args[0];
            var destinationFileName = args[1];
            if (!Directory.Exists(configsDir))
            {
                s_logger.Fatal($"Directory does not exist: {configsDir}");
                Console.WriteLine($"Error: directory does not exist: {configsDir}");
                return 1;
            }

            try
            {
                using (s_event = new ManualResetEvent(false))
                using (s_timer = new Timer(30000) { AutoReset = false })
                using (var watcher = new FileSystemWatcher(configsDir, ConfigurationFilePattern))
                {
                    Console.CancelKeyPress += OnCancelKeyPressed;
                    s_timer.Elapsed += OnTimerElapsed;
                    watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size;
                    watcher.Changed += OnFileChanged;
                    watcher.Created += OnFileChanged;
                    watcher.Deleted += OnFileChanged;
                    watcher.EnableRaisingEvents = true;
                    do
                    {
                        RebuildOverview(configsDir, destinationFileName);
                        s_event.WaitOne();
                        s_event.Reset();
                    }
                    while (!s_exit);
                }
            }
            catch (Exception e)
            {
                s_logger.Fatal($"Failed to watch directory: {configsDir}: {e.Message}");
                Console.WriteLine($"Error: failed to watch directory: {configsDir}: {e.Message}");
                return 1;
            }

            return 0;
        }

        private static void OnCancelKeyPressed(object sender, ConsoleCancelEventArgs e)
        {
            s_logger.Trace("CTRL-C pressed");
            e.Cancel = true;
            s_exit = true;
            s_event.Set();
        }

        private static void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            s_logger.Trace($"File '{e.Name}' modified; restarting timer");
            s_timer.Stop();
            s_timer.Start();
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            s_logger.Trace("Timer elapsed");
            s_event.Set();
        }

        private static SwitchConfiguration ParseFile(string fileName)
        {
            try
            {
                using (var file = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(file, Encoding.UTF8))
                using (var parser = new SwitchConfigurationParser(reader))
                {
                    return parser.Parse();
                }
            }
            catch (Exception e)
            {
                s_logger.Error($"Failed to parse file: {fileName}: {e.Message}");
                return null;
            }
        }

        private static void RebuildOverview(string configsDir, string destinationFileName)
        {
            try
            {
                var configurations = new SortedList<string, SwitchConfiguration>();
                foreach (var fileName in Directory.EnumerateFiles(configsDir, ConfigurationFilePattern))
                {
                    s_logger.Trace($"Parsing file: {fileName}");
                    var configuration = ParseFile(fileName);
                    if (configuration == null)
                    {
                        s_logger.Trace($"Skipping file: {fileName}");
                    }
                    else
                    {
                        configurations.Add(configuration.SnmpServerSysName, configuration);
                    }
                }

                s_logger.Trace($"Exporting configuration overview to: {destinationFileName}");
                var exporter = new Netgear.Visualization.HtmlExporter(configurations.Values);
                using (var overviewFile = File.Open(destinationFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    exporter.ExportTo(overviewFile);
                }
                s_logger.Info($"Configuration overview exported to: {destinationFileName}");
            }
            catch (Exception ex)
            {
                s_logger.Error($"Failed to enumerate files in: {configsDir}: {ex.Message}");
            }
        }

        private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
        private static ManualResetEvent s_event;
        private static Timer s_timer;
        private static bool s_exit = false;
    }
}
