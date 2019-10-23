using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Server.security.enterprise.auth.integration.bolt
{
	using LdapException = org.apache.directory.api.ldap.model.exception.LdapException;
	using LdapOperationErrorException = org.apache.directory.api.ldap.model.exception.LdapOperationErrorException;
	using CreateLdapServer = org.apache.directory.server.annotations.CreateLdapServer;
	using CreateTransport = org.apache.directory.server.annotations.CreateTransport;
	using SaslMechanism = org.apache.directory.server.annotations.SaslMechanism;
	using ApplyLdifFiles = org.apache.directory.server.core.annotations.ApplyLdifFiles;
	using ContextEntry = org.apache.directory.server.core.annotations.ContextEntry;
	using CreateDS = org.apache.directory.server.core.annotations.CreateDS;
	using CreatePartition = org.apache.directory.server.core.annotations.CreatePartition;
	using LoadSchema = org.apache.directory.server.core.annotations.LoadSchema;
	using EntryFilteringCursor = org.apache.directory.server.core.api.filtering.EntryFilteringCursor;
	using BaseInterceptor = org.apache.directory.server.core.api.interceptor.BaseInterceptor;
	using Interceptor = org.apache.directory.server.core.api.interceptor.Interceptor;
	using SearchOperationContext = org.apache.directory.server.core.api.interceptor.context.SearchOperationContext;
	using FrameworkRunner = org.apache.directory.server.core.integ.FrameworkRunner;
	using StartTlsHandler = org.apache.directory.server.ldap.handlers.extended.StartTlsHandler;
	using JndiLdapContextFactory = org.apache.shiro.realm.ldap.JndiLdapContextFactory;
	using Matcher = org.hamcrest.Matcher;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using Category = org.junit.experimental.categories.Category;
	using RunWith = org.junit.runner.RunWith;


	using Driver = Neo4Net.driver.v1.Driver;
	using Record = Neo4Net.driver.v1.Record;
	using Session = Neo4Net.driver.v1.Session;
	using Transaction = Neo4Net.driver.v1.Transaction;
	using ServiceUnavailableException = Neo4Net.driver.v1.exceptions.ServiceUnavailableException;
	using TransientException = Neo4Net.driver.v1.exceptions.TransientException;
	using Neo4Net.GraphDb.config;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using Neo4Net.Server.security.enterprise.auth;
	using PredefinedRoles = Neo4Net.Server.security.enterprise.auth.plugin.api.PredefinedRoles;
	using SecuritySettings = Neo4Net.Server.security.enterprise.configuration.SecuritySettings;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	internal interface TimeoutTests
	{ // Category marker
	}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") @RunWith(FrameworkRunner.class) @CreateDS(name = "Test", partitions = {@CreatePartition(name = "example", suffix = "dc=example,dc=com", contextEntry = @ContextEntry(entryLdif = "dn: dc=example,dc=com\n" + "dc: example\n" + "o: example\n" + "objectClass: top\n" + "objectClass: dcObject\n" + "objectClass: organization\n\n"))}, loadedSchemas = { @LoadSchema(name = "nis")}) @CreateLdapServer(transports = {@CreateTransport(protocol = "LDAP", port = 10389, address = "0.0.0.0"), @CreateTransport(protocol = "LDAPS", port = 10636, address = "0.0.0.0", ssl = true) }, saslMechanisms = { @SaslMechanism(name = "DIGEST-MD5", implClass = org.apache.directory.server.ldap.handlers.sasl.digestMD5.DigestMd5MechanismHandler.class), @SaslMechanism(name = "CRAM-MD5", implClass = org.apache.directory.server.ldap.handlers.sasl.cramMD5.CramMd5MechanismHandler.class) }, saslHost = "0.0.0.0", extendedOpHandlers = {StartTlsHandler.class}, keyStore = "target/test-classes/Neo4Net_ldap_test_keystore.jks", certificatePassword = "secret") @ApplyLdifFiles("ldap_test_data.ldif") public class LdapAuthIT extends EnterpriseAuthenticationTestBase
	public class LdapAuthIT : EnterpriseAuthenticationTestBase
	{
		 private const string LDAP_ERROR_MESSAGE_INVALID_CREDENTIALS = "LDAP: error code 49 - INVALID_CREDENTIALS";
		 private const string REFUSED_IP = "127.0.0.1"; // "0.6.6.6";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before @Override public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public override void Setup()
		 {
			  base.Setup();
			  LdapServer.ConfidentialityRequired = false;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected java.util.Map<org.Neo4Net.graphdb.config.Setting<?>,String> getSettings()
		 protected internal override IDictionary<Setting<object>, string> Settings
		 {
			 get
			 {
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: java.util.Map<org.Neo4Net.graphdb.config.Setting<?>,String> settings = new java.util.HashMap<>();
				  IDictionary<Setting<object>, string> settings = new Dictionary<Setting<object>, string>();
				  settings[SecuritySettings.auth_provider] = SecuritySettings.LDAP_REALM_NAME;
				  settings[SecuritySettings.ldap_server] = "0.0.0.0:10389";
				  settings[SecuritySettings.ldap_authentication_user_dn_template] = "cn={0},ou=users,dc=example,dc=com";
				  settings[SecuritySettings.ldap_authentication_cache_enabled] = "true";
				  settings[SecuritySettings.ldap_authorization_system_username] = "uid=admin,ou=system";
				  settings[SecuritySettings.ldap_authorization_system_password] = "secret";
				  settings[SecuritySettings.ldap_authorization_user_search_base] = "dc=example,dc=com";
				  settings[SecuritySettings.ldap_authorization_user_search_filter] = "(&(objectClass=*)(uid={0}))";
				  settings[SecuritySettings.ldap_authorization_group_membership_attribute_names] = "gidnumber";
				  settings[SecuritySettings.ldap_authorization_group_to_role_mapping] = "500=reader;501=publisher;502=architect;503=admin";
				  settings[SecuritySettings.procedure_roles] = "test.staticReadProcedure:role1";
				  settings[SecuritySettings.ldap_read_timeout] = "1s";
				  settings[SecuritySettings.ldap_authorization_use_system_account] = "false";
				  return settings;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowCurrentUser()
		 public virtual void ShouldShowCurrentUser()
		 {
			  using ( Driver driver = ConnectDriver( "smith", "abc123" ), Session session = driver.session() )
			  {
					// when
					Record record = session.run( "CALL dbms.showCurrentUser()" ).single();

					// then
					// Assuming showCurrentUser has fields username, roles, flags
					assertThat( record.get( 0 ).asString(), equalTo("smith") );
					assertThat( record.get( 1 ).asList(), equalTo(Collections.emptyList()) );
					assertThat( record.get( 2 ).asList(), equalTo(Collections.emptyList()) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLoginAndAuthorizeNoPermissionUserWithLdapOnlyAndNoGroupToRoleMapping() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToLoginAndAuthorizeNoPermissionUserWithLdapOnlyAndNoGroupToRoleMapping()
		 {
			  RestartServerWithOverriddenSettings( SecuritySettings.ldap_authorization_group_to_role_mapping.name(), null );
			  // Then
			  // User 'neo' has reader role by default, but since we are not passing a group-to-role mapping
			  // he should get no permissions
			  AssertReadFails( "neo", "abc123" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfAuthorizationExpiredWithserLdapContext()
		 public virtual void ShouldFailIfAuthorizationExpiredWithserLdapContext()
		 {
			  // Given
			  using ( Driver driver = ConnectDriver( "Neo4Net", "abc123" ) )
			  {
					AssertReadSucceeds( driver );

					using ( Session session = driver.session() )
					{
						 session.run( "CALL dbms.security.clearAuthCache()" );
					}

					try
					{
						 AssertReadFails( driver );
						 fail( "should have failed due to authorization expired" );
					}
					catch ( ServiceUnavailableException )
					{
						 // TODO Bolt should handle the AuthorizationExpiredException better
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedIfAuthorizationExpiredWithinTransactionWithUserLdapContext()
		 public virtual void ShouldSucceedIfAuthorizationExpiredWithinTransactionWithUserLdapContext()
		 {
			  // Given
			  using ( Driver driver = ConnectDriver( "Neo4Net", "abc123" ) )
			  {
					AssertReadSucceeds( driver );

					using ( Session session = driver.session() )
					{
						 using ( Transaction tx = session.BeginTransaction() )
						 {
							  tx.run( "CALL dbms.security.clearAuthCache()" );
							  assertThat( tx.run( "MATCH (n) RETURN count(n)" ).single().get(0).asInt(), greaterThanOrEqualTo(0) );
							  tx.success();
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepAuthorizationForLifetimeOfTransaction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepAuthorizationForLifetimeOfTransaction()
		 {
			  AssertKeepAuthorizationForLifetimeOfTransaction( "neo", tx => assertThat( tx.run( "MATCH (n) RETURN count(n)" ).single().get(0).asInt(), greaterThanOrEqualTo(0) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepAuthorizationForLifetimeOfTransactionWithProcedureAllowed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepAuthorizationForLifetimeOfTransactionWithProcedureAllowed()
		 {
			  RestartServerWithOverriddenSettings( SecuritySettings.ldap_authorization_group_to_role_mapping.name(), "503=admin;504=role1" );
			  DbRule.resolveDependency( typeof( Procedures ) ).registerProcedure( typeof( ProcedureInteractionTestBase.ClassWithProcedures ) );
			  AssertKeepAuthorizationForLifetimeOfTransaction( "smith", tx => assertThat( tx.run( "CALL test.staticReadProcedure()" ).single().get(0).asString(), equalTo("static") ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertKeepAuthorizationForLifetimeOfTransaction(String username, System.Action<org.Neo4Net.driver.v1.Transaction> assertion) throws Throwable
		 private void AssertKeepAuthorizationForLifetimeOfTransaction( string username, System.Action<Transaction> assertion )
		 {
			  DoubleLatch latch = new DoubleLatch( 2 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Throwable[] threadFail = {null};
			  Exception[] threadFail = new Exception[] { null };

			  Thread readerThread = new Thread(() =>
			  {
				try
				{
					 using ( Driver driver = ConnectDriver( username, "abc123" ), Session session = driver.session(), Transaction tx = session.BeginTransaction() )
					 {
						  assertion( tx );
						  latch.StartAndWaitForAllToStart();
						  latch.FinishAndWaitForAllToFinish();
						  assertion( tx );
						  tx.success();
					 }
				}
				catch ( Exception t )
				{
					 threadFail[0] = t;
					 // Always release the latch so we get the failure in the main thread
					 latch.Start();
					 latch.Finish();
				}
			  });

			  readerThread.Start();
			  latch.StartAndWaitForAllToStart();

			  ClearAuthCacheFromDifferentConnection();

			  latch.FinishAndWaitForAllToFinish();

			  readerThread.Join();
			  if ( threadFail[0] != null )
			  {
					throw threadFail[0];
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfInvalidLdapServer() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfInvalidLdapServer()
		 {
			  // When
			  RestartServerWithOverriddenSettings( SecuritySettings.ldap_server.name(), "ldap://127.0.0.1" );
			  try
			  {
					ConnectDriver( "neo", "abc123" );
					fail( "should have refused connection" );
			  }
			  catch ( TransientException e )
			  {
					assertThat( e.Message, equalTo( LdapRealm.LDAP_CONNECTION_REFUSED_CLIENT_MESSAGE ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Category(TimeoutTests.class) public void shouldTimeoutIfLdapServerDoesNotRespond() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTimeoutIfLdapServerDoesNotRespond()
		 {
			  using ( DirectoryServiceWaitOnSearch ignore = new DirectoryServiceWaitOnSearch( this, 5000 ) )
			  {
					RestartServerWithOverriddenSettings( SecuritySettings.ldap_read_timeout.name(), "1s", SecuritySettings.ldap_authorization_connection_pooling.name(), "true", SecuritySettings.ldap_authorization_use_system_account.name(), "true" );

					AssertReadFails( "neo", "abc123" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Category(TimeoutTests.class) public void shouldTimeoutIfLdapServerDoesNotRespondWithoutConnectionPooling() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTimeoutIfLdapServerDoesNotRespondWithoutConnectionPooling()
		 {
			  using ( DirectoryServiceWaitOnSearch ignore = new DirectoryServiceWaitOnSearch( this, 5000 ) )
			  {
					RestartServerWithOverriddenSettings( SecuritySettings.ldap_read_timeout.name(), "1s", SecuritySettings.ldap_authorization_connection_pooling.name(), "false", SecuritySettings.ldap_authorization_use_system_account.name(), "true" );

					AssertReadFails( "neo", "abc123" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Category(TimeoutTests.class) public void shouldFailIfLdapSearchFails() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfLdapSearchFails()
		 {
			  using ( DirectoryServiceFailOnSearch ignore = new DirectoryServiceFailOnSearch( this ) )
			  {
					RestartServerWithOverriddenSettings( SecuritySettings.ldap_read_timeout.name(), "1s", SecuritySettings.ldap_authorization_use_system_account.name(), "true" );

					AssertReadFails( "neo", "abc123" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Category(TimeoutTests.class) public void shouldTimeoutIfLdapServerDoesNotRespondWithLdapUserContext() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTimeoutIfLdapServerDoesNotRespondWithLdapUserContext()
		 {
			  using ( DirectoryServiceWaitOnSearch ignore = new DirectoryServiceWaitOnSearch( this, 5000 ) )
			  {
					// When
					RestartServerWithOverriddenSettings( SecuritySettings.ldap_read_timeout.name(), "1s" );

					try
					{
						 ConnectDriver( "neo", "abc123" );
						 fail( "should have timed out" );
					}
					catch ( TransientException e )
					{
						 assertThat( e.Message, equalTo( LdapRealm.LDAP_READ_TIMEOUT_CLIENT_MESSAGE ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetCombinedAuthorization() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetCombinedAuthorization()
		 {
			  RestartServerWithOverriddenSettings( SecuritySettings.auth_providers.name(), SecuritySettings.NATIVE_REALM_NAME + "," + SecuritySettings.LDAP_REALM_NAME, SecuritySettings.native_authentication_enabled.name(), "true", SecuritySettings.native_authorization_enabled.name(), "true", SecuritySettings.ldap_authentication_enabled.name(), "true", SecuritySettings.ldap_authorization_enabled.name(), "true", SecuritySettings.ldap_authorization_use_system_account.name(), "true" );

			  // Given
			  // we have a native 'tank' that is read only, and ldap 'tank' that is publisher
			  CreateNativeUser( "tank", "localpassword", PredefinedRoles.READER );

			  // Then
			  // the created "tank" can log in and gets roles from both providers
			  // because the system account is used to authorize over the ldap provider
			  using ( Driver driver = ConnectDriver( "tank", "localpassword", "native" ) )
			  {
					AssertRoles( driver, PredefinedRoles.READER, PredefinedRoles.PUBLISHER );
			  }

			  // the ldap "tank" can also log in and gets roles from both providers
			  using ( Driver driver = ConnectDriver( "tank", "abc123", "ldap" ) )
			  {
					AssertRoles( driver, PredefinedRoles.READER, PredefinedRoles.PUBLISHER );
			  }
		 }

		 // ===== Logging tests =====

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogErrorsFromLdapRealmWhenLoginSuccessfulInNativeRealmNativeFirst() throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLogErrorsFromLdapRealmWhenLoginSuccessfulInNativeRealmNativeFirst()
		 {
			  RestartServerWithOverriddenSettings( SecuritySettings.auth_providers.name(), SecuritySettings.NATIVE_REALM_NAME + "," + SecuritySettings.LDAP_REALM_NAME, SecuritySettings.native_authentication_enabled.name(), "true", SecuritySettings.native_authorization_enabled.name(), "true", SecuritySettings.ldap_authentication_enabled.name(), "true", SecuritySettings.ldap_authorization_enabled.name(), "true", SecuritySettings.ldap_authorization_use_system_account.name(), "true" );

			  // Given
			  // we have a native 'foo' that does not exist in ldap
			  CreateNativeUser( "foo", "bar" );

			  // Then
			  // the created "foo" can log in
			  AssertAuth( "foo", "bar" );

			  // We should not get errors spammed in the security log
			  AssertSecurityLogDoesNotContain( "ERROR" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogErrorsFromLdapRealmWhenLoginSuccessfulInNativeRealmLdapFirst() throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLogErrorsFromLdapRealmWhenLoginSuccessfulInNativeRealmLdapFirst()
		 {
			  RestartServerWithOverriddenSettings( SecuritySettings.auth_providers.name(), SecuritySettings.LDAP_REALM_NAME + "," + SecuritySettings.NATIVE_REALM_NAME, SecuritySettings.native_authentication_enabled.name(), "true", SecuritySettings.native_authorization_enabled.name(), "true", SecuritySettings.ldap_authentication_enabled.name(), "true", SecuritySettings.ldap_authorization_enabled.name(), "true", SecuritySettings.ldap_authorization_use_system_account.name(), "true" );

			  // Given
			  // we have a native 'foo' that does not exist in ldap
			  CreateNativeUser( "foo", "bar" );

			  // Then
			  // the created "foo" can log in
			  AssertAuth( "foo", "bar" );

			  // We should not get errors spammed in the security log
			  AssertSecurityLogDoesNotContain( "ERROR" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogInvalidCredentialErrorFromLdapRealm() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogInvalidCredentialErrorFromLdapRealm()
		 {
			  // When
			  AssertAuthFail( "neo", "wrong-password" );

			  // Then
			  AssertSecurityLogContains( LDAP_ERROR_MESSAGE_INVALID_CREDENTIALS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogInvalidCredentialErrorFromLdapRealmWhenAllProvidersFail() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogInvalidCredentialErrorFromLdapRealmWhenAllProvidersFail()
		 {
			  RestartServerWithOverriddenSettings( SecuritySettings.auth_providers.name(), SecuritySettings.NATIVE_REALM_NAME + ", " + SecuritySettings.LDAP_REALM_NAME, SecuritySettings.native_authentication_enabled.name(), "true", SecuritySettings.native_authorization_enabled.name(), "true", SecuritySettings.ldap_authentication_enabled.name(), "true", SecuritySettings.ldap_authorization_enabled.name(), "true", SecuritySettings.ldap_authorization_use_system_account.name(), "true" );

			  // Given
			  // we have a native 'foo' that does not exist in ldap
			  CreateNativeUser( "foo", "bar" );

			  // When
			  AssertAuthFail( "foo", "wrong-password" );

			  // Then
			  AssertSecurityLogContains( LDAP_ERROR_MESSAGE_INVALID_CREDENTIALS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogConnectionRefusedFromLdapRealm() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogConnectionRefusedFromLdapRealm()
		 {
			  // When
			  RestartServerWithOverriddenSettings( SecuritySettings.ldap_server.name(), "ldap://" + REFUSED_IP );

			  try
			  {
					ConnectDriver( "neo", "abc123" );
					fail( "Expected connection refused" );
			  }
			  catch ( TransientException e )
			  {
					assertThat( e.Message, equalTo( LdapRealm.LDAP_CONNECTION_REFUSED_CLIENT_MESSAGE ) );
			  }

			  AssertSecurityLogContains( "ERROR" );
			  AssertSecurityLogContains( "auth server connection refused" );
			  AssertSecurityLogContains( REFUSED_IP );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogConnectionRefusedFromLdapRealmWithMultipleRealms() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogConnectionRefusedFromLdapRealmWithMultipleRealms()
		 {
			  RestartServerWithOverriddenSettings( SecuritySettings.auth_providers.name(), SecuritySettings.NATIVE_REALM_NAME + ", " + SecuritySettings.LDAP_REALM_NAME, SecuritySettings.native_authentication_enabled.name(), "true", SecuritySettings.native_authorization_enabled.name(), "true", SecuritySettings.ldap_authentication_enabled.name(), "true", SecuritySettings.ldap_authorization_enabled.name(), "true", SecuritySettings.ldap_authorization_use_system_account.name(), "true", SecuritySettings.ldap_server.name(), "ldap://" + REFUSED_IP );

			  AssertAuthFail( "neo", "abc123" );

			  AssertSecurityLogContains( "ERROR" );
			  AssertSecurityLogContains( "LDAP connection refused" );
			  AssertSecurityLogContains( REFUSED_IP );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearAuthenticationCache() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearAuthenticationCache()
		 {
			  LdapServer.ConfidentialityRequired = true;

			  using ( EmbeddedTestCertificates ignore = new EmbeddedTestCertificates() )
			  {
					// When
					RestartServerWithOverriddenSettings( SecuritySettings.ldap_server.name(), "ldaps://localhost:10636" );

					// Then
					AssertAuth( "tank", "abc123" );
					ChangeLDAPPassword( "tank", "abc123", "123abc" );

					// When logging in without clearing cache

					// Then
					AssertAuthFail( "tank", "123abc" );
					AssertAuth( "tank", "abc123" );

					// When clearing cache and logging in
					ClearAuthCacheFromDifferentConnection();

					// Then
					AssertAuthFail( "tank", "abc123" );
					AssertAuth( "tank", "123abc" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearAuthorizationCache() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearAuthorizationCache()
		 {
			  LdapServer.ConfidentialityRequired = true;

			  using ( EmbeddedTestCertificates ignore = new EmbeddedTestCertificates() )
			  {
					// When
					RestartServerWithOverriddenSettings( SecuritySettings.ldap_server.name(), "ldaps://localhost:10636" );

					// Then
					using ( Driver driver = ConnectDriver( "tank", "abc123" ) )
					{
						 AssertReadSucceeds( driver );
						 AssertWriteSucceeds( driver );
					}

					ChangeLDAPGroup( "tank", "abc123", "reader" );

					// When logging in without clearing cache
					using ( Driver driver = ConnectDriver( "tank", "abc123" ) )
					{
						 // Then
						 AssertReadSucceeds( driver );
						 AssertWriteSucceeds( driver );
					}

					// When clearing cache and logging in
					ClearAuthCacheFromDifferentConnection();

					// Then
					using ( Driver driver = ConnectDriver( "tank", "abc123" ) )
					{
						 AssertReadSucceeds( driver );
						 AssertWriteFails( driver );
					}
			  }
		 }

		 // ===== Helpers =====

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void modifyLDAPAttribute(String username, Object credentials, String attribute, Object value) throws Throwable
		 private void ModifyLDAPAttribute( string username, object credentials, string attribute, object value )
		 {
			  string principal = string.Format( "cn={0},ou=users,dc=example,dc=com", username );
			  string principal1 = string.Format( "cn={0},ou=users,dc=example,dc=com", username );
			  JndiLdapContextFactory contextFactory = new JndiLdapContextFactory();
			  contextFactory.Url = "ldaps://localhost:10636";
			  LdapContext ctx = contextFactory.getLdapContext( principal1, credentials );

			  ModificationItem[] mods = new ModificationItem[1];
			  mods[0] = new ModificationItem( DirContext.REPLACE_ATTRIBUTE, new BasicAttribute( attribute, value ) );

			  // Perform the update
			  ctx.modifyAttributes( principal, mods );
			  ctx.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("SameParameterValue") private void changeLDAPPassword(String username, Object credentials, Object newCredentials) throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private void ChangeLDAPPassword( string username, object credentials, object newCredentials )
		 {
			  ModifyLDAPAttribute( username, credentials, "userpassword", newCredentials );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("SameParameterValue") private void changeLDAPGroup(String username, Object credentials, String group) throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private void ChangeLDAPGroup( string username, object credentials, string group )
		 {
			  string gid;
			  switch ( group )
			  {
			  case "reader":
					gid = "500";
					break;
			  case "publisher":
					gid = "501";
					break;
			  case "architect":
					gid = "502";
					break;
			  case "admin":
					gid = "503";
					break;
			  case "none":
					gid = "504";
					break;
			  default:
					throw new System.ArgumentException( "Invalid group name '" + group + "', expected one of none, reader, publisher, architect, or admin" );
			  }
			  ModifyLDAPAttribute( username, credentials, "gidnumber", gid );
		 }

		 private class DirectoryServiceWaitOnSearch : IDisposable
		 {
			 private readonly LdapAuthIT _outerInstance;

			  internal readonly Interceptor WaitOnSearchInterceptor;

			  internal DirectoryServiceWaitOnSearch( LdapAuthIT outerInstance, long waitingTimeMillis )
			  {
				  this._outerInstance = outerInstance;
					WaitOnSearchInterceptor = new BaseInterceptorAnonymousInnerClass( this, waitingTimeMillis );

					try
					{
						 Service.addFirst( WaitOnSearchInterceptor );
					}
					catch ( LdapException e )
					{
						 throw new Exception( e );
					}
			  }

			  private class BaseInterceptorAnonymousInnerClass : BaseInterceptor
			  {
				  private readonly DirectoryServiceWaitOnSearch _outerInstance;

				  private long _waitingTimeMillis;

				  public BaseInterceptorAnonymousInnerClass( DirectoryServiceWaitOnSearch outerInstance, long waitingTimeMillis )
				  {
					  this.outerInstance = outerInstance;
					  this._waitingTimeMillis = waitingTimeMillis;
				  }

				  public override string Name
				  {
					  get
					  {
	//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
							return this.GetType().FullName;
					  }
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.directory.server.core.api.filtering.EntryFilteringCursor search(org.apache.directory.server.core.api.interceptor.context.SearchOperationContext searchContext) throws org.apache.directory.api.ldap.model.exception.LdapException
				  public override EntryFilteringCursor search( SearchOperationContext searchContext )
				  {
						try
						{
							 Thread.Sleep( _waitingTimeMillis );
						}
						catch ( InterruptedException )
						{
							 Thread.CurrentThread.Interrupt();
						}
						return base.search( searchContext );
				  }
			  }

			  public override void Close()
			  {
					Service.remove( WaitOnSearchInterceptor.Name );
			  }
		 }

		 private class DirectoryServiceFailOnSearch : IDisposable
		 {
			 private readonly LdapAuthIT _outerInstance;

			  internal readonly Interceptor FailOnSearchInterceptor;

			  internal DirectoryServiceFailOnSearch( LdapAuthIT outerInstance )
			  {
				  this._outerInstance = outerInstance;
					FailOnSearchInterceptor = new BaseInterceptorAnonymousInnerClass( this );

					try
					{
						 Service.addFirst( FailOnSearchInterceptor );
					}
					catch ( LdapException e )
					{
						 throw new Exception( e );
					}
			  }

			  private class BaseInterceptorAnonymousInnerClass : BaseInterceptor
			  {
				  private readonly DirectoryServiceFailOnSearch _outerInstance;

				  public BaseInterceptorAnonymousInnerClass( DirectoryServiceFailOnSearch outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public override string Name
				  {
					  get
					  {
	//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
							return this.GetType().FullName;
					  }
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.directory.server.core.api.filtering.EntryFilteringCursor search(org.apache.directory.server.core.api.interceptor.context.SearchOperationContext searchContext) throws org.apache.directory.api.ldap.model.exception.LdapException
				  public override EntryFilteringCursor search( SearchOperationContext searchContext )
				  {
						throw new LdapOperationErrorException();
				  }
			  }

			  public override void Close()
			  {
					Service.remove( FailOnSearchInterceptor.Name );
			  }
		 }
	}

}