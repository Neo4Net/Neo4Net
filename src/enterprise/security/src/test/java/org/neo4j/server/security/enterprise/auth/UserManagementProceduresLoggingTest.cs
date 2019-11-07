using System;

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
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Neo4Net.Functions;
	using AuthorizationViolationException = Neo4Net.GraphDb.security.AuthorizationViolationException;
	using AccessMode = Neo4Net.Kernel.Api.Internal.security.AccessMode;
	using AuthSubject = Neo4Net.Kernel.Api.Internal.security.AuthSubject;
	using AuthenticationResult = Neo4Net.Kernel.Api.Internal.security.AuthenticationResult;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using EnterpriseSecurityContext = Neo4Net.Kernel.enterprise.api.security.EnterpriseSecurityContext;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using AuthenticationStrategy = Neo4Net.Server.Security.Auth.AuthenticationStrategy;
	using BasicPasswordPolicy = Neo4Net.Server.Security.Auth.BasicPasswordPolicy;
	using InMemoryUserRepository = Neo4Net.Server.Security.Auth.InMemoryUserRepository;
	using SecurityLog = Neo4Net.Server.security.enterprise.log.SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.security.AuthorizationViolationException.PERMISSION_DENIED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.security.enterprise.auth.plugin.api.PredefinedRoles.ADMIN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.security.enterprise.auth.plugin.api.PredefinedRoles.ARCHITECT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.security.enterprise.auth.plugin.api.PredefinedRoles.READER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.assertion.Assert.assertException;

	public class UserManagementProceduresLoggingTest
	{
		 protected internal TestUserManagementProcedures AuthProcedures;
		 private AssertableLogProvider _log;
		 private EnterpriseSecurityContext _matsContext;
		 private EnterpriseUserManager _generalUserManager;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _log = new AssertableLogProvider();
			  SecurityLog securityLog = new SecurityLog( _log.getLog( this.GetType() ) );

			  AuthProcedures = new TestUserManagementProcedures();
			  AuthProcedures.graph = mock( typeof( GraphDatabaseAPI ) );
			  AuthProcedures.securityLog = securityLog;

			  _generalUserManager = UserManager;
			  EnterpriseSecurityContext adminContext = new EnterpriseSecurityContext( new MockAuthSubject( "admin" ), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.Full, Collections.emptySet(), true );
			  _matsContext = new EnterpriseSecurityContext( new MockAuthSubject( "mats" ), Neo4Net.Kernel.Api.Internal.security.AccessMode_Static.None, Collections.emptySet(), false );

			  Subject = adminContext;
			  _log.clear();
		 }

		 private EnterpriseSecurityContext Subject
		 {
			 set
			 {
				  AuthProcedures.securityContext = value;
				  AuthProcedures.userManager = new PersonalUserManager( _generalUserManager, value.Subject(), AuthProcedures.securityLog, value.Admin );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected EnterpriseUserManager getUserManager() throws Throwable
		 protected internal virtual EnterpriseUserManager UserManager
		 {
			 get
			 {
				  InternalFlatFileRealm realm = new InternalFlatFileRealm(new InMemoryUserRepository(), new InMemoryRoleRepository(), new BasicPasswordPolicy(), mock(typeof(AuthenticationStrategy)), mock(typeof(JobScheduler)), new InMemoryUserRepository(), new InMemoryUserRepository()
															);
				  realm.Start(); // creates default user and roles
				  return realm;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogCreatingUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogCreatingUser()
		 {
			  AuthProcedures.createUser( "andres", "el password", true );
			  AuthProcedures.createUser( "mats", "el password", false );

			  _log.assertExactly( Info( "[admin]: created user `%s`%s", "andres", ", with password change required" ), Info( "[admin]: created user `%s`%s", "mats", "" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailureToCreateUser()
		 public virtual void ShouldLogFailureToCreateUser()
		 {
			  CatchInvalidArguments( () => AuthProcedures.createUser(null, "pw", true) );
			  CatchInvalidArguments( () => AuthProcedures.createUser("", "pw", true) );
			  CatchInvalidArguments( () => AuthProcedures.createUser("andres", "", true) );
			  CatchInvalidArguments( () => AuthProcedures.createUser("mats", null, true) );
			  CatchInvalidArguments( () => AuthProcedures.createUser("Neo4Net", "nonEmpty", true) );

			  _log.assertExactly( Error( "[admin]: tried to create user `%s`: %s", null, "The provided username is empty." ), Error( "[admin]: tried to create user `%s`: %s", "", "The provided username is empty." ), Error( "[admin]: tried to create user `%s`: %s", "andres", "A password cannot be empty." ), Error( "[admin]: tried to create user `%s`: %s", "mats", "A password cannot be empty." ), Error( "[admin]: tried to create user `%s`: %s", "Neo4Net", "The specified user 'Neo4Net' already exists." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUnauthorizedCreatingUser()
		 public virtual void ShouldLogUnauthorizedCreatingUser()
		 {
			  Subject = _matsContext;
			  CatchAuthorizationViolation( () => AuthProcedures.createUser("andres", "", true) );

			  _log.assertExactly( Error( "[mats]: tried to create user `%s`: %s", "andres", PERMISSION_DENIED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogDeletingUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogDeletingUser()
		 {
			  AuthProcedures.createUser( "andres", "el password", false );
			  AuthProcedures.deleteUser( "andres" );

			  _log.assertExactly( Info( "[admin]: created user `%s`%s", "andres", "" ), Info( "[admin]: deleted user `%s`", "andres" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogDeletingNonExistentUser()
		 public virtual void ShouldLogDeletingNonExistentUser()
		 {
			  CatchInvalidArguments( () => AuthProcedures.deleteUser("andres") );

			  _log.assertExactly( Error( "[admin]: tried to delete user `%s`: %s", "andres", "User 'andres' does not exist." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUnauthorizedDeleteUser()
		 public virtual void ShouldLogUnauthorizedDeleteUser()
		 {
			  Subject = _matsContext;
			  CatchAuthorizationViolation( () => AuthProcedures.deleteUser(ADMIN) );

			  _log.assertExactly( Error( "[mats]: tried to delete user `%s`: %s", ADMIN, PERMISSION_DENIED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogAddingRoleToUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogAddingRoleToUser()
		 {
			  AuthProcedures.createUser( "mats", "Neo4Net", false );
			  AuthProcedures.addRoleToUser( ARCHITECT, "mats" );

			  _log.assertExactly( Info( "[admin]: created user `%s`%s", "mats", "" ), Info( "[admin]: added role `%s` to user `%s`", ARCHITECT, "mats" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailureToAddRoleToUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogFailureToAddRoleToUser()
		 {
			  AuthProcedures.createUser( "mats", "Neo4Net", false );
			  CatchInvalidArguments( () => AuthProcedures.addRoleToUser("null", "mats") );

			  _log.assertExactly( Info( "[admin]: created user `%s`%s", "mats", "" ), Error( "[admin]: tried to add role `%s` to user `%s`: %s", "null", "mats", "Role 'null' does not exist." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUnauthorizedAddingRole()
		 public virtual void ShouldLogUnauthorizedAddingRole()
		 {
			  Subject = _matsContext;
			  CatchAuthorizationViolation( () => AuthProcedures.addRoleToUser(ADMIN, "mats") );

			  _log.assertExactly( Error( "[mats]: tried to add role `%s` to user `%s`: %s", ADMIN, "mats", PERMISSION_DENIED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogRemovalOfRoleFromUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogRemovalOfRoleFromUser()
		 {
			  // Given
			  AuthProcedures.createUser( "mats", "Neo4Net", false );
			  AuthProcedures.addRoleToUser( READER, "mats" );
			  _log.clear();

			  // When
			  AuthProcedures.removeRoleFromUser( READER, "mats" );

			  // Then
			  _log.assertExactly( Info( "[admin]: removed role `%s` from user `%s`", READER, "mats" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailureToRemoveRoleFromUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogFailureToRemoveRoleFromUser()
		 {
			  // Given
			  AuthProcedures.createUser( "mats", "Neo4Net", false );
			  AuthProcedures.addRoleToUser( READER, "mats" );
			  _log.clear();

			  // When
			  CatchInvalidArguments( () => AuthProcedures.removeRoleFromUser("notReader", "mats") );
			  CatchInvalidArguments( () => AuthProcedures.removeRoleFromUser(READER, "notMats") );

			  // Then
			  _log.assertExactly( Error( "[admin]: tried to remove role `%s` from user `%s`: %s", "notReader", "mats", "Role 'notReader' does not exist." ), Error( "[admin]: tried to remove role `%s` from user `%s`: %s", READER, "notMats", "User 'notMats' does not exist." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUnauthorizedRemovingRole()
		 public virtual void ShouldLogUnauthorizedRemovingRole()
		 {
			  Subject = _matsContext;
			  CatchAuthorizationViolation( () => AuthProcedures.removeRoleFromUser(ADMIN, ADMIN) );

			  _log.assertExactly( Error( "[mats]: tried to remove role `%s` from user `%s`: %s", ADMIN, ADMIN, PERMISSION_DENIED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUserPasswordChanges() throws java.io.IOException, Neo4Net.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogUserPasswordChanges()
		 {
			  // Given
			  AuthProcedures.createUser( "mats", "Neo4Net", true );
			  _log.clear();

			  // When
			  AuthProcedures.changeUserPassword( "mats", "longPassword", false );
			  AuthProcedures.changeUserPassword( "mats", "longerPassword", true );

			  Subject = _matsContext;
			  AuthProcedures.changeUserPassword( "mats", "evenLongerPassword", false );

			  AuthProcedures.changePassword( "superLongPassword", false );
			  AuthProcedures.changePassword( "infinitePassword", true );

			  // Then
			  _log.assertExactly( Info( "[admin]: changed password for user `%s`%s", "mats", "" ), Info( "[admin]: changed password for user `%s`%s", "mats", ", with password change required" ), Info( "[mats]: changed password%s", "" ), Info( "[mats]: changed password%s", "" ), Info( "[mats]: changed password%s", ", with password change required" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailureToChangeUserPassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogFailureToChangeUserPassword()
		 {
			  // Given
			  AuthProcedures.createUser( "andres", "Neo4Net", true );
			  _log.clear();

			  // When
			  CatchInvalidArguments( () => AuthProcedures.changeUserPassword("andres", "Neo4Net", false) );
			  CatchInvalidArguments( () => AuthProcedures.changeUserPassword("andres", "", false) );
			  CatchInvalidArguments( () => AuthProcedures.changeUserPassword("notAndres", "good password", false) );

			  // Then
			  _log.assertExactly( Error( "[admin]: tried to change password for user `%s`: %s", "andres", "Old password and new password cannot be the same." ), Error( "[admin]: tried to change password for user `%s`: %s", "andres", "A password cannot be empty." ), Error( "[admin]: tried to change password for user `%s`: %s", "notAndres", "User 'notAndres' does not exist." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailureToChangeOwnPassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogFailureToChangeOwnPassword()
		 {
			  // Given
			  AuthProcedures.createUser( "mats", "Neo4Net", true );
			  Subject = _matsContext;
			  _log.clear();

			  // When
			  CatchInvalidArguments( () => AuthProcedures.changeUserPassword("mats", "Neo4Net", false) );
			  CatchInvalidArguments( () => AuthProcedures.changeUserPassword("mats", "", false) );

			  CatchInvalidArguments( () => AuthProcedures.changePassword(null, false) );
			  CatchInvalidArguments( () => AuthProcedures.changePassword("", false) );
			  CatchInvalidArguments( () => AuthProcedures.changePassword("Neo4Net", false) );

			  // Then
			  _log.assertExactly( Error( "[mats]: tried to change password: %s", "Old password and new password cannot be the same." ), Error( "[mats]: tried to change password: %s", "A password cannot be empty." ), Error( "[mats]: tried to change password: %s", "A password cannot be empty." ), Error( "[mats]: tried to change password: %s", "A password cannot be empty." ), Error( "[mats]: tried to change password: %s", "Old password and new password cannot be the same." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUnauthorizedChangePassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogUnauthorizedChangePassword()
		 {
			  // Given
			  AuthProcedures.createUser( "andres", "Neo4Net", true );
			  _log.clear();
			  Subject = _matsContext;

			  // When
			  CatchAuthorizationViolation( () => AuthProcedures.changeUserPassword("andres", "otherPw", false) );

			  // Then
			  _log.assertExactly( Error( "[mats]: tried to change password for user `%s`: %s", "andres", PERMISSION_DENIED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogSuspendUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogSuspendUser()
		 {
			  // Given
			  AuthProcedures.createUser( "mats", "Neo4Net", false );
			  _log.clear();

			  // When
			  AuthProcedures.suspendUser( "mats" );
			  AuthProcedures.suspendUser( "mats" );

			  // Then
			  _log.assertExactly( Info( "[admin]: suspended user `%s`", "mats" ), Info( "[admin]: suspended user `%s`", "mats" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailureToSuspendUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogFailureToSuspendUser()
		 {
			  // Given
			  AuthProcedures.createUser( "mats", "Neo4Net", false );
			  _log.clear();

			  // When
			  CatchInvalidArguments( () => AuthProcedures.suspendUser("notMats") );
			  CatchInvalidArguments( () => AuthProcedures.suspendUser(ADMIN) );

			  // Then
			  _log.assertExactly( Error( "[admin]: tried to suspend user `%s`: %s", "notMats", "User 'notMats' does not exist." ), Error( "[admin]: tried to suspend user `%s`: %s", "admin", "Suspending yourself (user 'admin') is not allowed." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUnauthorizedSuspendUser()
		 public virtual void ShouldLogUnauthorizedSuspendUser()
		 {
			  // Given
			  Subject = _matsContext;

			  // When
			  CatchAuthorizationViolation( () => AuthProcedures.suspendUser(ADMIN) );

			  // Then
			  _log.assertExactly( Error( "[mats]: tried to suspend user `%s`: %s", "admin", PERMISSION_DENIED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogActivateUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogActivateUser()
		 {
			  // Given
			  AuthProcedures.createUser( "mats", "Neo4Net", false );
			  AuthProcedures.suspendUser( "mats" );
			  _log.clear();

			  // When
			  AuthProcedures.activateUser( "mats", false );
			  AuthProcedures.activateUser( "mats", false );

			  // Then
			  _log.assertExactly( Info( "[admin]: activated user `%s`", "mats" ), Info( "[admin]: activated user `%s`", "mats" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailureToActivateUser()
		 public virtual void ShouldLogFailureToActivateUser()
		 {
			  // When
			  CatchInvalidArguments( () => AuthProcedures.activateUser("notMats", false) );
			  CatchInvalidArguments( () => AuthProcedures.activateUser(ADMIN, false) );

			  // Then
			  _log.assertExactly( Error( "[admin]: tried to activate user `%s`: %s", "notMats", "User 'notMats' does not exist." ), Error( "[admin]: tried to activate user `%s`: %s", ADMIN, "Activating yourself (user 'admin') is not allowed." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUnauthorizedActivateUser()
		 public virtual void ShouldLogUnauthorizedActivateUser()
		 {
			  // Given
			  Subject = _matsContext;

			  // When
			  CatchAuthorizationViolation( () => AuthProcedures.activateUser("admin", true) );

			  // Then
			  _log.assertExactly( Error( "[mats]: tried to activate user `%s`: %s", "admin", PERMISSION_DENIED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogCreatingRole() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogCreatingRole()
		 {
			  // When
			  AuthProcedures.createRole( "role" );

			  // Then
			  _log.assertExactly( Info( "[admin]: created role `%s`", "role" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailureToCreateRole() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogFailureToCreateRole()
		 {
			  // Given
			  AuthProcedures.createRole( "role" );
			  _log.clear();

			  // When
			  CatchInvalidArguments( () => AuthProcedures.createRole(null) );
			  CatchInvalidArguments( () => AuthProcedures.createRole("") );
			  CatchInvalidArguments( () => AuthProcedures.createRole("role") );
			  CatchInvalidArguments( () => AuthProcedures.createRole("!@#$") );

			  // Then
			  _log.assertExactly( Error( "[admin]: tried to create role `%s`: %s", null, "The provided role name is empty." ), Error( "[admin]: tried to create role `%s`: %s", "", "The provided role name is empty." ), Error( "[admin]: tried to create role `%s`: %s", "role", "The specified role 'role' already exists." ), Error( "[admin]: tried to create role `%s`: %s", "!@#$", "Role name '!@#$' contains illegal characters. Use simple ascii characters and numbers." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUnauthorizedCreateRole()
		 public virtual void ShouldLogUnauthorizedCreateRole()
		 {
			  // Given
			  Subject = _matsContext;

			  // When
			  CatchAuthorizationViolation( () => AuthProcedures.createRole("role") );

			  // Then
			  _log.assertExactly( Error( "[mats]: tried to create role `%s`: %s", "role", PERMISSION_DENIED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogDeletingRole() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogDeletingRole()
		 {
			  // Given
			  AuthProcedures.createRole( "foo" );
			  _log.clear();

			  // When
			  AuthProcedures.deleteRole( "foo" );

			  // Then
			  _log.assertExactly( Info( "[admin]: deleted role `%s`", "foo" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailureToDeleteRole()
		 public virtual void ShouldLogFailureToDeleteRole()
		 {
			  // When
			  CatchInvalidArguments( () => AuthProcedures.deleteRole(null) );
			  CatchInvalidArguments( () => AuthProcedures.deleteRole("") );
			  CatchInvalidArguments( () => AuthProcedures.deleteRole("foo") );
			  CatchInvalidArguments( () => AuthProcedures.deleteRole(ADMIN) );

			  // Then
			  _log.assertExactly( Error( "[admin]: tried to delete role `%s`: %s", null, "Role 'null' does not exist." ), Error( "[admin]: tried to delete role `%s`: %s", "", "Role '' does not exist." ), Error( "[admin]: tried to delete role `%s`: %s", "foo", "Role 'foo' does not exist." ), Error( "[admin]: tried to delete role `%s`: %s", ADMIN, "'admin' is a predefined role and can not be deleted." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUnauthorizedDeletingRole()
		 public virtual void ShouldLogUnauthorizedDeletingRole()
		 {
			  // Given
			  Subject = _matsContext;

			  // When
			  CatchAuthorizationViolation( () => AuthProcedures.deleteRole(ADMIN) );

			  // Then
			  _log.assertExactly( Error( "[mats]: tried to delete role `%s`: %s", ADMIN, PERMISSION_DENIED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogIfUnexpectedErrorTerminatingTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogIfUnexpectedErrorTerminatingTransactions()
		 {
			  // Given
			  AuthProcedures.createUser( "johan", "Neo4Net", false );
			  AuthProcedures.failTerminateTransaction();
			  _log.clear();

			  // When
			  assertException( () => AuthProcedures.deleteUser("johan"), typeof(Exception), "Unexpected error" );

			  // Then
			  _log.assertExactly( Info( "[admin]: deleted user `%s`", "johan" ), Error( "[admin]: failed to terminate running transaction and bolt connections for user `%s` following %s: %s", "johan", "deletion", "Unexpected error" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUnauthorizedListUsers()
		 public virtual void ShouldLogUnauthorizedListUsers()
		 {
			  // Given
			  Subject = _matsContext;

			  // When
			  CatchAuthorizationViolation( () => AuthProcedures.listUsers() );

			  _log.assertExactly( Error( "[mats]: tried to list users: %s", PERMISSION_DENIED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUnauthorizedListRoles()
		 public virtual void ShouldLogUnauthorizedListRoles()
		 {
			  // Given
			  Subject = _matsContext;

			  // When
			  CatchAuthorizationViolation( () => AuthProcedures.listRoles() );

			  _log.assertExactly( Error( "[mats]: tried to list roles: %s", PERMISSION_DENIED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailureToListRolesForUser()
		 public virtual void ShouldLogFailureToListRolesForUser()
		 {
			  // Given

			  // When
			  CatchInvalidArguments( () => AuthProcedures.listRolesForUser(null) );
			  CatchInvalidArguments( () => AuthProcedures.listRolesForUser("") );
			  CatchInvalidArguments( () => AuthProcedures.listRolesForUser("nonExistent") );

			  _log.assertExactly( Error( "[admin]: tried to list roles for user `%s`: %s", null, "User 'null' does not exist." ), Error( "[admin]: tried to list roles for user `%s`: %s", "", "User '' does not exist." ), Error( "[admin]: tried to list roles for user `%s`: %s", "nonExistent", "User 'nonExistent' does not exist." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUnauthorizedListRolesForUser()
		 public virtual void ShouldLogUnauthorizedListRolesForUser()
		 {
			  // Given
			  Subject = _matsContext;

			  // When
			  CatchAuthorizationViolation( () => AuthProcedures.listRolesForUser("user") );

			  _log.assertExactly( Error( "[mats]: tried to list roles for user `%s`: %s", "user", PERMISSION_DENIED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFailureToListUsersForRole()
		 public virtual void ShouldLogFailureToListUsersForRole()
		 {
			  // Given

			  // When
			  CatchInvalidArguments( () => AuthProcedures.listUsersForRole(null) );
			  CatchInvalidArguments( () => AuthProcedures.listUsersForRole("") );
			  CatchInvalidArguments( () => AuthProcedures.listUsersForRole("nonExistent") );

			  _log.assertExactly( Error( "[admin]: tried to list users for role `%s`: %s", null, "Role 'null' does not exist." ), Error( "[admin]: tried to list users for role `%s`: %s", "", "Role '' does not exist." ), Error( "[admin]: tried to list users for role `%s`: %s", "nonExistent", "Role 'nonExistent' does not exist." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogUnauthorizedListUsersForRole()
		 public virtual void ShouldLogUnauthorizedListUsersForRole()
		 {
			  // Given
			  Subject = _matsContext;

			  // When
			  CatchAuthorizationViolation( () => AuthProcedures.listUsersForRole("role") );

			  _log.assertExactly( Error( "[mats]: tried to list users for role `%s`: %s", "role", PERMISSION_DENIED ) );
		 }

		 private void CatchInvalidArguments( ThrowingAction<Exception> f )
		 {
			  assertException( f, typeof( InvalidArgumentsException ) );
		 }

		 private void CatchAuthorizationViolation( ThrowingAction<Exception> f )
		 {
			  assertException( f, typeof( AuthorizationViolationException ) );
		 }

		 private AssertableLogProvider.LogMatcher Info( string message, params string[] arguments )
		 {
			  if ( arguments.Length == 0 )
			  {
					return inLog( this.GetType() ).info(message);
			  }
			  return inLog( this.GetType() ).info(message, (object[]) arguments);
		 }

		 private AssertableLogProvider.LogMatcher Error( string message, params string[] arguments )
		 {
			  return inLog( this.GetType() ).error(message, (object[]) arguments);
		 }

		 private class MockAuthSubject : AuthSubject
		 {
			  internal readonly string Name;

			  internal MockAuthSubject( string name )
			  {
					this.Name = name;
			  }

			  public override void Logout()
			  {
					throw new System.NotSupportedException();
			  }

			  public virtual AuthenticationResult AuthenticationResult
			  {
				  get
				  {
						return AuthenticationResult.SUCCESS;
				  }
			  }

			  public override void SetPasswordChangeNoLongerRequired()
			  {
			  }

			  public override bool HasUsername( string username )
			  {
					return Name.Equals( username );
			  }

			  public override string Username()
			  {
						 return Name;
			  }
		 }

		 protected internal class TestUserManagementProcedures : UserManagementProcedures
		 {
			  internal bool FailTerminateTransactions;

			  internal virtual void FailTerminateTransaction()
			  {
					FailTerminateTransactions = true;
			  }

			  protected internal override void TerminateTransactionsForValidUser( string username )
			  {
					if ( FailTerminateTransactions )
					{
						 throw new Exception( "Unexpected error" );
					}
			  }

			  protected internal override void TerminateConnectionsForValidUser( string username )
			  {
			  }
		 }
	}

}