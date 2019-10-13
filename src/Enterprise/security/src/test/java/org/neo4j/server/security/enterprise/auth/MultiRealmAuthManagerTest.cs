using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
	using MemoryConstrainedCacheManager = org.apache.shiro.cache.MemoryConstrainedCacheManager;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using SetDefaultAdminCommand = Neo4Net.CommandLine.Admin.security.SetDefaultAdminCommand;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using AuthSubject = Neo4Net.@internal.Kernel.Api.security.AuthSubject;
	using AuthenticationResult = Neo4Net.@internal.Kernel.Api.security.AuthenticationResult;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using SecurityContext = Neo4Net.@internal.Kernel.Api.security.SecurityContext;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using AuthManager = Neo4Net.Kernel.api.security.AuthManager;
	using AuthToken = Neo4Net.Kernel.api.security.AuthToken;
	using PasswordPolicy = Neo4Net.Kernel.api.security.PasswordPolicy;
	using InvalidAuthTokenException = Neo4Net.Kernel.api.security.exception.InvalidAuthTokenException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using User = Neo4Net.Kernel.impl.security.User;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using Log = Neo4Net.Logging.Log;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using AuthenticationStrategy = Neo4Net.Server.Security.Auth.AuthenticationStrategy;
	using CommunitySecurityModule = Neo4Net.Server.Security.Auth.CommunitySecurityModule;
	using InitialUserTest = Neo4Net.Server.Security.Auth.InitialUserTest;
	using LegacyCredential = Neo4Net.Server.Security.Auth.LegacyCredential;
	using UserRepository = Neo4Net.Server.Security.Auth.UserRepository;
	using PredefinedRoles = Neo4Net.Server.security.enterprise.auth.plugin.api.PredefinedRoles;
	using SecurityLog = Neo4Net.Server.security.enterprise.log.SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Strings.escape;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.BasicAuthManagerTest.clearedPasswordWithSameLenghtAs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.BasicAuthManagerTest.password;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.SecurityTestUtils.authToken;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertException;

	public class MultiRealmAuthManagerTest : InitialUserTest
	{
		 private AuthenticationStrategy _authStrategy;
		 private MultiRealmAuthManager _manager;
		 private EnterpriseUserManager _userManager;
		 private AssertableLogProvider _logProvider;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expect = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expect = ExpectedException.none();

		 private readonly System.Func<string, int> _token = s => -1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  Config = Config.defaults();
			  Users = CommunitySecurityModule.getUserRepository( Config, NullLogProvider.Instance, FsRule.get() );
			  _authStrategy = mock( typeof( AuthenticationStrategy ) );
			  _logProvider = new AssertableLogProvider();

			  _manager = CreateAuthManager( true );
			  _userManager = _manager.UserManager;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private MultiRealmAuthManager createAuthManager(boolean logSuccessfulAuthentications) throws Throwable
		 private MultiRealmAuthManager CreateAuthManager( bool logSuccessfulAuthentications )
		 {
			  Log log = _logProvider.getLog( this.GetType() );

			  InternalFlatFileRealm internalFlatFileRealm = new InternalFlatFileRealm( Users, new InMemoryRoleRepository(), mock(typeof(PasswordPolicy)), _authStrategy, mock(typeof(JobScheduler)), CommunitySecurityModule.getInitialUserRepository(Config, NullLogProvider.Instance, FsRule.get()), EnterpriseSecurityModule.GetDefaultAdminRepository(Config, NullLogProvider.Instance, FsRule.get()) );

			  _manager = new MultiRealmAuthManager( internalFlatFileRealm, Collections.singleton( internalFlatFileRealm ), new MemoryConstrainedCacheManager(), new SecurityLog(log), logSuccessfulAuthentications, false, Collections.emptyMap() );

			  _manager.init();
			  return _manager;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _manager.stop();
			  _manager.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeOnlyUserAdminIfNoRolesFile() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMakeOnlyUserAdminIfNoRolesFile()
		 {
			  // Given
			  Users.create( NewUser( "jake", "abc123", false ) );

			  // When
			  _manager.start();

			  // Then
			  assertThat( _manager.UserManager.getRoleNamesForUser( "jake" ), contains( PredefinedRoles.ADMIN ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeNeo4jUserAdminIfNoRolesFileButManyUsers() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMakeNeo4jUserAdminIfNoRolesFileButManyUsers()
		 {
			  // Given
			  Users.create( NewUser( "jake", "abc123", false ) );
			  Users.create( NewUser( "neo4j", "neo4j", false ) );

			  // When
			  _manager.start();

			  // Then
			  assertThat( _manager.UserManager.getRoleNamesForUser( "neo4j" ), contains( PredefinedRoles.ADMIN ) );
			  assertThat( _manager.UserManager.getRoleNamesForUser( "jake" ).Count, equalTo( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfNoRolesFileButManyUsersAndNoDefaultAdminOrNeo4j() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfNoRolesFileButManyUsersAndNoDefaultAdminOrNeo4j()
		 {
			  // Given
			  Users.create( NewUser( "jake", "abc123", false ) );
			  Users.create( NewUser( "jane", "123abc", false ) );

			  Expect.expect( typeof( InvalidArgumentsException ) );
			  Expect.expectMessage( "No roles defined, and cannot determine which user should be admin. " + "Please use `neo4j-admin " + SetDefaultAdminCommand.COMMAND_NAME + "` to select an admin." );

			  _manager.start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfNoRolesFileButManyUsersAndNonExistingDefaultAdmin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfNoRolesFileButManyUsersAndNonExistingDefaultAdmin()
		 {
			  // Given
			  UserRepository defaultAdminRepository = EnterpriseSecurityModule.GetDefaultAdminRepository( Config, NullLogProvider.Instance, FsRule.get() );
			  defaultAdminRepository.Start();
			  defaultAdminRepository.Create( ( new User.Builder( "foo", LegacyCredential.INACCESSIBLE ) ).withRequiredPasswordChange( false ).build() );
			  defaultAdminRepository.Shutdown();

			  Users.create( NewUser( "jake", "abc123", false ) );
			  Users.create( NewUser( "jane", "123abc", false ) );

			  Expect.expect( typeof( InvalidArgumentsException ) );
			  Expect.expectMessage( "No roles defined, and default admin user 'foo' does not exist. " + "Please use `neo4j-admin " + SetDefaultAdminCommand.COMMAND_NAME + "` to select a valid admin." );

			  _manager.start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindAndAuthenticateUserSuccessfully() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindAndAuthenticateUserSuccessfully()
		 {
			  // Given
			  Users.create( NewUser( "jake", "abc123", false ) );
			  _manager.start();
			  SetMockAuthenticationStrategyResult( "jake", "abc123", AuthenticationResult.SUCCESS );

			  // When
			  AuthenticationResult result = _manager.login( authToken( "jake", "abc123" ) ).subject().AuthenticationResult;

			  // Then
			  assertThat( result, equalTo( AuthenticationResult.SUCCESS ) );
			  _logProvider.assertExactly( Info( "[jake]: logged in" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogAuthenticationIfFlagSaysNo() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLogAuthenticationIfFlagSaysNo()
		 {
			  // Given
			  _manager.shutdown();
			  _manager = CreateAuthManager( false );

			  Users.create( NewUser( "jake", "abc123", false ) );
			  _manager.start();
			  SetMockAuthenticationStrategyResult( "jake", "abc123", AuthenticationResult.SUCCESS );

			  // When
			  AuthenticationResult result = _manager.login( authToken( "jake", "abc123" ) ).subject().AuthenticationResult;

			  // Then
			  assertThat( result, equalTo( AuthenticationResult.SUCCESS ) );
			  _logProvider.assertNone( Info( "[jake]: logged in" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTooManyAttemptsWhenThatIsAppropriate() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnTooManyAttemptsWhenThatIsAppropriate()
		 {
			  // Given
			  Users.create( NewUser( "jake", "abc123", true ) );
			  _manager.start();
			  SetMockAuthenticationStrategyResult( "jake", "wrong password", AuthenticationResult.TOO_MANY_ATTEMPTS );

			  // When
			  AuthSubject authSubject = _manager.login( authToken( "jake", "wrong password" ) ).subject();
			  AuthenticationResult result = authSubject.AuthenticationResult;

			  // Then
			  assertThat( result, equalTo( AuthenticationResult.TOO_MANY_ATTEMPTS ) );
			  _logProvider.assertExactly( Error( "[%s]: failed to log in: too many failed attempts", "jake" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindAndAuthenticateUserAndReturnPasswordChangeIfRequired() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindAndAuthenticateUserAndReturnPasswordChangeIfRequired()
		 {
			  // Given
			  Users.create( NewUser( "jake", "abc123", true ) );
			  _manager.start();
			  SetMockAuthenticationStrategyResult( "jake", "abc123", AuthenticationResult.SUCCESS );

			  // When
			  AuthenticationResult result = _manager.login( authToken( "jake", "abc123" ) ).subject().AuthenticationResult;

			  // Then
			  assertThat( result, equalTo( AuthenticationResult.PASSWORD_CHANGE_REQUIRED ) );
			  _logProvider.assertExactly( Info( "[jake]: logged in (password change required)" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenAuthTokenIsInvalid() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenAuthTokenIsInvalid()
		 {
			  _manager.start();

			  assertException( () => _manager.login(map(Neo4Net.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, "supercool", Neo4Net.Kernel.api.security.AuthToken_Fields.PRINCIPAL, "neo4j")), typeof(InvalidAuthTokenException), "Unsupported authentication token: { scheme='supercool', principal='neo4j' }" );

			  assertException( () => _manager.login(map(Neo4Net.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, "none")), typeof(InvalidAuthTokenException), "Unsupported authentication token, scheme='none' only allowed when auth is disabled: { scheme='none' }" );

			  assertException( () => _manager.login(map("key", "value")), typeof(InvalidAuthTokenException), "Unsupported authentication token, missing key `scheme`: { key='value' }" );

			  assertException( () => _manager.login(map(Neo4Net.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, "basic", Neo4Net.Kernel.api.security.AuthToken_Fields.PRINCIPAL, "neo4j")), typeof(InvalidAuthTokenException), "Unsupported authentication token, missing key `credentials`: { scheme='basic', principal='neo4j' }" );

			  assertException( () => _manager.login(map(Neo4Net.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, "basic", Neo4Net.Kernel.api.security.AuthToken_Fields.CREDENTIALS, "very-secret")), typeof(InvalidAuthTokenException), "Unsupported authentication token, missing key `principal`: { scheme='basic', credentials='******' }" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailAuthenticationIfUserIsNotFound() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailAuthenticationIfUserIsNotFound()
		 {
			  // Given
			  _manager.start();

			  // When
			  AuthSubject authSubject = _manager.login( authToken( "unknown", "abc123" ) ).subject();
			  AuthenticationResult result = authSubject.AuthenticationResult;

			  // Then
			  assertThat( result, equalTo( AuthenticationResult.FAILURE ) );
			  _logProvider.assertExactly( Error( "[%s]: failed to log in: %s", "unknown", "invalid principal or credentials" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailAuthenticationAndEscapeIfUserIsNotFound() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailAuthenticationAndEscapeIfUserIsNotFound()
		 {
			  // Given
			  _manager.start();

			  // When
			  AuthSubject authSubject = _manager.login( authToken( "unknown\n\t\r\"haxx0r\"", "abc123" ) ).subject();
			  AuthenticationResult result = authSubject.AuthenticationResult;

			  // Then
			  assertThat( result, equalTo( AuthenticationResult.FAILURE ) );
			  _logProvider.assertExactly( Error( "[%s]: failed to log in: %s", escape( "unknown\n\t\r\"haxx0r\"" ), "invalid principal or credentials" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateUser()
		 {
			  // Given
			  _manager.start();

			  // When
			  _userManager.newUser( "foo", password( "bar" ), true );

			  // Then
			  User user = Users.getUserByName( "foo" );
			  assertNotNull( user );
			  assertTrue( user.PasswordChangeRequired() );
			  assertTrue( user.Credentials().matchesPassword("bar") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeleteUser()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User user = newUser("jake", "abc123", true);
			  User user = NewUser( "jake", "abc123", true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User user2 = newUser("neo4j", "321cba", true);
			  User user2 = NewUser( "neo4j", "321cba", true );
			  Users.create( user );
			  Users.create( user2 );
			  _manager.start();

			  // When
			  _userManager.deleteUser( "jake" );

			  // Then
			  assertNull( Users.getUserByName( "jake" ) );
			  assertNotNull( Users.getUserByName( "neo4j" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailDeletingUnknownUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailDeletingUnknownUser()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User user = newUser("jake", "abc123", true);
			  User user = NewUser( "jake", "abc123", true );
			  Users.create( user );
			  _manager.start();

			  // When
			  assertException( () => _userManager.deleteUser("unknown"), typeof(InvalidArgumentsException), "User 'unknown' does not exist" );

			  // Then
			  assertNotNull( Users.getUserByName( "jake" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSuspendExistingUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSuspendExistingUser()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User user = newUser("jake", "abc123", true);
			  User user = NewUser( "jake", "abc123", true );
			  Users.create( user );
			  _manager.start();

			  // When
			  _userManager.suspendUser( "jake" );

			  // Then
			  SetMockAuthenticationStrategyResult( "jake", "abc123", AuthenticationResult.SUCCESS );
			  AuthenticationResult result = _manager.login( authToken( "jake", "abc123" ) ).subject().AuthenticationResult;
			  assertThat( result, equalTo( AuthenticationResult.FAILURE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldActivateExistingUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldActivateExistingUser()
		 {
			  // Given
			  Users.create( NewUser( "jake", "abc123", false ) );
			  _manager.start();

			  _userManager.suspendUser( "jake" );

			  // When
			  _userManager.activateUser( "jake", false );
			  SetMockAuthenticationStrategyResult( "jake", "abc123", AuthenticationResult.SUCCESS );

			  // Then
			  AuthenticationResult result = _manager.login( authToken( "jake", "abc123" ) ).subject().AuthenticationResult;
			  assertThat( result, equalTo( AuthenticationResult.SUCCESS ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSuspendSuspendedUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSuspendSuspendedUser()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User user = newUser("jake", "abc123", false);
			  User user = NewUser( "jake", "abc123", false );
			  Users.create( user );
			  _manager.start();
			  _userManager.suspendUser( "jake" );

			  // When
			  _userManager.suspendUser( "jake" );
			  SetMockAuthenticationStrategyResult( "jake", "abc123", AuthenticationResult.SUCCESS );

			  // Then
			  AuthenticationResult result = _manager.login( authToken( "jake", "abc123" ) ).subject().AuthenticationResult;
			  assertThat( result, equalTo( AuthenticationResult.FAILURE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldActivateActiveUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldActivateActiveUser()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User user = newUser("jake", "abc123", false);
			  User user = NewUser( "jake", "abc123", false );
			  Users.create( user );
			  _manager.start();
			  when( _authStrategy.authenticate( user, password( "abc123" ) ) ).thenReturn( AuthenticationResult.SUCCESS );

			  // When
			  _userManager.activateUser( "jake", false );
			  SetMockAuthenticationStrategyResult( "jake", "abc123", AuthenticationResult.SUCCESS );

			  // Then
			  AuthenticationResult result = _manager.login( authToken( "jake", "abc123" ) ).subject().AuthenticationResult;
			  assertThat( result, equalTo( AuthenticationResult.SUCCESS ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToSuspendNonExistingUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToSuspendNonExistingUser()
		 {
			  // Given
			  _manager.start();

			  // When
			  try
			  {
					_userManager.suspendUser( "jake" );
					fail( "Should throw exception on suspending unknown user" );
			  }
			  catch ( InvalidArgumentsException e )
			  {
					// Then
					assertThat( e.Message, containsString( "User 'jake' does not exist" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToActivateNonExistingUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToActivateNonExistingUser()
		 {
			  // Given
			  _manager.start();

			  // When
			  try
			  {
					_userManager.activateUser( "jake", false );
					fail( "Should throw exception on activating unknown user" );
			  }
			  catch ( InvalidArgumentsException e )
			  {
					// Then
					assertThat( e.Message, containsString( "User 'jake' does not exist" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetPassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetPassword()
		 {
			  // Given
			  Users.create( NewUser( "jake", "abc123", true ) );
			  _manager.start();

			  // When
			  _userManager.setUserPassword( "jake", password( "hello, world!" ), false );

			  // Then
			  User user = _userManager.getUser( "jake" );
			  assertTrue( user.Credentials().matchesPassword("hello, world!") );
			  assertThat( Users.getUserByName( "jake" ), equalTo( user ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRequestPasswordChangeWithInvalidCredentials() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotRequestPasswordChangeWithInvalidCredentials()
		 {
			  // Given
			  Users.create( NewUser( "neo", "abc123", true ) );
			  _manager.start();
			  SetMockAuthenticationStrategyResult( "neo", "abc123", AuthenticationResult.SUCCESS );
			  SetMockAuthenticationStrategyResult( "neo", "wrong", AuthenticationResult.FAILURE );

			  // When
			  AuthenticationResult result = _manager.login( authToken( "neo", "wrong" ) ).subject().AuthenticationResult;

			  // Then
			  assertThat( result, equalTo( AuthenticationResult.FAILURE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNullWhenSettingPasswordForUnknownUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnNullWhenSettingPasswordForUnknownUser()
		 {
			  // Given
			  _manager.start();

			  // When
			  try
			  {
					_userManager.setUserPassword( "unknown", password( "hello, world!" ), false );
					fail( "exception expected" );
			  }
			  catch ( InvalidArgumentsException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createTestUsers() throws Throwable
		 private void CreateTestUsers()
		 {
			  _userManager.newUser( "morpheus", password( "abc123" ), false );
			  _userManager.newRole( "admin", "morpheus" );
			  SetMockAuthenticationStrategyResult( "morpheus", "abc123", AuthenticationResult.SUCCESS );

			  _userManager.newUser( "trinity", password( "abc123" ), false );
			  _userManager.newRole( "architect", "trinity" );
			  SetMockAuthenticationStrategyResult( "trinity", "abc123", AuthenticationResult.SUCCESS );

			  _userManager.newUser( "tank", password( "abc123" ), false );
			  _userManager.newRole( "publisher", "tank" );
			  SetMockAuthenticationStrategyResult( "tank", "abc123", AuthenticationResult.SUCCESS );

			  _userManager.newUser( "neo", password( "abc123" ), false );
			  _userManager.newRole( "reader", "neo" );
			  SetMockAuthenticationStrategyResult( "neo", "abc123", AuthenticationResult.SUCCESS );

			  _userManager.newUser( "smith", password( "abc123" ), false );
			  _userManager.newRole( "agent", "smith" );
			  SetMockAuthenticationStrategyResult( "smith", "abc123", AuthenticationResult.SUCCESS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void defaultUserShouldHaveCorrectPermissions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DefaultUserShouldHaveCorrectPermissions()
		 {
			  // Given
			  _manager.start();
			  SetMockAuthenticationStrategyResult( "neo4j", "neo4j", AuthenticationResult.SUCCESS );

			  // When
			  SecurityContext securityContext = _manager.login( authToken( "neo4j", "neo4j" ) ).authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME );
			  _userManager.setUserPassword( "neo4j", password( "1234" ), false );
			  securityContext.Subject().logout();

			  SetMockAuthenticationStrategyResult( "neo4j", "1234", AuthenticationResult.SUCCESS );
			  securityContext = _manager.login( authToken( "neo4j", "1234" ) ).authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME );

			  // Then
			  assertTrue( securityContext.Mode().allowsReads() );
			  assertTrue( securityContext.Mode().allowsWrites() );
			  assertTrue( securityContext.Mode().allowsSchemaWrites() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userWithAdminRoleShouldHaveCorrectPermissions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UserWithAdminRoleShouldHaveCorrectPermissions()
		 {
			  // Given
			  CreateTestUsers();
			  _manager.start();

			  // When
			  SecurityContext securityContext = _manager.login( authToken( "morpheus", "abc123" ) ).authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME );

			  // Then
			  assertTrue( securityContext.Mode().allowsReads() );
			  assertTrue( securityContext.Mode().allowsWrites() );
			  assertTrue( securityContext.Mode().allowsSchemaWrites() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userWithArchitectRoleShouldHaveCorrectPermissions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UserWithArchitectRoleShouldHaveCorrectPermissions()
		 {
			  // Given
			  CreateTestUsers();
			  _manager.start();

			  // When
			  SecurityContext securityContext = _manager.login( authToken( "trinity", "abc123" ) ).authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME );

			  // Then
			  assertTrue( securityContext.Mode().allowsReads() );
			  assertTrue( securityContext.Mode().allowsWrites() );
			  assertTrue( securityContext.Mode().allowsSchemaWrites() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userWithPublisherRoleShouldHaveCorrectPermissions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UserWithPublisherRoleShouldHaveCorrectPermissions()
		 {
			  // Given
			  CreateTestUsers();
			  _manager.start();

			  // When
			  SecurityContext securityContext = _manager.login( authToken( "tank", "abc123" ) ).authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME );

			  // Then
			  assertTrue( "should allow reads", securityContext.Mode().allowsReads() );
			  assertTrue( "should allow writes", securityContext.Mode().allowsWrites() );
			  assertFalse( "should _not_ allow schema writes", securityContext.Mode().allowsSchemaWrites() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userWithReaderRoleShouldHaveCorrectPermissions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UserWithReaderRoleShouldHaveCorrectPermissions()
		 {
			  // Given
			  CreateTestUsers();
			  _manager.start();

			  // When
			  SecurityContext securityContext = _manager.login( authToken( "neo", "abc123" ) ).authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME );

			  // Then
			  assertTrue( securityContext.Mode().allowsReads() );
			  assertFalse( securityContext.Mode().allowsWrites() );
			  assertFalse( securityContext.Mode().allowsSchemaWrites() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void userWithNonPredefinedRoleShouldHaveNoPermissions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UserWithNonPredefinedRoleShouldHaveNoPermissions()
		 {
			  // Given
			  CreateTestUsers();
			  _manager.start();

			  // When
			  SecurityContext securityContext = _manager.login( authToken( "smith", "abc123" ) ).authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME );

			  // Then
			  assertFalse( securityContext.Mode().allowsReads() );
			  assertFalse( securityContext.Mode().allowsWrites() );
			  assertFalse( securityContext.Mode().allowsSchemaWrites() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveNoPermissionsAfterLogout() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveNoPermissionsAfterLogout()
		 {
			  // Given
			  CreateTestUsers();
			  _manager.start();

			  // When
			  LoginContext loginContext = _manager.login( authToken( "morpheus", "abc123" ) );
			  SecurityContext securityContext = loginContext.Authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME );
			  assertTrue( securityContext.Mode().allowsReads() );
			  assertTrue( securityContext.Mode().allowsWrites() );
			  assertTrue( securityContext.Mode().allowsSchemaWrites() );

			  loginContext.Subject().logout();

			  securityContext = loginContext.Authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME );
			  // Then
			  assertFalse( securityContext.Mode().allowsReads() );
			  assertFalse( securityContext.Mode().allowsWrites() );
			  assertFalse( securityContext.Mode().allowsSchemaWrites() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearPasswordOnLogin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearPasswordOnLogin()
		 {
			  // Given
			  when( _authStrategy.authenticate( any(), any() ) ).thenReturn(AuthenticationResult.SUCCESS);

			  _manager.start();
			  _userManager.newUser( "jake", password( "abc123" ), true );
			  sbyte[] password = password( "abc123" );
			  IDictionary<string, object> authToken = AuthToken.newBasicAuthToken( "jake", password );

			  // When
			  _manager.login( authToken );

			  // Then
			  assertThat( password, equalTo( clearedPasswordWithSameLenghtAs( "abc123" ) ) );
			  assertThat( authToken[Neo4Net.Kernel.api.security.AuthToken_Fields.CREDENTIALS], equalTo( clearedPasswordWithSameLenghtAs( "abc123" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearPasswordOnInvalidAuthToken() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearPasswordOnInvalidAuthToken()
		 {
			  // Given
			  _manager.start();
			  sbyte[] password = password( "abc123" );
			  IDictionary<string, object> authToken = AuthToken.newBasicAuthToken( "jake", password );
			  authToken[Neo4Net.Kernel.api.security.AuthToken_Fields.SCHEME_KEY] = null; // Null is not a valid scheme

			  // When
			  try
			  {
					_manager.login( authToken );
					fail( "exception expected" );
			  }
			  catch ( InvalidAuthTokenException )
			  {
					// expected
			  }
			  assertThat( password, equalTo( clearedPasswordWithSameLenghtAs( "abc123" ) ) );
			  assertThat( authToken[Neo4Net.Kernel.api.security.AuthToken_Fields.CREDENTIALS], equalTo( clearedPasswordWithSameLenghtAs( "abc123" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearPasswordOnNewUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearPasswordOnNewUser()
		 {
			  // Given
			  _manager.start();
			  sbyte[] password = password( "abc123" );

			  // When
			  _userManager.newUser( "jake", password, true );

			  // Then
			  assertThat( password, equalTo( clearedPasswordWithSameLenghtAs( "abc123" ) ) );
			  User user = _userManager.getUser( "jake" );
			  assertTrue( user.Credentials().matchesPassword("abc123") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearPasswordOnNewUserAlreadyExists() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearPasswordOnNewUserAlreadyExists()
		 {
			  // Given
			  _manager.start();
			  _userManager.newUser( "jake", password( "abc123" ), true );
			  sbyte[] password = password( "abc123" );

			  // When
			  try
			  {
					_userManager.newUser( "jake", password, true );
					fail( "exception expected" );
			  }
			  catch ( InvalidArgumentsException )
			  {
					// expected
			  }

			  // Then
			  assertThat( password, equalTo( clearedPasswordWithSameLenghtAs( "abc123" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearPasswordOnSetUserPassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearPasswordOnSetUserPassword()
		 {
			  // Given
			  _manager.start();
			  _userManager.newUser( "jake", password( "old" ), false );
			  sbyte[] newPassword = password( "abc123" );

			  // When
			  _userManager.setUserPassword( "jake", newPassword, false );

			  // Then
			  assertThat( newPassword, equalTo( clearedPasswordWithSameLenghtAs( "abc123" ) ) );
			  User user = _userManager.getUser( "jake" );
			  assertTrue( user.Credentials().matchesPassword("abc123") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearPasswordOnSetUserPasswordWithInvalidPassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearPasswordOnSetUserPasswordWithInvalidPassword()
		 {
			  // Given
			  _manager.start();
			  _userManager.newUser( "jake", password( "abc123" ), false );
			  sbyte[] newPassword = password( "abc123" );

			  // When
			  try
			  {
					_userManager.setUserPassword( "jake", newPassword, false );
					fail( "exception expected" );
			  }
			  catch ( InvalidArgumentsException )
			  {
					// expected
			  }

			  // Then
			  assertThat( newPassword, equalTo( clearedPasswordWithSameLenghtAs( "abc123" ) ) );
		 }

		 private AssertableLogProvider.LogMatcher Info( string message )
		 {
			  return inLog( this.GetType() ).info(message);
		 }

		 private AssertableLogProvider.LogMatcher Info( string message, params string[] arguments )
		 {
			  return inLog( this.GetType() ).info(message, (object[]) arguments);
		 }

		 private AssertableLogProvider.LogMatcher Error( string message, params string[] arguments )
		 {
			  return inLog( this.GetType() ).error(message, (object[]) arguments);
		 }

		 private void SetMockAuthenticationStrategyResult( string username, string password, AuthenticationResult result )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User user = users.getUserByName(username);
			  User user = Users.getUserByName( username );
			  when( _authStrategy.authenticate( user, password( password ) ) ).thenReturn( result );
		 }

		 protected internal override AuthManager AuthManager()
		 {
			  return _manager;
		 }
	}

}