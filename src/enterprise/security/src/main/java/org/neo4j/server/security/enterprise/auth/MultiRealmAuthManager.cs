using System;
using System.Collections.Generic;
using System.Diagnostics;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Server.security.enterprise.auth
{
	using AuthenticationException = org.apache.shiro.authc.AuthenticationException;
	using AuthenticationInfo = org.apache.shiro.authc.AuthenticationInfo;
	using ExcessiveAttemptsException = org.apache.shiro.authc.ExcessiveAttemptsException;
	using ModularRealmAuthenticator = org.apache.shiro.authc.pam.ModularRealmAuthenticator;
	using UnsupportedTokenException = org.apache.shiro.authc.pam.UnsupportedTokenException;
	using AuthorizationInfo = org.apache.shiro.authz.AuthorizationInfo;
	using Cache = org.apache.shiro.cache.Cache;
	using CacheManager = org.apache.shiro.cache.CacheManager;
	using DefaultSecurityManager = org.apache.shiro.mgt.DefaultSecurityManager;
	using DefaultSessionStorageEvaluator = org.apache.shiro.mgt.DefaultSessionStorageEvaluator;
	using DefaultSubjectDAO = org.apache.shiro.mgt.DefaultSubjectDAO;
	using SubjectDAO = org.apache.shiro.mgt.SubjectDAO;
	using AuthenticatingRealm = org.apache.shiro.realm.AuthenticatingRealm;
	using AuthorizingRealm = org.apache.shiro.realm.AuthorizingRealm;
	using CachingRealm = org.apache.shiro.realm.CachingRealm;
	using Realm = org.apache.shiro.realm.Realm;
	using PrincipalCollection = org.apache.shiro.subject.PrincipalCollection;
	using Initializable = org.apache.shiro.util.Initializable;
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;


	using AuthProviderFailedException = Neo4Net.GraphDb.security.AuthProviderFailedException;
	using AuthProviderTimeoutException = Neo4Net.GraphDb.security.AuthProviderTimeoutException;
	using AuthSubject = Neo4Net.Internal.Kernel.Api.security.AuthSubject;
	using AuthenticationResult = Neo4Net.Internal.Kernel.Api.security.AuthenticationResult;
	using AuthToken = Neo4Net.Kernel.api.security.AuthToken;
	using InvalidAuthTokenException = Neo4Net.Kernel.api.security.exception.InvalidAuthTokenException;
	using EnterpriseLoginContext = Neo4Net.Kernel.enterprise.api.security.EnterpriseLoginContext;
	using SecurityLog = Neo4Net.Server.security.enterprise.log.SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Strings.escape;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.security.AuthToken.invalidToken;

	internal class MultiRealmAuthManager : EnterpriseAuthAndUserManager
	{
		 private readonly EnterpriseUserManager _userManager;
		 private readonly ICollection<Realm> _realms;
		 private readonly DefaultSecurityManager _securityManager;
		 private readonly CacheManager _cacheManager;
		 private readonly SecurityLog _securityLog;
		 private readonly bool _logSuccessfulLogin;
		 private readonly bool _propertyAuthorization;
		 private readonly IDictionary<string, IList<string>> _roleToPropertyBlacklist;

		 internal MultiRealmAuthManager( EnterpriseUserManager userManager, ICollection<Realm> realms, CacheManager cacheManager, SecurityLog securityLog, bool logSuccessfulLogin, bool propertyAuthorization, IDictionary<string, IList<string>> roleToPropertyBlacklist )
		 {
			  this._userManager = userManager;
			  this._realms = realms;
			  this._cacheManager = cacheManager;

			  _securityManager = new DefaultSecurityManager( realms );
			  this._securityLog = securityLog;
			  this._logSuccessfulLogin = logSuccessfulLogin;
			  this._propertyAuthorization = propertyAuthorization;
			  this._roleToPropertyBlacklist = roleToPropertyBlacklist;
			  _securityManager.SubjectFactory = new ShiroSubjectFactory();
			  ( ( ModularRealmAuthenticator ) _securityManager.Authenticator ).AuthenticationStrategy = new ShiroAuthenticationStrategy();

			  _securityManager.SubjectDAO = CreateSubjectDAO();
		 }

		 private SubjectDAO CreateSubjectDAO()
		 {
			  DefaultSubjectDAO subjectDAO = new DefaultSubjectDAO();
			  DefaultSessionStorageEvaluator sessionStorageEvaluator = new DefaultSessionStorageEvaluator();
			  sessionStorageEvaluator.SessionStorageEnabled = false;
			  subjectDAO.SessionStorageEvaluator = sessionStorageEvaluator;
			  return subjectDAO;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.kernel.enterprise.api.security.EnterpriseLoginContext login(java.util.Map<String,Object> authToken) throws org.Neo4Net.kernel.api.security.exception.InvalidAuthTokenException
		 public override EnterpriseLoginContext Login( IDictionary<string, object> authToken )
		 {
			  try
			  {
					EnterpriseLoginContext securityContext;

					ShiroAuthToken token = new ShiroAuthToken( authToken );
					AssertValidScheme( token );

					try
					{
						 securityContext = new StandardEnterpriseLoginContext( this, ( ShiroSubject ) _securityManager.login( null, token ) );
						 AuthenticationResult authenticationResult = securityContext.Subject().AuthenticationResult;
						 if ( authenticationResult == AuthenticationResult.SUCCESS )
						 {
							  if ( _logSuccessfulLogin )
							  {
									_securityLog.info( securityContext.Subject(), "logged in" );
							  }
						 }
						 else if ( authenticationResult == AuthenticationResult.PASSWORD_CHANGE_REQUIRED )
						 {
							  _securityLog.info( securityContext.Subject(), "logged in (password change required)" );
						 }
						 else
						 {
							  string errorMessage = ( ( StandardEnterpriseLoginContext.NeoShiroSubject ) securityContext.Subject() ).AuthenticationFailureMessage;
							  _securityLog.error( "[%s]: failed to log in: %s", escape( token.Principal.ToString() ), errorMessage );
						 }
						 // No need to keep full Shiro authentication info around on the subject
						 ( ( StandardEnterpriseLoginContext.NeoShiroSubject ) securityContext.Subject() ).clearAuthenticationInfo();
					}
					catch ( UnsupportedTokenException e )
					{
						 _securityLog.error( "Unknown user failed to log in: %s", e.Message );
						 Exception cause = e.InnerException;
						 if ( cause is InvalidAuthTokenException )
						 {
							  throw new InvalidAuthTokenException( cause.Message + ": " + token );
						 }
						 throw invalidToken( ": " + token );
					}
					catch ( ExcessiveAttemptsException )
					{
						 // NOTE: We only get this with single (internal) realm authentication
						 securityContext = new StandardEnterpriseLoginContext( this, new ShiroSubject( _securityManager, AuthenticationResult.TOO_MANY_ATTEMPTS ) );
						 _securityLog.error( "[%s]: failed to log in: too many failed attempts", escape( token.Principal.ToString() ) );
					}
					catch ( AuthenticationException e )
					{
						 if ( e.InnerException != null && e.InnerException is AuthProviderTimeoutException )
						 {
							  Exception cause = e.InnerException.InnerException;
							  _securityLog.error( "[%s]: failed to log in: auth server timeout%s", escape( token.Principal.ToString() ), cause != null && cause.Message != null ? " (" + cause.Message + ")" : "" );
							  throw new AuthProviderTimeoutException( e.InnerException.Message, e.InnerException );
						 }
						 else if ( e.InnerException != null && e.InnerException is AuthProviderFailedException )
						 {
							  Exception cause = e.InnerException.InnerException;
							  _securityLog.error( "[%s]: failed to log in: auth server connection refused%s", escape( token.Principal.ToString() ), cause != null && cause.Message != null ? " (" + cause.Message + ")" : "" );
							  throw new AuthProviderFailedException( e.InnerException.Message, e.InnerException );
						 }
						 securityContext = new StandardEnterpriseLoginContext( this, new ShiroSubject( _securityManager, AuthenticationResult.FAILURE ) );
						 Exception cause = e.InnerException;
						 Exception causeCause = e.InnerException != null ? e.InnerException.InnerException : null;
						 string errorMessage = string.Format( "invalid principal or credentials{0}{1}", cause != null && cause.Message != null ? " (" + cause.Message + ")" : "", causeCause != null && causeCause.Message != null ? " (" + causeCause.Message + ")" : "" );
						 _securityLog.error( "[%s]: failed to log in: %s", escape( token.Principal.ToString() ), errorMessage );
					}

					return securityContext;
			  }
			  finally
			  {
					AuthToken.clearCredentials( authToken );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertValidScheme(ShiroAuthToken token) throws org.Neo4Net.kernel.api.security.exception.InvalidAuthTokenException
		 private void AssertValidScheme( ShiroAuthToken token )
		 {
			  string scheme = token.SchemeSilently;
			  if ( string.ReferenceEquals( scheme, null ) )
			  {
					throw invalidToken( "missing key `scheme`: " + token );
			  }
			  else if ( scheme.Equals( "none" ) )
			  {
					throw invalidToken( "scheme='none' only allowed when auth is disabled: " + token );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws Throwable
		 public override void Init()
		 {
			  foreach ( Realm realm in _realms )
			  {
					if ( realm is Initializable )
					{
						 ( ( Initializable ) realm ).init();
					}
					if ( realm is CachingRealm )
					{
						 ( ( CachingRealm ) realm ).CacheManager = _cacheManager;
					}
					if ( realm is RealmLifecycle )
					{
						 ( ( RealmLifecycle ) realm ).Initialize();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  foreach ( Realm realm in _realms )
			  {
					if ( realm is RealmLifecycle )
					{
						 ( ( RealmLifecycle ) realm ).Start();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  foreach ( Realm realm in _realms )
			  {
					if ( realm is RealmLifecycle )
					{
						 ( ( RealmLifecycle ) realm ).Stop();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			  foreach ( Realm realm in _realms )
			  {
					if ( realm is CachingRealm )
					{
						 ( ( CachingRealm ) realm ).CacheManager = null;
					}
					if ( realm is RealmLifecycle )
					{
						 ( ( RealmLifecycle ) realm ).Shutdown();
					}
			  }
		 }

		 public override EnterpriseUserManager GetUserManager( AuthSubject authSubject, bool isUserManager )
		 {
			  return new PersonalUserManager( _userManager, authSubject, _securityLog, isUserManager );
		 }

		 public override EnterpriseUserManager GetUserManager()
		 {
			  return _userManager;
		 }

		 public override void ClearAuthCache()
		 {
			  foreach ( Realm realm in _realms )
			  {
					if ( realm is AuthenticatingRealm )
					{
						 Cache<object, AuthenticationInfo> cache = ( ( AuthenticatingRealm ) realm ).AuthenticationCache;
						 if ( cache != null )
						 {
							  cache.clear();
						 }
					}
					if ( realm is AuthorizingRealm )
					{
						 Cache<object, AuthorizationInfo> cache = ( ( AuthorizingRealm ) realm ).AuthorizationCache;
						 if ( cache != null )
						 {
							  cache.clear();
						 }
					}
			  }
		 }

		 public virtual ICollection<AuthorizationInfo> GetAuthorizationInfo( PrincipalCollection principalCollection )
		 {
			  IList<AuthorizationInfo> infoList = new List<AuthorizationInfo>( 1 );
			  foreach ( Realm realm in _realms )
			  {
					if ( realm is ShiroAuthorizationInfoProvider )
					{
						 AuthorizationInfo info = ( ( ShiroAuthorizationInfoProvider ) realm ).GetAuthorizationInfoSnapshot( principalCollection );
						 if ( info != null )
						 {
							  infoList.Add( info );
						 }
					}
			  }
			  return infoList;
		 }

		 internal virtual System.Func<int, bool> GetPropertyPermissions( ISet<string> roles, System.Func<string, int> tokenLookup )
		 {
			  if ( _propertyAuthorization )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableIntSet blackListed = new org.eclipse.collections.impl.set.mutable.primitive.IntHashSet();
					MutableIntSet blackListed = new IntHashSet();
					foreach ( string role in roles )
					{
						 if ( _roleToPropertyBlacklist.ContainsKey( role ) )
						 {
							  Debug.Assert( _roleToPropertyBlacklist[role] != null, "Blacklist has to contain properties" );
							  foreach ( string propName in _roleToPropertyBlacklist[role] )
							  {

									try
									{
										 blackListed.add( tokenLookup( propName ) );
									}
									catch ( Exception )
									{
										 _securityLog.error( "Error in setting up property permissions, '" + propName + "' is not a valid property name." );
									}
							  }
						 }
					}
					return property => !blackListed.contains( property );
			  }
			  else
			  {
					return property => true;
			  }
		 }
	}

}