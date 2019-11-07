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
namespace Neo4Net.Dbms.CommandLine
{

	using AdminCommand = Neo4Net.CommandLine.Admin.AdminCommand;
	using AdminCommandSection = Neo4Net.CommandLine.Admin.AdminCommandSection;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using Arguments = Neo4Net.CommandLine.Args.Arguments;
	using Loader = Neo4Net.Dbms.archive.Loader;

	public class LoadCommandProvider : Neo4Net.CommandLine.Admin.AdminCommand_Provider
	{
		 public LoadCommandProvider() : base("load")
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public Neo4Net.commandline.arguments.Arguments allArguments()
		 public override Arguments AllArguments()
		 {
			  return LoadCommand.Arguments();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public String description()
		 public override string Description()
		 {
			  return "Load a database from an archive. <archive-path> must be an archive created with the dump " +
						 "command. <database> is the name of the database to create. Existing databases can be replaced " +
						 "by specifying --force. It is not possible to replace a database that is mounted in a running " +
						 "Neo4Net server.";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public String summary()
		 public override string Summary()
		 {
			  return "Load a database from an archive created with the dump command.";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public Neo4Net.commandline.admin.AdminCommandSection commandSection()
		 public override AdminCommandSection CommandSection()
		 {
			  return OfflineBackupCommandSection.Instance();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public Neo4Net.commandline.admin.AdminCommand create(java.nio.file.Path homeDir, java.nio.file.Path configDir, Neo4Net.commandline.admin.OutsideWorld outsideWorld)
		 public override AdminCommand Create( Path homeDir, Path configDir, OutsideWorld outsideWorld )
		 {
			  return new LoadCommand( homeDir, configDir, new Loader( outsideWorld.ErrorStream() ) );
		 }
	}

}