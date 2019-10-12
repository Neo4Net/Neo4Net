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

	using AccessMode = Neo4Net.@internal.Kernel.Api.security.AccessMode;
	using AuthSubject = Neo4Net.@internal.Kernel.Api.security.AuthSubject;
	using AuthenticationResult = Neo4Net.@internal.Kernel.Api.security.AuthenticationResult;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using SecurityContext = Neo4Net.@internal.Kernel.Api.security.SecurityContext;
	using User = Neo4Net.Kernel.impl.security.User;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.AuthenticationResult.FAILURE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.AuthenticationResult.PASSWORD_CHANGE_REQUIRED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.AuthenticationResult.SUCCESS;

	public class BasicLoginContext : LoginContext
	{
		 private readonly BasicAuthSubject _authSubject;
		 private AccessMode _accessMode;

		 public BasicLoginContext( User user, AuthenticationResult authenticationResult )
		 {
			  this._authSubject = new BasicAuthSubject( this, user, authenticationResult );

			  switch ( authenticationResult )
			  {
			  case AuthenticationResult.SUCCESS:
					_accessMode = Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.Full;
					break;
			  case AuthenticationResult.PASSWORD_CHANGE_REQUIRED:
					_accessMode = Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.CredentialsExpired;
					break;
			  default:
					_accessMode = Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.None;
				break;
			  }
		 }

		 private class BasicAuthSubject : AuthSubject
		 {
			 private readonly BasicLoginContext _outerInstance;

			  internal User User;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal AuthenticationResult AuthenticationResultConflict;

			  internal BasicAuthSubject( BasicLoginContext outerInstance, User user, AuthenticationResult authenticationResult )
			  {
				  this._outerInstance = outerInstance;
					this.User = user;
					this.AuthenticationResultConflict = authenticationResult;
			  }

			  public override void Logout()
			  {
					User = null;
					AuthenticationResultConflict = FAILURE;
			  }

			  public virtual AuthenticationResult AuthenticationResult
			  {
				  get
				  {
						return AuthenticationResultConflict;
				  }
			  }

			  public override void SetPasswordChangeNoLongerRequired()
			  {
					if ( AuthenticationResultConflict == PASSWORD_CHANGE_REQUIRED )
					{
						 AuthenticationResultConflict = SUCCESS;
						 outerInstance.accessMode = Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.Full;
					}
			  }

			  public override string Username()
			  {
					return User.name();
			  }

			  public override bool HasUsername( string username )
			  {
					return username().Equals(username);
			  }
		 }

		 public override AuthSubject Subject()
		 {
			  return _authSubject;
		 }

		 public override SecurityContext Authorize( System.Func<string, int> propertyIdLookup, string dbName )
		 {
			  return new SecurityContext( _authSubject, _accessMode );
		 }
	}

}