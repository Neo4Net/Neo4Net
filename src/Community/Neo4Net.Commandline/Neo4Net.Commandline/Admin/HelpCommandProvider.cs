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
namespace Neo4Net.Commandline.Admin
{

	using Arguments = Neo4Net.Commandline.Args.Arguments;

	public class HelpCommandProvider : AdminCommand_Provider
	{
		 private readonly Usage _usage;

		 public HelpCommandProvider( Usage usage ) : base( "help" )
		 {
			  this._usage = usage;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public org.neo4j.commandline.arguments.Arguments allArguments()
		 public override Arguments AllArguments()
		 {
			  return ( new Arguments() ).withOptionalPositionalArgument(0, "command");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public String description()
		 public override string Description()
		 {
			  return "This help text, or help for the command specified in <command>.";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public String summary()
		 public override string Summary()
		 {
			  return Description();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public AdminCommandSection commandSection()
		 public override AdminCommandSection CommandSection()
		 {
			  return AdminCommandSection.General();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public AdminCommand create(java.nio.file.Path homeDir, java.nio.file.Path configDir, OutsideWorld outsideWorld)
		 public override AdminCommand Create( Path homeDir, Path configDir, OutsideWorld outsideWorld )
		 {
			  return new HelpCommand( _usage, outsideWorld.stdOutLine, CommandLocator.fromServiceLocator() );
		 }
	}

}