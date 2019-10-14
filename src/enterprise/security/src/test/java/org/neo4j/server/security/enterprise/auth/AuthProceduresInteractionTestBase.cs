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
	using Test = org.junit.Test;


	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.security.AuthorizationViolationException.PERMISSION_DENIED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.security.AuthenticationResult.PASSWORD_CHANGE_REQUIRED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.BasicAuthManagerTest.password;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.InternalFlatFileRealm.IS_SUSPENDED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.ProcedureInteractionTestBase.ClassWithProcedures.exceptionsInProcedure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.ADMIN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.ARCHITECT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.EDITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.PUBLISHER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.READER;

	public abstract class AuthProceduresInteractionTestBase<S> : ProcedureInteractionTestBase<S>
	{
		 private static readonly string _pwdChange = PASSWORD_CHANGE_REQUIRED.name().ToLower();

		 //---------- General tests over all procedures -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveDescriptionsOnAllSecurityProcedures()
		 public virtual void ShouldHaveDescriptionsOnAllSecurityProcedures()
		 {
			  AssertSuccess(ReadSubject, "CALL dbms.procedures", r =>
			  {
				Stream<IDictionary<string, object>> securityProcedures = r.Where(s =>
				{
					 string name = s.get( "name" ).ToString();
					 string description = s.get( "description" ).ToString();
					 // TODO: remove filter for Transaction and Connection once those procedures are removed
					 if ( name.Contains( "dbms.security" ) && !( name.Contains( "Transaction" ) || name.Contains( "Connection" ) ) )
					 {
						  assertThat( "Description for '" + name + "' should not be empty", description.Trim().Length, greaterThan(0) );
						  return true;
					 }
					 return false;
				});
				assertThat( securityProcedures.count(), equalTo(16L) );
			  });
		 }

		 //---------- Change own password -----------

		 // Enterprise version of test in BuiltInProceduresIT.callChangePasswordWithAccessModeInDbmsMode.
		 // Uses community edition procedure in BuiltInProcedures
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangeOwnPassword()
		 public virtual void ShouldChangeOwnPassword()
		 {
			  AssertEmpty( ReadSubject, "CALL dbms.security.changePassword( '321' )" );
			  // Because RESTSubject caches an auth token that is sent with every request
			  Neo.updateAuthToken( ReadSubject, "readSubject", "321" );
			  Neo.assertAuthenticated( ReadSubject );
			  TestSuccessfulRead( ReadSubject, 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangeOwnPasswordEvenIfHasNoAuthorization()
		 public virtual void ShouldChangeOwnPasswordEvenIfHasNoAuthorization()
		 {
			  Neo.assertAuthenticated( NoneSubject );
			  AssertEmpty( NoneSubject, "CALL dbms.security.changePassword( '321' )" );
			  // Because RESTSubject caches an auth token that is sent with every request
			  Neo.updateAuthToken( NoneSubject, "noneSubject", "321" );
			  Neo.assertAuthenticated( NoneSubject );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotChangeOwnPasswordIfNewPasswordInvalid()
		 public virtual void ShouldNotChangeOwnPasswordIfNewPasswordInvalid()
		 {
			  AssertFail( ReadSubject, "CALL dbms.security.changePassword( '' )", "A password cannot be empty." );
			  AssertFail( ReadSubject, "CALL dbms.security.changePassword( '123' )", "Old password and new password cannot be the same." );
		 }

		 //---------- change user password -----------

		 // Should change password for admin subject and valid user
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangeUserPassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldChangeUserPassword()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.changeUserPassword( 'readSubject', '321', false )" );
			  // TODO: uncomment and fix
			  // testUnAuthenticated( readSubject );

			  Neo.assertInitFailed( Neo.login( "readSubject", "123" ) );
			  Neo.assertAuthenticated( Neo.login( "readSubject", "321" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangeUserPasswordAndRequirePasswordChangeOnNextLoginByDefault() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldChangeUserPasswordAndRequirePasswordChangeOnNextLoginByDefault()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.changeUserPassword( 'readSubject', '321' )" );
			  Neo.assertInitFailed( Neo.login( "readSubject", "123" ) );
			  Neo.assertPasswordChangeRequired( Neo.login( "readSubject", "321" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangeUserPasswordAndRequirePasswordChangeOnNextLoginOnRequest() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldChangeUserPasswordAndRequirePasswordChangeOnNextLoginOnRequest()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.changeUserPassword( 'readSubject', '321', true )" );
			  Neo.assertInitFailed( Neo.login( "readSubject", "123" ) );
			  Neo.assertPasswordChangeRequired( Neo.login( "readSubject", "321" ) );
		 }

		 // Should fail vaguely to change password for non-admin subject, regardless of user and password
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotChangeUserPasswordIfNotAdmin()
		 public virtual void ShouldNotChangeUserPasswordIfNotAdmin()
		 {
			  AssertFail( SchemaSubject, "CALL dbms.security.changeUserPassword( 'readSubject', '321' )", PERMISSION_DENIED );
			  AssertFail( SchemaSubject, "CALL dbms.security.changeUserPassword( 'jake', '321' )", PERMISSION_DENIED );
			  AssertFail( SchemaSubject, "CALL dbms.security.changeUserPassword( 'readSubject', '' )", PERMISSION_DENIED );
		 }

		 // Should change own password for non-admin or admin subject
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangeUserPasswordIfSameUser()
		 public virtual void ShouldChangeUserPasswordIfSameUser()
		 {
			  AssertEmpty( ReadSubject, "CALL dbms.security.changeUserPassword( 'readSubject', '321', false )" );
			  // Because RESTSubject caches an auth token that is sent with every request
			  Neo.updateAuthToken( ReadSubject, "readSubject", "321" );
			  Neo.assertAuthenticated( ReadSubject );
			  TestSuccessfulRead( ReadSubject, 3 );

			  AssertEmpty( AdminSubject, "CALL dbms.security.changeUserPassword( 'adminSubject', 'cba', false )" );
			  // Because RESTSubject caches an auth token that is sent with every request
			  Neo.updateAuthToken( AdminSubject, "adminSubject", "cba" );
			  Neo.assertAuthenticated( AdminSubject );
			  TestSuccessfulRead( AdminSubject, 3 );
		 }

		 // Should fail nicely to change own password for non-admin or admin subject if password invalid
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToChangeUserPasswordIfSameUserButInvalidPassword()
		 public virtual void ShouldFailToChangeUserPasswordIfSameUserButInvalidPassword()
		 {
			  AssertFail( ReadSubject, "CALL dbms.security.changeUserPassword( 'readSubject', '123' )", "Old password and new password cannot be the same." );

			  AssertFail( AdminSubject, "CALL dbms.security.changeUserPassword( 'adminSubject', 'abc' )", "Old password and new password cannot be the same." );
		 }

		 // Should fail nicely to change password for admin subject and non-existing user
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotChangeUserPasswordIfNonExistentUser()
		 public virtual void ShouldNotChangeUserPasswordIfNonExistentUser()
		 {
			  AssertFail( AdminSubject, "CALL dbms.security.changeUserPassword( 'jake', '321' )", "User 'jake' does not exist." );
		 }

		 // Should fail nicely to change password for admin subject and empty password
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotChangeUserPasswordIfEmptyPassword()
		 public virtual void ShouldNotChangeUserPasswordIfEmptyPassword()
		 {
			  AssertFail( AdminSubject, "CALL dbms.security.changeUserPassword( 'readSubject', '' )", "A password cannot be empty." );
		 }

		 // Should fail to change password for admin subject and same password
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotChangeUserPasswordIfSamePassword()
		 public virtual void ShouldNotChangeUserPasswordIfSamePassword()
		 {
			  AssertFail( AdminSubject, "CALL dbms.security.changeUserPassword( 'readSubject', '123' )", "Old password and new password cannot be the same." );
		 }

		 //---------- create user -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUserAndRequirePasswordChangeByDefault() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateUserAndRequirePasswordChangeByDefault()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('craig', '1234' )" );
			  UserManager.getUser( "craig" );
			  Neo.assertInitFailed( Neo.login( "craig", "321" ) );
			  Neo.assertPasswordChangeRequired( Neo.login( "craig", "1234" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUserAndRequirePasswordChangeIfRequested() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateUserAndRequirePasswordChangeIfRequested()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('craig', '1234', true)" );
			  UserManager.getUser( "craig" );
			  Neo.assertInitFailed( Neo.login( "craig", "321" ) );
			  Neo.assertPasswordChangeRequired( Neo.login( "craig", "1234" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUserAndRequireNoPasswordChangeIfRequested() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateUserAndRequireNoPasswordChangeIfRequested()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('craig', '1234', false)" );
			  UserManager.getUser( "craig" );
			  Neo.assertAuthenticated( Neo.login( "craig", "1234" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCreateUserIfInvalidUsername()
		 public virtual void ShouldNotCreateUserIfInvalidUsername()
		 {
			  AssertFail( AdminSubject, "CALL dbms.security.createUser(null, '1234', true)", "The provided username is empty." );
			  AssertFail( AdminSubject, "CALL dbms.security.createUser('', '1234', true)", "The provided username is empty." );
			  AssertFail( AdminSubject, "CALL dbms.security.createUser(',ss!', '1234', true)", "Username ',ss!' contains illegal characters." );
			  AssertFail( AdminSubject, "CALL dbms.security.createUser(',ss!', '', true)", "Username ',ss!' contains illegal characters." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCreateUserIfInvalidPassword()
		 public virtual void ShouldNotCreateUserIfInvalidPassword()
		 {
			  AssertFail( AdminSubject, "CALL dbms.security.createUser('craig', '', true)", "A password cannot be empty." );
			  AssertFail( AdminSubject, "CALL dbms.security.createUser('craig', null, true)", "A password cannot be empty." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCreateExistingUser()
		 public virtual void ShouldNotCreateExistingUser()
		 {
			  AssertFail( AdminSubject, "CALL dbms.security.createUser('readSubject', '1234', true)", "The specified user 'readSubject' already exists" );
			  AssertFail( AdminSubject, "CALL dbms.security.createUser('readSubject', '', true)", "A password cannot be empty." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowNonAdminCreateUser()
		 public virtual void ShouldNotAllowNonAdminCreateUser()
		 {
			  TestFailCreateUser( PwdSubject, ChangePwdErrMsg );
			  TestFailCreateUser( ReadSubject, PERMISSION_DENIED );
			  TestFailCreateUser( WriteSubject, PERMISSION_DENIED );
			  TestFailCreateUser( SchemaSubject, PERMISSION_DENIED );
		 }

		 //---------- delete user -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteUser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeleteUser()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.deleteUser('noneSubject')" );
			  try
			  {
					UserManager.getUser( "noneSubject" );
					fail( "User noneSubject should not exist" );
			  }
			  catch ( InvalidArgumentsException e )
			  {
					assertTrue( "User noneSubject should not exist", e.Message.contains( "User 'noneSubject' does not exist." ) );
			  }

			  UserManager.addRoleToUser( PUBLISHER, "readSubject" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.deleteUser('readSubject')" );
			  try
			  {
					UserManager.getUser( "readSubject" );
					fail( "User readSubject should not exist" );
			  }
			  catch ( InvalidArgumentsException e )
			  {
					assertTrue( "User readSubject should not exist", e.Message.contains( "User 'readSubject' does not exist." ) );
			  }
			  assertFalse( UserManager.getUsernamesForRole( READER ).Contains( "readSubject" ) );
			  assertFalse( UserManager.getUsernamesForRole( PUBLISHER ).Contains( "readSubject" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDeleteUserIfNotAdmin()
		 public virtual void ShouldNotDeleteUserIfNotAdmin()
		 {
			  TestFailDeleteUser( PwdSubject, "readSubject", ChangePwdErrMsg );
			  TestFailDeleteUser( ReadSubject, "readSubject", PERMISSION_DENIED );
			  TestFailDeleteUser( WriteSubject, "readSubject", PERMISSION_DENIED );

			  TestFailDeleteUser( SchemaSubject, "readSubject", PERMISSION_DENIED );
			  TestFailDeleteUser( SchemaSubject, "Craig", PERMISSION_DENIED );
			  TestFailDeleteUser( SchemaSubject, "", PERMISSION_DENIED );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowDeletingNonExistentUser()
		 public virtual void ShouldNotAllowDeletingNonExistentUser()
		 {
			  TestFailDeleteUser( AdminSubject, "Craig", "User 'Craig' does not exist." );
			  TestFailDeleteUser( AdminSubject, "", "User '' does not exist." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowDeletingYourself()
		 public virtual void ShouldNotAllowDeletingYourself()
		 {
			  TestFailDeleteUser( AdminSubject, "adminSubject", "Deleting yourself (user 'adminSubject') is not allowed." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateTransactionsOnUserDeletion() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateTransactionsOnUserDeletion()
		 {
			  ShouldTerminateTransactionsForUser( WriteSubject, "dbms.security.deleteUser( '%s' )" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateConnectionsOnUserDeletion() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateConnectionsOnUserDeletion()
		 {
			  TransportConnection conn = StartBoltSession( "writeSubject", "abc" );

			  IDictionary<string, long> boltConnections = CountBoltConnectionsByUsername();
			  assertThat( boltConnections["writeSubject"], equalTo( IsEmbedded ? 1L : 2L ) );

			  AssertEmpty( AdminSubject, "CALL dbms.security.deleteUser( 'writeSubject' )" );

			  boltConnections = CountBoltConnectionsByUsername();
			  assertThat( boltConnections["writeSubject"], equalTo( null ) );

			  conn.Disconnect();
		 }

		 //---------- suspend user -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSuspendUser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSuspendUser()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.suspendUser('readSubject')" );
			  assertTrue( UserManager.getUser( "readSubject" ).hasFlag( IS_SUSPENDED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSuspendSuspendedUser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSuspendSuspendedUser()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.suspendUser('readSubject')" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.suspendUser('readSubject')" );
			  assertTrue( UserManager.getUser( "readSubject" ).hasFlag( IS_SUSPENDED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToSuspendNonExistentUser()
		 public virtual void ShouldFailToSuspendNonExistentUser()
		 {
			  AssertFail( AdminSubject, "CALL dbms.security.suspendUser('Craig')", "User 'Craig' does not exist." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToSuspendIfNotAdmin()
		 public virtual void ShouldFailToSuspendIfNotAdmin()
		 {
			  AssertFail( SchemaSubject, "CALL dbms.security.suspendUser('readSubject')", PERMISSION_DENIED );
			  AssertFail( SchemaSubject, "CALL dbms.security.suspendUser('Craig')", PERMISSION_DENIED );
			  AssertFail( SchemaSubject, "CALL dbms.security.suspendUser('')", PERMISSION_DENIED );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToSuspendYourself()
		 public virtual void ShouldFailToSuspendYourself()
		 {
			  AssertFail( AdminSubject, "CALL dbms.security.suspendUser('adminSubject')", "Suspending yourself (user 'adminSubject') is not allowed." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateTransactionsOnUserSuspension() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateTransactionsOnUserSuspension()
		 {
			  ShouldTerminateTransactionsForUser( WriteSubject, "dbms.security.suspendUser( '%s' )" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateConnectionsOnUserSuspension() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateConnectionsOnUserSuspension()
		 {
			  TransportConnection conn = StartBoltSession( "writeSubject", "abc" );

			  IDictionary<string, long> boltConnections = CountBoltConnectionsByUsername();
			  assertThat( boltConnections["writeSubject"], equalTo( IsEmbedded ? 1L : 2L ) );

			  AssertEmpty( AdminSubject, "CALL dbms.security.suspendUser( 'writeSubject' )" );

			  boltConnections = CountBoltConnectionsByUsername();
			  assertThat( boltConnections["writeSubject"], equalTo( null ) );

			  conn.Disconnect();
		 }

		 //---------- activate user -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldActivateUserAndRequirePasswordChangeByDefault() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldActivateUserAndRequirePasswordChangeByDefault()
		 {
			  UserManager.suspendUser( "readSubject" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.activateUser('readSubject')" );
			  Neo.assertInitFailed( Neo.login( "readSubject", "321" ) );
			  Neo.assertPasswordChangeRequired( Neo.login( "readSubject", "123" ) );
			  assertFalse( UserManager.getUser( "readSubject" ).hasFlag( IS_SUSPENDED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldActivateUserAndRequirePasswordChangeIfRequested() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldActivateUserAndRequirePasswordChangeIfRequested()
		 {
			  UserManager.suspendUser( "readSubject" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.activateUser('readSubject', true)" );
			  Neo.assertInitFailed( Neo.login( "readSubject", "321" ) );
			  Neo.assertPasswordChangeRequired( Neo.login( "readSubject", "123" ) );
			  assertFalse( UserManager.getUser( "readSubject" ).hasFlag( IS_SUSPENDED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldActivateUserAndRequireNoPasswordChangeIfRequested() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldActivateUserAndRequireNoPasswordChangeIfRequested()
		 {
			  UserManager.suspendUser( "readSubject" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.activateUser('readSubject', false)" );
			  assertFalse( UserManager.getUser( "readSubject" ).hasFlag( IS_SUSPENDED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldActivateActiveUser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldActivateActiveUser()
		 {
			  UserManager.suspendUser( "readSubject" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.activateUser('readSubject')" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.activateUser('readSubject')" );
			  assertFalse( UserManager.getUser( "readSubject" ).hasFlag( IS_SUSPENDED ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToActivateNonExistentUser()
		 public virtual void ShouldFailToActivateNonExistentUser()
		 {
			  AssertFail( AdminSubject, "CALL dbms.security.activateUser('Craig')", "User 'Craig' does not exist." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToActivateIfNotAdmin() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToActivateIfNotAdmin()
		 {
			  UserManager.suspendUser( "readSubject" );
			  AssertFail( SchemaSubject, "CALL dbms.security.activateUser('readSubject')", PERMISSION_DENIED );
			  AssertFail( SchemaSubject, "CALL dbms.security.activateUser('Craig')", PERMISSION_DENIED );
			  AssertFail( SchemaSubject, "CALL dbms.security.activateUser('')", PERMISSION_DENIED );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToActivateYourself()
		 public virtual void ShouldFailToActivateYourself()
		 {
			  AssertFail( AdminSubject, "CALL dbms.security.activateUser('adminSubject')", "Activating yourself (user 'adminSubject') is not allowed." );
		 }

		 //---------- add user to role -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddRoleToUser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddRoleToUser()
		 {
			  assertFalse( "Should not have role publisher", UserHasRole( "readSubject", PUBLISHER ) );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'readSubject' )" );
			  assertTrue( "Should have role publisher", UserHasRole( "readSubject", PUBLISHER ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddRetainUserInRole() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddRetainUserInRole()
		 {
			  assertTrue( "Should have role reader", UserHasRole( "readSubject", READER ) );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + READER + "', 'readSubject')" );
			  assertTrue( "Should have still have role reader", UserHasRole( "readSubject", READER ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToAddNonExistentUserToRole()
		 public virtual void ShouldFailToAddNonExistentUserToRole()
		 {
			  TestFailAddRoleToUser( AdminSubject, PUBLISHER, "Olivia", "User 'Olivia' does not exist." );
			  TestFailAddRoleToUser( AdminSubject, "thisRoleDoesNotExist", "Olivia", "User 'Olivia' does not exist." );
			  TestFailAddRoleToUser( AdminSubject, "", "Olivia", "The provided role name is empty." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToAddUserToNonExistentRole()
		 public virtual void ShouldFailToAddUserToNonExistentRole()
		 {
			  TestFailAddRoleToUser( AdminSubject, "thisRoleDoesNotExist", "readSubject", "Role 'thisRoleDoesNotExist' does not exist." );
			  TestFailAddRoleToUser( AdminSubject, "", "readSubject", "The provided role name is empty." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToAddRoleToUserIfNotAdmin()
		 public virtual void ShouldFailToAddRoleToUserIfNotAdmin()
		 {
			  TestFailAddRoleToUser( PwdSubject, PUBLISHER, "readSubject", ChangePwdErrMsg );
			  TestFailAddRoleToUser( ReadSubject, PUBLISHER, "readSubject", PERMISSION_DENIED );
			  TestFailAddRoleToUser( WriteSubject, PUBLISHER, "readSubject", PERMISSION_DENIED );

			  TestFailAddRoleToUser( SchemaSubject, PUBLISHER, "readSubject", PERMISSION_DENIED );
			  TestFailAddRoleToUser( SchemaSubject, PUBLISHER, "Olivia", PERMISSION_DENIED );
			  TestFailAddRoleToUser( SchemaSubject, "thisRoleDoesNotExist", "Olivia", PERMISSION_DENIED );
		 }

		 //---------- remove user from role -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveRoleFromUser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveRoleFromUser()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.removeRoleFromUser('" + READER + "', 'readSubject')" );
			  assertFalse( "Should not have role reader", UserHasRole( "readSubject", READER ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepUserOutOfRole() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepUserOutOfRole()
		 {
			  assertFalse( "Should not have role publisher", UserHasRole( "readSubject", PUBLISHER ) );
			  AssertEmpty( AdminSubject, "CALL dbms.security.removeRoleFromUser('" + PUBLISHER + "', 'readSubject')" );
			  assertFalse( "Should not have role publisher", UserHasRole( "readSubject", PUBLISHER ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToRemoveNonExistentUserFromRole()
		 public virtual void ShouldFailToRemoveNonExistentUserFromRole()
		 {
			  TestFailRemoveRoleFromUser( AdminSubject, PUBLISHER, "Olivia", "User 'Olivia' does not exist." );
			  TestFailRemoveRoleFromUser( AdminSubject, "thisRoleDoesNotExist", "Olivia", "User 'Olivia' does not exist." );
			  TestFailRemoveRoleFromUser( AdminSubject, "", "Olivia", "The provided role name is empty." );
			  TestFailRemoveRoleFromUser( AdminSubject, "", "", "The provided role name is empty." );
			  TestFailRemoveRoleFromUser( AdminSubject, PUBLISHER, "", "The provided username is empty." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToRemoveUserFromNonExistentRole()
		 public virtual void ShouldFailToRemoveUserFromNonExistentRole()
		 {
			  TestFailRemoveRoleFromUser( AdminSubject, "thisRoleDoesNotExist", "readSubject", "Role 'thisRoleDoesNotExist' does not exist." );
			  TestFailRemoveRoleFromUser( AdminSubject, "", "readSubject", "The provided role name is empty." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToRemoveRoleFromUserIfNotAdmin()
		 public virtual void ShouldFailToRemoveRoleFromUserIfNotAdmin()
		 {
			  TestFailRemoveRoleFromUser( PwdSubject, PUBLISHER, "readSubject", ChangePwdErrMsg );
			  TestFailRemoveRoleFromUser( ReadSubject, PUBLISHER, "readSubject", PERMISSION_DENIED );
			  TestFailRemoveRoleFromUser( WriteSubject, PUBLISHER, "readSubject", PERMISSION_DENIED );

			  TestFailRemoveRoleFromUser( SchemaSubject, READER, "readSubject", PERMISSION_DENIED );
			  TestFailRemoveRoleFromUser( SchemaSubject, READER, "Olivia", PERMISSION_DENIED );
			  TestFailRemoveRoleFromUser( SchemaSubject, "thisRoleDoesNotExist", "Olivia", PERMISSION_DENIED );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToRemoveYourselfFromAdminRole()
		 public virtual void ShouldFailToRemoveYourselfFromAdminRole()
		 {
			  AssertFail( AdminSubject, "CALL dbms.security.removeRoleFromUser('" + ADMIN + "', 'adminSubject')", "Removing yourself (user 'adminSubject') from the admin role is not allowed." );
		 }

		 //---------- manage multiple roles -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowAddingAndRemovingUserFromMultipleRoles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowAddingAndRemovingUserFromMultipleRoles()
		 {
			  assertFalse( "Should not have role publisher", UserHasRole( "readSubject", PUBLISHER ) );
			  assertFalse( "Should not have role architect", UserHasRole( "readSubject", ARCHITECT ) );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'readSubject')" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + ARCHITECT + "', 'readSubject')" );
			  assertTrue( "Should have role publisher", UserHasRole( "readSubject", PUBLISHER ) );
			  assertTrue( "Should have role architect", UserHasRole( "readSubject", ARCHITECT ) );
			  AssertEmpty( AdminSubject, "CALL dbms.security.removeRoleFromUser('" + PUBLISHER + "', 'readSubject')" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.removeRoleFromUser('" + ARCHITECT + "', 'readSubject')" );
			  assertFalse( "Should not have role publisher", UserHasRole( "readSubject", PUBLISHER ) );
			  assertFalse( "Should not have role architect", UserHasRole( "readSubject", ARCHITECT ) );
		 }

		 //---------- create role -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateRole() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateRole()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createRole('new_role')" );
			  UserManager.assertRoleExists( "new_role" );
			  assertEquals( UserManager.getUsernamesForRole( "new_role" ).Count, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCreateRoleIfInvalidRoleName()
		 public virtual void ShouldNotCreateRoleIfInvalidRoleName()
		 {
			  AssertFail( AdminSubject, "CALL dbms.security.createRole('')", "The provided role name is empty." );
			  AssertFail( AdminSubject, "CALL dbms.security.createRole('&%ss!')", "Role name '&%ss!' contains illegal characters. Use simple ascii characters and numbers." );
			  AssertFail( AdminSubject, "CALL dbms.security.createRole('åäöø')", "Role name 'åäöø' contains illegal characters. Use simple ascii characters and numbers" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCreateExistingRole()
		 public virtual void ShouldNotCreateExistingRole()
		 {
			  AssertFail( AdminSubject, format( "CALL dbms.security.createRole('%s')", ARCHITECT ), "The specified role 'architect' already exists" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.createRole('new_role')" );
			  AssertFail( AdminSubject, "CALL dbms.security.createRole('new_role')", "The specified role 'new_role' already exists" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowNonAdminCreateRole()
		 public virtual void ShouldNotAllowNonAdminCreateRole()
		 {
			  TestFailCreateRole( PwdSubject, ChangePwdErrMsg );
			  TestFailCreateRole( ReadSubject, PERMISSION_DENIED );
			  TestFailCreateRole( WriteSubject, PERMISSION_DENIED );
			  TestFailCreateRole( SchemaSubject, PERMISSION_DENIED );
		 }

		 //---------- delete role -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIfTryingToDeletePredefinedRole()
		 public virtual void ShouldThrowIfTryingToDeletePredefinedRole()
		 {
			  TestFailDeleteRole( AdminSubject, ADMIN, format( "'%s' is a predefined role and can not be deleted.", ADMIN ) );
			  TestFailDeleteRole( AdminSubject, ARCHITECT, format( "'%s' is a predefined role and can not be deleted.", ARCHITECT ) );
			  TestFailDeleteRole( AdminSubject, PUBLISHER, format( "'%s' is a predefined role and can not be deleted.", PUBLISHER ) );
			  TestFailDeleteRole( AdminSubject, READER, format( "'%s' is a predefined role and can not be deleted.", READER ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIfNonAdminTryingToDeleteRole()
		 public virtual void ShouldThrowIfNonAdminTryingToDeleteRole()
		 {
			  AssertEmpty( AdminSubject, format( "CALL dbms.security.createRole('%s')", "new_role" ) );
			  TestFailDeleteRole( SchemaSubject, "new_role", PERMISSION_DENIED );
			  TestFailDeleteRole( WriteSubject, "new_role", PERMISSION_DENIED );
			  TestFailDeleteRole( ReadSubject, "new_role", PERMISSION_DENIED );
			  TestFailDeleteRole( NoneSubject, "new_role", PERMISSION_DENIED );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIfDeletingNonExistentRole()
		 public virtual void ShouldThrowIfDeletingNonExistentRole()
		 {
			  TestFailDeleteRole( AdminSubject, "nonExistent", "Role 'nonExistent' does not exist." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteRole() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeleteRole()
		 {
			  Neo.LocalUserManager.newRole( "new_role" );
			  AssertEmpty( AdminSubject, format( "CALL dbms.security.deleteRole('%s')", "new_role" ) );

			  assertThat( UserManager.AllRoleNames, not( contains( "new_role" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deletingRoleAssignedToSelfShouldWork() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DeletingRoleAssignedToSelfShouldWork()
		 {
			  AssertEmpty( AdminSubject, format( "CALL dbms.security.createRole('%s')", "new_role" ) );
			  AssertEmpty( AdminSubject, format( "CALL dbms.security.addRoleToUser('%s', '%s')", "new_role", "adminSubject" ) );
			  assertThat( UserManager.getRoleNamesForUser( "adminSubject" ), hasItem( "new_role" ) );

			  AssertEmpty( this.AdminSubject, format( "CALL dbms.security.deleteRole('%s')", "new_role" ) );
			  assertThat( UserManager.getRoleNamesForUser( "adminSubject" ), not( hasItem( "new_role" ) ) );
			  assertThat( UserManager.AllRoleNames, not( contains( "new_role" ) ) );
		 }

		 //---------- list users -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListUsers()
		 public virtual void ShouldListUsers()
		 {
			  AssertSuccess( AdminSubject, "CALL dbms.security.listUsers() YIELD username", r => assertKeyIs( r, "username", InitialUsers ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnUsersWithRoles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnUsersWithRoles()
		 {
			  IDictionary<string, object> expected = map( "adminSubject", ListOf( ADMIN ), "readSubject", ListOf( READER ), "schemaSubject", ListOf( ARCHITECT ), "writeSubject", ListOf( READER, PUBLISHER ), "editorSubject", ListOf( EDITOR ), "pwdSubject", ListOf(), "noneSubject", ListOf(), "neo4j", ListOf(ADMIN) );
			  UserManager.addRoleToUser( READER, "writeSubject" );
			  AssertSuccess( AdminSubject, "CALL dbms.security.listUsers()", r => assertKeyIsMap( r, "username", "roles", ValueOf( expected ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnUsersWithFlags() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnUsersWithFlags()
		 {
			  IDictionary<string, object> expected = map( "adminSubject", ListOf(), "readSubject", ListOf(), "schemaSubject", ListOf(), "editorSubject", ListOf(), "writeSubject", ListOf(IS_SUSPENDED), "pwdSubject", ListOf(_pwdChange, IS_SUSPENDED), "noneSubject", ListOf(), "neo4j", ListOf(_pwdChange.ToLower()) );
			  UserManager.suspendUser( "writeSubject" );
			  UserManager.suspendUser( "pwdSubject" );
			  AssertSuccess( AdminSubject, "CALL dbms.security.listUsers()", r => assertKeyIsMap( r, "username", "flags", ValueOf( expected ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowCurrentUser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShowCurrentUser()
		 {
			  UserManager.addRoleToUser( READER, "writeSubject" );
			  AssertSuccess( AdminSubject, "CALL dbms.showCurrentUser()", r => assertKeyIsMap( r, "username", "roles", ValueOf( map( "adminSubject", ListOf( ADMIN ) ) ) ) );
			  AssertSuccess( ReadSubject, "CALL dbms.showCurrentUser()", r => assertKeyIsMap( r, "username", "roles", ValueOf( map( "readSubject", ListOf( READER ) ) ) ) );
			  AssertSuccess( SchemaSubject, "CALL dbms.showCurrentUser()", r => assertKeyIsMap( r, "username", "roles", ValueOf( map( "schemaSubject", ListOf( ARCHITECT ) ) ) ) );
			  AssertSuccess( WriteSubject, "CALL dbms.showCurrentUser()", r => assertKeyIsMap( r, "username", "roles", ValueOf( map( "writeSubject", ListOf( READER, PUBLISHER ) ) ) ) );
			  AssertSuccess( NoneSubject, "CALL dbms.showCurrentUser()", r => assertKeyIsMap( r, "username", "roles", ValueOf( map( "noneSubject", ListOf() ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowNonAdminListUsers()
		 public virtual void ShouldNotAllowNonAdminListUsers()
		 {
			  TestFailListUsers( PwdSubject, 5, ChangePwdErrMsg );
			  TestFailListUsers( ReadSubject, 5, PERMISSION_DENIED );
			  TestFailListUsers( WriteSubject, 5, PERMISSION_DENIED );
			  TestFailListUsers( SchemaSubject, 5, PERMISSION_DENIED );
		 }

		 //---------- list roles -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListRoles()
		 public virtual void ShouldListRoles()
		 {
			  AssertSuccess( AdminSubject, "CALL dbms.security.listRoles() YIELD role", r => assertKeyIs( r, "role", InitialRoles ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnRolesWithUsers()
		 public virtual void ShouldReturnRolesWithUsers()
		 {
			  IDictionary<string, object> expected = map( ADMIN, ListOf( "adminSubject", "neo4j" ), READER, ListOf( "readSubject" ), ARCHITECT, ListOf( "schemaSubject" ), PUBLISHER, ListOf( "writeSubject" ), EDITOR, ListOf( "editorSubject" ), "empty", ListOf() );
			  AssertSuccess( AdminSubject, "CALL dbms.security.listRoles()", r => assertKeyIsMap( r, "role", "users", ValueOf( expected ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowNonAdminListRoles()
		 public virtual void ShouldNotAllowNonAdminListRoles()
		 {
			  TestFailListRoles( PwdSubject, ChangePwdErrMsg );
			  TestFailListRoles( ReadSubject, PERMISSION_DENIED );
			  TestFailListRoles( WriteSubject, PERMISSION_DENIED );
			  TestFailListRoles( SchemaSubject, PERMISSION_DENIED );
		 }

		 //---------- list roles for user -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListRolesForUser()
		 public virtual void ShouldListRolesForUser()
		 {
			  AssertSuccess( AdminSubject, "CALL dbms.security.listRolesForUser('adminSubject') YIELD value as roles RETURN roles", r => assertKeyIs( r, "roles", ADMIN ) );
			  AssertSuccess( AdminSubject, "CALL dbms.security.listRolesForUser('readSubject') YIELD value as roles RETURN roles", r => assertKeyIs( r, "roles", READER ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListNoRolesForUserWithNoRoles()
		 public virtual void ShouldListNoRolesForUserWithNoRoles()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.listRolesForUser('Henrik') YIELD value as roles RETURN roles" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotListRolesForNonExistentUser()
		 public virtual void ShouldNotListRolesForNonExistentUser()
		 {
			  AssertFail( AdminSubject, "CALL dbms.security.listRolesForUser('Petra') YIELD value as roles RETURN roles", "User 'Petra' does not exist." );
			  AssertFail( AdminSubject, "CALL dbms.security.listRolesForUser('') YIELD value as roles RETURN roles", "User '' does not exist." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListOwnRolesRoles()
		 public virtual void ShouldListOwnRolesRoles()
		 {
			  AssertSuccess( AdminSubject, "CALL dbms.security.listRolesForUser('adminSubject') YIELD value as roles RETURN roles", r => assertKeyIs( r, "roles", ADMIN ) );
			  AssertSuccess( ReadSubject, "CALL dbms.security.listRolesForUser('readSubject') YIELD value as roles RETURN roles", r => assertKeyIs( r, "roles", READER ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowNonAdminListUserRoles()
		 public virtual void ShouldNotAllowNonAdminListUserRoles()
		 {
			  TestFailListUserRoles( PwdSubject, "adminSubject", ChangePwdErrMsg );
			  TestFailListUserRoles( ReadSubject, "adminSubject", PERMISSION_DENIED );
			  TestFailListUserRoles( WriteSubject, "adminSubject", PERMISSION_DENIED );
			  TestFailListUserRoles( SchemaSubject, "adminSubject", PERMISSION_DENIED );
		 }

		 //---------- list users for role -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListUsersForRole()
		 public virtual void ShouldListUsersForRole()
		 {
			  AssertSuccess( AdminSubject, "CALL dbms.security.listUsersForRole('admin') YIELD value as users RETURN users", r => assertKeyIs( r, "users", "adminSubject", "neo4j" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListNoUsersForRoleWithNoUsers()
		 public virtual void ShouldListNoUsersForRoleWithNoUsers()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.listUsersForRole('empty') YIELD value as users RETURN users" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotListUsersForNonExistentRole()
		 public virtual void ShouldNotListUsersForNonExistentRole()
		 {
			  AssertFail( AdminSubject, "CALL dbms.security.listUsersForRole('poodle') YIELD value as users RETURN users", "Role 'poodle' does not exist." );
			  AssertFail( AdminSubject, "CALL dbms.security.listUsersForRole('') YIELD value as users RETURN users", "Role '' does not exist." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotListUsersForRoleIfNotAdmin()
		 public virtual void ShouldNotListUsersForRoleIfNotAdmin()
		 {
			  TestFailListRoleUsers( PwdSubject, ADMIN, ChangePwdErrMsg );
			  TestFailListRoleUsers( ReadSubject, ADMIN, PERMISSION_DENIED );
			  TestFailListRoleUsers( WriteSubject, ADMIN, PERMISSION_DENIED );
			  TestFailListRoleUsers( SchemaSubject, ADMIN, PERMISSION_DENIED );
		 }

		 //---------- clearing authentication cache -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowClearAuthCacheIfAdmin()
		 public virtual void ShouldAllowClearAuthCacheIfAdmin()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.clearAuthCache()" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotClearAuthCacheIfNotAdmin()
		 public virtual void ShouldNotClearAuthCacheIfNotAdmin()
		 {
			  AssertFail( PwdSubject, "CALL dbms.security.clearAuthCache()", ChangePwdErrMsg );
			  AssertFail( ReadSubject, "CALL dbms.security.clearAuthCache()", PERMISSION_DENIED );
			  AssertFail( WriteSubject, "CALL dbms.security.clearAuthCache()", PERMISSION_DENIED );
			  AssertFail( SchemaSubject, "CALL dbms.security.clearAuthCache()", PERMISSION_DENIED );
		 }

		 //---------- permissions -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintUserAndRolesWhenPermissionDenied() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPrintUserAndRolesWhenPermissionDenied()
		 {
			  UserManager.newUser( "mats", password( "foo" ), false );
			  UserManager.newRole( "failer", "mats" );
			  S mats = Neo.login( "mats", "foo" );

			  AssertFail( NoneSubject, "CALL test.numNodes", "Read operations are not allowed for user 'noneSubject' with no roles." );
			  AssertFail( ReadSubject, "CALL test.allowedWriteProcedure", "Write operations are not allowed for user 'readSubject' with roles [reader]." );
			  AssertFail( WriteSubject, "CALL test.allowedSchemaProcedure", "Schema operations are not allowed for user 'writeSubject' with roles [publisher]." );
			  AssertFail( mats, "CALL test.numNodes", "Read operations are not allowed for user 'mats' with roles [failer]." );
			  // UDFs
			  AssertFail( mats, "RETURN test.allowedFunction1()", "Read operations are not allowed for user 'mats' with roles [failer]." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowProcedureStartingTransactionInNewThread()
		 public virtual void ShouldAllowProcedureStartingTransactionInNewThread()
		 {
			  exceptionsInProcedure.clear();
			  DoubleLatch latch = new DoubleLatch( 2 );
			  ClassWithProcedures.DoubleLatch = latch;
			  latch.Start();
			  AssertEmpty( WriteSubject, "CALL test.threadTransaction" );
			  latch.FinishAndWaitForAllToFinish();
			  assertThat( exceptionsInProcedure.size(), equalTo(0) );
			  AssertSuccess( AdminSubject, "MATCH (:VeryUniqueLabel) RETURN toString(count(*)) as n", r => assertKeyIs( r, "n", "1" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInheritSecurityContextWhenProcedureStartingTransactionInNewThread()
		 public virtual void ShouldInheritSecurityContextWhenProcedureStartingTransactionInNewThread()
		 {
			  exceptionsInProcedure.clear();
			  DoubleLatch latch = new DoubleLatch( 2 );
			  ClassWithProcedures.DoubleLatch = latch;
			  latch.Start();
			  AssertEmpty( ReadSubject, "CALL test.threadReadDoingWriteTransaction" );
			  latch.FinishAndWaitForAllToFinish();
			  assertThat( exceptionsInProcedure.size(), equalTo(1) );
			  assertThat( exceptionsInProcedure.get( 0 ).Message, containsString( WriteOpsNotAllowed ) );
			  AssertSuccess( AdminSubject, "MATCH (:VeryUniqueLabel) RETURN toString(count(*)) as n", r => assertKeyIs( r, "n", "0" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetCorrectUnAuthenticatedPermissions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetCorrectUnAuthenticatedPermissions()
		 {
			  S unknownUser = Neo.login( "Batman", "Matban" );
			  AssertFail( unknownUser, "MATCH (n) RETURN n", "" );

			  unknownUser = Neo.login( "Batman", "Matban" );
			  AssertFail( unknownUser, "CREATE (:Node)", "" );

			  unknownUser = Neo.login( "Batman", "Matban" );
			  AssertFail( unknownUser, "CREATE INDEX ON :Node(number)", "" );

			  unknownUser = Neo.login( "Batman", "Matban" );
			  AssertFail( unknownUser, "CALL dbms.security.changePassword( '321' )", "" );

			  unknownUser = Neo.login( "Batman", "Matban" );
			  AssertFail( unknownUser, "CALL dbms.security.createUser('Henrik', 'bar', true)", "" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetCorrectPasswordChangeRequiredPermissions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetCorrectPasswordChangeRequiredPermissions()
		 {
			  TestFailRead( PwdSubject, 3, PwdReqErrMsg( ReadOpsNotAllowed ) );
			  TestFailWrite( PwdSubject, PwdReqErrMsg( WriteOpsNotAllowed ) );
			  TestFailSchema( PwdSubject, PwdReqErrMsg( SchemaOpsNotAllowed ) );
			  AssertPasswordChangeWhenPasswordChangeRequired( PwdSubject, "321" );

			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', true)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + ARCHITECT + "', 'Henrik')" );
			  S henrik = Neo.login( "Henrik", "bar" );
			  Neo.assertPasswordChangeRequired( henrik );
			  TestFailRead( henrik, 3, PwdReqErrMsg( ReadOpsNotAllowed ) );
			  TestFailWrite( henrik, PwdReqErrMsg( WriteOpsNotAllowed ) );
			  TestFailSchema( henrik, PwdReqErrMsg( SchemaOpsNotAllowed ) );
			  AssertPasswordChangeWhenPasswordChangeRequired( henrik, "321" );

			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Olivia', 'bar', true)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + ADMIN + "', 'Olivia')" );
			  S olivia = Neo.login( "Olivia", "bar" );
			  Neo.assertPasswordChangeRequired( olivia );
			  TestFailRead( olivia, 3, PwdReqErrMsg( ReadOpsNotAllowed ) );
			  TestFailWrite( olivia, PwdReqErrMsg( WriteOpsNotAllowed ) );
			  TestFailSchema( olivia, PwdReqErrMsg( SchemaOpsNotAllowed ) );
			  AssertFail( olivia, "CALL dbms.security.createUser('OliviasFriend', 'bar', false)", ChangePwdErrMsg );
			  AssertPasswordChangeWhenPasswordChangeRequired( olivia, "321" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetCorrectNoRolePermissions()
		 public virtual void ShouldSetCorrectNoRolePermissions()
		 {
			  TestFailRead( NoneSubject, 3 );
			  TestFailWrite( NoneSubject );
			  TestFailSchema( NoneSubject );
			  TestFailCreateUser( NoneSubject, PERMISSION_DENIED );
			  AssertEmpty( NoneSubject, "CALL dbms.security.changePassword( '321' )" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetCorrectReaderPermissions()
		 public virtual void ShouldSetCorrectReaderPermissions()
		 {
			  TestSuccessfulRead( ReadSubject, 3 );
			  TestFailWrite( ReadSubject );
			  TestFailTokenWrite( ReadSubject, WriteOpsNotAllowed );
			  TestFailSchema( ReadSubject );
			  TestFailCreateUser( ReadSubject, PERMISSION_DENIED );
			  AssertEmpty( ReadSubject, "CALL dbms.security.changePassword( '321' )" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetCorrectEditorPermissions()
		 public virtual void ShouldSetCorrectEditorPermissions()
		 {
			  TestSuccessfulRead( EditorSubject, 3 );
			  TestSuccessfulWrite( EditorSubject );
			  TestFailTokenWrite( EditorSubject );
			  TestFailSchema( EditorSubject );
			  TestFailCreateUser( EditorSubject, PERMISSION_DENIED );
			  AssertEmpty( EditorSubject, "CALL dbms.security.changePassword( '321' )" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetCorrectPublisherPermissions()
		 public virtual void ShouldSetCorrectPublisherPermissions()
		 {
			  TestSuccessfulRead( WriteSubject, 3 );
			  TestSuccessfulWrite( WriteSubject );
			  TestSuccessfulTokenWrite( WriteSubject );
			  TestFailSchema( WriteSubject );
			  TestFailCreateUser( WriteSubject, PERMISSION_DENIED );
			  AssertEmpty( WriteSubject, "CALL dbms.security.changePassword( '321' )" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetCorrectSchemaPermissions()
		 public virtual void ShouldSetCorrectSchemaPermissions()
		 {
			  TestSuccessfulRead( SchemaSubject, 3 );
			  TestSuccessfulWrite( SchemaSubject );
			  TestSuccessfulTokenWrite( SchemaSubject );
			  TestSuccessfulSchema( SchemaSubject );
			  TestFailCreateUser( SchemaSubject, PERMISSION_DENIED );
			  AssertEmpty( SchemaSubject, "CALL dbms.security.changePassword( '321' )" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetCorrectAdminPermissions()
		 public virtual void ShouldSetCorrectAdminPermissions()
		 {
			  TestSuccessfulRead( AdminSubject, 3 );
			  TestSuccessfulWrite( AdminSubject );
			  TestSuccessfulTokenWrite( AdminSubject );
			  TestSuccessfulSchema( AdminSubject );
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Olivia', 'bar', true)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.changePassword( '321' )" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetCorrectMultiRolePermissions()
		 public virtual void ShouldSetCorrectMultiRolePermissions()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + READER + "', 'schemaSubject')" );

			  TestSuccessfulRead( SchemaSubject, 3 );
			  TestSuccessfulWrite( SchemaSubject );
			  TestSuccessfulSchema( SchemaSubject );
			  TestFailCreateUser( SchemaSubject, PERMISSION_DENIED );
			  AssertEmpty( SchemaSubject, "CALL dbms.security.changePassword( '321' )" );
		 }
	}

}