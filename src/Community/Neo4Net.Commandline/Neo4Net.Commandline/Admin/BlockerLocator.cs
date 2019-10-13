using System.Collections.Generic;

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

	using Service = Neo4Net.Helpers.Service;

	public interface BlockerLocator
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Iterable<AdminCommand_Blocker> findBlockers(String name) throws java.util.NoSuchElementException;
		 IEnumerable<AdminCommand_Blocker> FindBlockers( string name );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static BlockerLocator fromServiceLocator()
	//	 {
	//		  return commandName ->
	//		  {
	//				ArrayList<AdminCommand.Blocker> blockers = new ArrayList<>();
	//				for (AdminCommand.Blocker blocker : Service.load(AdminCommand.Blocker.class))
	//				{
	//					 if (blocker.commands().contains(commandName))
	//					 {
	//						  blockers.add(blocker);
	//					 }
	//				}
	//				return blockers;
	//		  };
	//	 }
	}

}