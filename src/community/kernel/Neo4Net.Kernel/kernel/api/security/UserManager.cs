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
namespace Neo4Net.Kernel.Api.security
{

	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using User = Neo4Net.Kernel.impl.security.User;

	public interface UserManager
	{

		 /// <summary>
		 /// NOTE: The initialPassword byte array will be cleared (overwritten with zeroes)
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Neo4Net.kernel.impl.security.User newUser(String username, byte[] initialPassword, boolean requirePasswordChange) throws java.io.IOException, Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
		 User NewUser( string username, sbyte[] initialPassword, bool requirePasswordChange );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean deleteUser(String username) throws java.io.IOException, Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
		 bool DeleteUser( string username );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Neo4Net.kernel.impl.security.User getUser(String username) throws Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
		 User GetUser( string username );

		 User SilentlyGetUser( string username );

		 /// <summary>
		 /// NOTE: The password byte array will be cleared (overwritten with zeroes)
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setUserPassword(String username, byte[] password, boolean requirePasswordChange) throws java.io.IOException, Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
		 void SetUserPassword( string username, sbyte[] password, bool requirePasswordChange );

		 ISet<string> AllUsernames { get; }

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 UserManager NO_AUTH = new UserManager()
	//	 {
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
	//				return null;
	//		  }
	//	 };
	}

	public static class UserManager_Fields
	{
		 public const string INITIAL_USER_NAME = "Neo4Net";
		 public const string INITIAL_PASSWORD = "Neo4Net";
	}

}