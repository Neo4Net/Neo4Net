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
	using Ticker = com.github.benmanes.caffeine.cache.Ticker;
	using CacheManager = org.apache.shiro.cache.CacheManager;
	using Realm = org.apache.shiro.realm.Realm;


	using SetDefaultAdminCommand = Neo4Net.CommandLine.Admin.security.SetDefaultAdminCommand;
	using DatabaseManagementSystemSettings = Neo4Net.Dbms.DatabaseManagementSystemSettings;
	using Service = Neo4Net.Helpers.Service;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using SecurityContext = Neo4Net.Kernel.Api.Internal.security.SecurityContext;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using AuthManager = Neo4Net.Kernel.Api.security.AuthManager;
	using SecurityModule = Neo4Net.Kernel.Api.security.SecurityModule;
	using UserManagerSupplier = Neo4Net.Kernel.Api.security.UserManagerSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using EnterpriseAuthManager = Neo4Net.Kernel.enterprise.api.security.EnterpriseAuthManager;
	using EnterpriseSecurityContext = Neo4Net.Kernel.enterprise.api.security.EnterpriseSecurityContext;
	using EnterpriseEditionSettings = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using AccessCapability = Neo4Net.Kernel.impl.factory.AccessCapability;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using AuthenticationStrategy = Neo4Net.Server.Security.Auth.AuthenticationStrategy;
	using BasicPasswordPolicy = Neo4Net.Server.Security.Auth.BasicPasswordPolicy;
	using CommunitySecurityModule = Neo4Net.Server.Security.Auth.CommunitySecurityModule;
	using FileUserRepository = Neo4Net.Server.Security.Auth.FileUserRepository;
	using RateLimitedAuthenticationStrategy = Neo4Net.Server.Security.Auth.RateLimitedAuthenticationStrategy;
	using UserRepository = Neo4Net.Server.Security.Auth.UserRepository;
	using PluginRealm = Neo4Net.Server.security.enterprise.auth.plugin.PluginRealm;
	using AuthPlugin = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthPlugin;
	using AuthenticationPlugin = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthenticationPlugin;
	using AuthorizationPlugin = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthorizationPlugin;
	using SecuritySettings = Neo4Net.Server.security.enterprise.configuration.SecuritySettings;
	using SecurityLog = Neo4Net.Server.security.enterprise.log.SecurityLog;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.proc.Context_Fields.SECURITY_CONTEXT;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(SecurityModule.class) public class EnterpriseSecurityModule extends Neo4Net.kernel.api.security.SecurityModule
	public class EnterpriseSecurityModule : SecurityModule
	{
		 public const string ROLE_STORE_FILENAME = "roles";
		 private const string DEFAULT_ADMIN_STORE_FILENAME = SetDefaultAdminCommand.ADMIN_INI;

		 private EnterpriseAuthAndUserManager _authManager;
		 protected internal SecurityConfig SecurityConfig;

		 public EnterpriseSecurityModule() : base(EnterpriseEditionSettings.ENTERPRISE_SECURITY_MODULE_ID)
		 {
		 }

		 public EnterpriseSecurityModule( string securityModuleId ) : base( securityModuleId )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setup(Dependencies dependencies) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 public override void Setup( Dependencies dependencies )
		 {
			  Config config = dependencies.Config();
			  Procedures procedures = dependencies.Procedures();
			  LogProvider logProvider = dependencies.LogService().UserLogProvider;
			  IJobScheduler jobScheduler = dependencies.Scheduler();
			  FileSystemAbstraction fileSystem = dependencies.FileSystem();
			  AccessCapability accessCapability = dependencies.AccessCapability();

			  SecurityLog securityLog = SecurityLog.create( config, dependencies.LogService().getInternalLog(typeof(GraphDatabaseFacade)), fileSystem, jobScheduler );
			  Life.add( securityLog );

			  _authManager = NewAuthManager( config, logProvider, securityLog, fileSystem, jobScheduler, accessCapability );
			  Life.add( dependencies.DependencySatisfier().satisfyDependency(_authManager) );

			  // Register procedures
			  procedures.RegisterComponent( typeof( SecurityLog ), ctx => securityLog, false );
			  procedures.RegisterComponent( typeof( EnterpriseAuthManager ), ctx => _authManager, false );
			  procedures.RegisterComponent( typeof( EnterpriseSecurityContext ), ctx => AsEnterprise( ctx.get( SECURITY_CONTEXT ) ), true );

			  if ( SecurityConfig.nativeAuthEnabled )
			  {
					procedures.RegisterComponent( typeof( EnterpriseUserManager ), ctx => _authManager.getUserManager( ctx.get( SECURITY_CONTEXT ).subject(), ctx.get(SECURITY_CONTEXT).Admin ), true );
					if ( config.Get( SecuritySettings.auth_providers ).Count > 1 )
					{
						 procedures.RegisterProcedure( typeof( UserManagementProcedures ), true, "%s only applies to native users." );
					}
					else
					{
						 procedures.RegisterProcedure( typeof( UserManagementProcedures ), true );
					}
			  }
			  else
			  {
					procedures.RegisterComponent( typeof( EnterpriseUserManager ), ctx => EnterpriseUserManager.NOOP, true );
			  }

			  procedures.RegisterProcedure( typeof( SecurityProcedures ), true );
		 }

		 public override AuthManager AuthManager()
		 {
			  return _authManager;
		 }

		 public override UserManagerSupplier UserManagerSupplier()
		 {
			  return _authManager;
		 }

		 private EnterpriseSecurityContext AsEnterprise( SecurityContext securityContext )
		 {
			  if ( securityContext is EnterpriseSecurityContext )
			  {
					return ( EnterpriseSecurityContext ) securityContext;
			  }
			  // TODO: better handling of this possible cast failure
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new Exception( "Expected EnterpriseSecurityContext, got " + securityContext.GetType().FullName );
		 }

		 public virtual EnterpriseAuthAndUserManager NewAuthManager( Config config, LogProvider logProvider, SecurityLog securityLog, FileSystemAbstraction fileSystem, IJobScheduler jobScheduler, AccessCapability accessCapability )
		 {
			  SecurityConfig = GetValidatedSecurityConfig( config );

			  IList<Realm> realms = new List<Realm>( SecurityConfig.authProviders.Count + 1 );
			  SecureHasher secureHasher = new SecureHasher();

			  EnterpriseUserManager internalRealm = CreateInternalRealm( config, logProvider, fileSystem, jobScheduler, securityLog, accessCapability );
			  if ( internalRealm != null )
			  {
					realms.Add( ( Realm ) internalRealm );
			  }

			  if ( SecurityConfig.hasLdapProvider )
			  {
					realms.Add( new LdapRealm( config, securityLog, secureHasher ) );
			  }

			  if ( SecurityConfig.pluginAuthProviders.Count > 0 )
			  {
					( ( IList<Realm> )realms ).AddRange( CreatePluginRealms( config, securityLog, secureHasher, SecurityConfig ) );
			  }

			  // Select the active realms in the order they are configured
			  IList<Realm> orderedActiveRealms = SelectOrderedActiveRealms( SecurityConfig.authProviders, realms );

			  if ( orderedActiveRealms.Count == 0 )
			  {
					throw IllegalConfiguration( "No valid auth provider is active." );
			  }

			  return new MultiRealmAuthManager( internalRealm, orderedActiveRealms, CreateCacheManager( config ), securityLog, config.Get( SecuritySettings.security_log_successful_authentication ), SecurityConfig.propertyAuthorization, SecurityConfig.propertyBlacklist );
		 }

		 protected internal virtual SecurityConfig GetValidatedSecurityConfig( Config config )
		 {
			  SecurityConfig securityConfig = new SecurityConfig( config );
			  securityConfig.Validate();
			  return securityConfig;
		 }

		 private static IList<Realm> SelectOrderedActiveRealms( IList<string> configuredRealms, IList<Realm> availableRealms )
		 {
			  IList<Realm> orderedActiveRealms = new List<Realm>( configuredRealms.Count );
			  foreach ( string configuredRealmName in configuredRealms )
			  {
					foreach ( Realm realm in availableRealms )
					{
						 if ( configuredRealmName.Equals( realm.Name ) )
						 {
							  orderedActiveRealms.Add( realm );
							  break;
						 }
					}
			  }
			  return orderedActiveRealms;
		 }

		 protected internal virtual EnterpriseUserManager CreateInternalRealm( Config config, LogProvider logProvider, FileSystemAbstraction fileSystem, IJobScheduler jobScheduler, SecurityLog securityLog, AccessCapability accessCapability )
		 {
			  EnterpriseUserManager internalRealm = null;
			  if ( SecurityConfig.hasNativeProvider )
			  {
					internalRealm = CreateInternalFlatFileRealm( config, logProvider, fileSystem, jobScheduler );
			  }
			  return internalRealm;
		 }

		 protected internal static InternalFlatFileRealm CreateInternalFlatFileRealm( Config config, LogProvider logProvider, FileSystemAbstraction fileSystem, IJobScheduler jobScheduler )
		 {
			  return new InternalFlatFileRealm( CommunitySecurityModule.getUserRepository( config, logProvider, fileSystem ), GetRoleRepository( config, logProvider, fileSystem ), new BasicPasswordPolicy(), CreateAuthenticationStrategy(config), config.Get(SecuritySettings.native_authentication_enabled), config.Get(SecuritySettings.native_authorization_enabled), jobScheduler, CommunitySecurityModule.getInitialUserRepository(config, logProvider, fileSystem), GetDefaultAdminRepository(config, logProvider, fileSystem) );
		 }

		 protected internal static AuthenticationStrategy CreateAuthenticationStrategy( Config config )
		 {
			  return new RateLimitedAuthenticationStrategy( Clocks.systemClock(), config );
		 }

		 private static CacheManager CreateCacheManager( Config config )
		 {
			  long ttl = config.Get( SecuritySettings.auth_cache_ttl ).toMillis();
			  bool useTTL = config.Get( SecuritySettings.auth_cache_use_ttl );
			  int maxCapacity = config.Get( SecuritySettings.auth_cache_max_capacity );
			  return new ShiroCaffeineCache.Manager( Ticker.systemTicker(), ttl, maxCapacity, useTTL );
		 }

		 private static IList<PluginRealm> CreatePluginRealms( Config config, SecurityLog securityLog, SecureHasher secureHasher, SecurityConfig securityConfig )
		 {
			  IList<PluginRealm> availablePluginRealms = new List<PluginRealm>();
			  ISet<Type> excludedClasses = new HashSet<Type>();

			  if ( securityConfig.PluginAuthentication && securityConfig.PluginAuthorization )
			  {
					foreach ( AuthPlugin plugin in Service.load( typeof( AuthPlugin ) ) )
					{
						 PluginRealm pluginRealm = new PluginRealm( plugin, config, securityLog, Clocks.systemClock(), secureHasher );
						 availablePluginRealms.Add( pluginRealm );
					}
			  }

			  if ( securityConfig.PluginAuthentication )
			  {
					foreach ( AuthenticationPlugin plugin in Service.load( typeof( AuthenticationPlugin ) ) )
					{
						 PluginRealm pluginRealm;

						 if ( securityConfig.PluginAuthorization && plugin is AuthorizationPlugin )
						 {
							  // This plugin implements both interfaces, create a combined plugin
							  pluginRealm = new PluginRealm( plugin, ( AuthorizationPlugin ) plugin, config, securityLog, Clocks.systemClock(), secureHasher );

							  // We need to make sure we do not add a duplicate when the AuthorizationPlugin service gets loaded
							  // so we allow only one instance per combined plugin class
							  excludedClasses.Add( plugin.GetType() );
						 }
						 else
						 {
							  pluginRealm = new PluginRealm( plugin, null, config, securityLog, Clocks.systemClock(), secureHasher );
						 }
						 availablePluginRealms.Add( pluginRealm );
					}
			  }

			  if ( securityConfig.PluginAuthorization )
			  {
					foreach ( AuthorizationPlugin plugin in Service.load( typeof( AuthorizationPlugin ) ) )
					{
						 if ( !excludedClasses.Contains( plugin.GetType() ) )
						 {
							  availablePluginRealms.add(new PluginRealm(null, plugin, config, securityLog, Clocks.systemClock(), secureHasher)
								  );
						 }
					}
			  }

			  foreach ( string pluginRealmName in securityConfig.PluginAuthProviders )
			  {
					if ( availablePluginRealms.noneMatch( r => r.Name.Equals( pluginRealmName ) ) )
					{
						 throw IllegalConfiguration( format( "Failed to load auth plugin '%s'.", pluginRealmName ) );
					}
			  }

			  IList<PluginRealm> realms = availablePluginRealms.Where( realm => securityConfig.PluginAuthProviders.Contains( realm.Name ) ).ToList();

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  bool missingAuthenticatingRealm = securityConfig.OnlyPluginAuthentication() && realms.noneMatch(PluginRealm::canAuthenticate);
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  bool missingAuthorizingRealm = securityConfig.OnlyPluginAuthorization() && realms.noneMatch(PluginRealm::canAuthorize);

			  if ( missingAuthenticatingRealm || missingAuthorizingRealm )
			  {
					string missingProvider = ( missingAuthenticatingRealm && missingAuthorizingRealm ) ? "authentication or authorization" : missingAuthenticatingRealm ? "authentication" : "authorization";

					throw IllegalConfiguration( format( "No plugin %s provider loaded even though required by configuration.", missingProvider ) );
			  }

			  return realms;
		 }

		 public static RoleRepository GetRoleRepository( Config config, LogProvider logProvider, FileSystemAbstraction fileSystem )
		 {
			  return new FileRoleRepository( fileSystem, GetRoleRepositoryFile( config ), logProvider );
		 }

		 public static UserRepository GetDefaultAdminRepository( Config config, LogProvider logProvider, FileSystemAbstraction fileSystem )
		 {
			  return new FileUserRepository( fileSystem, GetDefaultAdminRepositoryFile( config ), logProvider );
		 }

		 public static File GetRoleRepositoryFile( Config config )
		 {
			  return new File( config.Get( DatabaseManagementSystemSettings.auth_store_directory ), ROLE_STORE_FILENAME );
		 }

		 private static File GetDefaultAdminRepositoryFile( Config config )
		 {
			  return new File( config.Get( DatabaseManagementSystemSettings.auth_store_directory ), DEFAULT_ADMIN_STORE_FILENAME );
		 }

		 protected internal static System.ArgumentException IllegalConfiguration( string message )
		 {
			  return new System.ArgumentException( "Illegal configuration: " + message );
		 }

		 protected internal class SecurityConfig
		 {
			  protected internal readonly IList<string> AuthProviders;
			  public readonly bool HasNativeProvider;
			  protected internal readonly bool HasLdapProvider;
			  protected internal readonly IList<string> PluginAuthProviders;
			  protected internal readonly bool NativeAuthentication;
			  protected internal readonly bool NativeAuthorization;
			  protected internal readonly bool LdapAuthentication;
			  protected internal readonly bool LdapAuthorization;
			  protected internal readonly bool PluginAuthentication;
			  protected internal readonly bool PluginAuthorization;
			  protected internal readonly bool PropertyAuthorization;
			  internal readonly string PropertyAuthMapping;
			  internal readonly IDictionary<string, IList<string>> PropertyBlacklist = new Dictionary<string, IList<string>>();
			  protected internal bool NativeAuthEnabled;

			  protected internal SecurityConfig( Config config )
			  {
					AuthProviders = config.Get( SecuritySettings.auth_providers );
					HasNativeProvider = AuthProviders.Contains( SecuritySettings.NATIVE_REALM_NAME );
					HasLdapProvider = AuthProviders.Contains( SecuritySettings.LDAP_REALM_NAME );
					PluginAuthProviders = AuthProviders.Where( r => r.StartsWith( SecuritySettings.PLUGIN_REALM_NAME_PREFIX ) ).ToList();

					NativeAuthentication = config.Get( SecuritySettings.native_authentication_enabled );
					NativeAuthorization = config.Get( SecuritySettings.native_authorization_enabled );
					NativeAuthEnabled = NativeAuthentication || NativeAuthorization;
					LdapAuthentication = config.Get( SecuritySettings.ldap_authentication_enabled );
					LdapAuthorization = config.Get( SecuritySettings.ldap_authorization_enabled );
					PluginAuthentication = config.Get( SecuritySettings.plugin_authentication_enabled );
					PluginAuthorization = config.Get( SecuritySettings.plugin_authorization_enabled );
					PropertyAuthorization = config.Get( SecuritySettings.property_level_authorization_enabled );
					PropertyAuthMapping = config.Get( SecuritySettings.property_level_authorization_permissions );
			  }

			  protected internal virtual void Validate()
			  {
					if ( !NativeAuthentication && !LdapAuthentication && !PluginAuthentication )
					{
						 throw IllegalConfiguration( "All authentication providers are disabled." );
					}

					if ( !NativeAuthorization && !LdapAuthorization && !PluginAuthorization )
					{
						 throw IllegalConfiguration( "All authorization providers are disabled." );
					}

					if ( HasNativeProvider && !NativeAuthentication && !NativeAuthorization )
					{
						 throw IllegalConfiguration( "Native auth provider configured, but both authentication and authorization are disabled." );
					}

					if ( HasLdapProvider && !LdapAuthentication && !LdapAuthorization )
					{
						 throw IllegalConfiguration( "LDAP auth provider configured, but both authentication and authorization are disabled." );
					}

					if ( PluginAuthProviders.Count > 0 && !PluginAuthentication && !PluginAuthorization )
					{
						 throw IllegalConfiguration( "Plugin auth provider configured, but both authentication and authorization are disabled." );
					}
					if ( PropertyAuthorization && !ParsePropertyPermissions() )
					{
						 throw IllegalConfiguration( "Property level authorization is enabled but there is a error in the permissions mapping." );
					}
			  }

			  protected internal virtual bool ParsePropertyPermissions()
			  {
					if ( !string.ReferenceEquals( PropertyAuthMapping, null ) && PropertyAuthMapping.Length > 0 )
					{
						 string rolePattern = "\\s*[a-zA-Z0-9_]+\\s*";
						 string propertyPattern = "\\s*[a-zA-Z0-9_]+\\s*";
						 string roleToPerm = rolePattern + "=" + propertyPattern + "(," + propertyPattern + ")*";
						 string multiLine = roleToPerm + "(;" + roleToPerm + ")*";

						 bool valid = PropertyAuthMapping.matches( multiLine );
						 if ( !valid )
						 {
							  return false;
						 }

						 foreach ( string rolesAndPermissions in PropertyAuthMapping.Split( ";", true ) )
						 {
							  if ( rolesAndPermissions.Length > 0 )
							  {
									string[] split = rolesAndPermissions.Split( "=", true );
									string role = split[0].Trim();
									string permissions = split[1];
									IList<string> permissionsList = new List<string>();
									foreach ( string perm in permissions.Split( ",", true ) )
									{
										 if ( perm.Length > 0 )
										 {
											  permissionsList.Add( perm.Trim() );
										 }
									}
									PropertyBlacklist[role] = permissionsList;
							  }
						 }
					}
					return true;
			  }

			  protected internal virtual bool OnlyPluginAuthentication()
			  {
					return !NativeAuthentication && !LdapAuthentication && PluginAuthentication;
			  }

			  protected internal virtual bool OnlyPluginAuthorization()
			  {
					return !NativeAuthorization && !LdapAuthorization && PluginAuthorization;
			  }
		 }
	}

}