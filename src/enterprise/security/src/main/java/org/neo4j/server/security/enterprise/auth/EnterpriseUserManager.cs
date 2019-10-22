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
	using User = Neo4Net.Kernel.impl.security.User;
	using UserManager = Neo4Net.Kernel.api.security.UserManager;

	public interface EnterpriseUserManager : UserManager
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void suspendUser(String username) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
		 void SuspendUser( string username );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void activateUser(String username, boolean requirePasswordChange) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
		 void ActivateUser( string username, bool requirePasswordChange );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void newRole(String roleName, String... usernames) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
		 void NewRole( string roleName, params string[] usernames );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean deleteRole(String roleName) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
		 bool DeleteRole( string roleName );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void assertRoleExists(String roleName) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
		 void AssertRoleExists( string roleName );

		 /// <summary>
		 /// Assign a role to a user. The role and the user have to exist.
		 /// </summary>
		 /// <param name="roleName"> name of role </param>
		 /// <param name="username"> name of user </param>
		 /// <exception cref="InvalidArgumentsException"> if the role does not exist </exception>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void addRoleToUser(String roleName, String username) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
		 void AddRoleToUser( string roleName, string username );

		 /// <summary>
		 /// Unassign a role from a user. The role and the user have to exist.
		 /// </summary>
		 /// <param name="roleName"> name of role </param>
		 /// <param name="username"> name of user </param>
		 /// <exception cref="InvalidArgumentsException"> if the username or the role does not exist </exception>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void removeRoleFromUser(String roleName, String username) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
		 void RemoveRoleFromUser( string roleName, string username );

		 ISet<string> AllRoleNames { get; }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Set<String> getRoleNamesForUser(String username) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
		 ISet<string> GetRoleNamesForUser( string username );

		 ISet<string> SilentlyGetRoleNamesForUser( string username );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Set<String> getUsernamesForRole(String roleName) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
		 ISet<string> GetUsernamesForRole( string roleName );

		 ISet<string> SilentlyGetUsernamesForRole( string roleName );

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 EnterpriseUserManager NOOP = new EnterpriseUserManager()
	//	 {
	//		  @@Override public void suspendUser(String username)
	//		  {
	//		  }
	//
	//		  @@Override public void activateUser(String username, boolean requirePasswordChange)
	//		  {
	//		  }
	//
	//		  @@Override public void newRole(String roleName, String... usernames)
	//		  {
	//		  }
	//
	//		  @@Override public boolean deleteRole(String roleName)
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public void assertRoleExists(String roleName)
	//		  {
	//		  }
	//
	//		  @@Override public void addRoleToUser(String roleName, String username)
	//		  {
	//		  }
	//
	//		  @@Override public void removeRoleFromUser(String roleName, String username)
	//		  {
	//		  }
	//
	//		  @@Override public Set<String> getAllRoleNames()
	//		  {
	//				return emptySet();
	//		  }
	//
	//		  @@Override public Set<String> getRoleNamesForUser(String username)
	//		  {
	//				return emptySet();
	//		  }
	//
	//		  @@Override public Set<String> silentlyGetRoleNamesForUser(String username)
	//		  {
	//				return emptySet();
	//		  }
	//
	//		  @@Override public Set<String> getUsernamesForRole(String roleName)
	//		  {
	//				return emptySet();
	//		  }
	//
	//		  @@Override public Set<String> silentlyGetUsernamesForRole(String roleName)
	//		  {
	//				return emptySet();
	//		  }
	//
	//		  @@Override public User newUser(String username, byte[] initialPassword, boolean requirePasswordChange)
	//		  {
	//				if (initialPassword != null)
	//				{
	//					 Arrays.fill(initialPassword, (byte) 0);
	//				}
	//				return null;
	//		  }
	//
	//		  @@Override public boolean deleteUser(String username)
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public User getUser(String username)
	//		  {
	//				return null;
	//		  }
	//
	//		  @@Override public User silentlyGetUser(String username)
	//		  {
	//				return null;
	//		  }
	//
	//		  @@Override public void setUserPassword(String username, byte[] password, boolean requirePasswordChange)
	//		  {
	//				if (password != null)
	//				{
	//					 Arrays.fill(password, (byte) 0);
	//				}
	//		  }
	//
	//		  @@Override public Set<String> getAllUsernames()
	//		  {
	//				return emptySet();
	//		  }
	//	 };
	}

}