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

using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;


namespace Netgear.Parser
{
    
    public sealed partial class SwitchConfigurationParser : IDisposable
    {
        private const char CommentChar = '!';

        public SwitchConfigurationParser(TextReader textReader)
        {
            m_textReader = textReader ?? throw new ArgumentNullException(nameof(textReader));
        }

        public void Dispose()
        {
            if (m_textReader != null)
            {
                try
                {
                    m_textReader.Dispose();
                }
                catch
                { }
                m_textReader = null;
            }
        }

        private string ConsumeLine()
        {
            var line = m_line;
            m_line = null;
            return line;
        }

        private bool NextLine(bool skipComments = true)
        {
            while (true)
            {
                if (m_line == null)
                {
                    m_line = m_textReader.ReadLine();
                    ++m_lineNumber;
                }
                s_logger.Trace($"Next line: {m_line}");
                if (m_line == null)
                {
                    s_logger.Trace("End of file");
                    return false;
                }
                if (string.IsNullOrWhiteSpace(m_line) || (skipComments && m_line[0] == CommentChar))
                {
                    s_logger.Trace(string.IsNullOrWhiteSpace(m_line) ? "Skipping blank line" : $"Skipping comment: {m_line}");
                    ConsumeLine();
                    continue;
                }
                m_line = m_line.Trim();
                return true;
            };
        }

        public SwitchConfiguration Parse()
        {
            try
            {
                ParseHeader();
                ParseBody();
                Debug.Assert(m_textReader.Read() == -1);
                return m_configuration;
            }
            catch (ParseException e)
            {
                while (e != null)
                {
                    if (e.InnerException is ParseException innerException)
                    {
                        s_logger.Error($"From line {e.LineNumber}: {e.Message}");
                        e = innerException;
                    }
                    else
                    {
                        s_logger.Error($"On line {e.LineNumber}: {e.Message}");
                        break;
                    }
                }
                throw;
            }
        }

        private void Parse(string name, IEnumerable<LineMatcher> matchers)
        {
            var where = name == null ? "at top-level" : $"in '{name}' section";
            while (NextLine())
            {
                var line = ConsumeLine();
                var lineNumber = m_lineNumber;
                foreach (var matcher in matchers)
                {
                    LineMatcher.Result result;
                    try
                    {
                        result = matcher.ActOnMatch(line);
                    }
                    catch (ParseException e)
                    {
                        throw new ParseException($"Error {where}", lineNumber, e);
                    }
                    if (result == LineMatcher.Result.ContinueNextLine)
                    {
                        line = null;
                        break;
                    }
                    if (result == LineMatcher.Result.Exit)
                    {
                        return;
                    }
                }
                if (line != null)
                {
                    throw new ParseException($"Unexpected input {where}: {line}", m_lineNumber);
                }
            }
        }

        private abstract class LineMatcher
        {
            public enum Result
            {
                ContinueNextLine,
                Exit,
                TryNextMatcher              
            }

            public abstract Result ActOnMatch(string line);
        }

        private sealed class ExactMatcher : LineMatcher
        {
            public ExactMatcher(string contents, Action action)
            {
                m_contents = contents;
                m_action = action;
            }

            public override Result ActOnMatch(string line)
            {
                if (line == m_contents)
                {
                    if (m_action == null)
                    {
                        return Result.Exit;
                    }
                    m_action();
                    return Result.ContinueNextLine;
                }
                return Result.TryNextMatcher;
            }

            private readonly string m_contents;
            private readonly Action m_action;
        }

        private sealed class RegexMatcher : LineMatcher
        {
            public RegexMatcher(string expression, Action<GroupCollection> action)
            {
                m_regex = new Regex(expression, RegexOptions.ExplicitCapture);
                m_action = action;
            }

            public override Result ActOnMatch(string line)
            {
                var match = m_regex.Match(line);
                if (!match.Success)
                {
                    return Result.TryNextMatcher;
                }
                if (m_action == null)
                {
                    return Result.Exit;
                }
                m_action(match.Groups);
                return Result.ContinueNextLine;
            }

            private readonly Regex m_regex;
            private readonly Action<GroupCollection> m_action;
        }

        private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
        private TextReader m_textReader;
        private string m_line;
        private int m_lineNumber;
        private SwitchConfiguration m_configuration;
    }
}