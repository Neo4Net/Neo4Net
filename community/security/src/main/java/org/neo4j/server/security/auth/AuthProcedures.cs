using System;
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
namespace Org.Neo4j.Server.Security.Auth
{

	using AuthorizationViolationException = Org.Neo4j.Graphdb.security.AuthorizationViolationException;
	using AuthSubject = Org.Neo4j.@internal.Kernel.Api.security.AuthSubject;
	using SecurityContext = Org.Neo4j.@internal.Kernel.Api.security.SecurityContext;
	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;
	using UserManager = Org.Neo4j.Kernel.api.security.UserManager;
	using User = Org.Neo4j.Kernel.impl.security.User;
	using Context = Org.Neo4j.Procedure.Context;
	using Description = Org.Neo4j.Procedure.Description;
	using Name = Org.Neo4j.Procedure.Name;
	using Procedure = Org.Neo4j.Procedure.Procedure;
	using UTF8 = Org.Neo4j.@string.UTF8;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.DBMS;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unused", "WeakerAccess"}) public class AuthProcedures
	public class AuthProcedures
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.internal.kernel.api.security.SecurityContext securityContext;
		 public SecurityContext SecurityContext;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.kernel.api.security.UserManager userManager;
		 public UserManager UserManager;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Create a new user.") @Procedure(name = "dbms.security.createUser", mode = DBMS) public void createUser(@Name("username") String username, @Name("password") String password, @Name(value = "requirePasswordChange", defaultValue = "true") boolean requirePasswordChange) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Create a new user."), Procedure(name : "dbms.security.createUser", mode : DBMS)]
		 public virtual void CreateUser( string username, string password, bool requirePasswordChange )
		 {
			  // TODO: Deprecate this and create a new procedure that takes password as a byte[]
			  SecurityContext.assertCredentialsNotExpired();
			  UserManager.newUser( username, !string.ReferenceEquals( password, null ) ? UTF8.encode( password ) : null, requirePasswordChange );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Delete the specified user.") @Procedure(name = "dbms.security.deleteUser", mode = DBMS) public void deleteUser(@Name("username") String username) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Delete the specified user."), Procedure(name : "dbms.security.deleteUser", mode : DBMS)]
		 public virtual void DeleteUser( string username )
		 {
			  SecurityContext.assertCredentialsNotExpired();
			  if ( SecurityContext.subject().hasUsername(username) )
			  {
					throw new InvalidArgumentsException( "Deleting yourself (user '" + username + "') is not allowed." );
			  }
			  UserManager.deleteUser( username );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Change the current user's password. Deprecated by dbms.security.changePassword.") @Procedure(name = "dbms.changePassword", mode = DBMS, deprecatedBy = "dbms.security.changePassword") public void changePasswordDeprecated(@Name("password") String password) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Obsolete, Description("Change the current user's password. Deprecated by dbms.security.changePassword."), Procedure(name : "dbms.changePassword", mode : DBMS, deprecatedBy : "dbms.security.changePassword")]
		 public virtual void ChangePasswordDeprecated( string password )
		 {
			  // TODO: Deprecate this and create a new procedure that takes password as a byte[]
			  ChangePassword( password );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Change the current user's password.") @Procedure(name = "dbms.security.changePassword", mode = DBMS) public void changePassword(@Name("password") String password) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Change the current user's password."), Procedure(name : "dbms.security.changePassword", mode : DBMS)]
		 public virtual void ChangePassword( string password )
		 {
			  // TODO: Deprecate this and create a new procedure that takes password as a byte[]
			  if ( SecurityContext.subject() == AuthSubject.ANONYMOUS )
			  {
					throw new AuthorizationViolationException( "Anonymous cannot change password" );
			  }
			  UserManager.setUserPassword( SecurityContext.subject().username(), UTF8.encode(password), false );
			  SecurityContext.subject().setPasswordChangeNoLongerRequired();
		 }

		 [Description("Show the current user."), Procedure(name : "dbms.showCurrentUser", mode : DBMS)]
		 public virtual Stream<UserResult> ShowCurrentUser()
		 {
			  return Stream.of( UserResultForName( SecurityContext.subject().username() ) );
		 }

		 [Obsolete, Description("Show the current user. Deprecated by dbms.showCurrentUser."), Procedure(name : "dbms.security.showCurrentUser", mode : DBMS, deprecatedBy : "dbms.showCurrentUser")]
		 public virtual Stream<UserResult> ShowCurrentUserDeprecated()
		 {
			  return ShowCurrentUser();
		 }

		 [Description("List all native users."), Procedure(name : "dbms.security.listUsers", mode : DBMS)]
		 public virtual Stream<UserResult> ListUsers()
		 {
			  SecurityContext.assertCredentialsNotExpired();
			  ISet<string> usernames = UserManager.AllUsernames;

			  if ( usernames.Count == 0 )
			  {
					return ShowCurrentUser();
			  }
			  else
			  {
					return usernames.Select( this.userResultForName );
			  }
		 }

		 private UserResult UserResultForName( string username )
		 {
			  User user = UserManager.silentlyGetUser( username );
			  IEnumerable<string> flags = user == null ? emptyList() : user.Flags;
			  return new UserResult( username, flags );
		 }

		 public class UserResult
		 {
			  public readonly string Username;
			  public readonly IList<string> Flags;

			  internal UserResult( string username, IEnumerable<string> flags )
			  {
					this.Username = username;
					this.Flags = new List<string>();
					foreach ( string f in flags )
					{
						 this.Flags.Add( f );
					}
			  }
		 }
	}

}