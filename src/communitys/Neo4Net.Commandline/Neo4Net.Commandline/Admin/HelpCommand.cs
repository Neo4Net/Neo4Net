using System.Text;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.CommandLine.Admin
{

	public class HelpCommand : AdminCommand
	{

		 private readonly Usage _usage;
		 private readonly System.Action<string> _output;
		 private readonly CommandLocator _locator;

		 internal HelpCommand( Usage usage, System.Action<string> output, CommandLocator locator )
		 {
			  this._usage = usage;
			  this._output = output;
			  this._locator = locator;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute(String... args) throws IncorrectUsage
		 public override void Execute( params string[] args )
		 {
			  if ( args.Length > 0 )
			  {
					try
					{
						 AdminCommand_Provider commandProvider = this._locator.findProvider( args[0] );
						 _usage.printUsageForCommand( commandProvider, _output );
					}
					catch ( NoSuchElementException )
					{
						 StringBuilder validCommands = new StringBuilder();
						 _locator.AllProviders.forEach( commandProvider => validCommands.Append( commandProvider.name() ).Append(" ") );

						 throw new IncorrectUsage( format( "Unknown command: %s. Available commands are: %s\n", args[0], validCommands ) );
					}
			  }
			  else
			  {
					_usage.print( _output );
			  }
		 }
	}

}