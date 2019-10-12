/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Logging.@internal
{

	public sealed class LogMessageUtil
	{
		 private static readonly Pattern _slf4jPlaceholder = Pattern.compile( "\\{}" );

		 private LogMessageUtil()
		 {
		 }

		 /// <summary>
		 /// Replace SLF4J-style placeholders like {@code {}} with <seealso cref="String.format(string, object...)"/> placeholders like {@code %s} in the given string.
		 /// This is nessesary for logging adapters that redirect SLF4J loggers to neo4j <seealso cref="Log"/>. Former uses {@code {}} while later {@code %s}.
		 /// </summary>
		 /// <param name="template"> the message template to modify. </param>
		 /// <returns> new message template which can be safely formatted using <seealso cref="String.format(string, object...)"/>. </returns>
		 public static string Slf4jToStringFormatPlaceholders( string template )
		 {
			  return _slf4jPlaceholder.matcher( template ).replaceAll( "%s" );
		 }
	}

}