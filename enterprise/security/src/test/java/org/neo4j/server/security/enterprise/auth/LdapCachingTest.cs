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
namespace Org.Neo4j.Server.security.enterprise.auth
{
	using FakeTicker = com.google.common.testing.FakeTicker;
	using AuthenticationException = org.apache.shiro.authc.AuthenticationException;
	using AuthenticationInfo = org.apache.shiro.authc.AuthenticationInfo;
	using AuthenticationToken = org.apache.shiro.authc.AuthenticationToken;
	using AuthorizationInfo = org.apache.shiro.authz.AuthorizationInfo;
	using Permission = org.apache.shiro.authz.Permission;
	using Realm = org.apache.shiro.realm.Realm;
	using PrincipalCollection = org.apache.shiro.subject.PrincipalCollection;
	using SimplePrincipalCollection = org.apache.shiro.subject.SimplePrincipalCollection;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using InvalidAuthTokenException = Org.Neo4j.Kernel.api.security.exception.InvalidAuthTokenException;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using EnterpriseLoginContext = Org.Neo4j.Kernel.enterprise.api.security.EnterpriseLoginContext;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using BasicPasswordPolicy = Org.Neo4j.Server.Security.Auth.BasicPasswordPolicy;
	using InMemoryUserRepository = Org.Neo4j.Server.Security.Auth.InMemoryUserRepository;
	using RateLimitedAuthenticationStrategy = Org.Neo4j.Server.Security.Auth.RateLimitedAuthenticationStrategy;
	using SecuritySettings = Org.Neo4j.Server.security.enterprise.configuration.SecuritySettings;
	using SecurityLog = Org.Neo4j.Server.security.enterprise.log.SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.BasicAuthManagerTest.password;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.SecurityTestUtils.authToken;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.AuthTestUtil.listOf;

	public class LdapCachingTest
	{
		 private MultiRealmAuthManager _authManager;
		 private TestRealm _testRealm;
		 private FakeTicker _fakeTicker;

		 private readonly System.Func<string, int> _token = s => -1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  SecurityLog securityLog = mock( typeof( SecurityLog ) );
			  InternalFlatFileRealm internalFlatFileRealm = new InternalFlatFileRealm(new InMemoryUserRepository(), new InMemoryRoleRepository(), new BasicPasswordPolicy(), new RateLimitedAuthenticationStrategy(Clock.systemUTC(), Config.defaults()), mock(typeof(JobScheduler)), new InMemoryUserRepository(), new InMemoryUserRepository()
				  );

			  _testRealm = new TestRealm( this, LdapConfig, securityLog, new SecureHasher() );

			  IList<Realm> realms = listOf( internalFlatFileRealm, _testRealm );

			  _fakeTicker = new FakeTicker();
			  _authManager = new MultiRealmAuthManager( internalFlatFileRealm, realms, new ShiroCaffeineCache.Manager( _fakeTicker.read, 100, 10, true ), securityLog, false, false, Collections.emptyMap() );
			  _authManager.init();
			  _authManager.start();

			  _authManager.UserManager.newUser( "mike", password( "123" ), false );
			  _authManager.UserManager.newUser( "mats", password( "456" ), false );
		 }

		 private static Config LdapConfig
		 {
			 get
			 {
				  return Config.defaults( stringMap( SecuritySettings.native_authentication_enabled.name(), "false", SecuritySettings.native_authorization_enabled.name(), "false", SecuritySettings.ldap_authentication_enabled.name(), "true", SecuritySettings.ldap_authorization_enabled.name(), "true", SecuritySettings.ldap_authorization_user_search_base.name(), "dc=example,dc=com", SecuritySettings.ldap_authorization_group_membership_attribute_names.name(), "gidnumber" ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCacheAuthenticationInfo() throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCacheAuthenticationInfo()
		 {
			  // Given
			  _authManager.login( authToken( "mike", "123" ) );
			  assertThat( "Test realm did not receive a call", _testRealm.takeAuthenticationFlag(), @is(true) );

			  // When
			  _authManager.login( authToken( "mike", "123" ) );

			  // Then
			  assertThat( "Test realm received a call", _testRealm.takeAuthenticationFlag(), @is(false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCacheAuthorizationInfo() throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCacheAuthorizationInfo()
		 {
			  // Given
			  EnterpriseLoginContext mike = _authManager.login( authToken( "mike", "123" ) );
			  mike.Authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME ).mode().allowsReads();
			  assertThat( "Test realm did not receive a call", _testRealm.takeAuthorizationFlag(), @is(true) );

			  // When
			  mike.Authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME ).mode().allowsWrites();

			  // Then
			  assertThat( "Test realm received a call", _testRealm.takeAuthorizationFlag(), @is(false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvalidateAuthorizationCacheAfterTTL() throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvalidateAuthorizationCacheAfterTTL()
		 {
			  // Given
			  EnterpriseLoginContext mike = _authManager.login( authToken( "mike", "123" ) );
			  mike.Authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME ).mode().allowsReads();
			  assertThat( "Test realm did not receive a call", _testRealm.takeAuthorizationFlag(), @is(true) );

			  // When
			  _fakeTicker.advance( 99, TimeUnit.MILLISECONDS );
			  mike.Authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME ).mode().allowsWrites();

			  // Then
			  assertThat( "Test realm received a call", _testRealm.takeAuthorizationFlag(), @is(false) );

			  // When
			  _fakeTicker.advance( 2, TimeUnit.MILLISECONDS );
			  mike.Authorize( _token, GraphDatabaseSettings.DEFAULT_DATABASE_NAME ).mode().allowsWrites();

			  // Then
			  assertThat( "Test realm did not received a call", _testRealm.takeAuthorizationFlag(), @is(true) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvalidateAuthenticationCacheAfterTTL() throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvalidateAuthenticationCacheAfterTTL()
		 {
			  // Given
			  IDictionary<string, object> mike = authToken( "mike", "123" );
			  _authManager.login( mike );
			  assertThat( "Test realm did not receive a call", _testRealm.takeAuthenticationFlag(), @is(true) );

			  // When
			  _fakeTicker.advance( 99, TimeUnit.MILLISECONDS );
			  _authManager.login( mike );

			  // Then
			  assertThat( "Test realm received a call", _testRealm.takeAuthenticationFlag(), @is(false) );

			  // When
			  _fakeTicker.advance( 2, TimeUnit.MILLISECONDS );
			  _authManager.login( mike );

			  // Then
			  assertThat( "Test realm did not received a call", _testRealm.takeAuthenticationFlag(), @is(true) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvalidateAuthenticationCacheOnDemand() throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvalidateAuthenticationCacheOnDemand()
		 {
			  // Given
			  IDictionary<string, object> mike = authToken( "mike", "123" );
			  _authManager.login( mike );
			  assertThat( "Test realm did not receive a call", _testRealm.takeAuthenticationFlag(), @is(true) );

			  // When
			  _fakeTicker.advance( 2, TimeUnit.MILLISECONDS );
			  _authManager.login( mike );

			  // Then
			  assertThat( "Test realm received a call", _testRealm.takeAuthenticationFlag(), @is(false) );

			  // When
			  _authManager.clearAuthCache();
			  _authManager.login( mike );

			  // Then
			  assertThat( "Test realm did not receive a call", _testRealm.takeAuthenticationFlag(), @is(true) );
		 }

		 private class TestRealm : LdapRealm
		 {
			 private readonly LdapCachingTest _outerInstance;

			  internal bool AuthenticationFlag;
			  internal bool AuthorizationFlag;

			  internal virtual bool TakeAuthenticationFlag()
			  {
					bool t = AuthenticationFlag;
					AuthenticationFlag = false;
					return t;
			  }

			  internal virtual bool TakeAuthorizationFlag()
			  {
					bool t = AuthorizationFlag;
					AuthorizationFlag = false;
					return t;
			  }

			  internal TestRealm( LdapCachingTest outerInstance, Config config, SecurityLog securityLog, SecureHasher secureHasher ) : base( config, securityLog, secureHasher )
			  {
				  this._outerInstance = outerInstance;
					AuthenticationCachingEnabled = true;
					AuthorizationCachingEnabled = true;
			  }

			  public override string Name
			  {
				  get
				  {
						return "TestRealm wrapping " + base.Name;
				  }
			  }

			  public override bool Supports( AuthenticationToken token )
			  {
					return base.Supports( token );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.apache.shiro.authc.AuthenticationInfo doGetAuthenticationInfo(org.apache.shiro.authc.AuthenticationToken token) throws org.apache.shiro.authc.AuthenticationException
			  protected internal override AuthenticationInfo DoGetAuthenticationInfo( AuthenticationToken token )
			  {
					AuthenticationFlag = true;
					return new AuthenticationInfoAnonymousInnerClass( this );
			  }

			  private class AuthenticationInfoAnonymousInnerClass : AuthenticationInfo
			  {
				  private readonly TestRealm _outerInstance;

				  public AuthenticationInfoAnonymousInnerClass( TestRealm outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public override PrincipalCollection Principals
				  {
					  get
					  {
							return new SimplePrincipalCollection();
					  }
				  }

				  public override object Credentials
				  {
					  get
					  {
							return "123";
					  }
				  }
			  }

			  protected internal override AuthorizationInfo DoGetAuthorizationInfo( PrincipalCollection principals )
			  {
					AuthorizationFlag = true;
					return new AuthorizationInfoAnonymousInnerClass( this );
			  }

			  private class AuthorizationInfoAnonymousInnerClass : AuthorizationInfo
			  {
				  private readonly TestRealm _outerInstance;

				  public AuthorizationInfoAnonymousInnerClass( TestRealm outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public override ICollection<string> Roles
				  {
					  get
					  {
							return Collections.emptyList();
					  }
				  }

				  public override ICollection<string> StringPermissions
				  {
					  get
					  {
							return Collections.emptyList();
					  }
				  }

				  public override ICollection<Permission> ObjectPermissions
				  {
					  get
					  {
							return Collections.emptyList();
					  }
				  }
			  }
		 }

	}

}