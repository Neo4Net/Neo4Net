using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Server.security.enterprise.auth
{

	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Neo4Net.Server.Security.Auth;
	using User = Neo4Net.Kernel.impl.security.User;
	using ConcurrentModificationException = Neo4Net.Server.Security.Auth.exception.ConcurrentModificationException;

	/// <summary>
	/// A component that can store and retrieve roles. Implementations must be thread safe.
	/// </summary>
	public interface RoleRepository : Lifecycle
	{
		 RoleRecord GetRoleByName( string roleName );

		 ISet<string> GetRoleNamesByUsername( string username );

		 /// <summary>
		 /// Clears all cached role data.
		 /// </summary>
		 void Clear();

		 /// <summary>
		 /// Create a role, given that the roles token is unique.
		 /// </summary>
		 /// <param name="role"> the new role object </param>
		 /// <exception cref="InvalidArgumentsException"> if the role name is not valid or the role name already exists </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void create(RoleRecord role) throws Neo4Net.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException;
		 void Create( RoleRecord role );

		 /// <summary>
		 /// Replaces the roles in the repository with the given roles. </summary>
		 /// <param name="roles"> the new roles </param>
		 /// <exception cref="InvalidArgumentsException"> if any role name is not valid </exception>
		 /// <exception cref="IOException"> if the underlying storage for roles fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setRoles(Neo4Net.server.security.auth.ListSnapshot<RoleRecord> roles) throws Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
		 ListSnapshot<RoleRecord> Roles { set; }

		 /// <summary>
		 /// Update a role, given that the role token is unique.
		 /// </summary>
		 /// <param name="existingRole"> the existing role object, which must match the current state in this repository </param>
		 /// <param name="updatedRole"> the updated role object </param>
		 /// <exception cref="ConcurrentModificationException"> if the existingRole does not match the current state in the repository </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void update(RoleRecord existingRole, RoleRecord updatedRole) throws Neo4Net.server.security.auth.exception.ConcurrentModificationException, java.io.IOException;
		 void Update( RoleRecord existingRole, RoleRecord updatedRole );

		 /// <summary>
		 /// Deletes a role.
		 /// </summary>
		 /// <param name="role"> the role to delete </param>
		 /// <returns> true if the role was found and deleted </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean delete(RoleRecord role) throws java.io.IOException;
		 bool Delete( RoleRecord role );

		 int NumberOfRoles();

		 /// <summary>
		 /// Asserts whether the given role name is valid or not. A valid role name is non-null, non-empty, and contains
		 /// only simple ascii characters. </summary>
		 /// <param name="roleName"> the role name to be tested. </param>
		 /// <exception cref="InvalidArgumentsException"> if the role name was invalid. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void assertValidRoleName(String roleName) throws Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
		 void AssertValidRoleName( string roleName );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void removeUserFromAllRoles(String username) throws Neo4Net.server.security.auth.exception.ConcurrentModificationException, java.io.IOException;
		 void RemoveUserFromAllRoles( string username );

		 ISet<string> AllRoleNames { get; }

		 /// <summary>
		 /// Returns a snapshot of the current persisted role repository </summary>
		 /// <returns> a snapshot of the current persisted role repository </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Neo4Net.server.security.auth.ListSnapshot<RoleRecord> getPersistedSnapshot() throws java.io.IOException;
		 ListSnapshot<RoleRecord> PersistedSnapshot { get; }

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

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static boolean validate(java.util.List<Neo4Net.kernel.impl.security.User> users, java.util.List<RoleRecord> roles)
	//	 {
	//		  Set<String> usernamesInRoles = roles.stream().flatMap(rr -> rr.users().stream()).collect(Collectors.toSet());
	//		  Set<String> usernameInUsers = users.stream().map(User::name).collect(Collectors.toSet());
	//		  return usernameInUsers.containsAll(usernamesInRoles);
	//	 }
	}

}