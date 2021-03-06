﻿/*
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

	using Arguments = Org.Neo4j.Commandline.arguments.Arguments;

	internal class CommandUsage
	{
		 private readonly AdminCommand_Provider _command;
		 private readonly string _scriptName;

		 internal CommandUsage( AdminCommand_Provider command, string scriptName )
		 {
			  this._command = command;
			  this._scriptName = scriptName;
		 }

		 internal virtual void PrintDetailed( System.Action<string> output )
		 {
			  foreach ( Arguments arguments in _command.possibleArguments() )
			  {
					string left = format( "usage: %s %s", _scriptName, _command.name() );

					output( Arguments.rightColumnFormatted( left, arguments.Usage(), left.Length + 1 ) );
			  }
			  output( "" );
			  Usage.PrintEnvironmentVariables( output );
			  output( _command.allArguments().description(_command.description()) );
		 }
	}

}