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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.dbms.DiagnosticsReportCommand.DEFAULT_CLASSIFIERS;

	public class DiagnosticsReportCommandProvider : Org.Neo4j.Commandline.admin.AdminCommand_Provider
	{
		 public DiagnosticsReportCommandProvider() : base("report")
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.commandline.arguments.Arguments allArguments()
		 public override Arguments AllArguments()
		 {
			  return DiagnosticsReportCommand.AllArguments();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public String summary()
		 public override string Summary()
		 {
			  return "Produces a zip/tar of the most common information needed for remote assessments.";
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
			  return "Will collect information about the system and package everything in an archive. If you specify 'all', " +
						 "everything will be included. You can also fine tune the selection by passing classifiers to the tool, " +
						 "e.g 'logs tx threads'. For a complete list of all available classifiers call the tool with " +
						 "the '--list' flag. If no classifiers are passed, the default list of `" +
						 string.join( " ", DEFAULT_CLASSIFIERS ) + "` will be used.";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.commandline.admin.AdminCommand create(java.nio.file.Path homeDir, java.nio.file.Path configDir, org.neo4j.commandline.admin.OutsideWorld outsideWorld)
		 public override AdminCommand Create( Path homeDir, Path configDir, OutsideWorld outsideWorld )
		 {
			  return new DiagnosticsReportCommand( homeDir, configDir, outsideWorld );
		 }
	}

}