using System;
using System.Collections.Generic;
using System.Text;

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
	using IsEqual = org.hamcrest.core.IsEqual;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using AuthenticationResult = Org.Neo4j.@internal.Kernel.Api.security.AuthenticationResult;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;
	using AuthManager = Org.Neo4j.Kernel.api.security.AuthManager;
	using AuthToken = Org.Neo4j.Kernel.api.security.AuthToken;
	using PasswordPolicy = Org.Neo4j.Kernel.api.security.PasswordPolicy;
	using InvalidAuthTokenException = Org.Neo4j.Kernel.api.security.exception.InvalidAuthTokenException;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using User = Org.Neo4j.Kernel.impl.security.User;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
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
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.AuthenticationResult.FAILURE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.AuthenticationResult.PASSWORD_CHANGE_REQUIRED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.AuthenticationResult.SUCCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.AuthenticationResult.TOO_MANY_ATTEMPTS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.SecurityTestUtils.authToken;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertException;

	public class BasicAuthManagerTest : InitialUserTest
	{
		 private BasicAuthManager _manager;
		 private AuthenticationStrategy _authStrategy = mock( typeof( AuthenticationStrategy ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  Config = Config.defaults();
			  Users = CommunitySecurityModule.GetUserRepository( Config, NullLogProvider.Instance, FsRule.get() );
			  UserRepository initUserRepository = CommunitySecurityModule.GetInitialUserRepository( Config, NullLogProvider.Instance, FsRule.get() );
			  _manager = new BasicAuthManager( Users, mock( typeof( PasswordPolicy ) ), _authStrategy, initUserRepository );
			  _manager.init();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void teardown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Teardown()
		 {
			  _manager.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindAndAuthenticateUserSuccessfully() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindAndAuthenticateUserSuccessfully()
		 {
			  // Given
			  _manager.start();
			  User user1 = NewUser( "jake", "abc123", false );
			  Users.create( user1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User user = user1;
			  User user = user1;

			  // When
			  when( _authStrategy.authenticate( user, Password( "abc123" ) ) ).thenReturn( SUCCESS );

			  // Then
			  AssertLoginGivesResult( "jake", "abc123", SUCCESS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindAndAuthenticateUserAndReturnAuthStrategyResult() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindAndAuthenticateUserAndReturnAuthStrategyResult()
		 {
			  // Given
			  _manager.start();
			  User user1 = NewUser( "jake", "abc123", true );
			  Users.create( user1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User user = user1;
			  User user = user1;

			  // When
			  when( _authStrategy.authenticate( user, Password( "abc123" ) ) ).thenReturn( TOO_MANY_ATTEMPTS );

			  // Then
			  AssertLoginGivesResult( "jake", "abc123", TOO_MANY_ATTEMPTS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindAndAuthenticateUserAndReturnPasswordChangeIfRequired() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindAndAuthenticateUserAndReturnPasswordChangeIfRequired()
		 {
			  // Given
			  _manager.start();
			  User user1 = NewUser( "jake", "abc123", true );
			  Users.create( user1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.security.User user = user1;
			  User user = user1;

			  // When
			  when( _authStrategy.authenticate( user, Password( "abc123" ) ) ).thenReturn( SUCCESS );

			  // Then
			  AssertLoginGivesResult( "jake", "abc123", PASSWORD_CHANGE_REQUIRED );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailAuthenticationIfUserIsNotFound() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailAuthenticationIfUserIsNotFound()
		 {
			  // Given
			  _manager.start();
			  User user = NewUser( "jake", "abc123", true );
			  Users.create( user );

			  // Then
			  AssertLoginGivesResult( "unknown", "abc123", FAILURE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateUser()
		 {
			  // Given
			  _manager.start();

			  // When
			  _manager.newUser( "foo", Password( "bar" ), true );

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
			  _manager.start();
			  _manager.newUser( "jake", Password( "abc123" ), true );

			  // When
			  _manager.deleteUser( "jake" );

			  // Then
			  assertNull( Users.getUserByName( "jake" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToDeleteUnknownUser() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToDeleteUnknownUser()
		 {
			  // Given
			  _manager.start();
			  _manager.newUser( "jake", Password( "abc123" ), true );

			  try
			  {
					// When
					_manager.deleteUser( "nonExistentUser" );
					fail( "User 'nonExistentUser' should no longer exist, expected exception." );
			  }
			  catch ( InvalidArgumentsException e )
			  {
					assertThat( e.Message, containsString( "User 'nonExistentUser' does not exist." ) );
			  }
			  catch ( Exception t )
			  {
					assertThat( t.GetType(), IsEqual.equalTo(typeof(InvalidArgumentsException)) );
			  }

			  // Then
			  assertNotNull( Users.getUserByName( "jake" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetPassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetPassword()
		 {
			  // Given
			  _manager.start();
			  _manager.newUser( "jake", Password( "abc123" ), true );

			  // When
			  _manager.setUserPassword( "jake", Password( "hello, world!" ), false );

			  // Then
			  User user = _manager.getUser( "jake" );
			  assertTrue( user.Credentials().matchesPassword("hello, world!") );
			  assertThat( Users.getUserByName( "jake" ), equalTo( user ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearPasswordOnLogin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearPasswordOnLogin()
		 {
			  // Given
			  when( _authStrategy.authenticate( any(), any() ) ).thenReturn(AuthenticationResult.SUCCESS);

			  _manager.start();
			  _manager.newUser( "jake", Password( "abc123" ), true );
			  sbyte[] password = password( "abc123" );
			  IDictionary<string, object> authToken = AuthToken.newBasicAuthToken( "jake", password );

			  // When
			  _manager.login( authToken );

			  // Then
			  assertThat( password, equalTo( ClearedPasswordWithSameLenghtAs( "abc123" ) ) );
			  assertThat( authToken[Org.Neo4j.Kernel.api.security.AuthToken_Fields.CREDENTIALS], equalTo( ClearedPasswordWithSameLenghtAs( "abc123" ) ) );
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
			  authToken[Org.Neo4j.Kernel.api.security.AuthToken_Fields.SCHEME_KEY] = null; // Null is not a valid scheme

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
			  assertThat( password, equalTo( ClearedPasswordWithSameLenghtAs( "abc123" ) ) );
			  assertThat( authToken[Org.Neo4j.Kernel.api.security.AuthToken_Fields.CREDENTIALS], equalTo( ClearedPasswordWithSameLenghtAs( "abc123" ) ) );
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
			  _manager.newUser( "jake", password, true );

			  // Then
			  assertThat( password, equalTo( ClearedPasswordWithSameLenghtAs( "abc123" ) ) );
			  User user = _manager.getUser( "jake" );
			  assertTrue( user.Credentials().matchesPassword("abc123") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearPasswordOnNewUserAlreadyExists() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearPasswordOnNewUserAlreadyExists()
		 {
			  // Given
			  _manager.start();
			  _manager.newUser( "jake", Password( "abc123" ), true );
			  sbyte[] password = password( "abc123" );

			  // When
			  try
			  {
					_manager.newUser( "jake", password, true );
					fail( "exception expected" );
			  }
			  catch ( InvalidArgumentsException )
			  {
					// expected
			  }

			  // Then
			  assertThat( password, equalTo( ClearedPasswordWithSameLenghtAs( "abc123" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearPasswordOnSetUserPassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearPasswordOnSetUserPassword()
		 {
			  // Given
			  _manager.start();
			  _manager.newUser( "jake", Password( "old" ), false );
			  sbyte[] newPassword = Password( "abc123" );

			  // When
			  _manager.setUserPassword( "jake", newPassword, false );

			  // Then
			  assertThat( newPassword, equalTo( ClearedPasswordWithSameLenghtAs( "abc123" ) ) );
			  User user = _manager.getUser( "jake" );
			  assertTrue( user.Credentials().matchesPassword("abc123") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearPasswordOnSetUserPasswordWithInvalidPassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearPasswordOnSetUserPasswordWithInvalidPassword()
		 {
			  // Given
			  _manager.start();
			  _manager.newUser( "jake", Password( "abc123" ), false );
			  sbyte[] newPassword = Password( "abc123" );

			  // When
			  try
			  {
					_manager.setUserPassword( "jake", newPassword, false );
					fail( "exception expected" );
			  }
			  catch ( InvalidArgumentsException )
			  {
					// expected
			  }

			  // Then
			  assertThat( newPassword, equalTo( ClearedPasswordWithSameLenghtAs( "abc123" ) ) );
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
					_manager.setUserPassword( "unknown", Password( "hello, world!" ), false );
					fail( "exception expected" );
			  }
			  catch ( InvalidArgumentsException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenAuthTokenIsInvalid() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenAuthTokenIsInvalid()
		 {
			  _manager.start();

			  assertException( () => _manager.login(map(Org.Neo4j.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, "supercool", Org.Neo4j.Kernel.api.security.AuthToken_Fields.PRINCIPAL, "neo4j")), typeof(InvalidAuthTokenException), "Unsupported authentication token, scheme 'supercool' is not supported." );

			  assertException( () => _manager.login(map(Org.Neo4j.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, "none")), typeof(InvalidAuthTokenException), "Unsupported authentication token, scheme 'none' is only allowed when auth is disabled" );

			  assertException( () => _manager.login(map("key", "value")), typeof(InvalidAuthTokenException), "Unsupported authentication token, missing key `scheme`" );

			  assertException( () => _manager.login(map(Org.Neo4j.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, "basic", Org.Neo4j.Kernel.api.security.AuthToken_Fields.PRINCIPAL, "neo4j")), typeof(InvalidAuthTokenException), "Unsupported authentication token, missing key `credentials`" );

			  assertException( () => _manager.login(map(Org.Neo4j.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, "basic", Org.Neo4j.Kernel.api.security.AuthToken_Fields.CREDENTIALS, "very-secret")), typeof(InvalidAuthTokenException), "Unsupported authentication token, missing key `principal`" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertLoginGivesResult(String username, String password, org.neo4j.internal.kernel.api.security.AuthenticationResult expectedResult) throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
		 private void AssertLoginGivesResult( string username, string password, AuthenticationResult expectedResult )
		 {
			  LoginContext securityContext = _manager.login( authToken( username, password ) );
			  assertThat( securityContext.Subject().AuthenticationResult, equalTo(expectedResult) );
		 }

		 protected internal override AuthManager AuthManager()
		 {
			  return _manager;
		 }

		 public static sbyte[] Password( string passwordString )
		 {
			  return !string.ReferenceEquals( passwordString, null ) ? passwordString.GetBytes( Encoding.UTF8 ) : null;
		 }

		 public static sbyte[] ClearedPasswordWithSameLenghtAs( string passwordString )
		 {
			  sbyte[] password = passwordString.GetBytes( Encoding.UTF8 );
			  Arrays.fill( password, ( sbyte ) 0 );
			  return password;
		 }
	}

}