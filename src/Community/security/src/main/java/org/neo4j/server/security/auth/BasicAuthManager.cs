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
namespace Neo4Net.Server.Security.Auth
{

	using AuthSubject = Neo4Net.@internal.Kernel.Api.security.AuthSubject;
	using AuthenticationResult = Neo4Net.@internal.Kernel.Api.security.AuthenticationResult;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using AuthManager = Neo4Net.Kernel.api.security.AuthManager;
	using AuthToken = Neo4Net.Kernel.api.security.AuthToken;
	using PasswordPolicy = Neo4Net.Kernel.api.security.PasswordPolicy;
	using UserManager = Neo4Net.Kernel.api.security.UserManager;
	using UserManagerSupplier = Neo4Net.Kernel.api.security.UserManagerSupplier;
	using InvalidAuthTokenException = Neo4Net.Kernel.api.security.exception.InvalidAuthTokenException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using User = Neo4Net.Kernel.impl.security.User;
	using ConcurrentModificationException = Neo4Net.Server.Security.Auth.exception.ConcurrentModificationException;
	using UTF8 = Neo4Net.@string.UTF8;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.security.AuthToken.invalidToken;

	/// <summary>
	/// Manages server authentication and authorization.
	/// <para>
	/// Through the BasicAuthManager you can create, update and delete userRepository, and authenticate using credentials.
	/// </para>
	/// <para>
	/// NOTE: AuthManager will manage the lifecycle of the given UserRepository,
	///       so the given UserRepository should not be added to another LifeSupport.
	/// </para>
	/// </summary>
	public class BasicAuthManager : AuthManager, UserManager, UserManagerSupplier
	{
		 protected internal readonly AuthenticationStrategy AuthStrategy;
		 protected internal readonly UserRepository UserRepository;
		 protected internal readonly PasswordPolicy PasswordPolicy;
		 private readonly UserRepository _initialUserRepository;

		 public BasicAuthManager( UserRepository userRepository, PasswordPolicy passwordPolicy, AuthenticationStrategy authStrategy, UserRepository initialUserRepository )
		 {
			  this.UserRepository = userRepository;
			  this.PasswordPolicy = passwordPolicy;
			  this.AuthStrategy = authStrategy;
			  this._initialUserRepository = initialUserRepository;
		 }

		 public BasicAuthManager( UserRepository userRepository, PasswordPolicy passwordPolicy, Clock clock, UserRepository initialUserRepository, Config config ) : this( userRepository, passwordPolicy, CreateAuthenticationStrategy( clock, config ), initialUserRepository )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws Throwable
		 public override void Init()
		 {
			  UserRepository.init();
			  _initialUserRepository.init();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  UserRepository.start();
			  _initialUserRepository.start();

			  if ( UserRepository.numberOfUsers() == 0 )
			  {
					User neo4j = NewUser( Neo4Net.Kernel.api.security.UserManager_Fields.INITIAL_USER_NAME, UTF8.encode( Neo4Net.Kernel.api.security.UserManager_Fields.INITIAL_PASSWORD ), true );
					if ( _initialUserRepository.numberOfUsers() > 0 )
					{
						 User user = _initialUserRepository.getUserByName( Neo4Net.Kernel.api.security.UserManager_Fields.INITIAL_USER_NAME );
						 if ( user != null )
						 {
							  UserRepository.update( neo4j, user );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  UserRepository.stop();
			  _initialUserRepository.stop();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			  UserRepository.shutdown();
			  _initialUserRepository.shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.security.LoginContext login(java.util.Map<String,Object> authToken) throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
		 public override LoginContext Login( IDictionary<string, object> authToken )
		 {
			  try
			  {
					AssertValidScheme( authToken );

					string username = AuthToken.safeCast( Neo4Net.Kernel.api.security.AuthToken_Fields.PRINCIPAL, authToken );
					sbyte[] password = AuthToken.safeCastCredentials( Neo4Net.Kernel.api.security.AuthToken_Fields.CREDENTIALS, authToken );

					User user = UserRepository.getUserByName( username );
					AuthenticationResult result = AuthenticationResult.FAILURE;
					if ( user != null )
					{
						 result = AuthStrategy.authenticate( user, password );
						 if ( result == AuthenticationResult.SUCCESS && user.PasswordChangeRequired() )
						 {
							  result = AuthenticationResult.PASSWORD_CHANGE_REQUIRED;
						 }
					}
					return new BasicLoginContext( user, result );
			  }
			  finally
			  {
					AuthToken.clearCredentials( authToken );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.security.User newUser(String username, byte[] initialPassword, boolean requirePasswordChange) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 public override User NewUser( string username, sbyte[] initialPassword, bool requirePasswordChange )
		 {
			  try
			  {
					UserRepository.assertValidUsername( username );

					PasswordPolicy.validatePassword( initialPassword );

					User user = ( new User.Builder() ).withName(username).withCredentials(LegacyCredential.forPassword(initialPassword)).withRequiredPasswordChange(requirePasswordChange).build();
					UserRepository.create( user );

					return user;
			  }
			  finally
			  {
					// Clear password
					if ( initialPassword != null )
					{
						 Arrays.fill( initialPassword, ( sbyte ) 0 );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean deleteUser(String username) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 public override bool DeleteUser( string username )
		 {
			  User user = GetUser( username );
			  return user != null && UserRepository.delete( user );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.security.User getUser(String username) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 public override User GetUser( string username )
		 {
			  User user = UserRepository.getUserByName( username );
			  if ( user == null )
			  {
					throw new InvalidArgumentsException( "User '" + username + "' does not exist." );
			  }
			  return user;
		 }

		 public override User SilentlyGetUser( string username )
		 {
			  return UserRepository.getUserByName( username );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setUserPassword(String username, byte[] password, boolean requirePasswordChange) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 public override void SetUserPassword( string username, sbyte[] password, bool requirePasswordChange )
		 {
			  try
			  {
					User existingUser = GetUser( username );

					PasswordPolicy.validatePassword( password );

					if ( existingUser.Credentials().matchesPassword(password) )
					{
						 throw new InvalidArgumentsException( "Old password and new password cannot be the same." );
					}

					try
					{
						 User updatedUser = existingUser.Augment().withCredentials(LegacyCredential.forPassword(password)).withRequiredPasswordChange(requirePasswordChange).build();
						 UserRepository.update( existingUser, updatedUser );
					}
					catch ( ConcurrentModificationException )
					{
						 // try again
						 SetUserPassword( username, password, requirePasswordChange );
					}
			  }
			  finally
			  {
					// Clear password
					if ( password != null )
					{
						 Arrays.fill( password, ( sbyte ) 0 );
					}
			  }
		 }

		 public virtual ISet<string> AllUsernames
		 {
			 get
			 {
				  return UserRepository.AllUsernames;
			 }
		 }

		 public override UserManager GetUserManager( AuthSubject authSubject, bool isUserManager )
		 {
			  return this;
		 }

		 public override UserManager GetUserManager()
		 {
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertValidScheme(java.util.Map<String,Object> token) throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
		 private void AssertValidScheme( IDictionary<string, object> token )
		 {
			  string scheme = AuthToken.safeCast( Neo4Net.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, token );
			  if ( scheme.Equals( "none" ) )
			  {
					throw invalidToken( ", scheme 'none' is only allowed when auth is disabled." );
			  }
			  if ( !scheme.Equals( Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME ) )
			  {
					throw invalidToken( ", scheme '" + scheme + "' is not supported." );
			  }
		 }

		 private static AuthenticationStrategy CreateAuthenticationStrategy( Clock clock, Config config )
		 {
			  return new RateLimitedAuthenticationStrategy( clock, config );
		 }
	}

}