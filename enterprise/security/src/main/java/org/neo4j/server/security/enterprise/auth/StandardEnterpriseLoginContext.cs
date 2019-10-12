using System;
using System.Collections.Generic;
using System.Text;

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
	using AuthorizationInfo = org.apache.shiro.authz.AuthorizationInfo;


	using AuthorizationViolationException = Org.Neo4j.Graphdb.security.AuthorizationViolationException;
	using AccessMode = Org.Neo4j.@internal.Kernel.Api.security.AccessMode;
	using AuthSubject = Org.Neo4j.@internal.Kernel.Api.security.AuthSubject;
	using AuthenticationResult = Org.Neo4j.@internal.Kernel.Api.security.AuthenticationResult;
	using EnterpriseLoginContext = Org.Neo4j.Kernel.enterprise.api.security.EnterpriseLoginContext;
	using EnterpriseSecurityContext = Org.Neo4j.Kernel.enterprise.api.security.EnterpriseSecurityContext;

	internal class StandardEnterpriseLoginContext : EnterpriseLoginContext
	{
		 private const string SCHEMA_READ_WRITE = "schema:read,write";
		 private const string TOKEN_CREATE = "token:create";
		 private const string READ_WRITE = "data:read,write";
		 private const string READ = "data:read";

		 private readonly MultiRealmAuthManager _authManager;
		 private readonly ShiroSubject _shiroSubject;
		 private readonly NeoShiroSubject _neoShiroSubject;

		 internal StandardEnterpriseLoginContext( MultiRealmAuthManager authManager, ShiroSubject shiroSubject )
		 {
			  this._authManager = authManager;
			  this._shiroSubject = shiroSubject;
			  this._neoShiroSubject = new NeoShiroSubject( this );
		 }

		 private bool Admin
		 {
			 get
			 {
				  return _shiroSubject.Authenticated && _shiroSubject.isPermitted( "*" );
			 }
		 }

		 public override AuthSubject Subject()
		 {
			  return _neoShiroSubject;
		 }

		 private StandardAccessMode Mode( System.Func<string, int> tokenLookup )
		 {
			  bool isAuthenticated = _shiroSubject.Authenticated;
			  return new StandardAccessMode( isAuthenticated && _shiroSubject.isPermitted( READ ), isAuthenticated && _shiroSubject.isPermitted( READ_WRITE ), isAuthenticated && _shiroSubject.isPermitted( TOKEN_CREATE ), isAuthenticated && _shiroSubject.isPermitted( SCHEMA_READ_WRITE ), _shiroSubject.AuthenticationResult == AuthenticationResult.PASSWORD_CHANGE_REQUIRED, QueryForRoleNames(), QueryForPropertyPermissions(tokenLookup) );
		 }

		 public override EnterpriseSecurityContext Authorize( System.Func<string, int> propertyIdLookup, string dbName )
		 {
			  StandardAccessMode mode = mode( propertyIdLookup );
			  return new EnterpriseSecurityContext( _neoShiroSubject, mode, mode.Roles, Admin );
		 }

		 public override ISet<string> Roles()
		 {
			  return QueryForRoleNames();
		 }

		 private ISet<string> QueryForRoleNames()
		 {
			  ICollection<AuthorizationInfo> authorizationInfo = _authManager.getAuthorizationInfo( _shiroSubject.Principals );
			  return authorizationInfo.stream().flatMap(authInfo =>
			  {
						  ICollection<string> roles = authInfo.Roles;
						  return roles == null ? Stream.empty() : roles.stream();
			  }).collect( Collectors.toSet() );
		 }

		 private System.Func<int, bool> QueryForPropertyPermissions( System.Func<string, int> tokenLookup )
		 {
			  return _authManager.getPropertyPermissions( Roles(), tokenLookup );
		 }

		 private class StandardAccessMode : AccessMode
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly bool AllowsReadsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly bool AllowsWritesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly bool AllowsSchemaWritesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly bool AllowsTokenCreatesConflict;
			  internal readonly bool PasswordChangeRequired;
			  internal readonly ISet<string> Roles;
			  internal readonly System.Func<int, bool> PropertyPermissions;

			  internal StandardAccessMode( bool allowsReads, bool allowsWrites, bool allowsTokenCreates, bool allowsSchemaWrites, bool passwordChangeRequired, ISet<string> roles, System.Func<int, bool> propertyPermissions )
			  {
					this.AllowsReadsConflict = allowsReads;
					this.AllowsWritesConflict = allowsWrites;
					this.AllowsTokenCreatesConflict = allowsTokenCreates;
					this.AllowsSchemaWritesConflict = allowsSchemaWrites;
					this.PasswordChangeRequired = passwordChangeRequired;
					this.Roles = roles;
					this.PropertyPermissions = propertyPermissions;
			  }

			  public override bool AllowsReads()
			  {
					return AllowsReadsConflict;
			  }

			  public override bool AllowsWrites()
			  {
					return AllowsWritesConflict;
			  }

			  public override bool AllowsTokenCreates()
			  {
					return AllowsTokenCreatesConflict;
			  }

			  public override bool AllowsSchemaWrites()
			  {
					return AllowsSchemaWritesConflict;
			  }

			  public override bool AllowsPropertyReads( int propertyKey )
			  {
					return PropertyPermissions.test( propertyKey );
			  }

			  public override bool AllowsProcedureWith( string[] roleNames )
			  {
					foreach ( string roleName in roleNames )
					{
						 if ( Roles.Contains( roleName ) )
						 {
							  return true;
						 }
					}
					return false;
			  }

			  public override AuthorizationViolationException OnViolation( string msg )
			  {
					if ( PasswordChangeRequired )
					{
						 return Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.CredentialsExpired.onViolation( msg );
					}
					else
					{
						 return new AuthorizationViolationException( msg );
					}
			  }

			  public override string Name()
			  {
					ISet<string> sortedRoles = new SortedSet<string>( Roles );
					return Roles.Count == 0 ? "no roles" : "roles [" + string.join( ",", sortedRoles ) + "]";
			  }
		 }

		 internal class NeoShiroSubject : AuthSubject
		 {
			 private readonly StandardEnterpriseLoginContext _outerInstance;

			 public NeoShiroSubject( StandardEnterpriseLoginContext outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }


			  public override string Username()
			  {
					object principal = outerInstance.shiroSubject.Principal;
					if ( principal != null )
					{
						 return principal.ToString();
					}
					else
					{
						 return ""; // Should never clash with a valid username
					}
			  }

			  public override void Logout()
			  {
					outerInstance.shiroSubject.logout();
			  }

			  public virtual AuthenticationResult AuthenticationResult
			  {
				  get
				  {
						return outerInstance.shiroSubject.AuthenticationResult;
				  }
			  }

			  public override void SetPasswordChangeNoLongerRequired()
			  {
					if ( AuthenticationResult == AuthenticationResult.PASSWORD_CHANGE_REQUIRED )
					{
						 outerInstance.shiroSubject.AuthenticationResult = AuthenticationResult.SUCCESS;
					}
			  }

			  public override bool HasUsername( string username )
			  {
					object principal = outerInstance.shiroSubject.Principal;
					return principal != null && !string.ReferenceEquals( username, null ) && username.Equals( principal );
			  }

			  public virtual string AuthenticationFailureMessage
			  {
				  get
				  {
						string message = "";
						IList<Exception> throwables = outerInstance.shiroSubject.AuthenticationInfo.Throwables;
						switch ( outerInstance.shiroSubject.AuthenticationResult )
						{
						case FAILURE:
						{
								  message = BuildMessageFromThrowables( "invalid principal or credentials", throwables );
						}
							 break;
						case TOO_MANY_ATTEMPTS:
						{
								  message = BuildMessageFromThrowables( "too many failed attempts", throwables );
						}
							 break;
						case PASSWORD_CHANGE_REQUIRED:
						{
								  message = BuildMessageFromThrowables( "password change required", throwables );
						}
							 break;
						default:
					break;
						}
						return message;
				  }
			  }

			  public virtual void ClearAuthenticationInfo()
			  {
					outerInstance.shiroSubject.ClearAuthenticationInfo();
			  }
		 }

		 private static string BuildMessageFromThrowables( string baseMessage, IList<Exception> throwables )
		 {
			  if ( throwables == null )
			  {
					return baseMessage;
			  }

			  StringBuilder sb = new StringBuilder( baseMessage );

			  foreach ( Exception t in throwables )
			  {
					if ( t.Message != null )
					{
						 sb.Append( " (" );
						 sb.Append( t.Message );
						 sb.Append( ")" );
					}
					Exception cause = t.InnerException;
					if ( cause != null && cause.Message != null )
					{
						 sb.Append( " (" );
						 sb.Append( cause.Message );
						 sb.Append( ")" );
					}
					Exception causeCause = cause != null ? cause.InnerException : null;
					if ( causeCause != null && causeCause.Message != null )
					{
						 sb.Append( " (" );
						 sb.Append( causeCause.Message );
						 sb.Append( ")" );
					}
			  }
			  return sb.ToString();
		 }
	}

}