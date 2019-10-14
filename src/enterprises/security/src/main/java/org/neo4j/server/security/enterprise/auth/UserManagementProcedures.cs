using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Server.security.enterprise.auth
{

	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using Admin = Neo4Net.Procedure.Admin;
	using Description = Neo4Net.Procedure.Description;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;
	using UTF8 = Neo4Net.Strings.UTF8;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.DBMS;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unused", "WeakerAccess"}) public class UserManagementProcedures extends AuthProceduresBase
	public class UserManagementProcedures : AuthProceduresBase
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Create a new user.") @Procedure(name = "dbms.security.createUser", mode = DBMS) public void createUser(@Name("username") String username, @Name("password") String password, @Name(value = "requirePasswordChange", defaultValue = "true") boolean requirePasswordChange) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Create a new user."), Procedure(name : "dbms.security.createUser", mode : DBMS)]
		 public virtual void CreateUser( string username, string password, bool requirePasswordChange )
		 {
			  // TODO: Deprecate this and create a new procedure that takes password as a byte[]
			  UserManager.newUser( username, !string.ReferenceEquals( password, null ) ? UTF8.encode( password ) : null, requirePasswordChange );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Change the current user's password. Deprecated by dbms.security.changePassword.") @Procedure(name = "dbms.changePassword", mode = DBMS, deprecatedBy = "dbms.security.changePassword") public void changePasswordDeprecated(@Name("password") String password) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Obsolete, Description("Change the current user's password. Deprecated by dbms.security.changePassword."), Procedure(name : "dbms.changePassword", mode : DBMS, deprecatedBy : "dbms.security.changePassword")]
		 public virtual void ChangePasswordDeprecated( string password )
		 {
			  // TODO: Deprecate this and create a new procedure that takes password as a byte[]
			  ChangePassword( password, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Change the current user's password.") @Procedure(name = "dbms.security.changePassword", mode = DBMS) public void changePassword(@Name("password") String password, @Name(value = "requirePasswordChange", defaultValue = "false") boolean requirePasswordChange) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Change the current user's password."), Procedure(name : "dbms.security.changePassword", mode : DBMS)]
		 public virtual void ChangePassword( string password, bool requirePasswordChange )
		 {
			  // TODO: Deprecate this and create a new procedure that takes password as a byte[]
			  SetUserPassword( SecurityContext.subject().username(), password, requirePasswordChange );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Change the given user's password.") @Procedure(name = "dbms.security.changeUserPassword", mode = DBMS) public void changeUserPassword(@Name("username") String username, @Name("newPassword") String newPassword, @Name(value = "requirePasswordChange", defaultValue = "true") boolean requirePasswordChange) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Change the given user's password."), Procedure(name : "dbms.security.changeUserPassword", mode : DBMS)]
		 public virtual void ChangeUserPassword( string username, string newPassword, bool requirePasswordChange )
		 {
			  // TODO: Deprecate this and create a new procedure that takes password as a byte[]
			  SecurityContext.assertCredentialsNotExpired();
			  SetUserPassword( username, newPassword, requirePasswordChange );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Assign a role to the user.") @Procedure(name = "dbms.security.addRoleToUser", mode = DBMS) public void addRoleToUser(@Name("roleName") String roleName, @Name("username") String username) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Assign a role to the user."), Procedure(name : "dbms.security.addRoleToUser", mode : DBMS)]
		 public virtual void AddRoleToUser( string roleName, string username )
		 {
			  UserManager.addRoleToUser( roleName, username );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Unassign a role from the user.") @Procedure(name = "dbms.security.removeRoleFromUser", mode = DBMS) public void removeRoleFromUser(@Name("roleName") String roleName, @Name("username") String username) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Unassign a role from the user."), Procedure(name : "dbms.security.removeRoleFromUser", mode : DBMS)]
		 public virtual void RemoveRoleFromUser( string roleName, string username )
		 {
			  UserManager.removeRoleFromUser( roleName, username );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Delete the specified user.") @Procedure(name = "dbms.security.deleteUser", mode = DBMS) public void deleteUser(@Name("username") String username) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Delete the specified user."), Procedure(name : "dbms.security.deleteUser", mode : DBMS)]
		 public virtual void DeleteUser( string username )
		 {
			  if ( UserManager.deleteUser( username ) )
			  {
					KickoutUser( username, "deletion" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Suspend the specified user.") @Procedure(name = "dbms.security.suspendUser", mode = DBMS) public void suspendUser(@Name("username") String username) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Suspend the specified user."), Procedure(name : "dbms.security.suspendUser", mode : DBMS)]
		 public virtual void SuspendUser( string username )
		 {
			  UserManager.suspendUser( username );
			  KickoutUser( username, "suspension" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Activate a suspended user.") @Procedure(name = "dbms.security.activateUser", mode = DBMS) public void activateUser(@Name("username") String username, @Name(value = "requirePasswordChange", defaultValue = "true") boolean requirePasswordChange) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Activate a suspended user."), Procedure(name : "dbms.security.activateUser", mode : DBMS)]
		 public virtual void ActivateUser( string username, bool requirePasswordChange )
		 {
			  UserManager.activateUser( username, requirePasswordChange );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("List all local users.") @Procedure(name = "dbms.security.listUsers", mode = DBMS) public java.util.stream.Stream<UserResult> listUsers()
		 [Description("List all local users."), Procedure(name : "dbms.security.listUsers", mode : DBMS)]
		 public virtual Stream<UserResult> ListUsers()
		 {
			  ISet<string> users = UserManager.AllUsernames;
			  if ( users.Count == 0 )
			  {
					return Stream.of( UserResultForSubject() );
			  }
			  else
			  {
					return users.Select( this.userResultForName );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("List all available roles.") @Procedure(name = "dbms.security.listRoles", mode = DBMS) public java.util.stream.Stream<RoleResult> listRoles()
		 [Description("List all available roles."), Procedure(name : "dbms.security.listRoles", mode : DBMS)]
		 public virtual Stream<RoleResult> ListRoles()
		 {
			  ISet<string> roles = UserManager.AllRoleNames;
			  return roles.Select( this.roleResultForName );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("List all roles assigned to the specified user.") @Procedure(name = "dbms.security.listRolesForUser", mode = DBMS) public java.util.stream.Stream<StringResult> listRolesForUser(@Name("username") String username) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("List all roles assigned to the specified user."), Procedure(name : "dbms.security.listRolesForUser", mode : DBMS)]
		 public virtual Stream<StringResult> ListRolesForUser( string username )
		 {
			  SecurityContext.assertCredentialsNotExpired();
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return UserManager.getRoleNamesForUser( username ).Select( StringResult::new );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("List all users currently assigned the specified role.") @Procedure(name = "dbms.security.listUsersForRole", mode = DBMS) public java.util.stream.Stream<StringResult> listUsersForRole(@Name("roleName") String roleName) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("List all users currently assigned the specified role."), Procedure(name : "dbms.security.listUsersForRole", mode : DBMS)]
		 public virtual Stream<StringResult> ListUsersForRole( string roleName )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return UserManager.getUsernamesForRole( roleName ).Select( StringResult::new );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Create a new role.") @Procedure(name = "dbms.security.createRole", mode = DBMS) public void createRole(@Name("roleName") String roleName) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Create a new role."), Procedure(name : "dbms.security.createRole", mode : DBMS)]
		 public virtual void CreateRole( string roleName )
		 {
			  UserManager.newRole( roleName );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Delete the specified role. Any role assignments will be removed.") @Procedure(name = "dbms.security.deleteRole", mode = DBMS) public void deleteRole(@Name("roleName") String roleName) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Delete the specified role. Any role assignments will be removed."), Procedure(name : "dbms.security.deleteRole", mode : DBMS)]
		 public virtual void DeleteRole( string roleName )
		 {
			  UserManager.deleteRole( roleName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setUserPassword(String username, String newPassword, boolean requirePasswordChange) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 private void SetUserPassword( string username, string newPassword, bool requirePasswordChange )
		 {
			  UserManager.setUserPassword( username, !string.ReferenceEquals( newPassword, null ) ? UTF8.encode( newPassword ) : null, requirePasswordChange );
			  if ( SecurityContext.subject().hasUsername(username) )
			  {
					SecurityContext.subject().setPasswordChangeNoLongerRequired();
			  }
		 }
	}

}