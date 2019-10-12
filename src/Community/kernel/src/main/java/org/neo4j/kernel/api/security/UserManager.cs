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
namespace Neo4Net.Kernel.api.security
{

	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using User = Neo4Net.Kernel.impl.security.User;

	public interface UserManager
	{

		 /// <summary>
		 /// NOTE: The initialPassword byte array will be cleared (overwritten with zeroes)
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.kernel.impl.security.User newUser(String username, byte[] initialPassword, boolean requirePasswordChange) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException;
		 User NewUser( string username, sbyte[] initialPassword, bool requirePasswordChange );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean deleteUser(String username) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException;
		 bool DeleteUser( string username );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.kernel.impl.security.User getUser(String username) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException;
		 User GetUser( string username );

		 User SilentlyGetUser( string username );

		 /// <summary>
		 /// NOTE: The password byte array will be cleared (overwritten with zeroes)
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setUserPassword(String username, byte[] password, boolean requirePasswordChange) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException;
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
		 public const string INITIAL_USER_NAME = "neo4j";
		 public const string INITIAL_PASSWORD = "neo4j";
	}

}