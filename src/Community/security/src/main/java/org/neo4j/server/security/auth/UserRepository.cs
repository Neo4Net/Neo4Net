using System.Collections.Generic;

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
namespace Neo4Net.Server.Security.Auth
{

	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using User = Neo4Net.Kernel.impl.security.User;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using ConcurrentModificationException = Neo4Net.Server.Security.Auth.exception.ConcurrentModificationException;

	/// <summary>
	/// A component that can store and retrieve users. Implementations must be thread safe.
	/// </summary>
	public interface UserRepository : Lifecycle
	{
		 /// <summary>
		 /// Clears all cached user data.
		 /// </summary>
		 void Clear();

		 /// <summary>
		 /// Return the user associated with the given username. </summary>
		 /// <param name="username"> the username </param>
		 /// <returns> the associated user, or null if no user exists </returns>
		 User GetUserByName( string username );

		 /// <summary>
		 /// Create a user, given that the users token is unique. </summary>
		 /// <param name="user"> the new user object </param>
		 /// <exception cref="InvalidArgumentsException"> if the username is not valid </exception>
		 /// <exception cref="IOException"> if the underlying storage for users fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void create(org.neo4j.kernel.impl.security.User user) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException;
		 void Create( User user );

		 /// <summary>
		 /// Replaces the users in the repository with the given users. </summary>
		 /// <param name="users"> the new users </param>
		 /// <exception cref="InvalidArgumentsException"> if any username is not valid </exception>
		 /// <exception cref="IOException"> if the underlying storage for users fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setUsers(ListSnapshot<org.neo4j.kernel.impl.security.User> users) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException;
		 ListSnapshot<User> Users { set; }

		 /// <summary>
		 /// Update a user, given that the users token is unique. </summary>
		 /// <param name="existingUser"> the existing user object, which must match the current state in this repository </param>
		 /// <param name="updatedUser"> the updated user object </param>
		 /// <exception cref="ConcurrentModificationException"> if the existingUser does not match the current state in the repository </exception>
		 /// <exception cref="IOException"> if the underlying storage for users fails </exception>
		 /// <exception cref="InvalidArgumentsException"> if the existing and updated users have different names </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void update(org.neo4j.kernel.impl.security.User existingUser, org.neo4j.kernel.impl.security.User updatedUser) throws org.neo4j.server.security.auth.exception.ConcurrentModificationException, java.io.IOException;
		 void Update( User existingUser, User updatedUser );

		 /// <summary>
		 /// Deletes a user. </summary>
		 /// <param name="user"> the user to delete </param>
		 /// <exception cref="IOException"> if the underlying storage for users fails </exception>
		 /// <returns> true if the user was found and deleted </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean delete(org.neo4j.kernel.impl.security.User user) throws java.io.IOException;
		 bool Delete( User user );

		 int NumberOfUsers();

		 /// <summary>
		 /// Asserts whether the given username is valid or not. A valid username is non-null, non-empty, and contains
		 /// only simple ascii characters. </summary>
		 /// <param name="username"> the username to be tested. </param>
		 /// <exception cref="InvalidArgumentsException"> if the username was invalid. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void assertValidUsername(String username) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException;
		 void AssertValidUsername( string username );

		 ISet<string> AllUsernames { get; }

		 /// <summary>
		 /// Returns a snapshot of the current persisted user repository </summary>
		 /// <returns> a snapshot of the current persisted user repository </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ListSnapshot<org.neo4j.kernel.impl.security.User> getPersistedSnapshot() throws java.io.IOException;
		 ListSnapshot<User> PersistedSnapshot { get; }

		 /// <summary>
		 /// Permanently deletes all data in this repository </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void purge() throws java.io.IOException;
		 void Purge();

		 /// <summary>
		 /// Mark this repository as migrated to prevent accidental use. </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void markAsMigrated() throws java.io.IOException;
		 void MarkAsMigrated();
	}

}