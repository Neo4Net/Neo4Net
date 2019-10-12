using System;
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
namespace Org.Neo4j.Bolt.security.auth
{

	using AuthorizationViolationException = Org.Neo4j.Graphdb.security.AuthorizationViolationException;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using AuthManager = Org.Neo4j.Kernel.api.security.AuthManager;
	using AuthToken = Org.Neo4j.Kernel.api.security.AuthToken;
	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;
	using UserManagerSupplier = Org.Neo4j.Kernel.api.security.UserManagerSupplier;
	using InvalidAuthTokenException = Org.Neo4j.Kernel.api.security.exception.InvalidAuthTokenException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.security.AuthToken_Fields.NEW_CREDENTIALS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.security.AuthToken_Fields.PRINCIPAL;

	/// <summary>
	/// Performs basic authentication with user name and password.
	/// </summary>
	public class BasicAuthentication : Authentication
	{
		 private readonly AuthManager _authManager;
		 private readonly UserManagerSupplier _userManagerSupplier;

		 public BasicAuthentication( AuthManager authManager, UserManagerSupplier userManagerSupplier )
		 {
			  this._authManager = authManager;
			  this._userManagerSupplier = userManagerSupplier;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public AuthenticationResult authenticate(java.util.Map<String,Object> authToken) throws AuthenticationException
		 public override AuthenticationResult Authenticate( IDictionary<string, object> authToken )
		 {
			  if ( authToken.ContainsKey( NEW_CREDENTIALS ) )
			  {
					return Update( authToken );
			  }
			  else
			  {
					return DoAuthenticate( authToken );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private AuthenticationResult doAuthenticate(java.util.Map<String,Object> authToken) throws AuthenticationException
		 private AuthenticationResult DoAuthenticate( IDictionary<string, object> authToken )
		 {
			  try
			  {
					LoginContext loginContext = _authManager.login( authToken );

					switch ( loginContext.Subject().AuthenticationResult )
					{
					case SUCCESS:
					case PASSWORD_CHANGE_REQUIRED:
						 break;
					case TOO_MANY_ATTEMPTS:
						 throw new AuthenticationException( Org.Neo4j.Kernel.Api.Exceptions.Status_Security.AuthenticationRateLimit );
					default:
						 throw new AuthenticationException( Org.Neo4j.Kernel.Api.Exceptions.Status_Security.Unauthorized );
					}

					return new BasicAuthenticationResult( loginContext );
			  }
			  catch ( InvalidAuthTokenException e )
			  {
					throw new AuthenticationException( e.Status(), e.Message );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private AuthenticationResult update(java.util.Map<String,Object> authToken) throws AuthenticationException
		 private AuthenticationResult Update( IDictionary<string, object> authToken )
		 {
			  try
			  {
					// We need to copy the new password here since it will be cleared by login()
					sbyte[] newPassword = AuthToken.safeCastCredentials( NEW_CREDENTIALS, authToken ).clone();

					LoginContext loginContext = _authManager.login( authToken );

					switch ( loginContext.Subject().AuthenticationResult )
					{
					case SUCCESS:
					case PASSWORD_CHANGE_REQUIRED:
						 string username = AuthToken.safeCast( PRINCIPAL, authToken );
						 _userManagerSupplier.getUserManager( loginContext.Subject(), false ).setUserPassword(username, newPassword, false); // NOTE: This will overwrite newPassword with zeroes
						 loginContext.Subject().setPasswordChangeNoLongerRequired();
						 break;
					default:
						 throw new AuthenticationException( Org.Neo4j.Kernel.Api.Exceptions.Status_Security.Unauthorized );
					}

					return new BasicAuthenticationResult( loginContext );
			  }
			  catch ( Exception e ) when ( e is AuthorizationViolationException || e is InvalidArgumentsException || e is InvalidAuthTokenException )
			  {
					throw new AuthenticationException( e.status(), e.Message, e );
			  }
			  catch ( IOException e )
			  {
					throw new AuthenticationException( Org.Neo4j.Kernel.Api.Exceptions.Status_Security.Unauthorized, e.Message, e );
			  }
		 }
	}

}