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
namespace Neo4Net.Server.security.enterprise.auth
{
	using AuthenticationInfo = org.apache.shiro.authc.AuthenticationInfo;
	using AuthenticationToken = org.apache.shiro.authc.AuthenticationToken;
	using AllowAllCredentialsMatcher = org.apache.shiro.authc.credential.AllowAllCredentialsMatcher;
	using AuthorizationException = org.apache.shiro.authz.AuthorizationException;
	using AuthorizationInfo = org.apache.shiro.authz.AuthorizationInfo;
	using SimpleAuthorizationInfo = org.apache.shiro.authz.SimpleAuthorizationInfo;
	using Cache = org.apache.shiro.cache.Cache;
	using SimpleHash = org.apache.shiro.crypto.hash.SimpleHash;
	using DefaultLdapRealm = org.apache.shiro.realm.ldap.DefaultLdapRealm;
	using JndiLdapContextFactory = org.apache.shiro.realm.ldap.JndiLdapContextFactory;
	using LdapContextFactory = org.apache.shiro.realm.ldap.LdapContextFactory;
	using LdapUtils = org.apache.shiro.realm.ldap.LdapUtils;
	using PrincipalCollection = org.apache.shiro.subject.PrincipalCollection;


	using AuthProviderFailedException = Neo4Net.GraphDb.security.AuthProviderFailedException;
	using AuthProviderTimeoutException = Neo4Net.GraphDb.security.AuthProviderTimeoutException;
	using AuthorizationExpiredException = Neo4Net.GraphDb.security.AuthorizationExpiredException;
	using AuthenticationResult = Neo4Net.Internal.Kernel.Api.security.AuthenticationResult;
	using AuthToken = Neo4Net.Kernel.api.security.AuthToken;
	using InvalidAuthTokenException = Neo4Net.Kernel.api.security.exception.InvalidAuthTokenException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using SecuritySettings = Neo4Net.Server.security.enterprise.configuration.SecuritySettings;
	using SecurityLog = Neo4Net.Server.security.enterprise.log.SecurityLog;

	/// <summary>
	/// Shiro realm for LDAP based on configuration settings
	/// </summary>
	public class LdapRealm : DefaultLdapRealm, RealmLifecycle, ShiroAuthorizationInfoProvider
	{
		 private const string GROUP_DELIMITER = ";";
		 private const string KEY_VALUE_DELIMITER = "=";
		 private const string ROLE_DELIMITER = ",";
		 public const string LDAP_REALM = "ldap";

		 private const string JNDI_LDAP_CONNECT_TIMEOUT = "com.sun.jndi.ldap.connect.timeout";
		 private const string JNDI_LDAP_READ_TIMEOUT = "com.sun.jndi.ldap.read.timeout";
		 private const string JNDI_LDAP_CONNECTION_TIMEOUT_MESSAGE_PART = "timed out"; // "connect timed out"
		 private const string JNDI_LDAP_READ_TIMEOUT_MESSAGE_PART = "timed out"; // "LDAP response read timed out"

		 public const string LDAP_CONNECTION_TIMEOUT_CLIENT_MESSAGE = "LDAP connection timed out.";
		 public const string LDAP_READ_TIMEOUT_CLIENT_MESSAGE = "LDAP response timed out.";
		 public const string LDAP_AUTHORIZATION_FAILURE_CLIENT_MESSAGE = "LDAP authorization request failed.";
		 public const string LDAP_CONNECTION_REFUSED_CLIENT_MESSAGE = "LDAP connection refused.";

		 private bool? _authenticationEnabled;
		 private bool? _authorizationEnabled;
		 private bool? _useStartTls;
		 private bool _useSAMAccountName;
		 private string _userSearchBase;
		 private string _userSearchFilter;
		 private IList<string> _membershipAttributeNames;
		 private bool? _useSystemAccountForAuthorization;
		 private IDictionary<string, ICollection<string>> _groupToRoleMapping;
		 private readonly SecurityLog _securityLog;
		 private readonly SecureHasher _secureHasher;

		 // Parser regex for group-to-role-mapping
		 private const string KEY_GROUP = "\\s*('(.+)'|\"(.+)\"|(\\S)|(\\S.*\\S))\\s*";
		 private const string VALUE_GROUP = "\\s*(.*)";
		 private Pattern _keyValuePattern = Pattern.compile( KEY_GROUP + KEY_VALUE_DELIMITER + VALUE_GROUP );

		 public LdapRealm( Config config, SecurityLog securityLog, SecureHasher secureHasher ) : base()
		 {
			  this._securityLog = securityLog;
			  this._secureHasher = secureHasher;
			  Name = SecuritySettings.LDAP_REALM_NAME;
			  RolePermissionResolver = PredefinedRolesBuilder.RolePermissionResolver;
			  ConfigureRealm( config );
			  if ( AuthenticationCachingEnabled )
			  {
					CredentialsMatcher = secureHasher.HashedCredentialsMatcher;
			  }
			  else
			  {
					CredentialsMatcher = new AllowAllCredentialsMatcher();
			  }
		 }

		 private string WithRealm( string template, params object[] args )
		 {
			  return "{LdapRealm}: " + format( template, args );
		 }

		 private string Server( JndiLdapContextFactory jndiLdapContextFactory )
		 {
			  return "'" + jndiLdapContextFactory.Url + "'" + ( _useStartTls ? " using StartTLS" : "" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.apache.shiro.authc.AuthenticationInfo queryForAuthenticationInfo(org.apache.shiro.authc.AuthenticationToken token, org.apache.shiro.realm.ldap.LdapContextFactory ldapContextFactory) throws javax.naming.NamingException
		 protected internal override AuthenticationInfo QueryForAuthenticationInfo( AuthenticationToken token, LdapContextFactory ldapContextFactory )
		 {
			  if ( _authenticationEnabled.Value )
			  {
					if ( _useSAMAccountName )
					{
						 return QueryForAuthenticationInfoSAM( token, ldapContextFactory );
					}
					else
					{
						 string serverString = Server( ( JndiLdapContextFactory ) ldapContextFactory );
						 try
						 {
							  AuthenticationInfo info = _useStartTls ? QueryForAuthenticationInfoUsingStartTls( token, ldapContextFactory ) : base.QueryForAuthenticationInfo( token, ldapContextFactory );
							  _securityLog.debug( WithRealm( "Authenticated user '%s' against %s", token.Principal, serverString ) );
							  return info;
						 }
						 catch ( Exception e )
						 {
							  if ( IsExceptionAnLdapConnectionTimeout( e ) )
							  {
									throw new AuthProviderTimeoutException( LDAP_CONNECTION_TIMEOUT_CLIENT_MESSAGE, e );
							  }
							  else if ( IsExceptionAnLdapReadTimeout( e ) )
							  {
									throw new AuthProviderTimeoutException( LDAP_READ_TIMEOUT_CLIENT_MESSAGE, e );
							  }
							  else if ( IsExceptionConnectionRefused( e ) )
							  {
									throw new AuthProviderFailedException( LDAP_CONNECTION_REFUSED_CLIENT_MESSAGE, e );
							  }
							  // This exception will be caught and rethrown by Shiro, and then by us, so we do not need to wrap it here
							  throw e;
						 }
					}
			  }
			  else
			  {
					return null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.apache.shiro.authc.AuthenticationInfo queryForAuthenticationInfoUsingStartTls(org.apache.shiro.authc.AuthenticationToken token, org.apache.shiro.realm.ldap.LdapContextFactory ldapContextFactory) throws javax.naming.NamingException
		 protected internal virtual AuthenticationInfo QueryForAuthenticationInfoUsingStartTls( AuthenticationToken token, LdapContextFactory ldapContextFactory )
		 {
			  object principal = getLdapPrincipal( token );
			  object credentials = token.Credentials;

			  LdapContext ctx = null;

			  try
			  {
					ctx = GetLdapContextUsingStartTls( ldapContextFactory, principal, credentials );
					return CreateAuthenticationInfo( token, principal, credentials, ctx );
			  }
			  finally
			  {
					LdapUtils.closeContext( ctx );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private javax.naming.ldap.LdapContext getLdapContextUsingStartTls(org.apache.shiro.realm.ldap.LdapContextFactory ldapContextFactory, Object principal, Object credentials) throws javax.naming.NamingException
		 private LdapContext GetLdapContextUsingStartTls( LdapContextFactory ldapContextFactory, object principal, object credentials )
		 {
			  JndiLdapContextFactory jndiLdapContextFactory = ( JndiLdapContextFactory ) ldapContextFactory;
			  Dictionary<string, object> env = new Dictionary<string, object>();
			  env[Context.INITIAL_CONTEXT_FACTORY] = jndiLdapContextFactory.ContextFactoryClassName;
			  env[Context.PROVIDER_URL] = jndiLdapContextFactory.Url;

			  LdapContext ctx = null;

			  try
			  {
					ctx = new InitialLdapContext( env, null );

					StartTlsRequest startTlsRequest = new StartTlsRequest();
					StartTlsResponse tls = ( StartTlsResponse ) ctx.extendedOperation( startTlsRequest );

					tls.negotiate();

					ctx.addToEnvironment( Context.SECURITY_AUTHENTICATION, jndiLdapContextFactory.AuthenticationMechanism );
					ctx.addToEnvironment( Context.SECURITY_PRINCIPAL, principal );
					ctx.addToEnvironment( Context.SECURITY_CREDENTIALS, credentials );

					// do a lookup of the user to trigger authentication
					ctx.lookup( principal.ToString() );

					return ctx;
			  }
			  catch ( IOException e )
			  {
					LdapUtils.closeContext( ctx );
					_securityLog.error( WithRealm( "Failed to negotiate TLS connection with '%s': ", Server( jndiLdapContextFactory ), e ) );
					throw new CommunicationException( e.Message );
			  }
			  catch ( Exception t )
			  {
					LdapUtils.closeContext( ctx );
					_securityLog.error( WithRealm( "Unexpected failure to negotiate TLS connection with '%s': ", Server( jndiLdapContextFactory ), t ) );
					throw t;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.apache.shiro.authz.AuthorizationInfo queryForAuthorizationInfo(org.apache.shiro.subject.PrincipalCollection principals, org.apache.shiro.realm.ldap.LdapContextFactory ldapContextFactory) throws javax.naming.NamingException
		 protected internal override AuthorizationInfo QueryForAuthorizationInfo( PrincipalCollection principals, LdapContextFactory ldapContextFactory )
		 {
			  if ( _authorizationEnabled.Value )
			  {
					string username = GetUsername( principals );
					if ( string.ReferenceEquals( username, null ) )
					{
						 return null;
					}

					if ( _useSystemAccountForAuthorization.Value )
					{
						 // Perform context search using the system context
						 LdapContext ldapContext = _useStartTls ? GetSystemLdapContextUsingStartTls( ldapContextFactory ) : ldapContextFactory.SystemLdapContext;

						 ISet<string> roleNames;
						 try
						 {
							  roleNames = FindRoleNamesForUser( username, ldapContext );
						 }
						 finally
						 {
							  LdapUtils.closeContext( ldapContext );
						 }

						 return new SimpleAuthorizationInfo( roleNames );
					}
					else
					{
						 // Authorization info is cached during authentication
						 Cache<object, AuthorizationInfo> authorizationCache = AuthorizationCache;
						 AuthorizationInfo authorizationInfo = authorizationCache.get( username );
						 if ( authorizationInfo == null )
						 {
							  // The cached authorization info has expired.
							  // Since we do not have the subject's credentials we cannot perform a new LDAP search
							  // for authorization info. Instead we need to fail with a special status,
							  // so that the client can react by re-authenticating.
							  throw new AuthorizationExpiredException( "LDAP authorization info expired." );
						 }
						 return authorizationInfo;
					}
			  }
			  return null;
		 }

		 private string GetUsername( PrincipalCollection principals )
		 {
			  string username = null;
			  System.Collections.ICollection ldapPrincipals = principals.fromRealm( Name );
			  if ( ldapPrincipals.Count > 0 )
			  {
					username = ( string ) ldapPrincipals.GetEnumerator().next();
			  }
			  else if ( _useSystemAccountForAuthorization.Value )
			  {
					username = ( string ) principals.PrimaryPrincipal;
			  }
			  return username;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private javax.naming.ldap.LdapContext getSystemLdapContextUsingStartTls(org.apache.shiro.realm.ldap.LdapContextFactory ldapContextFactory) throws javax.naming.NamingException
		 private LdapContext GetSystemLdapContextUsingStartTls( LdapContextFactory ldapContextFactory )
		 {
			  JndiLdapContextFactory jndiLdapContextFactory = ( JndiLdapContextFactory ) ldapContextFactory;
			  return GetLdapContextUsingStartTls( ldapContextFactory, jndiLdapContextFactory.SystemUsername, jndiLdapContextFactory.SystemPassword );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.apache.shiro.authc.AuthenticationInfo createAuthenticationInfo(org.apache.shiro.authc.AuthenticationToken token, Object ldapPrincipal, Object ldapCredentials, javax.naming.ldap.LdapContext ldapContext) throws javax.naming.NamingException
		 protected internal override AuthenticationInfo CreateAuthenticationInfo( AuthenticationToken token, object ldapPrincipal, object ldapCredentials, LdapContext ldapContext )
		 {
			  // If authorization is enabled but useSystemAccountForAuthorization is disabled, we should perform
			  // the search for groups directly here while the user's authenticated ldap context is open.
			  if ( _authorizationEnabled && !_useSystemAccountForAuthorization )
			  {
					string username = ( string ) token.Principal;
					ISet<string> roleNames = FindRoleNamesForUser( username, ldapContext );
					CacheAuthorizationInfo( username, roleNames );
			  }

			  if ( AuthenticationCachingEnabled )
			  {
					SimpleHash hashedCredentials = _secureHasher.hash( ( sbyte[] ) token.Credentials );
					return new ShiroAuthenticationInfo( token.Principal, hashedCredentials.Bytes, hashedCredentials.Salt, Name, AuthenticationResult.SUCCESS );
			  }
			  else
			  {
					return new ShiroAuthenticationInfo( token.Principal, Name, AuthenticationResult.SUCCESS );
			  }
		 }

		 public override bool Supports( AuthenticationToken token )
		 {
			  return SupportsSchemeAndRealm( token );
		 }

		 private bool SupportsSchemeAndRealm( AuthenticationToken token )
		 {
			  try
			  {
					if ( token is ShiroAuthToken )
					{
						 ShiroAuthToken shiroAuthToken = ( ShiroAuthToken ) token;
						 return shiroAuthToken.Scheme.Equals( Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME ) && ( shiroAuthToken.SupportsRealm( LDAP_REALM ) );
					}
					return false;
			  }
			  catch ( InvalidAuthTokenException )
			  {
					return false;
			  }
		 }

		 protected internal override AuthorizationInfo DoGetAuthorizationInfo( PrincipalCollection principals )
		 {
			  try
			  {
					AuthorizationInfo info = base.DoGetAuthorizationInfo( principals );
					_securityLog.debug( WithRealm( "Queried for authorization info for user '%s'", principals.PrimaryPrincipal ) );
					return info;
			  }
			  catch ( AuthorizationException e )
			  {
					_securityLog.warn( WithRealm( "Failed to get authorization info: '%s' caused by '%s'", e.Message, e.InnerException.Message ) );
					return null;
			  }
		 }

		 // Unfortunately we need to identify timeouts by looking at the exception messages, which is not very robust.
		 // To make it slightly more robust we look for a key part of the actual message
		 private bool IsExceptionAnLdapReadTimeout( Exception e )
		 {
			  return e is NamingException && e.Message.contains( JNDI_LDAP_READ_TIMEOUT_MESSAGE_PART );
		 }

		 private bool IsExceptionAnLdapConnectionTimeout( Exception e )
		 {
			  return e is CommunicationException && ( ( ( CommunicationException ) e ).RootCause is SocketTimeoutException || ( ( CommunicationException ) e ).RootCause.Message.contains( JNDI_LDAP_CONNECTION_TIMEOUT_MESSAGE_PART ) );
		 }

		 private bool IsExceptionConnectionRefused( Exception e )
		 {
			  return e is CommunicationException && ( ( CommunicationException ) e ).RootCause is ConnectException;
		 }

		 private bool IsAuthorizationExceptionAnLdapReadTimeout( AuthorizationException e )
		 {
			  // Shiro's doGetAuthorizationInfo() wraps a NamingException in an AuthorizationException
			  return e.InnerException != null && e.InnerException is NamingException && e.InnerException.Message.contains( JNDI_LDAP_READ_TIMEOUT_MESSAGE_PART );
		 }

		 private void CacheAuthorizationInfo( string username, ISet<string> roleNames )
		 {
			  // Use the existing authorizationCache in our base class
			  Cache<object, AuthorizationInfo> authorizationCache = AuthorizationCache;
			  authorizationCache.put( username, new SimpleAuthorizationInfo( roleNames ) );
		 }

		 private void ConfigureRealm( Config config )
		 {
			  JndiLdapContextFactory contextFactory = new JndiLdapContextFactory();
			  IDictionary<string, object> environment = contextFactory.Environment;
			  long? connectionTimeoutMillis = config.Get( SecuritySettings.ldap_connection_timeout ).toMillis();
			  long? readTimeoutMillis = config.Get( SecuritySettings.ldap_read_timeout ).toMillis();
			  environment[JNDI_LDAP_CONNECT_TIMEOUT] = connectionTimeoutMillis.ToString();
			  environment[JNDI_LDAP_READ_TIMEOUT] = readTimeoutMillis.ToString();
			  contextFactory.Environment = environment;
			  contextFactory.Url = ParseLdapServerUrl( config.Get( SecuritySettings.ldap_server ) );
			  contextFactory.AuthenticationMechanism = config.Get( SecuritySettings.ldap_authentication_mechanism );
			  contextFactory.Referral = config.Get( SecuritySettings.ldap_referral );
			  contextFactory.SystemUsername = config.Get( SecuritySettings.ldap_authorization_system_username );
			  contextFactory.SystemPassword = config.Get( SecuritySettings.ldap_authorization_system_password );
			  contextFactory.PoolingEnabled = config.Get( SecuritySettings.ldap_authorization_connection_pooling );

			  ContextFactory = contextFactory;

			  string userDnTemplate = config.Get( SecuritySettings.ldap_authentication_user_dn_template );
			  if ( !string.ReferenceEquals( userDnTemplate, null ) )
			  {
					UserDnTemplate = userDnTemplate;
			  }

			  _authenticationEnabled = config.Get( SecuritySettings.ldap_authentication_enabled );
			  _authorizationEnabled = config.Get( SecuritySettings.ldap_authorization_enabled );
			  _useStartTls = config.Get( SecuritySettings.ldap_use_starttls );

			  _userSearchBase = config.Get( SecuritySettings.ldap_authorization_user_search_base );
			  _userSearchFilter = config.Get( SecuritySettings.ldap_authorization_user_search_filter );
			  _useSAMAccountName = config.Get( SecuritySettings.ldap_authentication_use_samaccountname );
			  _membershipAttributeNames = config.Get( SecuritySettings.ldap_authorization_group_membership_attribute_names );
			  _useSystemAccountForAuthorization = config.Get( SecuritySettings.ldap_authorization_use_system_account );
			  _groupToRoleMapping = ParseGroupToRoleMapping( config.Get( SecuritySettings.ldap_authorization_group_to_role_mapping ) );

			  AuthenticationCachingEnabled = config.Get( SecuritySettings.ldap_authentication_cache_enabled );
			  AuthorizationCachingEnabled = true;
		 }

		 private string ParseLdapServerUrl( string rawLdapServer )
		 {
			  return ( string.ReferenceEquals( rawLdapServer, null ) ) ? null : rawLdapServer.Contains( "://" ) ? rawLdapServer : "ldap://" + rawLdapServer;
		 }

		 private IDictionary<string, ICollection<string>> ParseGroupToRoleMapping( string groupToRoleMappingString )
		 {
			  IDictionary<string, ICollection<string>> map = new Dictionary<string, ICollection<string>>();

			  if ( !string.ReferenceEquals( groupToRoleMappingString, null ) )
			  {
					foreach ( string groupAndRoles in groupToRoleMappingString.Split( GROUP_DELIMITER, true ) )
					{
						 if ( groupAndRoles.Length > 0 )
						 {
							  Matcher matcher = _keyValuePattern.matcher( groupAndRoles );
							  if ( !( matcher.find() && matcher.groupCount() == 6 ) )
							  {
									string errorMessage = format( "Failed to parse setting %s: wrong number of fields", SecuritySettings.ldap_authorization_group_to_role_mapping.name() );
									throw new System.ArgumentException( errorMessage );
							  }

							  string group = matcher.group( 2 ) != null ? matcher.group( 2 ) : matcher.group( 3 ) != null ? matcher.group( 3 ) : matcher.group( 4 ) != null ? matcher.group( 4 ) : matcher.group( 5 ) != null ? matcher.group( 5 ) : "";

							  if ( group.Length == 0 )
							  {
									string errorMessage = format( "Failed to parse setting %s: empty group name", SecuritySettings.ldap_authorization_group_to_role_mapping.name() );
									throw new System.ArgumentException( errorMessage );
							  }
							  ICollection<string> roleList = new List<string>();
							  foreach ( string role in matcher.group( 6 ).Trim().Split(ROLE_DELIMITER) )
							  {
									if ( role.Length > 0 )
									{
										 roleList.Add( role );
									}
							  }
							  // We only support case-insensitive comparison of group DNs
							  map[group.ToLower()] = roleList;
						 }
					}
			  }

			  return map;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.shiro.authc.AuthenticationInfo queryForAuthenticationInfoSAM(org.apache.shiro.authc.AuthenticationToken token, org.apache.shiro.realm.ldap.LdapContextFactory ldapContextFactory) throws javax.naming.NamingException
		 private AuthenticationInfo QueryForAuthenticationInfoSAM( AuthenticationToken token, LdapContextFactory ldapContextFactory )
		 {
			  object principal = token.Principal;
			  object credentials = token.Credentials;

			  LdapContext ctx = null;
			  try
			  {
					ctx = _useStartTls ? GetSystemLdapContextUsingStartTls( ldapContextFactory ) : ldapContextFactory.SystemLdapContext;
					string[] attrs = new string[] { "cn" };
					SearchControls searchCtls = new SearchControls( SearchControls.SUBTREE_SCOPE, 1, 0, attrs, false, false );
					object[] searchArguments = new object[]{ principal };
					string filter = "sAMAccountName={0}";
					NamingEnumeration<SearchResult> search = ctx.search( _userSearchBase, filter, searchArguments, searchCtls );
					if ( search.hasMore() )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.naming.directory.SearchResult next = search.next();
						 SearchResult next = search.next();
						 string loginUser = next.NameInNamespace;
						 if ( search.hasMore() )
						 {
							  _securityLog.error( "More than one user matching: " + principal );
							  throw new AuthenticationException( "More than one user matching: " + principal );
						 }
						 else
						 {
							  LdapContext ctx2 = ldapContextFactory.getLdapContext( loginUser, credentials );
							  LdapUtils.closeContext( ctx2 );
						 }
					}
					else
					{
						 throw new AuthenticationException( "No user matching: " + principal );
					}
					return CreateAuthenticationInfo( token, principal, credentials, ctx );
			  }
			  finally
			  {
					LdapUtils.closeContext( ctx );
			  }
		 }

		 // TODO: Extract to an LdapAuthorizationStrategy ? This ("group by attribute") is one of multiple possible strategies
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Set<String> findRoleNamesForUser(String username, javax.naming.ldap.LdapContext ldapContext) throws javax.naming.NamingException
		 internal virtual ISet<string> FindRoleNamesForUser( string username, LdapContext ldapContext )
		 {
			  ISet<string> roleNames = new LinkedHashSet<string>();

			  SearchControls searchCtls = new SearchControls();
			  searchCtls.SearchScope = SearchControls.SUBTREE_SCOPE;
			  searchCtls.ReturningAttributes = _membershipAttributeNames.ToArray();

			  // Use search argument to prevent potential code injection
			  object[] searchArguments = new object[]{ username };

			  NamingEnumeration result = ldapContext.search( _userSearchBase, _userSearchFilter, searchArguments, searchCtls );

			  if ( result.hasMoreElements() )
			  {
					SearchResult searchResult = ( SearchResult ) result.next();

					if ( result.hasMoreElements() )
					{
						 _securityLog.warn( _securityLog.DebugEnabled ? WithRealm( "LDAP user search for user principal '%s' is ambiguous. The first match that will " + "be checked for group membership is '%s' but the search also matches '%s'. " + "Please check your LDAP realm configuration.", username, searchResult.ToString(), result.next().ToString() ) : WithRealm("LDAP user search for user principal '%s' is ambiguous. The search matches more " + "than one entry. Please check your LDAP realm configuration.", username) );
					}

					Attributes attributes = searchResult.Attributes;
					if ( attributes != null )
					{
						 NamingEnumeration attributeEnumeration = attributes.All;
						 while ( attributeEnumeration.hasMore() )
						 {
							  Attribute attribute = ( Attribute ) attributeEnumeration.next();
							  string attributeId = attribute.ID;
							  if ( _membershipAttributeNames.Any( attributeId.equalsIgnoreCase ) )
							  {
									ICollection<string> groupNames = LdapUtils.getAllAttributeValues( attribute );
									ICollection<string> rolesForGroups = GetRoleNamesForGroups( groupNames );
									roleNames.addAll( rolesForGroups );
							  }
						 }
					}
			  }
			  return roleNames;
		 }

		 private void AssertValidUserSearchSettings()
		 {
			  bool proceedWithSearch = true;

			  if ( string.ReferenceEquals( _userSearchBase, null ) || _userSearchBase.Length == 0 )
			  {
					_securityLog.error( "LDAP user search base is empty." );
					proceedWithSearch = false;
			  }
			  if ( string.ReferenceEquals( _userSearchFilter, null ) || !_userSearchFilter.Contains( "{0}" ) )
			  {
					_securityLog.warn( "LDAP user search filter does not contain the argument placeholder {0}, " + "so the search result will be independent of the user principal." );
			  }
			  if ( _membershipAttributeNames == null || _membershipAttributeNames.Count == 0 )
			  {
					// If we don't have any attributes to look for we will never find anything
					_securityLog.error( "LDAP group membership attribute names are empty. Authorization will not be possible." );
					proceedWithSearch = false;
			  }

			  if ( !proceedWithSearch )
			  {
					throw new System.ArgumentException( "Illegal LDAP user search settings, see security log for details." );
			  }
		 }

		 private ICollection<string> GetRoleNamesForGroups( ICollection<string> groupNames )
		 {
			  ICollection<string> roles = new List<string>();
			  foreach ( string group in groupNames )
			  {
					ICollection<string> rolesForGroup = _groupToRoleMapping[group.ToLower()];
					if ( rolesForGroup != null )
					{
						 roles.addAll( rolesForGroup );
					}
			  }
			  return roles;
		 }

		 // Exposed for testing
		 internal virtual IDictionary<string, ICollection<string>> GroupToRoleMapping
		 {
			 get
			 {
				  return _groupToRoleMapping;
			 }
		 }

		 public override void Initialize()
		 {
			  if ( _authorizationEnabled.Value )
			  {
					// For some combinations of settings we will never find anything
					AssertValidUserSearchSettings();
			  }
		 }

		 public override void Start()
		 {
		 }

		 public override void Stop()
		 {
		 }

		 public override void Shutdown()
		 {
		 }

		 public override AuthorizationInfo GetAuthorizationInfoSnapshot( PrincipalCollection principalCollection )
		 {
			  return getAuthorizationInfo( principalCollection );
		 }
	}

}