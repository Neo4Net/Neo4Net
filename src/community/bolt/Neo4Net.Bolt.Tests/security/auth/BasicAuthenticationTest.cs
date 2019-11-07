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
namespace Neo4Net.Bolt.security.auth
{
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using PasswordPolicy = Neo4Net.Kernel.Api.security.PasswordPolicy;
	using Config = Neo4Net.Kernel.configuration.Config;
	using BasicAuthManager = Neo4Net.Server.Security.Auth.BasicAuthManager;
	using InMemoryUserRepository = Neo4Net.Server.Security.Auth.InMemoryUserRepository;
	using UserRepository = Neo4Net.Server.Security.Auth.UserRepository;
	using UTF8 = Neo4Net.Strings.UTF8;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.map;

	public class BasicAuthenticationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private Authentication _authentication;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDoAnythingOnSuccess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDoAnythingOnSuccess()
		 {
			  // When
			  AuthenticationResult result = _authentication.authenticate( map( "scheme", "basic", "principal", "mike", "credentials", UTF8.encode( "secret2" ) ) );

			  // Then
			  assertThat( result.LoginContext.subject().username(), equalTo("mike") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowAndLogOnFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowAndLogOnFailure()
		 {
			  // Expect
			  Exception.expect( typeof( AuthenticationException ) );
			  Exception.expect( HasStatus( Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized ) );
			  Exception.expectMessage( "The client is unauthorized due to authentication failure." );

			  // When
			  _authentication.authenticate( map( "scheme", "basic", "principal", "bob", "credentials", UTF8.encode( "banana" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIndicateThatCredentialsExpired() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIndicateThatCredentialsExpired()
		 {
			  // When
			  AuthenticationResult result = _authentication.authenticate( map( "scheme", "basic", "principal", "bob", "credentials", UTF8.encode( "secret" ) ) );

			  // Then
			  assertTrue( result.CredentialsExpired() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenTooManyAttempts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenTooManyAttempts()
		 {
			  // Given
			  int maxFailedAttempts = ThreadLocalRandom.current().Next(1, 10);
			  Authentication auth = CreateAuthentication( maxFailedAttempts );

			  for ( int i = 0; i < maxFailedAttempts; ++i )
			  {
					try
					{
						 auth.Authenticate( map( "scheme", "basic", "principal", "bob", "credentials", UTF8.encode( "gelato" ) ) );
					}
					catch ( AuthenticationException e )
					{
						 assertThat( e.Status(), equalTo(Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized) );
					}
			  }

			  // Expect
			  Exception.expect( typeof( AuthenticationException ) );
			  Exception.expect( HasStatus( Neo4Net.Kernel.Api.Exceptions.Status_Security.AuthenticationRateLimit ) );
			  Exception.expectMessage( "The client has provided incorrect authentication details too many times in a row." );

			  //When
			  auth.Authenticate( map( "scheme", "basic", "principal", "bob", "credentials", UTF8.encode( "gelato" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUpdateCredentials() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToUpdateCredentials()
		 {
			  // When
			  _authentication.authenticate( map( "scheme", "basic", "principal", "mike", "credentials", UTF8.encode( "secret2" ), "new_credentials", UTF8.encode( "secret" ) ) );

			  // Then
			  _authentication.authenticate( map( "scheme", "basic", "principal", "mike", "credentials", UTF8.encode( "secret" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearCredentialsAfterUse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearCredentialsAfterUse()
		 {
			  // When
			  sbyte[] oldPassword = UTF8.encode( "secret2" );
			  sbyte[] newPassword1 = UTF8.encode( "secret" );
			  sbyte[] newPassword2 = UTF8.encode( "secret" );

			  _authentication.authenticate( map( "scheme", "basic", "principal", "mike", "credentials", oldPassword, "new_credentials", newPassword1 ) );

			  _authentication.authenticate( map( "scheme", "basic", "principal", "mike", "credentials", newPassword2 ) );

			  // Then
			  assertThat( oldPassword, Cleared );
			  assertThat( newPassword1, Cleared );
			  assertThat( newPassword2, Cleared );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUpdateExpiredCredentials() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToUpdateExpiredCredentials()
		 {
			  // When
			  AuthenticationResult result = _authentication.authenticate( map( "scheme", "basic", "principal", "bob", "credentials", UTF8.encode( "secret" ), "new_credentials", UTF8.encode( "secret2" ) ) );

			  // Then
			  assertThat( result.CredentialsExpired(), equalTo(false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToUpdateCredentialsIfOldCredentialsAreInvalid() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToUpdateCredentialsIfOldCredentialsAreInvalid()
		 {
			  // Expect
			  Exception.expect( typeof( AuthenticationException ) );
			  Exception.expect( HasStatus( Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized ) );
			  Exception.expectMessage( "The client is unauthorized due to authentication failure." );

			  // When
			  _authentication.authenticate( map( "scheme", "basic", "principal", "bob", "credentials", UTF8.encode( "gelato" ), "new_credentials", UTF8.encode( "secret2" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWithNoScheme() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowWithNoScheme()
		 {
			  // Expect
			  Exception.expect( typeof( AuthenticationException ) );
			  Exception.expect( HasStatus( Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized ) );

			  // When
			  _authentication.authenticate( map( "principal", "bob", "credentials", UTF8.encode( "secret" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnInvalidAuthToken() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOnInvalidAuthToken()
		 {
			  // Expect
			  Exception.expect( typeof( AuthenticationException ) );
			  Exception.expect( HasStatus( Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized ) );

			  // When
			  _authentication.authenticate( map( "this", "does", "not", "matter", "for", "test" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnMalformedToken() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOnMalformedToken()
		 {
			  // Expect
			  Exception.expect( typeof( AuthenticationException ) );
			  Exception.expect( HasStatus( Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized ) );
			  Exception.expectMessage( "Unsupported authentication token, the value associated with the key `principal` " + "must be a String but was: SingletonList" );

			  // When
			  _authentication.authenticate( map( "scheme", "basic", "principal", singletonList( "bob" ), "credentials", UTF8.encode( "secret" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _authentication = CreateAuthentication( 3 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static Authentication createAuthentication(int maxFailedAttempts) throws Exception
		 private static Authentication CreateAuthentication( int maxFailedAttempts )
		 {
			  UserRepository users = new InMemoryUserRepository();
			  PasswordPolicy policy = mock( typeof( PasswordPolicy ) );

			  Config config = Config.defaults( GraphDatabaseSettings.auth_max_failed_attempts, maxFailedAttempts.ToString() );

			  BasicAuthManager manager = new BasicAuthManager( users, policy, Clocks.systemClock(), users, config );
			  Authentication authentication = new BasicAuthentication( manager, manager );
			  manager.NewUser( "bob", UTF8.encode( "secret" ), true );
			  manager.NewUser( "mike", UTF8.encode( "secret2" ), false );

			  return authentication;
		 }

		 private HasStatus HasStatus( Status status )
		 {
			  return new HasStatus( status );
		 }

		 internal class HasStatus : TypeSafeMatcher<Neo4Net.Kernel.Api.Exceptions.Status_HasStatus>
		 {
			  internal Status Status;

			  internal HasStatus( Status status )
			  {
					this.Status = status;
			  }

			  protected internal override bool MatchesSafely( Neo4Net.Kernel.Api.Exceptions.Status_HasStatus item )
			  {
					return item.Status() == Status;
			  }

			  public override void DescribeTo( Description description )
			  {
					description.appendText( "expects status " ).appendValue( Status );
			  }

			  protected internal override void DescribeMismatchSafely( Neo4Net.Kernel.Api.Exceptions.Status_HasStatus item, Description mismatchDescription )
			  {
					mismatchDescription.appendText( "was " ).appendValue( item.Status() );
			  }
		 }

		 internal static CredentialsClearedMatcher Cleared
		 {
			 get
			 {
				  return new CredentialsClearedMatcher();
			 }
		 }

		 internal class CredentialsClearedMatcher : BaseMatcher<sbyte[]>
		 {
			  public override bool Matches( object o )
			  {
					if ( o is sbyte[] )
					{
						 sbyte[] bytes = ( sbyte[] ) o;
						 for ( int i = 0; i < bytes.Length; i++ )
						 {
							  if ( bytes[i] != ( sbyte ) 0 )
							  {
									return false;
							  }
						 }
						 return true;
					}
					return false;
			  }

			  public override void DescribeTo( Description description )
			  {
					description.appendText( "Byte array should contain only zeroes" );
			  }
		 }
	}

}