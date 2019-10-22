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
	using Test = org.junit.Test;


	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.security.AuthorizationViolationException.PERMISSION_DENIED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.security.enterprise.auth.plugin.api.PredefinedRoles.ADMIN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.security.enterprise.auth.plugin.api.PredefinedRoles.ARCHITECT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.security.enterprise.auth.plugin.api.PredefinedRoles.PUBLISHER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.security.enterprise.auth.plugin.api.PredefinedRoles.READER;

	public abstract class AuthScenariosInteractionTestBase<S> : ProcedureInteractionTestBase<S>
	{

		 //---------- User creation -----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readOperationsShouldNotBeAllowedWhenPasswordChangeRequired() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadOperationsShouldNotBeAllowedWhenPasswordChangeRequired()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', true)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + READER + "', 'Henrik')" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertPasswordChangeRequired( subject );
			  TestFailRead( subject, 3, PwdReqErrMsg( ReadOpsNotAllowed ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void passwordChangeShouldEnableRolePermissions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PasswordChangeShouldEnableRolePermissions()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', true)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + READER + "', 'Henrik')" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertPasswordChangeRequired( subject );
			  AssertPasswordChangeWhenPasswordChangeRequired( subject, "foo" );
			  subject = Neo.login( "Henrik", "foo" );
			  Neo.assertAuthenticated( subject );
			  TestFailWrite( subject );
			  TestSuccessfulRead( subject, 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void loginShouldFailWithIncorrectPassword() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LoginShouldFailWithIncorrectPassword()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', true)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + READER + "', 'Henrik')" );
			  S subject = Neo.login( "Henrik", "foo" );
			  Neo.assertInitFailed( subject );
		 }

		 /*
		  * Logging scenario smoke test
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogSecurityEvents() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogSecurityEvents()
		 {
			  S mats = Neo.login( "mats", "Neo4Net" );
			  // for REST, login doesn't happen until the subject does something
			  Neo.executeQuery(mats, "UNWIND [] AS i RETURN 1", Collections.emptyMap(), r =>
			  {
			  });
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('mats', 'Neo4Net', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.createRole('role1')" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.deleteRole('role1')" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('reader', 'mats')" );
			  mats = Neo.login( "mats", "Neo4Net" );
			  AssertEmpty( mats, "MATCH (n) WHERE id(n) < 0 RETURN 1" );
			  AssertFail( mats, "CALL dbms.security.changeUserPassword('Neo4Net', 'hackerPassword')", PERMISSION_DENIED );
			  AssertFail( mats, "CALL dbms.security.changeUserPassword('mats', '')", "A password cannot be empty." );
			  AssertEmpty( mats, "CALL dbms.security.changeUserPassword('mats', 'hackerPassword')" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.removeRoleFromUser('reader', 'mats')" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.deleteUser('mats')" );

			  // flush log
			  Neo.LocalGraph.shutdown();

			  // assert on log content
			  SecurityLog log = new SecurityLog( this );
			  log.Load();

			  log.AssertHasLine( "mats", "failed to log in" );
			  log.AssertHasLine( "adminSubject", "created user `mats`" );
			  log.AssertHasLine( "adminSubject", "created role `role1`" );
			  log.AssertHasLine( "adminSubject", "deleted role `role1`" );
			  log.AssertHasLine( "mats", "logged in" );
			  log.AssertHasLine( "adminSubject", "added role `reader` to user `mats`" );
			  log.AssertHasLine( "mats", "tried to change password for user `Neo4Net`: " + PERMISSION_DENIED );
			  log.AssertHasLine( "mats", "tried to change password: A password cannot be empty." );
			  log.AssertHasLine( "mats", "changed password" );
			  log.AssertHasLine( "adminSubject", "removed role `reader` from user `mats`" );
			  log.AssertHasLine( "adminSubject", "deleted user `mats`" );
		 }

		 /*
		 Admin creates user Henrik with password bar
		 Henrik logs in with correct password (gets prompted to change - change to foo)
		 Henrik starts read transaction → permission denied
		 Admin adds user Henrik to role Reader
		 Henrik starts write transaction → permission denied
		 Henrik starts read transaction → ok
		 Henrik logs off
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userCreation2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UserCreation2()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', true)" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertPasswordChangeRequired( subject );
			  AssertPasswordChangeWhenPasswordChangeRequired( subject, "foo" );
			  subject = Neo.login( "Henrik", "foo" );
			  Neo.assertAuthenticated( subject );
			  TestFailRead( subject, 3 );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + READER + "', 'Henrik')" );
			  TestFailWrite( subject );
			  TestSuccessfulRead( subject, 3 );
		 }

		 /*
		 Admin creates user Henrik with password bar
		 Henrik logs in with correct password
		 Henrik starts read transaction → permission denied
		 Admin adds user Henrik to role Publisher
		 Henrik starts write transaction → ok
		 Henrik starts read transaction → ok
		 Henrik starts schema transaction → permission denied
		 Henrik logs off
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userCreation3() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UserCreation3()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( subject );
			  TestFailRead( subject, 3 );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'Henrik')" );
			  TestSuccessfulWrite( subject );
			  TestSuccessfulRead( subject, 4 );
			  TestFailSchema( subject );
		 }

		 /*
		 Admin creates user Henrik with password bar
		 Henrik logs in with correct password
		 Henrik starts read transaction → permission denied
		 Henrik starts write transaction → permission denied
		 Henrik starts schema transaction → permission denied
		 Henrik creates user Craig → permission denied
		 Admin adds user Henrik to role Architect
		 Henrik starts write transaction → ok
		 Henrik starts read transaction → ok
		 Henrik starts schema transaction → ok
		 Henrik creates user Craig → permission denied
		 Henrik logs off
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userCreation4() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UserCreation4()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( subject );
			  TestFailRead( subject, 3 );
			  TestFailWrite( subject );
			  TestFailSchema( subject );
			  TestFailCreateUser( subject, PERMISSION_DENIED );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + ARCHITECT + "', 'Henrik')" );
			  TestSuccessfulWrite( subject );
			  TestSuccessfulRead( subject, 4 );
			  TestSuccessfulSchema( subject );
			  TestFailCreateUser( subject, PERMISSION_DENIED );
		 }

		 /*
		 Admin creates user Henrik with password bar
		 Admin adds user Henrik to role Publisher
		 Henrik logs in with correct password
		 Henrik creates user Craig → permission denied
		 Henrik logs off
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userCreation5() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UserCreation5()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'Henrik')" );
			  S subject = Neo.login( "Henrik", "bar" );
			  TestFailCreateUser( subject, PERMISSION_DENIED );
		 }

		 //---------- User deletion -----------

		 /*
		 Admin creates user Henrik with password bar
		 Admin deletes user Henrik
		 Henrik logs in with correct password → fail
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userDeletion1() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UserDeletion1()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.deleteUser('Henrik')" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertInitFailed( subject );
		 }

		 /*
		 Admin creates user Henrik with password bar
		 Admin deletes user Henrik
		 Admin adds user Henrik to role Publisher → fail
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userDeletion2()
		 public virtual void UserDeletion2()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.deleteUser('Henrik')" );
			  AssertFail( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'Henrik')", "User 'Henrik' does not exist" );
		 }

		 /*
		 Admin creates user Henrik with password bar
		 Admin adds user Henrik to role Publisher
		 Admin deletes user Henrik
		 Admin removes user Henrik from role Publisher → fail
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userDeletion3()
		 public virtual void UserDeletion3()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'Henrik')" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.deleteUser('Henrik')" );
			  AssertFail( AdminSubject, "CALL dbms.security.removeRoleFromUser('" + PUBLISHER + "', 'Henrik')", "User 'Henrik' does not exist" );
		 }

		 /*
		 Admin creates user Henrik with password bar
		 Admin adds user Henrik to role Publisher
		 User Henrik logs in with correct password → ok
		 Admin deletes user Henrik
		 Henrik starts transaction with read query → fail
		 Henrik tries to login again → fail
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userDeletion4() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UserDeletion4()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'Henrik')" );
			  S henrik = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( henrik );
			  AssertEmpty( AdminSubject, "CALL dbms.security.deleteUser('Henrik')" );
			  Neo.assertSessionKilled( henrik );
			  henrik = Neo.login( "Henrik", "bar" );
			  Neo.assertInitFailed( henrik );
		 }

		 //---------- Role management -----------

		 /*
		 Admin creates user Henrik with password bar
		 Admin adds user Henrik to role Publisher
		 Henrik logs in with correct password
		 Henrik starts transaction with write query → ok
		 Admin removes user Henrik from role Publisher
		 Henrik starts transaction with read query → permission denied
		 Admin adds Henrik to role Reader
		 Henrik starts transaction with write query → permission denied
		 Henrik starts transaction with read query → ok
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void roleManagement1() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RoleManagement1()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'Henrik')" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( subject );
			  TestSuccessfulWrite( subject );
			  AssertEmpty( AdminSubject, "CALL dbms.security.removeRoleFromUser('" + PUBLISHER + "', 'Henrik')" );
			  TestFailRead( subject, 4 );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + READER + "', 'Henrik')" );
			  TestFailWrite( subject );
			  TestSuccessfulRead( subject, 4 );
		 }

		 /*
		 Admin creates user Henrik with password bar
		 Henrik logs in with correct password
		 Henrik starts transaction with write query → permission denied
		 Admin adds user Henrik to role Publisher → ok
		 Admin adds user Henrik to role Publisher → ok
		 Henrik starts transaction with write query → ok
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void roleManagement2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RoleManagement2()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( subject );
			  TestFailWrite( subject );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'Henrik')" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'Henrik')" );
			  TestSuccessfulWrite( subject );
		 }

		 /*
		 Admin creates user Henrik with password bar
		 Admin adds user Henrik to role Publisher
		 Henrik logs in with correct password
		 Admin adds user Henrik to role Reader
		 Henrik starts transaction with write query → ok
		 Henrik starts transaction with read query → ok
		 Admin removes user Henrik from role Publisher
		 Henrik starts transaction with write query → permission denied
		 Henrik starts transaction with read query → ok
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void roleManagement3() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RoleManagement3()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'Henrik')" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( subject );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + READER + "', 'Henrik')" );
			  TestSuccessfulWrite( subject );
			  TestSuccessfulRead( subject, 4 );
			  AssertEmpty( AdminSubject, "CALL dbms.security.removeRoleFromUser('" + PUBLISHER + "', 'Henrik')" );
			  TestFailWrite( subject );
			  TestSuccessfulRead( subject, 4 );
		 }

		 /*
		 Admin creates user Henrik with password bar
		 Admin adds user Henrik to role Publisher
		 Henrik logs in with correct password
		 Admin adds user Henrik to role Reader
		 Henrik starts transaction with write query → ok
		 Henrik starts transaction with read query → ok
		 Admin removes user Henrik from all roles
		 Henrik starts transaction with write query → permission denied
		 Henrik starts transaction with read query → permission denied
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void roleManagement4() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RoleManagement4()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'Henrik')" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( subject );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + READER + "', 'Henrik')" );
			  TestSuccessfulWrite( subject );
			  TestSuccessfulRead( subject, 4 );
			  AssertEmpty( AdminSubject, "CALL dbms.security.removeRoleFromUser('" + READER + "', 'Henrik')" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.removeRoleFromUser('" + PUBLISHER + "', 'Henrik')" );
			  TestFailWrite( subject );
			  TestFailRead( subject, 4 );
		 }

		 /*
		 Admin creates user Henrik with password bar
		 Admin adds user Henrik to role Publisher
		 Henrik logs in with correct password
		 Henrik starts transaction with long running writing query Q
		 Admin removes user Henrik from role Publisher (while Q still running)
		 Q finishes and transaction is committed → ok
		 Henrik starts new transaction with write query → permission denied
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void roleManagement5() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RoleManagement5()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'Henrik')" );
			  S henrik = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( henrik );

			  DoubleLatch latch = new DoubleLatch( 2 );
			  ThreadedTransaction<S> write = new ThreadedTransaction<S>( Neo, latch );
			  write.ExecuteCreateNode( ThreadingConflict, henrik );
			  latch.StartAndWaitForAllToStart();

			  AssertEmpty( AdminSubject, "CALL dbms.security.removeRoleFromUser('" + PUBLISHER + "', 'Henrik')" );

			  latch.FinishAndWaitForAllToFinish();

			  write.CloseAndAssertSuccess();
			  TestFailWrite( henrik );
		 }

		 /*
		  * Procedure 'test.allowedReadProcedure' with READ mode and 'allowed = role1' is loaded.
		  * Procedure 'test.allowedWriteProcedure' with WRITE mode and 'allowed = role1' is loaded.
		  * Procedure 'test.allowedSchemaProcedure' with SCHEMA mode and 'allowed = role1' is loaded.
		  * Admin creates a new user 'mats'.
		  * 'mats' logs in.
		  * 'mats' executes the procedures, access denied.
		  * Admin creates 'role1'.
		  * 'mats' executes the procedures, access denied.
		  * Admin adds role 'role1' to 'mats'.
		  * 'mats' executes the procedures successfully.
		  * Admin removes the role 'role1'.
		  * 'mats' executes the procedures, access denied.
		  * Admin creates the role 'role1' again (new).
		  * 'mats' executes the procedures, access denied.
		  * Admin adds role 'architect' to 'mats'.
		  * 'mats' executes the procedures successfully.
		  * Admin adds 'role1' to 'mats'.
		  * 'mats' executes the procedures successfully.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void customRoleWithProcedureAccess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CustomRoleWithProcedureAccess()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('mats', 'Neo4Net', false)" );
			  S mats = Neo.login( "mats", "Neo4Net" );
			  TestFailTestProcs( mats );
			  AssertEmpty( AdminSubject, "CALL dbms.security.createRole('role1')" );
			  TestFailTestProcs( mats );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('role1', 'mats')" );
			  TestSuccessfulTestProcs( mats );
			  AssertEmpty( AdminSubject, "CALL dbms.security.deleteRole('role1')" );
			  TestFailTestProcs( mats );
			  AssertEmpty( AdminSubject, "CALL dbms.security.createRole('role1')" );
			  TestFailTestProcs( mats );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('architect', 'mats')" );
			  TestSuccessfulTestProcs( mats );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('role1', 'mats')" );
			  TestSuccessfulTestProcs( mats );
		 }

		 //---------- User suspension -----------

		 /*
		 Admin creates user Henrik with password bar
		 Henrik logs in with correct password → ok
		 Henrik logs off
		 Admin suspends user Henrik
		 User Henrik logs in with correct password → fail
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userSuspension1() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UserSuspension1()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( subject );
			  Neo.logout( subject );
			  AssertEmpty( AdminSubject, "CALL dbms.security.suspendUser('Henrik')" );
			  subject = Neo.login( "Henrik", "bar" );
			  Neo.assertInitFailed( subject );
		 }

		 /*
		 Admin creates user Henrik with password bar
		 Admin adds user Henrik to role Reader
		 Henrik logs in with correct password → ok
		 Henrik starts and completes transaction with read query → ok
		 Admin suspends user Henrik
		 Henrik’s session is terminated
		 Henrik logs in with correct password → fail
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userSuspension2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UserSuspension2()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + READER + "', 'Henrik')" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( subject );
			  TestSuccessfulRead( subject, 3 );
			  AssertEmpty( AdminSubject, "CALL dbms.security.suspendUser('Henrik')" );

			  Neo.assertSessionKilled( subject );

			  subject = Neo.login( "Henrik", "bar" );
			  Neo.assertInitFailed( subject );
		 }

		 //---------- User activation -----------

		 /*
		 Admin creates user Henrik with password bar
		 Admin suspends user Henrik
		 Henrik logs in with correct password → fail
		 Admin reinstates user Henrik
		 Henrik logs in with correct password → ok
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userActivation1() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UserActivation1()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.suspendUser('Henrik')" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertInitFailed( subject );
			  AssertEmpty( AdminSubject, "CALL dbms.security.activateUser('Henrik', false)" );
			  subject = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( subject );
		 }

		 //---------- list users / roles -----------

		 /*
		 Admin lists all users → ok
		 Admin creates user Henrik with password bar
		 Admin lists all users → ok
		 Henrik logs in with correct password → ok
		 Henrik lists all users → permission denied
		 Admin adds user Henrik to role Admin
		 Henrik lists all users → ok
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userListing() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UserListing()
		 {
			  TestSuccessfulListUsers( AdminSubject, InitialUsers );
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  TestSuccessfulListUsers( AdminSubject, With( InitialUsers, "Henrik" ) );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( subject );
			  TestFailListUsers( subject, 6, PERMISSION_DENIED );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + ADMIN + "', 'Henrik')" );
			  TestSuccessfulListUsers( subject, With( InitialUsers, "Henrik" ) );
		 }

		 /*
		 Admin creates user Henrik with password bar
		 Henrik logs in with correct password → ok
		 Henrik lists all roles → permission denied
		 Admin lists all roles → ok
		 Admin adds user Henrik to role Admin
		 Henrik lists all roles → ok
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rolesListing() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RolesListing()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( subject );
			  TestFailListRoles( subject, PERMISSION_DENIED );
			  TestSuccessfulListRoles( AdminSubject, InitialRoles );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + ADMIN + "', 'Henrik')" );
			  TestSuccessfulListRoles( subject, InitialRoles );
		 }

		 /*
		 Admin creates user Henrik with password bar
		 Admin creates user Craig
		 Admin adds user Craig to role Publisher
		 Henrik logs in with correct password → ok
		 Henrik lists all roles for user Craig → permission denied
		 Admin lists all roles for user Craig → ok
		 Admin adds user Henrik to role Publisher
		 Craig logs in with correct password → ok
		 Craig lists all roles for user Craig → ok
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listingUserRoles() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListingUserRoles()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Craig', 'foo', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'Craig')" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( subject );

			  TestFailListUserRoles( subject, "Craig", PERMISSION_DENIED );
			  AssertSuccess( AdminSubject, "CALL dbms.security.listRolesForUser('Craig') YIELD value as roles RETURN roles", r => assertKeyIs( r, "roles", PUBLISHER ) );

			  S craigSubject = Neo.login( "Craig", "foo" );
			  AssertSuccess( craigSubject, "CALL dbms.security.listRolesForUser('Craig') YIELD value as roles RETURN roles", r => assertKeyIs( r, "roles", PUBLISHER ) );
		 }

		 /*
		 Admin creates user Henrik with password bar
		 Admin creates user Craig
		 Admin adds user Henrik to role Publisher
		 Admin adds user Craig to role Publisher
		 Henrik logs in with correct password → ok
		 Henrik lists all users for role Publisher → permission denied
		 Admin lists all users for role Publisher → ok
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listingRoleUsers() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListingRoleUsers()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Craig', 'foo', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'Craig')" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'Henrik')" );
			  S subject = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( subject );
			  TestFailListRoleUsers( subject, PUBLISHER, PERMISSION_DENIED );
			  AssertSuccess( AdminSubject, "CALL dbms.security.listUsersForRole('" + PUBLISHER + "') YIELD value as users RETURN users", r => assertKeyIs( r, "users", "Henrik", "Craig", "writeSubject" ) );
		 }

		 //---------- calling procedures -----------

		 /*
		 Admin creates user Henrik with password bar
		 Admin adds user Henrik to role Publisher
		 Henrik logs in with correct password → ok
		 Henrik calls procedure marked as read-only → ok
		 Henrik calls procedure marked as read-write → ok
		 Admin adds user Henrik to role Reader
		 Henrik calls procedure marked as read-only → ok
		 Henrik calls procedure marked as read-write → ok
		 Admin removes Henrik from role Publisher
		 Henrik calls procedure marked as read-only → ok
		 Henrik calls procedure marked as read-write → permission denied
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void callProcedures1() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CallProcedures1()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'bar', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + PUBLISHER + "', 'Henrik')" );
			  S henrik = Neo.login( "Henrik", "bar" );
			  Neo.assertAuthenticated( henrik );

			  AssertEmpty( henrik, "CALL test.createNode()" );
			  AssertSuccess( henrik, "CALL test.numNodes() YIELD count as count RETURN count", r => assertKeyIs( r, "count", "4" ) );

			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + READER + "', 'Henrik')" );

			  AssertEmpty( henrik, "CALL test.createNode()" );
			  AssertSuccess( henrik, "CALL test.numNodes() YIELD count as count RETURN count", r => assertKeyIs( r, "count", "5" ) );

			  AssertEmpty( AdminSubject, "CALL dbms.security.removeRoleFromUser('" + PUBLISHER + "', 'Henrik')" );

			  AssertFail( henrik, "CALL test.createNode()", "Write operations are not allowed for user 'Henrik' with roles [reader]." );
		 }

		 //---------- change password -----------

		 /*
		 Admin creates user Henrik with password abc
		 Admin adds user Henrik to role Reader
		 Henrik logs in with correct password → ok
		 Henrik starts transaction with read query → ok
		 Henrik changes password to 123
		 Henrik starts transaction with read query → ok
		 Henrik logs out
		 Henrik logs in with password abc → fail
		 Henrik logs in with password 123 → ok
		 Henrik starts transaction with read query → ok
		 Henrik logs out
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void changeUserPassword1() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChangeUserPassword1()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'abc', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + READER + "', 'Henrik')" );
			  S subject = Neo.login( "Henrik", "abc" );
			  Neo.assertAuthenticated( subject );
			  TestSuccessfulRead( subject, 3 );
			  AssertEmpty( subject, "CALL dbms.security.changeUserPassword('Henrik', '123', false)" );
			  Neo.updateAuthToken( subject, "Henrik", "123" ); // Because RESTSubject caches an auth token that is sent with every request
			  TestSuccessfulRead( subject, 3 );
			  Neo.logout( subject );
			  subject = Neo.login( "Henrik", "abc" );
			  Neo.assertInitFailed( subject );
			  subject = Neo.login( "Henrik", "123" );
			  Neo.assertAuthenticated( subject );
			  TestSuccessfulRead( subject, 3 );
		 }

		 /*
		 Admin creates user Henrik with password abc
		 Admin adds user Henrik to role Reader
		 Henrik logs in with password abc → ok
		 Henrik starts transaction with read query → ok
		 Admin changes user Henrik’s password to 123
		 Henrik logs out
		 Henrik logs in with password abc → fail
		 Henrik logs in with password 123 → ok
		 Henrik starts transaction with read query → ok
		 Henrik logs out
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void changeUserPassword2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChangeUserPassword2()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'abc', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + READER + "', 'Henrik')" );
			  S subject = Neo.login( "Henrik", "abc" );
			  Neo.assertAuthenticated( subject );
			  TestSuccessfulRead( subject, 3 );
			  AssertEmpty( AdminSubject, "CALL dbms.security.changeUserPassword('Henrik', '123', false)" );
			  Neo.logout( subject );
			  subject = Neo.login( "Henrik", "abc" );
			  Neo.assertInitFailed( subject );
			  subject = Neo.login( "Henrik", "123" );
			  Neo.assertAuthenticated( subject );
			  TestSuccessfulRead( subject, 3 );
		 }

		 /*
		 Admin creates user Henrik with password abc
		 Admin creates user Craig
		 Admin adds user Henrik to role Reader
		 Henrik logs in with password abc → ok
		 Henrik starts transaction with read query → ok
		 Henrik changes Craig’s password to 123 → fail
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void changeUserPassword3() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChangeUserPassword3()
		 {
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Craig', 'abc', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.createUser('Henrik', 'abc', false)" );
			  AssertEmpty( AdminSubject, "CALL dbms.security.addRoleToUser('" + READER + "', 'Henrik')" );
			  S subject = Neo.login( "Henrik", "abc" );
			  Neo.assertAuthenticated( subject );
			  TestSuccessfulRead( subject, 3 );
			  AssertFail( subject, "CALL dbms.security.changeUserPassword('Craig', '123')", PERMISSION_DENIED );
		 }

		 // OTHER TESTS

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTryToCreateTokensWhenReading()
		 public virtual void ShouldNotTryToCreateTokensWhenReading()
		 {
			  AssertEmpty( AdminSubject, "CREATE (:MyNode)" );

			  AssertSuccess( ReadSubject, "MATCH (n:MyNode) WHERE n.nonExistent = 'foo' RETURN toString(count(*)) AS c", r => assertKeyIs( r, "c", "0" ) );
			  AssertFail( ReadSubject, "MATCH (n:MyNode) SET n.nonExistent = 'foo' RETURN toString(count(*)) AS c", TokenCreateOpsNotAllowed );
			  AssertFail( ReadSubject, "MATCH (n:MyNode) SET n:Foo RETURN toString(count(*)) AS c", TokenCreateOpsNotAllowed );
			  AssertSuccess( SchemaSubject, "MATCH (n:MyNode) SET n.nonExistent = 'foo' RETURN toString(count(*)) AS c", r => assertKeyIs( r, "c", "1" ) );
			  AssertSuccess( ReadSubject, "MATCH (n:MyNode) WHERE n.nonExistent = 'foo' RETURN toString(count(*)) AS c", r => assertKeyIs( r, "c", "1" ) );
		 }

		 private class SecurityLog
		 {
			 private readonly AuthScenariosInteractionTestBase<S> _outerInstance;

			 public SecurityLog( AuthScenariosInteractionTestBase<S> outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal IList<string> Lines;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void load() throws java.io.IOException
			  internal virtual void Load()
			  {
					File securityLog = new File( _outerInstance.securityLog.AbsolutePath );
					using ( FileSystemAbstraction fileSystem = outerInstance.Neo.fileSystem(), StreamReader bufferedReader = new StreamReader(fileSystem.OpenAsReader(securityLog, StandardCharsets.UTF_8)) )
					{
						 Lines = bufferedReader.lines().collect(java.util.stream.Collectors.toList());
					}
			  }

			  internal virtual void AssertHasLine( string subject, string msg )
			  {
					Objects.requireNonNull( Lines );
					assertThat( Lines, hasItem( containsString( "[" + subject + "]: " + msg ) ) );
			  }
		 }
	}

}