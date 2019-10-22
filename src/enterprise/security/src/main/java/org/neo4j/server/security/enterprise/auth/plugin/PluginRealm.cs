using System;
using System.Collections.Generic;

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
namespace Neo4Net.Server.security.enterprise.auth.plugin
{
	using AuthenticationException = org.apache.shiro.authc.AuthenticationException;
	using AuthenticationInfo = org.apache.shiro.authc.AuthenticationInfo;
	using AuthenticationToken = org.apache.shiro.authc.AuthenticationToken;
	using AuthorizationInfo = org.apache.shiro.authz.AuthorizationInfo;
	using Cache = org.apache.shiro.cache.Cache;
	using AuthorizingRealm = org.apache.shiro.realm.AuthorizingRealm;
	using PrincipalCollection = org.apache.shiro.subject.PrincipalCollection;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using InvalidAuthTokenException = Neo4Net.Kernel.api.security.exception.InvalidAuthTokenException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Version = Neo4Net.Kernel.Internal.Version;
	using Log = Neo4Net.Logging.Log;
	using AuthProviderOperations = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthProviderOperations;
	using AuthToken = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthToken;
	using AuthorizationExpiredException = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthorizationExpiredException;
	using AuthInfo = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthInfo;
	using AuthPlugin = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthPlugin;
	using AuthenticationPlugin = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthenticationPlugin;
	using AuthorizationPlugin = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthorizationPlugin;
	using CustomCacheableAuthenticationInfo = Neo4Net.Server.security.enterprise.auth.plugin.spi.CustomCacheableAuthenticationInfo;
	using SecurityLog = Neo4Net.Server.security.enterprise.log.SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.security.enterprise.configuration.SecuritySettings.PLUGIN_REALM_NAME_PREFIX;

	public class PluginRealm : AuthorizingRealm, RealmLifecycle, ShiroAuthorizationInfoProvider
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_authProviderOperations = new PluginRealmOperations( this );
		}

		 private AuthenticationPlugin _authenticationPlugin;
		 private AuthorizationPlugin _authorizationPlugin;
		 private readonly Config _config;
		 private AuthPlugin _authPlugin;
		 private readonly Log _log;
		 private readonly Clock _clock;
		 private readonly SecureHasher _secureHasher;

		 private AuthProviderOperations _authProviderOperations;

		 public PluginRealm( Config config, SecurityLog securityLog, Clock clock, SecureHasher secureHasher )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._config = config;
			  this._clock = clock;
			  this._secureHasher = secureHasher;
			  this._log = securityLog;

			  CredentialsMatcher = new CredentialsMatcher( this );

			  // Synchronize this default value with the javadoc for AuthProviderOperations.setAuthenticationCachingEnabled
			  AuthenticationCachingEnabled = false;

			  // Synchronize this default value with the javadoc for AuthProviderOperations.setAuthorizationCachingEnabled
			  AuthorizationCachingEnabled = true;

			  RolePermissionResolver = PredefinedRolesBuilder.rolePermissionResolver;
		 }

		 public PluginRealm( AuthenticationPlugin authenticationPlugin, AuthorizationPlugin authorizationPlugin, Config config, SecurityLog securityLog, Clock clock, SecureHasher secureHasher ) : this( config, securityLog, clock, secureHasher )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._authenticationPlugin = authenticationPlugin;
			  this._authorizationPlugin = authorizationPlugin;
			  ResolvePluginName();
		 }

		 public PluginRealm( AuthPlugin authPlugin, Config config, SecurityLog securityLog, Clock clock, SecureHasher secureHasher ) : this( config, securityLog, clock, secureHasher )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._authPlugin = authPlugin;
			  ResolvePluginName();
		 }

		 private void ResolvePluginName()
		 {
			  string pluginName = null;
			  if ( _authPlugin != null )
			  {
					pluginName = _authPlugin.name();
			  }
			  else if ( _authenticationPlugin != null )
			  {
					pluginName = _authenticationPlugin.name();
			  }
			  else if ( _authorizationPlugin != null )
			  {
					pluginName = _authorizationPlugin.name();
			  }

			  if ( !string.ReferenceEquals( pluginName, null ) && pluginName.Length > 0 )
			  {
					Name = PLUGIN_REALM_NAME_PREFIX + pluginName;
			  }
			  // Otherwise we rely on the Shiro default generated name
		 }

		 private ICollection<Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthorizationPlugin_PrincipalAndProvider> GetPrincipalAndProviderCollection( PrincipalCollection principalCollection )
		 {
			  ICollection<Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthorizationPlugin_PrincipalAndProvider> principalAndProviderCollection = new List<Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthorizationPlugin_PrincipalAndProvider>();

			  foreach ( string realm in principalCollection.RealmNames )
			  {
					foreach ( object principal in principalCollection.fromRealm( realm ) )
					{
						 principalAndProviderCollection.Add( new Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthorizationPlugin_PrincipalAndProvider( principal, realm ) );
					}
			  }

			  return principalAndProviderCollection;
		 }

		 protected internal override AuthorizationInfo DoGetAuthorizationInfo( PrincipalCollection principals )
		 {
			  if ( _authorizationPlugin != null )
			  {
					Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthorizationInfo authorizationInfo;
					try
					{
						  authorizationInfo = _authorizationPlugin.authorize( GetPrincipalAndProviderCollection( principals ) );
					}
					catch ( AuthorizationExpiredException e )
					{
						 throw new Neo4Net.GraphDb.security.AuthorizationExpiredException( "Plugin '" + Name + "' authorization info expired: " + e.Message, e );
					}
					if ( authorizationInfo != null )
					{
						 return PluginAuthorizationInfo.Create( authorizationInfo );
					}
			  }
			  else if ( _authPlugin != null && !principals.fromRealm( Name ).Empty )
			  {
					// The cached authorization info has expired.
					// Since we do not have the subject's credentials we cannot perform a new
					// authenticateAndAuthorize() to renew authorization info.
					// Instead we need to fail with a special status, so that the client can react by re-authenticating.
					throw new Neo4Net.GraphDb.security.AuthorizationExpiredException( "Plugin '" + Name + "' authorization info expired." );
			  }
			  return null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.apache.shiro.authc.AuthenticationInfo doGetAuthenticationInfo(org.apache.shiro.authc.AuthenticationToken token) throws org.apache.shiro.authc.AuthenticationException
		 protected internal override AuthenticationInfo DoGetAuthenticationInfo( AuthenticationToken token )
		 {
			  if ( token is ShiroAuthToken )
			  {
					try
					{
						 PluginApiAuthToken pluginAuthToken = PluginApiAuthToken.CreateFromMap( ( ( ShiroAuthToken ) token ).AuthTokenMap );
						 try
						 {
							  if ( _authPlugin != null )
							  {
									AuthInfo authInfo = _authPlugin.authenticateAndAuthorize( pluginAuthToken );
									if ( authInfo != null )
									{
										 PluginAuthInfo pluginAuthInfo = PluginAuthInfo.CreateCacheable( authInfo, Name, _secureHasher );

										 CacheAuthorizationInfo( pluginAuthInfo );

										 return pluginAuthInfo;
									}
							  }
							  else if ( _authenticationPlugin != null )
							  {
									Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthenticationInfo authenticationInfo = _authenticationPlugin.authenticate( pluginAuthToken );
									if ( authenticationInfo != null )
									{
										 return PluginAuthenticationInfo.CreateCacheable( authenticationInfo, Name, _secureHasher );
									}
							  }
						 }
						 finally
						 {
							  // Clear credentials
							  pluginAuthToken.ClearCredentials();
						 }
					}
					catch ( Exception e ) when ( e is Neo4Net.Server.security.enterprise.auth.plugin.api.AuthenticationException || e is InvalidAuthTokenException )
					{
						 throw new AuthenticationException( e.Message, e.Cause );
					}
			  }
			  return null;
		 }

		 private void CacheAuthorizationInfo( PluginAuthInfo authInfo )
		 {
			  // Use the existing authorizationCache in our base class
			  Cache<object, AuthorizationInfo> authorizationCache = AuthorizationCache;
			  object key = GetAuthorizationCacheKey( authInfo.Principals );
			  authorizationCache.put( key, authInfo );
		 }

		 public virtual bool CanAuthenticate()
		 {
			  return _authPlugin != null || _authenticationPlugin != null;
		 }

		 public virtual bool CanAuthorize()
		 {
			  return _authPlugin != null || _authorizationPlugin != null;
		 }

		 public override AuthorizationInfo GetAuthorizationInfoSnapshot( PrincipalCollection principalCollection )
		 {
			  return getAuthorizationInfo( principalCollection );
		 }

		 protected internal override object GetAuthorizationCacheKey( PrincipalCollection principals )
		 {
			  return getAvailablePrincipal( principals );
		 }

		 protected internal override object GetAuthenticationCacheKey( AuthenticationToken token )
		 {
			  return token != null ? token.Principal : null;
		 }

		 public override bool Supports( AuthenticationToken token )
		 {
			  return SupportsSchemeAndRealm( token );
		 }

		 private bool SupportsSchemeAndRealm( AuthenticationToken token )
		 {
			  if ( token is ShiroAuthToken )
			  {
					ShiroAuthToken shiroAuthToken = ( ShiroAuthToken ) token;
					return shiroAuthToken.SupportsRealm( Name );
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void initialize() throws Throwable
		 public override void Initialize()
		 {
			  if ( _authenticationPlugin != null )
			  {
					_authenticationPlugin.initialize( _authProviderOperations );
			  }
			  if ( _authorizationPlugin != null && _authorizationPlugin != _authenticationPlugin )
			  {
					_authorizationPlugin.initialize( _authProviderOperations );
			  }
			  if ( _authPlugin != null )
			  {
					_authPlugin.initialize( _authProviderOperations );
			  }
		 }

		 public override void Start()
		 {
			  if ( _authenticationPlugin != null )
			  {
					_authenticationPlugin.start();
			  }
			  if ( _authorizationPlugin != null && _authorizationPlugin != _authenticationPlugin )
			  {
					_authorizationPlugin.start();
			  }
			  if ( _authPlugin != null )
			  {
					_authPlugin.start();
			  }
		 }

		 public override void Stop()
		 {
			  if ( _authenticationPlugin != null )
			  {
					_authenticationPlugin.stop();
			  }
			  if ( _authorizationPlugin != null && _authorizationPlugin != _authenticationPlugin )
			  {
					_authorizationPlugin.stop();
			  }
			  if ( _authPlugin != null )
			  {
					_authPlugin.stop();
			  }
		 }

		 public override void Shutdown()
		 {
			  if ( _authenticationPlugin != null )
			  {
					_authenticationPlugin.shutdown();
			  }
			  if ( _authorizationPlugin != null && _authorizationPlugin != _authenticationPlugin )
			  {
					_authorizationPlugin.shutdown();
			  }
			  if ( _authPlugin != null )
			  {
					_authPlugin.shutdown();
			  }
		 }

		 private static Neo4Net.Server.security.enterprise.auth.plugin.spi.CustomCacheableAuthenticationInfo_CredentialsMatcher GetCustomCredentialsMatcherIfPresent( AuthenticationInfo info )
		 {
			  if ( info is CustomCredentialsMatcherSupplier )
			  {
					return ( ( CustomCredentialsMatcherSupplier ) info ).CredentialsMatcher;
			  }
			  return null;
		 }

		 private class CredentialsMatcher : org.apache.shiro.authc.credential.CredentialsMatcher
		 {
			 private readonly PluginRealm _outerInstance;

			 public CredentialsMatcher( PluginRealm outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override bool DoCredentialsMatch( AuthenticationToken token, AuthenticationInfo info )
			  {
					Neo4Net.Server.security.enterprise.auth.plugin.spi.CustomCacheableAuthenticationInfo_CredentialsMatcher customCredentialsMatcher = GetCustomCredentialsMatcherIfPresent( info );

					if ( customCredentialsMatcher != null )
					{
						 // Authentication info is originating from a CustomCacheableAuthenticationInfo
						 IDictionary<string, object> authToken = ( ( ShiroAuthToken ) token ).AuthTokenMap;
						 try
						 {
							  AuthToken pluginApiAuthToken = PluginApiAuthToken.CreateFromMap( authToken );
							  try
							  {
									return customCredentialsMatcher.DoCredentialsMatch( pluginApiAuthToken );
							  }
							  finally
							  {
									// Clear credentials
									char[] credentials = pluginApiAuthToken.Credentials();
									if ( credentials != null )
									{
										 Arrays.fill( credentials, ( char ) 0 );
									}
							  }
						 }
						 catch ( InvalidAuthTokenException e )
						 {
							  throw new AuthenticationException( e.Message );
						 }
					}
					else if ( info.Credentials != null )
					{
						 // Authentication info is originating from a CacheableAuthenticationInfo or a CacheableAuthInfo
						 PluginShiroAuthToken pluginShiroAuthToken = PluginShiroAuthToken.Of( token );
						 try
						 {
							  return outerInstance.secureHasher.HashedCredentialsMatcher.doCredentialsMatch( pluginShiroAuthToken, info );
						 }
						 finally
						 {
							  pluginShiroAuthToken.ClearCredentials();
						 }
					}
					else
					{
						 // Authentication info is originating from an AuthenticationInfo or an AuthInfo
						 if ( _outerInstance.AuthenticationCachingEnabled )
						 {
							  outerInstance.log.Error( "Authentication caching is enabled in plugin %s but it does not return " + "cacheable credentials. This configuration is not secure.", Name );
							  return false;
						 }
						 return true; // Always match if we do not cache credentials
					}
			  }
		 }

		 private class PluginRealmOperations : AuthProviderOperations
		 {
			 private readonly PluginRealm _outerInstance;

			 public PluginRealmOperations( PluginRealm outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal Log innerLog = new LogAnonymousInnerClass();

			  private class LogAnonymousInnerClass : Log
			  {
				  private string withPluginName( string msg )
				  {
						return "{" + Name + "} " + msg;
				  }

				  public void debug( string message )
				  {
						outerInstance.outerInstance.log.debug( withPluginName( message ) );
				  }

				  public void info( string message )
				  {
						outerInstance.outerInstance.log.info( withPluginName( message ) );
				  }

				  public void warn( string message )
				  {
						outerInstance.outerInstance.log.warn( withPluginName( message ) );
				  }

				  public void error( string message )
				  {
						outerInstance.outerInstance.log.error( withPluginName( message ) );
				  }

				  public bool DebugEnabled
				  {
					  get
					  {
							return outerInstance.outerInstance.log.DebugEnabled;
					  }
				  }
			  }

			  public override Path Neo4NetHome()
			  {
					return outerInstance.config.Get( GraphDatabaseSettings.Neo4Net_home ).AbsoluteFile.toPath();
			  }

			  public override Optional<Path> Neo4NetConfigFile()
			  {
					return null;
			  }

			  public override string Neo4NetVersion()
			  {
					return Version.Neo4NetVersion;
			  }

			  public override Clock Clock()
			  {
					return outerInstance.clock;
			  }

			  public override Log Log()
			  {
					return innerLog;
			  }

			  public virtual bool AuthenticationCachingEnabled
			  {
				  set
				  {
						_outerInstance.AuthenticationCachingEnabled = value;
				  }
			  }

			  public virtual bool AuthorizationCachingEnabled
			  {
				  set
				  {
						_outerInstance.AuthorizationCachingEnabled = value;
				  }
			  }
		 }
	}

}