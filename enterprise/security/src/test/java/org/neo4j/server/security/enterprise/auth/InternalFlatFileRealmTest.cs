﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Server.security.enterprise.auth
{
	using AuthenticationException = org.apache.shiro.authc.AuthenticationException;
	using AuthenticationInfo = org.apache.shiro.authc.AuthenticationInfo;
	using AuthenticationToken = org.apache.shiro.authc.AuthenticationToken;
	using AuthorizationInfo = org.apache.shiro.authz.AuthorizationInfo;
	using MemoryConstrainedCacheManager = org.apache.shiro.cache.MemoryConstrainedCacheManager;
	using Realm = org.apache.shiro.realm.Realm;
	using PrincipalCollection = org.apache.shiro.subject.PrincipalCollection;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using SetDefaultAdminCommand = Org.Neo4j.Commandline.admin.security.SetDefaultAdminCommand;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using AuthenticationResult = Org.Neo4j.@internal.Kernel.Api.security.AuthenticationResult;
	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;
	using PasswordPolicy = Org.Neo4j.Kernel.api.security.PasswordPolicy;
	using InvalidAuthTokenException = Org.Neo4j.Kernel.api.security.exception.InvalidAuthTokenException;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using EnterpriseLoginContext = Org.Neo4j.Kernel.enterprise.api.security.EnterpriseLoginContext;
	using User = Org.Neo4j.Kernel.impl.security.User;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using AuthenticationStrategy = Org.Neo4j.Server.Security.Auth.AuthenticationStrategy;
	using BasicPasswordPolicy = Org.Neo4j.Server.Security.Auth.BasicPasswordPolicy;
	using InMemoryUserRepository = Org.Neo4j.Server.Security.Auth.InMemoryUserRepository;
	using LegacyCredential = Org.Neo4j.Server.Security.Auth.LegacyCredential;
	using Org.Neo4j.Server.Security.Auth;
	using RateLimitedAuthenticationStrategy = Org.Neo4j.Server.Security.Auth.RateLimitedAuthenticationStrategy;
	using UserRepository = Org.Neo4j.Server.Security.Auth.UserRepository;
	using PredefinedRoles = Org.Neo4j.Server.security.enterprise.auth.plugin.api.PredefinedRoles;
	using SecurityLog = Org.Neo4j.Server.security.enterprise.log.SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.BasicAuthManagerTest.password;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.SecurityTestUtils.authToken;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.AuthTestUtil.listOf;

	public class InternalFlatFileRealmTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private MultiRealmAuthManager _authManager;
		 private TestRealm _testRealm;
		 private readonly System.Func<string, int> _token = s => -1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _testRealm = new TestRealm(thisnew InMemoryUserRepository(), new InMemoryRoleRepository(), new BasicPasswordPolicy(), NewRateLimitedAuthStrategy(), mock(typeof(JobScheduler)), new InMemoryUserRepository(), new InMemoryUserRepository()
							 );

			  IList<Realm> realms = listOf( _testRealm );

			  _authManager = new MultiRealmAuthManager( _testRealm, realms, new MemoryConstrainedCacheManager(), mock(typeof(SecurityLog)), true, false, Collections.emptyMap() );

			  _authManager.init();
			  _authManager.start();

			  _authManager.UserManager.newUser( "mike", password( "123" ), false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCacheAuthenticationInfo() throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCacheAuthenticationInfo()
		 {
			  // Given
			  EnterpriseLoginContext mike = _authManager.login( authToken( "mike", "123" ) );
			  assertThat( mike.Subject().AuthenticationResult, equalTo(AuthenticationResult.SUCCESS) );
			  assertThat( "Test realm did not receive a call", _testRealm.takeAuthenticationFlag(), @is(true) );

			  // When
			  mike = _authManager.login( authToken( "mike", "123" ) );
			  assertThat( mike.Subject().AuthenticationResult, equalTo(AuthenticationResult.SUCCESS) );

			  // Then
			  assertThat( "Test realm did not receive a call", _testRealm.takeAuthenticationFlag(), @is(true) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCacheAuthorizationInfo() throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCacheAuthorizationInfo()
		 {
			  // Given
			  EnterpriseLoginContext mike = _authManager.login( authToken( "mike", "123" ) );
			  assertThat( mike.Subject().AuthenticationResult, equalTo(AuthenticationResult.SUCCESS) );

			  mike.Authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME ).mode().allowsReads();
			  assertThat( "Test realm did not receive a call", _testRealm.takeAuthorizationFlag(), @is(true) );

			  // When
			  mike.Authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME ).mode().allowsWrites();

			  // Then
			  assertThat( "Test realm did not receive a call", _testRealm.takeAuthorizationFlag(), @is(true) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyReloadUsersOrRolesIfNeeded() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOnlyReloadUsersOrRolesIfNeeded()
		 {
			  AssertSetUsersAndRolesNTimes( false, false, 0, 0 );
			  AssertSetUsersAndRolesNTimes( false, true, 0, 1 );
			  AssertSetUsersAndRolesNTimes( true, false, 1, 0 );
			  AssertSetUsersAndRolesNTimes( true, true, 1, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAssignAdminRoleToDefaultUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAssignAdminRoleToDefaultUser()
		 {
			  // Given
			  InternalFlatFileRealm realm = InternalTestRealmWithUsers( Collections.emptyList(), Collections.emptyList() );

			  // When
			  realm.Initialize();
			  realm.Start();

			  // Then
			  assertThat( realm.GetUsernamesForRole( PredefinedRoles.ADMIN ), contains( InternalFlatFileRealm.INITIAL_USER_NAME ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAssignAdminRoleToSpecifiedUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAssignAdminRoleToSpecifiedUser()
		 {
			  // Given
			  InternalFlatFileRealm realm = InternalTestRealmWithUsers( Arrays.asList( "neo4j", "morpheus", "trinity" ), Collections.singletonList( "morpheus" ) );

			  // When
			  realm.Initialize();
			  realm.Start();

			  // Then
			  assertThat( realm.GetUsernamesForRole( PredefinedRoles.ADMIN ), contains( "morpheus" ) );
			  assertThat( realm.GetUsernamesForRole( PredefinedRoles.ADMIN ).Count, equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAssignAdminRoleToOnlyUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAssignAdminRoleToOnlyUser()
		 {
			  // Given
			  InternalFlatFileRealm realm = InternalTestRealmWithUsers( Collections.singletonList( "morpheus" ), Collections.emptyList() );

			  // When
			  realm.Initialize();
			  realm.Start();

			  // Then
			  assertThat( realm.GetUsernamesForRole( PredefinedRoles.ADMIN ), contains( "morpheus" ) );
			  assertThat( realm.GetUsernamesForRole( PredefinedRoles.ADMIN ).Count, equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAssignAdminToNonExistentUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAssignAdminToNonExistentUser()
		 {
			  // Given
			  InternalFlatFileRealm realm = InternalTestRealmWithUsers( Collections.singletonList( "neo4j" ), Collections.singletonList( "morpheus" ) );

			  // Expect
			  Exception.expect( typeof( InvalidArgumentsException ) );
			  Exception.expectMessage( "No roles defined, and default admin user 'morpheus' does not exist. Please use `neo4j-admin " + SetDefaultAdminCommand.COMMAND_NAME + "` to select a valid admin." );

			  // When
			  realm.Initialize();
			  realm.Start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveErrorOnMultipleUsersNoDefault() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveErrorOnMultipleUsersNoDefault()
		 {
			  // Given
			  InternalFlatFileRealm realm = InternalTestRealmWithUsers( Arrays.asList( "morpheus", "trinity" ), Collections.emptyList() );

			  // Expect
			  Exception.expect( typeof( InvalidArgumentsException ) );
			  Exception.expectMessage( "No roles defined, and cannot determine which user should be admin. Please use `neo4j-admin " + SetDefaultAdminCommand.COMMAND_NAME + "` to select an admin." );

			  // When
			  realm.Initialize();
			  realm.Start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToAssignMultipleDefaultAdmins() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToAssignMultipleDefaultAdmins()
		 {
			  // Given
			  InternalFlatFileRealm realm = InternalTestRealmWithUsers( Arrays.asList( "morpheus", "trinity", "tank" ), Arrays.asList( "morpheus", "trinity" ) );

			  // Expect
			  Exception.expect( typeof( InvalidArgumentsException ) );
			  Exception.expectMessage( "No roles defined, and multiple users defined as default admin user. Please use `neo4j-admin " + SetDefaultAdminCommand.COMMAND_NAME + "` to select a valid admin." );

			  // When
			  realm.Initialize();
			  realm.Start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAssignAdminRoleAfterBadSetting() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAssignAdminRoleAfterBadSetting()
		 {
			  UserRepository userRepository = new InMemoryUserRepository();
			  UserRepository initialUserRepository = new InMemoryUserRepository();
			  UserRepository adminUserRepository = new InMemoryUserRepository();
			  RoleRepository roleRepository = new InMemoryRoleRepository();
			  userRepository.Create( NewUser( "morpheus", "123", false ) );
			  userRepository.Create( NewUser( "trinity", "123", false ) );

			  InternalFlatFileRealm realm = new InternalFlatFileRealm( userRepository, roleRepository, new BasicPasswordPolicy(), NewRateLimitedAuthStrategy(), new InternalFlatFileRealmIT.TestJobScheduler(), initialUserRepository, adminUserRepository );

			  try
			  {
					realm.Initialize();
					realm.Start();
					fail( "Multiple users, no default admin provided" );
			  }
			  catch ( InvalidArgumentsException )
			  {
					realm.Stop();
					realm.Shutdown();
			  }
			  adminUserRepository.Create( ( new User.Builder( "trinity", LegacyCredential.INACCESSIBLE ) ).build() );
			  realm.Initialize();
			  realm.Start();
			  assertThat( realm.GetUsernamesForRole( PredefinedRoles.ADMIN ).Count, equalTo( 1 ) );
			  assertThat( realm.GetUsernamesForRole( PredefinedRoles.ADMIN ), contains( "trinity" ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private InternalFlatFileRealm internalTestRealmWithUsers(java.util.List<String> existing, java.util.List<String> defaultAdmin) throws Throwable
		 private InternalFlatFileRealm InternalTestRealmWithUsers( IList<string> existing, IList<string> defaultAdmin )
		 {
			  UserRepository userRepository = new InMemoryUserRepository();
			  UserRepository initialUserRepository = new InMemoryUserRepository();
			  UserRepository adminUserRepository = new InMemoryUserRepository();
			  RoleRepository roleRepository = new InMemoryRoleRepository();
			  foreach ( string user in existing )
			  {
					userRepository.Create( NewUser( user, "123", false ) );
			  }
			  foreach ( string user in defaultAdmin )
			  {
					adminUserRepository.Create( ( new User.Builder( user, LegacyCredential.INACCESSIBLE ) ).build() );
			  }
			  return new InternalFlatFileRealm( userRepository, roleRepository, new BasicPasswordPolicy(), NewRateLimitedAuthStrategy(), new InternalFlatFileRealmIT.TestJobScheduler(), initialUserRepository, adminUserRepository );
		 }

		 private User NewUser( string userName, string password, bool pwdChange )
		 {
			  return ( new User.Builder( userName, LegacyCredential.forPassword( password ) ) ).withRequiredPasswordChange( pwdChange ).build();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertSetUsersAndRolesNTimes(boolean usersChanged, boolean rolesChanged, int nSetUsers, int nSetRoles) throws Throwable
		 private void AssertSetUsersAndRolesNTimes( bool usersChanged, bool rolesChanged, int nSetUsers, int nSetRoles )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.server.security.auth.UserRepository userRepository = mock(org.neo4j.server.security.auth.UserRepository.class);
			  UserRepository userRepository = mock( typeof( UserRepository ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RoleRepository roleRepository = mock(RoleRepository.class);
			  RoleRepository roleRepository = mock( typeof( RoleRepository ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.server.security.auth.UserRepository initialUserRepository = mock(org.neo4j.server.security.auth.UserRepository.class);
			  UserRepository initialUserRepository = mock( typeof( UserRepository ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.server.security.auth.UserRepository defaultAdminRepository = mock(org.neo4j.server.security.auth.UserRepository.class);
			  UserRepository defaultAdminRepository = mock( typeof( UserRepository ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.api.security.PasswordPolicy passwordPolicy = new org.neo4j.server.security.auth.BasicPasswordPolicy();
			  PasswordPolicy passwordPolicy = new BasicPasswordPolicy();
			  AuthenticationStrategy authenticationStrategy = NewRateLimitedAuthStrategy();
			  InternalFlatFileRealmIT.TestJobScheduler jobScheduler = new InternalFlatFileRealmIT.TestJobScheduler();
			  InternalFlatFileRealm realm = new InternalFlatFileRealm( userRepository, roleRepository, passwordPolicy, authenticationStrategy, jobScheduler, initialUserRepository, defaultAdminRepository );

			  when( userRepository.PersistedSnapshot ).thenReturn( new ListSnapshot<>( 10L, Collections.emptyList(), usersChanged ) );
			  when( userRepository.GetUserByName( any() ) ).thenReturn((new User.Builder()).build());
			  when( roleRepository.PersistedSnapshot ).thenReturn( new ListSnapshot<>( 10L, Collections.emptyList(), rolesChanged ) );
			  when( roleRepository.GetRoleByName( anyString() ) ).thenReturn(new RoleRecord(""));

			  realm.init();
			  realm.Start();

			  jobScheduler.ScheduledRunnable.run();

			  verify( userRepository, times( nSetUsers ) ).Users = any();
			  verify( roleRepository, times( nSetRoles ) ).Roles = any();
		 }

		 private static AuthenticationStrategy NewRateLimitedAuthStrategy()
		 {
			  return new RateLimitedAuthenticationStrategy( Clock.systemUTC(), Config.defaults() );
		 }

		 private class TestRealm : InternalFlatFileRealm
		 {
			 private readonly InternalFlatFileRealmTest _outerInstance;

			  internal bool AuthenticationFlag;
			  internal bool AuthorizationFlag;

			  internal TestRealm( InternalFlatFileRealmTest outerInstance, UserRepository userRepository, RoleRepository roleRepository, PasswordPolicy passwordPolicy, AuthenticationStrategy authenticationStrategy, JobScheduler jobScheduler, UserRepository initialUserRepository, UserRepository defaultAdminRepository ) : base( userRepository, roleRepository, passwordPolicy, authenticationStrategy, jobScheduler, initialUserRepository, defaultAdminRepository )
			  {
				  this._outerInstance = outerInstance;
			  }

			  internal virtual bool TakeAuthenticationFlag()
			  {
					bool t = AuthenticationFlag;
					AuthenticationFlag = false;
					return t;
			  }

			  internal virtual bool TakeAuthorizationFlag()
			  {
					bool t = AuthorizationFlag;
					AuthorizationFlag = false;
					return t;
			  }

			  public override string Name
			  {
				  get
				  {
						return "TestRealm wrapping " + base.Name;
				  }
			  }

			  public override bool Supports( AuthenticationToken token )
			  {
					return base.Supports( token );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.apache.shiro.authc.AuthenticationInfo doGetAuthenticationInfo(org.apache.shiro.authc.AuthenticationToken token) throws org.apache.shiro.authc.AuthenticationException
			  protected internal override AuthenticationInfo DoGetAuthenticationInfo( AuthenticationToken token )
			  {
					AuthenticationFlag = true;
					return base.DoGetAuthenticationInfo( token );
			  }

			  protected internal override AuthorizationInfo DoGetAuthorizationInfo( PrincipalCollection principals )
			  {
					AuthorizationFlag = true;
					return base.DoGetAuthorizationInfo( principals );
			  }
		 }
	}

}