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
namespace Neo4Net.CommandLine.Admin.security
{

	using Arguments = Neo4Net.CommandLine.Args.Arguments;

	public class SetDefaultAdminCommandProvider : Neo4Net.CommandLine.Admin.AdminCommand_Provider
	{
		 public SetDefaultAdminCommandProvider() : base(SetDefaultAdminCommand.COMMAND_NAME)
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public Neo4Net.commandline.arguments.Arguments allArguments()
		 public override Arguments AllArguments()
		 {
			  return SetDefaultAdminCommand.Arguments();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public String description()
		 public override string Description()
		 {
			  return "Sets the user to become admin if users but no roles are present, " +
						 "for example when upgrading to Neo4Net 3.1 enterprise.";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public String summary()
		 public override string Summary()
		 {
			  return "Sets the default admin user when no roles are present.";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public Neo4Net.commandline.admin.AdminCommandSection commandSection()
		 public override AdminCommandSection CommandSection()
		 {
			  return AuthenticationCommandSection.Instance();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public Neo4Net.commandline.admin.AdminCommand create(java.nio.file.Path homeDir, java.nio.file.Path configDir, Neo4Net.commandline.admin.OutsideWorld outsideWorld)
		 public override AdminCommand Create( Path homeDir, Path configDir, OutsideWorld outsideWorld )
		 {
			  return new SetDefaultAdminCommand( homeDir, configDir, outsideWorld );
		 }
	}

}