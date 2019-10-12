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
namespace Neo4Net.Server.security.enterprise.auth.integration.bolt
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Test = org.junit.Test;


	using AuthToken = Neo4Net.driver.v1.AuthToken;
	using AuthTokens = Neo4Net.driver.v1.AuthTokens;
	using Driver = Neo4Net.driver.v1.Driver;
	using Session = Neo4Net.driver.v1.Session;
	using Transaction = Neo4Net.driver.v1.Transaction;
	using Neo4Net.Graphdb.config;
	using TestCacheableAuthPlugin = Neo4Net.Server.security.enterprise.auth.plugin.TestCacheableAuthPlugin;
	using TestCacheableAuthenticationPlugin = Neo4Net.Server.security.enterprise.auth.plugin.TestCacheableAuthenticationPlugin;
	using TestCustomCacheableAuthenticationPlugin = Neo4Net.Server.security.enterprise.auth.plugin.TestCustomCacheableAuthenticationPlugin;
	using SecuritySettings = Neo4Net.Server.security.enterprise.configuration.SecuritySettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

	public class PluginAuthenticationIT : EnterpriseAuthenticationTestBase
	{
		 private static readonly IList<string> _defaultTestPluginRealmList = Arrays.asList( "TestAuthenticationPlugin", "TestAuthPlugin", "TestCacheableAdminAuthPlugin", "TestCacheableAuthenticationPlugin", "TestCacheableAuthPlugin", "TestCustomCacheableAuthenticationPlugin", "TestCustomParametersAuthenticationPlugin" );

		 private static readonly string _defaultTestPluginRealms = string.join( ", ", _defaultTestPluginRealmList.stream().map(s => StringUtils.prependIfMissing(s, SecuritySettings.PLUGIN_REALM_NAME_PREFIX)).collect(Collectors.toList()) );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected java.util.Map<org.neo4j.graphdb.config.Setting<?>,String> getSettings()
		 protected internal override IDictionary<Setting<object>, string> Settings
		 {
			 get
			 {
				  return Collections.singletonMap( SecuritySettings.auth_providers, _defaultTestPluginRealms );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAuthenticateWithTestAuthenticationPlugin()
		 public virtual void ShouldAuthenticateWithTestAuthenticationPlugin()
		 {
			  AssertAuth( "neo4j", "neo4j", "plugin-TestAuthenticationPlugin" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAuthenticateWithTestCacheableAuthenticationPlugin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAuthenticateWithTestCacheableAuthenticationPlugin()
		 {
			  RestartServerWithOverriddenSettings( SecuritySettings.auth_cache_ttl.name(), "60m" );

			  TestCacheableAuthenticationPlugin.getAuthenticationInfoCallCount.set( 0 );

			  // When we log in the first time our plugin should get a call
			  AssertAuth( "neo4j", "neo4j", "plugin-TestCacheableAuthenticationPlugin" );
			  assertThat( TestCacheableAuthenticationPlugin.getAuthenticationInfoCallCount.get(), equalTo(1) );

			  // When we log in the second time our plugin should _not_ get a call since authentication info should be cached
			  AssertAuth( "neo4j", "neo4j", "plugin-TestCacheableAuthenticationPlugin" );
			  assertThat( TestCacheableAuthenticationPlugin.getAuthenticationInfoCallCount.get(), equalTo(1) );

			  // When we log in the with the wrong credentials it should fail and
			  // our plugin should _not_ get a call since authentication info should be cached
			  AssertAuthFail( "neo4j", "wrong_password", "plugin-TestCacheableAuthenticationPlugin" );
			  assertThat( TestCacheableAuthenticationPlugin.getAuthenticationInfoCallCount.get(), equalTo(1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAuthenticateWithTestCustomCacheableAuthenticationPlugin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAuthenticateWithTestCustomCacheableAuthenticationPlugin()
		 {
			  TestCustomCacheableAuthenticationPlugin.getAuthenticationInfoCallCount.set( 0 );

			  RestartServerWithOverriddenSettings( SecuritySettings.auth_cache_ttl.name(), "60m" );

			  // When we log in the first time our plugin should get a call
			  AssertAuth( "neo4j", "neo4j", "plugin-TestCustomCacheableAuthenticationPlugin" );
			  assertThat( TestCustomCacheableAuthenticationPlugin.getAuthenticationInfoCallCount.get(), equalTo(1) );

			  // When we log in the second time our plugin should _not_ get a call since authentication info should be cached
			  AssertAuth( "neo4j", "neo4j", "plugin-TestCustomCacheableAuthenticationPlugin" );
			  assertThat( TestCustomCacheableAuthenticationPlugin.getAuthenticationInfoCallCount.get(), equalTo(1) );

			  // When we log in the with the wrong credentials it should fail and
			  // our plugin should _not_ get a call since authentication info should be cached
			  AssertAuthFail( "neo4j", "wrong_password", "plugin-TestCustomCacheableAuthenticationPlugin" );
			  assertThat( TestCustomCacheableAuthenticationPlugin.getAuthenticationInfoCallCount.get(), equalTo(1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAuthenticateAndAuthorizeWithTestAuthPlugin()
		 public virtual void ShouldAuthenticateAndAuthorizeWithTestAuthPlugin()
		 {
			  using ( Driver driver = ConnectDriver( "neo4j", "neo4j", "plugin-TestAuthPlugin" ) )
			  {
					AssertReadSucceeds( driver );
					AssertWriteFails( driver );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAuthenticateAndAuthorizeWithCacheableTestAuthPlugin()
		 public virtual void ShouldAuthenticateAndAuthorizeWithCacheableTestAuthPlugin()
		 {
			  using ( Driver driver = ConnectDriver( "neo4j", "neo4j", "plugin-TestCacheableAuthPlugin" ) )
			  {
					AssertReadSucceeds( driver );
					AssertWriteFails( driver );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAuthenticateWithTestCacheableAuthPlugin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAuthenticateWithTestCacheableAuthPlugin()
		 {
			  TestCacheableAuthPlugin.getAuthInfoCallCount.set( 0 );

			  RestartServerWithOverriddenSettings( SecuritySettings.auth_cache_ttl.name(), "60m" );

			  // When we log in the first time our plugin should get a call
			  using ( Driver driver = ConnectDriver( "neo4j", "neo4j", "plugin-TestCacheableAuthPlugin" ) )
			  {
					assertThat( TestCacheableAuthPlugin.getAuthInfoCallCount.get(), equalTo(1) );
					AssertReadSucceeds( driver );
					AssertWriteFails( driver );
			  }

			  // When we log in the second time our plugin should _not_ get a call since auth info should be cached
			  using ( Driver driver = ConnectDriver( "neo4j", "neo4j", "plugin-TestCacheableAuthPlugin" ) )
			  {
					assertThat( TestCacheableAuthPlugin.getAuthInfoCallCount.get(), equalTo(1) );
					AssertReadSucceeds( driver );
					AssertWriteFails( driver );
			  }

			  // When we log in the with the wrong credentials it should fail and
			  // our plugin should _not_ get a call since auth info should be cached
			  AssertAuthFail( "neo4j", "wrong_password", "plugin-TestCacheableAuthPlugin" );
			  assertThat( TestCacheableAuthPlugin.getAuthInfoCallCount.get(), equalTo(1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAuthenticateAndAuthorizeWithTestCombinedAuthPlugin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAuthenticateAndAuthorizeWithTestCombinedAuthPlugin()
		 {
			  RestartServerWithOverriddenSettings( SecuritySettings.auth_providers.name(), "plugin-TestCombinedAuthPlugin" );

			  using ( Driver driver = ConnectDriver( "neo4j", "neo4j", "plugin-TestCombinedAuthPlugin" ) )
			  {
					AssertReadSucceeds( driver );
					AssertWriteFails( driver );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAuthenticateAndAuthorizeWithTwoSeparateTestPlugins() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAuthenticateAndAuthorizeWithTwoSeparateTestPlugins()
		 {
			  RestartServerWithOverriddenSettings( SecuritySettings.auth_providers.name(), "plugin-TestAuthenticationPlugin,plugin-TestAuthorizationPlugin" );

			  using ( Driver driver = ConnectDriver( "neo4j", "neo4j" ) )
			  {
					AssertReadSucceeds( driver );
					AssertWriteFails( driver );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfAuthorizationExpiredWithAuthPlugin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfAuthorizationExpiredWithAuthPlugin()
		 {
			  RestartServerWithOverriddenSettings( SecuritySettings.auth_providers.name(), "plugin-TestCacheableAdminAuthPlugin" );

			  using ( Driver driver = ConnectDriver( "neo4j", "neo4j", "plugin-TestCacheableAdminAuthPlugin" ) )
			  {
					AssertReadSucceeds( driver );

					// When
					ClearAuthCacheFromDifferentConnection( "neo4j", "neo4j", "plugin-TestCacheableAdminAuthPlugin" );

					// Then
					AssertAuthorizationExpired( driver );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedIfAuthorizationExpiredWithinTransactionWithAuthPlugin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSucceedIfAuthorizationExpiredWithinTransactionWithAuthPlugin()
		 {
			  RestartServerWithOverriddenSettings( SecuritySettings.auth_providers.name(), "plugin-TestCacheableAdminAuthPlugin" );

			  // Then
			  using ( Driver driver = ConnectDriver( "neo4j", "neo4j", "plugin-TestCacheableAdminAuthPlugin" ), Session session = driver.session() )
			  {
					using ( Transaction tx = session.beginTransaction() )
					{
						 tx.run( "CALL dbms.security.clearAuthCache()" );
						 assertThat( tx.run( "MATCH (n) RETURN count(n)" ).single().get(0).asInt(), greaterThanOrEqualTo(0) );
						 tx.success();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAuthenticateWithTestCustomParametersAuthenticationPlugin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAuthenticateWithTestCustomParametersAuthenticationPlugin()
		 {
			  AuthToken token = AuthTokens.custom( "neo4j", "", "plugin-TestCustomParametersAuthenticationPlugin", "custom", map( "my_credentials", Arrays.asList( 1L, 2L, 3L, 4L ) ) );
			  AssertAuth( token );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPassOnAuthorizationExpiredException() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPassOnAuthorizationExpiredException()
		 {
			  RestartServerWithOverriddenSettings( SecuritySettings.auth_providers.name(), "plugin-TestCombinedAuthPlugin" );

			  using ( Driver driver = ConnectDriver( "authorization_expired_user", "neo4j" ) )
			  {
					AssertAuthorizationExpired( driver );
			  }
		 }
	}

}