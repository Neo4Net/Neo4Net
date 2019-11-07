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
namespace Neo4Net.Server.Security.Auth
{

	using User = Neo4Net.Kernel.impl.security.User;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.security.auth.ListSnapshot.FROM_MEMORY;

	/// <summary>
	/// A user repository implementation that just stores users in memory </summary>
	public class InMemoryUserRepository : AbstractUserRepository
	{
		 protected internal override void PersistUsers()
		 {
			  // Nothing to do
		 }

		 protected internal override ListSnapshot<User> ReadPersistedUsers()
		 {
			  return null;
		 }

		 public override ListSnapshot<User> PersistedSnapshot
		 {
			 get
			 {
				  return new ListSnapshot<User>( LastLoaded.get(), new List<User>(UsersConflict), FROM_MEMORY );
			 }
		 }
	}

}