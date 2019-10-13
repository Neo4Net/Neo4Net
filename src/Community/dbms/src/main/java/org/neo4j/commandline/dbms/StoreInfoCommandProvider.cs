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
namespace Neo4Net.CommandLine.dbms
{

	using AdminCommand = Neo4Net.CommandLine.Admin.AdminCommand;
	using AdminCommandSection = Neo4Net.CommandLine.Admin.AdminCommandSection;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using Arguments = Neo4Net.CommandLine.Args.Arguments;

	public class StoreInfoCommandProvider : Neo4Net.CommandLine.Admin.AdminCommand_Provider
	{
		 public StoreInfoCommandProvider() : base("store-info")
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public org.neo4j.commandline.arguments.Arguments allArguments()
		 public override Arguments AllArguments()
		 {
			  return StoreInfoCommand.Arguments();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public String summary()
		 public override string Summary()
		 {
			  return "Prints information about a Neo4j database store.";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public org.neo4j.commandline.admin.AdminCommandSection commandSection()
		 public override AdminCommandSection CommandSection()
		 {
			  return AdminCommandSection.general();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public String description()
		 public override string Description()
		 {
			  return "Prints information about a Neo4j database store, such as what version of Neo4j created it. Note that " +
						 "this command expects a path to a store directory, for example --store=data/databases/graph.db.";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public org.neo4j.commandline.admin.AdminCommand create(java.nio.file.Path homeDir, java.nio.file.Path configDir, org.neo4j.commandline.admin.OutsideWorld outsideWorld)
		 public override AdminCommand Create( Path homeDir, Path configDir, OutsideWorld outsideWorld )
		 {
			  return new StoreInfoCommand( outsideWorld.stdOutLine );
		 }
	}

}