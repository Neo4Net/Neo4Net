using System;
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
	using AuthenticationInfo = org.apache.shiro.authc.AuthenticationInfo;
	using SimpleAuthenticationInfo = org.apache.shiro.authc.SimpleAuthenticationInfo;
	using ByteSource = org.apache.shiro.util.ByteSource;


	using AuthenticationResult = Neo4Net.@internal.Kernel.Api.security.AuthenticationResult;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.AuthenticationResult.FAILURE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.AuthenticationResult.PASSWORD_CHANGE_REQUIRED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.AuthenticationResult.SUCCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.AuthenticationResult.TOO_MANY_ATTEMPTS;

	public class ShiroAuthenticationInfo : SimpleAuthenticationInfo
	{
		 private AuthenticationResult _authenticationResult;
		 private IList<Exception> _throwables;

		 public ShiroAuthenticationInfo() : base()
		 {
			  this._authenticationResult = AuthenticationResult.FAILURE;
			  this._throwables = new List<Exception>( 1 );
		 }

		 public ShiroAuthenticationInfo( object principal, string realmName, AuthenticationResult authenticationResult ) : base( principal, null, realmName )
		 {
			  this._authenticationResult = authenticationResult;
		 }

		 public ShiroAuthenticationInfo( object principal, object hashedCredentials, ByteSource credentialsSalt, string realmName, AuthenticationResult authenticationResult ) : base( principal, hashedCredentials, credentialsSalt, realmName )
		 {
			  this._authenticationResult = authenticationResult;
		 }

		 public virtual AuthenticationResult AuthenticationResult
		 {
			 get
			 {
				  return _authenticationResult;
			 }
		 }

		 public virtual void AddThrowable( Exception t )
		 {
			  _throwables.Add( t );
		 }

		 public virtual IList<Exception> Throwables
		 {
			 get
			 {
				  return _throwables;
			 }
		 }

		 public override void Merge( AuthenticationInfo info )
		 {
			  if ( info == null || info.Principals == null || info.Principals.Empty )
			  {
					return;
			  }

			  base.Merge( info );

			  if ( info is ShiroAuthenticationInfo )
			  {
					_authenticationResult = MergeAuthenticationResult( _authenticationResult, ( ( ShiroAuthenticationInfo ) info ).AuthenticationResult );
			  }
			  else
			  {
					// If we get here (which means no AuthenticationException or UnknownAccountException was thrown)
					// it means the realm that provided the info was able to authenticate the subject,
					// so we claim the result to be an implicit success
					_authenticationResult = MergeAuthenticationResult( _authenticationResult, AuthenticationResult.SUCCESS );
			  }
		 }

		 private static AuthenticationResult[][] _mergeMatrix = new AuthenticationResult[][]
		 {
			 new AuthenticationResult[] { SUCCESS, SUCCESS, SUCCESS, PASSWORD_CHANGE_REQUIRED },
			 new AuthenticationResult[] { SUCCESS, FAILURE, TOO_MANY_ATTEMPTS, PASSWORD_CHANGE_REQUIRED },
			 new AuthenticationResult[] { SUCCESS, TOO_MANY_ATTEMPTS, TOO_MANY_ATTEMPTS, PASSWORD_CHANGE_REQUIRED },
			 new AuthenticationResult[] { PASSWORD_CHANGE_REQUIRED, PASSWORD_CHANGE_REQUIRED, PASSWORD_CHANGE_REQUIRED, PASSWORD_CHANGE_REQUIRED }
		 };

		 private static AuthenticationResult MergeAuthenticationResult( AuthenticationResult result, AuthenticationResult newResult )
		 {
			  AuthenticationResult mergedResult = _mergeMatrix[( int )result][( int )newResult];
			  return mergedResult;
		 }
	}

}