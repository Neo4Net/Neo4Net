﻿/*
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
namespace Org.Neo4j.Server.rest.dbms
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using AuthenticationResult = Org.Neo4j.@internal.Kernel.Api.security.AuthenticationResult;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;
	using PasswordPolicy = Org.Neo4j.Kernel.api.security.PasswordPolicy;
	using UserManager = Org.Neo4j.Kernel.api.security.UserManager;
	using UserManagerSupplier = Org.Neo4j.Kernel.api.security.UserManagerSupplier;
	using LegacyCredential = Org.Neo4j.Server.Security.Auth.LegacyCredential;
	using User = Org.Neo4j.Kernel.impl.security.User;
	using OutputFormat = Org.Neo4j.Server.rest.repr.OutputFormat;
	using JsonFormat = Org.Neo4j.Server.rest.repr.formats.JsonFormat;
	using AuthenticationStrategy = Org.Neo4j.Server.Security.Auth.AuthenticationStrategy;
	using BasicAuthManager = Org.Neo4j.Server.Security.Auth.BasicAuthManager;
	using BasicLoginContext = Org.Neo4j.Server.Security.Auth.BasicLoginContext;
	using BasicPasswordPolicy = Org.Neo4j.Server.Security.Auth.BasicPasswordPolicy;
	using InMemoryUserRepository = Org.Neo4j.Server.Security.Auth.InMemoryUserRepository;
	using UserRepository = Org.Neo4j.Server.Security.Auth.UserRepository;
	using EntityOutputFormat = Org.Neo4j.Test.server.EntityOutputFormat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class UserServiceTest
	{
		 protected internal static readonly User Neo4jUser = new User.Builder( "neo4j", LegacyCredential.forPassword( "neo4j" ) ).withRequiredPasswordChange( true ).build();

		 protected internal readonly PasswordPolicy PasswordPolicy = new BasicPasswordPolicy();
		 protected internal readonly UserRepository UserRepository = new InMemoryUserRepository();

		 protected internal UserManagerSupplier UserManagerSupplier;
		 protected internal LoginContext Neo4jContext;
		 protected internal Principal Neo4jPrinciple;
		 private HttpServletRequest _request;

		 protected internal virtual void SetupAuthManagerAndSubject()
		 {

			  UserManagerSupplier = new BasicAuthManager( UserRepository, PasswordPolicy, mock( typeof( AuthenticationStrategy ) ), new InMemoryUserRepository() );
			  Neo4jContext = new BasicLoginContext( Neo4jUser, AuthenticationResult.SUCCESS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _request = mock( typeof( HttpServletRequest ) );
			  UserRepository.create( Neo4jUser );
			  SetupAuthManagerAndSubject();
			  Neo4jPrinciple = new DelegatingPrincipal( "neo4j", Neo4jContext );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  UserRepository.delete( Neo4jUser );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnValidUserRepresentation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnValidUserRepresentation()
		 {
			  // Given
			  when( _request.UserPrincipal ).thenReturn( Neo4jPrinciple );

			  OutputFormat outputFormat = new EntityOutputFormat( new JsonFormat(), new URI("http://www.example.com"), null );
			  UserService userService = new UserService( UserManagerSupplier, new JsonFormat(), outputFormat );

			  // When
			  Response response = userService.GetUser( "neo4j", _request );

			  // Then
			  assertThat( response.Status, equalTo( 200 ) );
			  string json = StringHelper.NewString( ( sbyte[] ) response.Entity );
			  assertNotNull( json );
			  assertThat( json, containsString( "\"username\" : \"neo4j\"" ) );
			  assertThat( json, containsString( "\"password_change\" : \"http://www.example.com/user/neo4j/password\"" ) );
			  assertThat( json, containsString( "\"password_change_required\" : true" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn404WhenRequestingUserIfNotAuthenticated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturn404WhenRequestingUserIfNotAuthenticated()
		 {
			  // Given
			  when( _request.UserPrincipal ).thenReturn( null );

			  OutputFormat outputFormat = new EntityOutputFormat( new JsonFormat(), new URI("http://www.example.com"), null );
			  UserService userService = new UserService( UserManagerSupplier, new JsonFormat(), outputFormat );

			  // When
			  Response response = userService.GetUser( "neo4j", _request );

			  // Then
			  assertThat( response.Status, equalTo( 404 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn404WhenRequestingUserIfDifferentUser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturn404WhenRequestingUserIfDifferentUser()
		 {
			  // Given
			  when( _request.UserPrincipal ).thenReturn( Neo4jPrinciple );

			  OutputFormat outputFormat = new EntityOutputFormat( new JsonFormat(), new URI("http://www.example.com"), null );
			  UserService userService = new UserService( mock( typeof( BasicAuthManager ) ), new JsonFormat(), outputFormat );

			  // When
			  Response response = userService.GetUser( "fred", _request );

			  // Then
			  assertThat( response.Status, equalTo( 404 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn404WhenRequestingUserIfUnknownUser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturn404WhenRequestingUserIfUnknownUser()
		 {
			  // Given
			  when( _request.UserPrincipal ).thenReturn( Neo4jPrinciple );

			  UserManagerSupplier.UserManager.deleteUser( "neo4j" );

			  OutputFormat outputFormat = new EntityOutputFormat( new JsonFormat(), new URI("http://www.example.com"), null );
			  UserService userService = new UserService( UserManagerSupplier, new JsonFormat(), outputFormat );

			  // When
			  Response response = userService.GetUser( "neo4j", _request );

			  // Then
			  assertThat( response.Status, equalTo( 404 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangePasswordAndReturnSuccess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldChangePasswordAndReturnSuccess()
		 {
			  // Given
			  when( _request.UserPrincipal ).thenReturn( Neo4jPrinciple );

			  OutputFormat outputFormat = new EntityOutputFormat( new JsonFormat(), new URI("http://www.example.com"), null );
			  UserService userService = new UserService( UserManagerSupplier, new JsonFormat(), outputFormat );

			  // When
			  Response response = userService.SetPassword( "neo4j", _request, "{ \"password\" : \"test\" }" );

			  // Then
			  assertThat( response.Status, equalTo( 200 ) );
			  UserManagerSupplier.UserManager.getUser( "neo4j" ).credentials().matchesPassword("test");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn404WhenChangingPasswordIfNotAuthenticated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturn404WhenChangingPasswordIfNotAuthenticated()
		 {
			  // Given
			  when( _request.UserPrincipal ).thenReturn( null );

			  OutputFormat outputFormat = new EntityOutputFormat( new JsonFormat(), new URI("http://www.example.com"), null );
			  UserService userService = new UserService( mock( typeof( BasicAuthManager ) ), new JsonFormat(), outputFormat );

			  // When
			  Response response = userService.SetPassword( "neo4j", _request, "{ \"password\" : \"test\" }" );

			  // Then
			  assertThat( response.Status, equalTo( 404 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn404WhenChangingPasswordIfDifferentUser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturn404WhenChangingPasswordIfDifferentUser()
		 {
			  // Given
			  when( _request.UserPrincipal ).thenReturn( Neo4jPrinciple );

			  UserManager userManager = mock( typeof( UserManager ) );

			  OutputFormat outputFormat = new EntityOutputFormat( new JsonFormat(), new URI("http://www.example.com"), null );
			  UserService userService = new UserService( UserManagerSupplier, new JsonFormat(), outputFormat );

			  // When
			  Response response = userService.SetPassword( "fred", _request, "{ \"password\" : \"test\" }" );

			  // Then
			  assertThat( response.Status, equalTo( 404 ) );
			  verifyZeroInteractions( userManager );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn422WhenChangingPasswordIfUnknownUser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturn422WhenChangingPasswordIfUnknownUser()
		 {
			  // Given
			  when( _request.UserPrincipal ).thenReturn( Neo4jPrinciple );

			  OutputFormat outputFormat = new EntityOutputFormat( new JsonFormat(), new URI("http://www.example.com"), null );
			  UserService userService = new UserService( UserManagerSupplier, new JsonFormat(), outputFormat );

			  UserRepository.delete( Neo4jUser );

			  // When
			  Response response = userService.SetPassword( "neo4j", _request, "{ \"password\" : \"test\" }" );

			  // Then
			  assertThat( response.Status, equalTo( 422 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn400IfPayloadIsInvalid() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturn400IfPayloadIsInvalid()
		 {
			  // Given
			  when( _request.UserPrincipal ).thenReturn( Neo4jPrinciple );

			  OutputFormat outputFormat = new EntityOutputFormat( new JsonFormat(), new URI("http://www.example.com"), null );
			  UserService userService = new UserService( mock( typeof( BasicAuthManager ) ), new JsonFormat(), outputFormat );

			  // When
			  Response response = userService.SetPassword( "neo4j", _request, "xxx" );

			  // Then
			  assertThat( response.Status, equalTo( 400 ) );
			  string json = StringHelper.NewString( ( sbyte[] ) response.Entity );
			  assertNotNull( json );
			  assertThat( json, containsString( "\"code\" : \"Neo.ClientError.Request.InvalidFormat\"" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn422IfMissingPassword() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturn422IfMissingPassword()
		 {
			  // Given
			  when( _request.UserPrincipal ).thenReturn( Neo4jPrinciple );

			  OutputFormat outputFormat = new EntityOutputFormat( new JsonFormat(), new URI("http://www.example.com"), null );
			  UserService userService = new UserService( mock( typeof( BasicAuthManager ) ), new JsonFormat(), outputFormat );

			  // When
			  Response response = userService.SetPassword( "neo4j", _request, "{ \"unknown\" : \"unknown\" }" );

			  // Then
			  assertThat( response.Status, equalTo( 422 ) );
			  string json = StringHelper.NewString( ( sbyte[] ) response.Entity );
			  assertNotNull( json );
			  assertThat( json, containsString( "\"code\" : \"Neo.ClientError.Request.InvalidFormat\"" ) );
			  assertThat( json, containsString( "\"message\" : \"Required parameter 'password' is missing.\"" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn422IfInvalidPasswordType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturn422IfInvalidPasswordType()
		 {
			  // Given
			  when( _request.UserPrincipal ).thenReturn( Neo4jPrinciple );

			  OutputFormat outputFormat = new EntityOutputFormat( new JsonFormat(), new URI("http://www.example.com"), null );
			  UserService userService = new UserService( mock( typeof( BasicAuthManager ) ), new JsonFormat(), outputFormat );

			  // When
			  Response response = userService.SetPassword( "neo4j", _request, "{ \"password\" : 1 }" );

			  // Then
			  assertThat( response.Status, equalTo( 422 ) );
			  string json = StringHelper.NewString( ( sbyte[] ) response.Entity );
			  assertNotNull( json );
			  assertThat( json, containsString( "\"code\" : \"Neo.ClientError.Request.InvalidFormat\"" ) );
			  assertThat( json, containsString( "\"message\" : \"Expected 'password' to be a string.\"" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn422IfEmptyPassword() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturn422IfEmptyPassword()
		 {
			  // Given
			  when( _request.UserPrincipal ).thenReturn( Neo4jPrinciple );

			  OutputFormat outputFormat = new EntityOutputFormat( new JsonFormat(), new URI("http://www.example.com"), null );
			  UserService userService = new UserService( UserManagerSupplier, new JsonFormat(), outputFormat );

			  // When
			  Response response = userService.SetPassword( "neo4j", _request, "{ \"password\" : \"\" }" );

			  // Then
			  assertThat( response.Status, equalTo( 422 ) );
			  string json = StringHelper.NewString( ( sbyte[] ) response.Entity );
			  assertNotNull( json );
			  assertThat( json, containsString( "\"code\" : \"Neo.ClientError.General.InvalidArguments\"" ) );
			  assertThat( json, containsString( "\"message\" : \"A password cannot be empty.\"" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturn422IfPasswordIdentical() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturn422IfPasswordIdentical()
		 {
			  // Given
			  when( _request.UserPrincipal ).thenReturn( Neo4jPrinciple );

			  OutputFormat outputFormat = new EntityOutputFormat( new JsonFormat(), new URI("http://www.example.com"), null );
			  UserService userService = new UserService( UserManagerSupplier, new JsonFormat(), outputFormat );

			  // When
			  Response response = userService.SetPassword( "neo4j", _request, "{ \"password\" : \"neo4j\" }" );

			  // Then
			  assertThat( response.Status, equalTo( 422 ) );
			  string json = StringHelper.NewString( ( sbyte[] ) response.Entity );
			  assertNotNull( json );
			  assertThat( json, containsString( "\"code\" : \"Neo.ClientError.General.InvalidArguments\"" ) );
			  assertThat( json, containsString( "\"message\" : \"Old password and new password cannot be the same.\"" ) );
		 }
	}

}