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
namespace Org.Neo4j.Commandline.dbms
{

	using AdminCommand = Org.Neo4j.Commandline.admin.AdminCommand;
	using AdminCommandSection = Org.Neo4j.Commandline.admin.AdminCommandSection;
	using OutsideWorld = Org.Neo4j.Commandline.admin.OutsideWorld;
	using Arguments = Org.Neo4j.Commandline.arguments.Arguments;

	public class MemoryRecommendationsCommandProvider : Org.Neo4j.Commandline.admin.AdminCommand_Provider
	{
		 public MemoryRecommendationsCommandProvider() : base("memrec")
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.commandline.arguments.Arguments allArguments()
		 public override Arguments AllArguments()
		 {
			  return MemoryRecommendationsCommand.BuildArgs();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public String summary()
		 public override string Summary()
		 {
			  return "Print Neo4j heap and pagecache memory settings recommendations.";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.commandline.admin.AdminCommandSection commandSection()
		 public override AdminCommandSection CommandSection()
		 {
			  return AdminCommandSection.general();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public String description()
		 public override string Description()
		 {
			  return format( "Print heuristic memory setting recommendations for the Neo4j JVM heap and pagecache. The " + "heuristic is based on the total memory of the system the command is running on, or on the amount of " + "memory specified with the --memory argument. The heuristic assumes that the system is dedicated to " + "running Neo4j. If this is not the case, then use the --memory argument to specify how much memory " + "can be expected to be dedicated to Neo4j.%n" + "%n" + "The output is formatted such that it can be copy-posted into the neo4j.conf file." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.commandline.admin.AdminCommand create(java.nio.file.Path homeDir, java.nio.file.Path configDir, org.neo4j.commandline.admin.OutsideWorld outsideWorld)
		 public override AdminCommand Create( Path homeDir, Path configDir, OutsideWorld outsideWorld )
		 {
			  return new MemoryRecommendationsCommand( homeDir, configDir, outsideWorld );
		 }
	}

}