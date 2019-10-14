/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using SecuritySettings = Neo4Net.Server.security.enterprise.configuration.SecuritySettings;
	using SecurityLog = Neo4Net.Server.security.enterprise.log.SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class EnterpriseSecurityModuleTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException thrown = org.junit.rules.ExpectedException.none();
		 public ExpectedException Thrown = ExpectedException.none();
		 private Config _config;
		 private LogProvider _mockLogProvider;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnIllegalRealmNameConfiguration()
		 public virtual void ShouldFailOnIllegalRealmNameConfiguration()
		 {
			  // Given
			  NativeAuth( true, true );
			  LdapAuth( true, true );
			  PluginAuth( false, false );
			  AuthProviders( "this-realm-does-not-exist" );

			  // Then
			  Thrown.expect( typeof( System.ArgumentException ) );
			  Thrown.expectMessage( "Illegal configuration: No valid auth provider is active." );

			  // When
			  ( new EnterpriseSecurityModule() ).NewAuthManager(_config, _mockLogProvider, mock(typeof(SecurityLog)), null, null, null);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnNoAuthenticationMechanism()
		 public virtual void ShouldFailOnNoAuthenticationMechanism()
		 {
			  // Given
			  NativeAuth( false, true );
			  LdapAuth( false, false );
			  PluginAuth( false, false );
			  AuthProviders( SecuritySettings.NATIVE_REALM_NAME );

			  // Then
			  Thrown.expect( typeof( System.ArgumentException ) );
			  Thrown.expectMessage( "Illegal configuration: All authentication providers are disabled." );

			  // When
			  ( new EnterpriseSecurityModule() ).NewAuthManager(_config, _mockLogProvider, mock(typeof(SecurityLog)), null, null, null);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnNoAuthorizationMechanism()
		 public virtual void ShouldFailOnNoAuthorizationMechanism()
		 {
			  // Given
			  NativeAuth( true, false );
			  LdapAuth( false, false );
			  PluginAuth( false, false );
			  AuthProviders( SecuritySettings.NATIVE_REALM_NAME );

			  // Then
			  Thrown.expect( typeof( System.ArgumentException ) );
			  Thrown.expectMessage( "Illegal configuration: All authorization providers are disabled." );

			  // When
			  ( new EnterpriseSecurityModule() ).NewAuthManager(_config, _mockLogProvider, mock(typeof(SecurityLog)), null, null, null);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnIllegalAdvancedRealmConfiguration()
		 public virtual void ShouldFailOnIllegalAdvancedRealmConfiguration()
		 {
			  // Given
			  NativeAuth( false, false );
			  LdapAuth( false, false );
			  PluginAuth( true, true );
			  AuthProviders( SecuritySettings.NATIVE_REALM_NAME, SecuritySettings.LDAP_REALM_NAME );

			  // Then
			  Thrown.expect( typeof( System.ArgumentException ) );
			  Thrown.expectMessage( "Illegal configuration: Native auth provider configured, " + "but both authentication and authorization are disabled." );

			  // When
			  ( new EnterpriseSecurityModule() ).NewAuthManager(_config, _mockLogProvider, mock(typeof(SecurityLog)), null, null, null);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnNotLoadedPluginAuthProvider()
		 public virtual void ShouldFailOnNotLoadedPluginAuthProvider()
		 {
			  // Given
			  NativeAuth( false, false );
			  LdapAuth( false, false );
			  PluginAuth( true, true );
			  AuthProviders( SecuritySettings.PLUGIN_REALM_NAME_PREFIX + "TestAuthenticationPlugin", SecuritySettings.PLUGIN_REALM_NAME_PREFIX + "IllConfiguredAuthorizationPlugin" );

			  // Then
			  Thrown.expect( typeof( System.ArgumentException ) );
			  Thrown.expectMessage( "Illegal configuration: Failed to load auth plugin 'plugin-IllConfiguredAuthorizationPlugin'." );

			  // When
			  ( new EnterpriseSecurityModule() ).NewAuthManager(_config, _mockLogProvider, mock(typeof(SecurityLog)), null, null, null);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFailNativeWithPluginAuthorizationProvider()
		 public virtual void ShouldNotFailNativeWithPluginAuthorizationProvider()
		 {
			  // Given
			  NativeAuth( true, true );
			  LdapAuth( false, false );
			  PluginAuth( true, true );
			  AuthProviders( SecuritySettings.NATIVE_REALM_NAME, SecuritySettings.PLUGIN_REALM_NAME_PREFIX + "TestAuthorizationPlugin" );

			  // When
			  ( new EnterpriseSecurityModule() ).NewAuthManager(_config, _mockLogProvider, mock(typeof(SecurityLog)), null, null, null);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFailWithPropertyLevelPermissions()
		 public virtual void ShouldNotFailWithPropertyLevelPermissions()
		 {
			  NativeAuth( true, true );
			  LdapAuth( false, false );
			  PluginAuth( false, false );
			  AuthProviders( SecuritySettings.NATIVE_REALM_NAME );

			  when( _config.get( SecuritySettings.property_level_authorization_enabled ) ).thenReturn( true );
			  when( _config.get( SecuritySettings.property_level_authorization_permissions ) ).thenReturn( "smith=alias" );

			  ( new EnterpriseSecurityModule() ).NewAuthManager(_config, _mockLogProvider, mock(typeof(SecurityLog)), null, null, null);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnIllegalPropertyLevelPermissions()
		 public virtual void ShouldFailOnIllegalPropertyLevelPermissions()
		 {
			  NativeAuth( true, true );
			  LdapAuth( false, false );
			  PluginAuth( false, false );
			  AuthProviders( SecuritySettings.NATIVE_REALM_NAME );

			  when( _config.get( SecuritySettings.property_level_authorization_enabled ) ).thenReturn( true );
			  when( _config.get( SecuritySettings.property_level_authorization_permissions ) ).thenReturn( "smithmalias" );

			  Thrown.expect( typeof( System.ArgumentException ) );
			  Thrown.expectMessage( "Illegal configuration: Property level authorization is enabled but there is a error in the permissions mapping." );

			  ( new EnterpriseSecurityModule() ).NewAuthManager(_config, _mockLogProvider, mock(typeof(SecurityLog)), null, null, null);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParsePropertyLevelPermissions()
		 public virtual void ShouldParsePropertyLevelPermissions()
		 {
			  NativeAuth( true, true );
			  LdapAuth( false, false );
			  PluginAuth( false, false );
			  AuthProviders( SecuritySettings.NATIVE_REALM_NAME );

			  when( _config.get( SecuritySettings.property_level_authorization_enabled ) ).thenReturn( true );
			  when( _config.get( SecuritySettings.property_level_authorization_permissions ) ).thenReturn( "smith = alias;merovingian=alias ,location;\n abel=alias,\t\thasSilver" );

			  EnterpriseSecurityModule.SecurityConfig securityConfig = new EnterpriseSecurityModule.SecurityConfig( _config );
			  securityConfig.Validate();
			  assertThat( securityConfig.PropertyBlacklist["smith"], equalTo( Collections.singletonList( "alias" ) ) );
			  assertThat( securityConfig.PropertyBlacklist["merovingian"], equalTo( Arrays.asList( "alias", "location" ) ) );
			  assertThat( securityConfig.PropertyBlacklist["abel"], equalTo( Arrays.asList( "alias", "hasSilver" ) ) );
		 }

		 // --------- HELPERS ----------

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _config = mock( typeof( Config ) );
			  _mockLogProvider = mock( typeof( LogProvider ) );
			  Log mockLog = mock( typeof( Log ) );
			  when( _mockLogProvider.getLog( anyString() ) ).thenReturn(mockLog);
			  when( mockLog.DebugEnabled ).thenReturn( true );
			  when( _config.get( SecuritySettings.property_level_authorization_enabled ) ).thenReturn( false );
			  when( _config.get( SecuritySettings.auth_cache_ttl ) ).thenReturn( Duration.ZERO );
			  when( _config.get( SecuritySettings.auth_cache_max_capacity ) ).thenReturn( 10 );
			  when( _config.get( SecuritySettings.auth_cache_use_ttl ) ).thenReturn( true );
			  when( _config.get( SecuritySettings.security_log_successful_authentication ) ).thenReturn( false );
			  when( _config.get( GraphDatabaseSettings.auth_max_failed_attempts ) ).thenReturn( 3 );
			  when( _config.get( GraphDatabaseSettings.auth_lock_time ) ).thenReturn( Duration.ofSeconds( 5 ) );
		 }

		 private void NativeAuth( bool authn, bool authr )
		 {
			  when( _config.get( SecuritySettings.native_authentication_enabled ) ).thenReturn( authn );
			  when( _config.get( SecuritySettings.native_authorization_enabled ) ).thenReturn( authr );
		 }

		 private void LdapAuth( bool authn, bool authr )
		 {
			  when( _config.get( SecuritySettings.ldap_authentication_enabled ) ).thenReturn( authn );
			  when( _config.get( SecuritySettings.ldap_authorization_enabled ) ).thenReturn( authr );
		 }

		 private void PluginAuth( bool authn, bool authr )
		 {
			  when( _config.get( SecuritySettings.plugin_authentication_enabled ) ).thenReturn( authn );
			  when( _config.get( SecuritySettings.plugin_authorization_enabled ) ).thenReturn( authr );
		 }

		 private void AuthProviders( params string[] authProviders )
		 {
			  when( _config.get( SecuritySettings.auth_providers ) ).thenReturn( Arrays.asList( authProviders ) );
		 }
	}

}