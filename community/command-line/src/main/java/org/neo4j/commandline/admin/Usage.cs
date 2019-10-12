using System.Collections.Generic;

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
namespace Org.Neo4j.Commandline.admin
{

	public class Usage
	{
		 private readonly string _scriptName;
		 private readonly CommandLocator _commands;

		 public Usage( string scriptName, CommandLocator commands )
		 {
			  this._scriptName = scriptName;
			  this._commands = commands;
		 }

		 public virtual void PrintUsageForCommand( AdminCommand_Provider command, System.Action<string> output )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CommandUsage commandUsage = new CommandUsage(command, scriptName);
			  CommandUsage commandUsage = new CommandUsage( command, _scriptName );
			  commandUsage.PrintDetailed( output );
		 }

		 public virtual void Print( System.Action<string> output )
		 {
			  output( format( "usage: %s <command>", _scriptName ) );
			  output( "" );
			  output( "Manage your Neo4j instance." );
			  output( "" );

			  PrintEnvironmentVariables( output );

			  output( "available commands:" );
			  PrintCommands( output );

			  output( "" );
			  output( format( "Use %s help <command> for more details.", _scriptName ) );
		 }

		 internal static void PrintEnvironmentVariables( System.Action<string> output )
		 {
			  output( "environment variables:" );
			  output( "    NEO4J_CONF    Path to directory which contains neo4j.conf." );
			  output( "    NEO4J_DEBUG   Set to anything to enable debug output." );
			  output( "    NEO4J_HOME    Neo4j home directory." );
			  output( "    HEAP_SIZE     Set JVM maximum heap size during command execution." );
			  output( "                  Takes a number and a unit, for example 512m." );
			  output( "" );
		 }

		 private void PrintCommands( System.Action<string> output )
		 {
			  IDictionary<AdminCommandSection, IList<AdminCommand_Provider>> groupedProviders = GroupProvidersBySection();

			  AdminCommandSection.General().printAllCommandsUnderSection(output, groupedProviders.Remove(AdminCommandSection.General()));

			  groupedProviders.SetOfKeyValuePairs().OrderBy(System.Collections.IComparer.comparing(groupedProvider => groupedProvider.Key.printable())).ForEach(entry => entry.Key.printAllCommandsUnderSection(output, entry.Value));
		 }

		 private IDictionary<AdminCommandSection, IList<AdminCommand_Provider>> GroupProvidersBySection()
		 {
			  IList<AdminCommand_Provider> providers = new List<AdminCommand_Provider>();
			  _commands.AllProviders.forEach( providers.add );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return providers.collect( Collectors.groupingBy( AdminCommand_Provider::commandSection ) );
		 }
	}

}