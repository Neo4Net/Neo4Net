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
namespace Neo4Net.Server.security.enterprise.auth.integration.bolt
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Ignore = org.junit.Ignore;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4NetPackV1 = Neo4Net.Bolt.v1.messaging.Neo4NetPackV1;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using Neo4NetWithSocket = Neo4Net.Bolt.v1.transport.integration.Neo4NetWithSocket;
	using TransportTestUtil = Neo4Net.Bolt.v1.transport.integration.TransportTestUtil;
	using SecureSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SecureSocketConnection;
	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using Neo4Net.Functions;
	using Neo4Net.GraphDb.config;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using SecuritySettings = Neo4Net.Server.security.enterprise.configuration.SecuritySettings;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.messaging.util.MessageMatchers.msgFailure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.messaging.util.MessageMatchers.msgSuccess;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.transport.integration.TransportTestUtil.eventuallyReceives;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.map;

	/*
	 * Only run these tests when the appropriate ActiveDirectory server is in fact live.
	 * The tests remain here because they are very useful when developing and testing Active Directory
	 * security features. Regular automated testing of Active Directory security should also be handled
	 * in the smoke tests run downstream of the main build, so the fact that these tests are not run during
	 * the main build should not be of serious concern.
	 *
	 * Note also that most of the security code related to Active Directory is identical to the LDAP code,
	 * and so the tests in LdapAuthIT, which are run during normal build, do in fact test that
	 * code. Testing against a real Active Directory is not possible during a build phase, and therefor
	 * we keep this disabled by default.
	 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore public class ActiveDirectoryAuthenticationIT
	public class ActiveDirectoryAuthenticationIT
	{
		private bool InstanceFieldsInitialized = false;

		public ActiveDirectoryAuthenticationIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Server = new Neo4NetWithSocket( this.GetType(), TestGraphDatabaseFactory, AsSettings(SettingsFunction) );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.bolt.v1.transport.integration.Neo4NetWithSocket server = new Neo4Net.bolt.v1.transport.integration.Neo4NetWithSocket(getClass(), getTestGraphDatabaseFactory(), asSettings(getSettingsFunction()));
		 public Neo4NetWithSocket Server;

		 private void RestartNeo4NetServerWithOverriddenSettings<T1>( System.Action<T1> overrideSettingsFunction )
		 {
			  Server.shutdownDatabase();
			  Server.ensureDatabase( AsSettings( overrideSettingsFunction ) );
		 }

		 private System.Action<IDictionary<string, string>> AsSettings<T1>( System.Action<T1> overrideSettingsFunction )
		 {
			  return settings =>
			  {
				IDictionary<Setting<object>, string> o = new LinkedHashMap<Setting<object>, string>();
				overrideSettingsFunction( o );
				foreach ( Setting key in o.Keys )
				{
					 settings.put( key.name(), o.get(key) );
				}
			  };
		 }

		 protected internal virtual TestGraphDatabaseFactory TestGraphDatabaseFactory
		 {
			 get
			 {
				  return new TestEnterpriseGraphDatabaseFactory();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected System.Action<java.util.Map<Neo4Net.graphdb.config.Setting<?>,String>> getSettingsFunction()
		 protected internal virtual System.Action<IDictionary<Setting<object>, string>> SettingsFunction
		 {
			 get
			 {
				  return settings =>
				  {
					settings.put( GraphDatabaseSettings.auth_enabled, "true" );
					settings.put( SecuritySettings.auth_provider, "ldap" );
					settings.put( SecuritySettings.ldap_server, "activedirectory.neohq.net" );
					settings.put( SecuritySettings.ldap_authentication_user_dn_template, "CN={0},CN=Users,DC=Neo4Net,DC=com" );
					settings.put( SecuritySettings.ldap_authorization_use_system_account, "false" );
					settings.put( SecuritySettings.ldap_authorization_user_search_base, "cn=Users,dc=Neo4Net,dc=com" );
					settings.put( SecuritySettings.ldap_authorization_user_search_filter, "(&(objectClass=*)(CN={0}))" );
					settings.put( SecuritySettings.ldap_authorization_group_membership_attribute_names, "memberOf" );
					settings.put( SecuritySettings.ldap_authorization_group_to_role_mapping, "'CN=Neo4Net Read Only,CN=Users,DC=Neo4Net,DC=com'=reader;" + "CN=Neo4Net Read-Write,CN=Users,DC=Neo4Net,DC=com=publisher;" + "CN=Neo4Net Schema Manager,CN=Users,DC=Neo4Net,DC=com=architect;" + "CN=Neo4Net Administrator,CN=Users,DC=Neo4Net,DC=com=admin" );
				  };
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private System.Action<java.util.Map<Neo4Net.graphdb.config.Setting<?>,String>> useSystemAccountSettings = settings ->
		 private System.Action<IDictionary<Setting<object>, string>> _useSystemAccountSettings = settings =>
		 {
		  settings.put( SecuritySettings.ldap_authorization_use_system_account, "true" );
		  settings.put( SecuritySettings.ldap_authorization_system_username, "Neo4Net System" );
		  settings.put( SecuritySettings.ldap_authorization_system_password, "ProudListingsMedia1" );
		 };

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 public IFactory<TransportConnection> Cf = ( IFactory<TransportConnection> ) SecureSocketConnection::new;

		 private HostnamePort _address;
		 private TransportConnection _client;
		 private TransportTestUtil _util;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  this._client = Cf.newInstance();
			  this._address = Server.lookupDefaultConnector();
			  this._util = new TransportTestUtil( new Neo4NetPackV1() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void teardown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Teardown()
		 {
			  if ( _client != null )
			  {
					_client.disconnect();
			  }
		 }

		 //------------------------------------------------------------------
		 // Active Directory tests on EC2
		 // NOTE: These rely on an external server and are not executed by automated testing
		 //       They are here as a convenience for running local testing.

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToLoginUnknownUserOnEC2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToLoginUnknownUserOnEC2()
		 {

			  AssertAuthFail( "unknown", "ProudListingsMedia1" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLoginAndAuthorizeReaderWithUserLdapContextOnEC2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToLoginAndAuthorizeReaderWithUserLdapContextOnEC2()
		 {
			  AssertAuth( "neo", "ProudListingsMedia1" );
			  AssertReadSucceeds();
			  AssertWriteFails( "'neo' with roles [reader]" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLoginAndAuthorizeReaderOnEC2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToLoginAndAuthorizeReaderOnEC2()
		 {
			  RestartNeo4NetServerWithOverriddenSettings( _useSystemAccountSettings );

			  AssertAuth( "neo", "ProudListingsMedia1" );
			  AssertReadSucceeds();
			  AssertWriteFails( "'neo' with roles [reader]" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLoginAndAuthorizePublisherWithUserLdapContextOnEC2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToLoginAndAuthorizePublisherWithUserLdapContextOnEC2()
		 {
			  AssertAuth( "tank", "ProudListingsMedia1" );
			  AssertWriteSucceeds();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLoginAndAuthorizePublisherOnEC2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToLoginAndAuthorizePublisherOnEC2()
		 {
			  RestartNeo4NetServerWithOverriddenSettings( _useSystemAccountSettings );

			  AssertAuth( "tank", "ProudListingsMedia1" );
			  AssertWriteSucceeds();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLoginAndAuthorizeNoPermissionUserWithUserLdapContextOnEC2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToLoginAndAuthorizeNoPermissionUserWithUserLdapContextOnEC2()
		 {
			  AssertAuth( "smith", "ProudListingsMedia1" );
			  AssertReadFails( "'smith' with no roles" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLoginAndAuthorizeNoPermissionUserOnEC2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToLoginAndAuthorizeNoPermissionUserOnEC2()
		 {
			  RestartNeo4NetServerWithOverriddenSettings( _useSystemAccountSettings );

			  AssertAuth( "smith", "ProudListingsMedia1" );
			  AssertReadFails( "'smith' with no roles" );
		 }

		 //------------------------------------------------------------------
		 // Secure Active Directory tests on EC2
		 // NOTE: These tests does not work together with EmbeddedTestCertificates used in the embedded secure LDAP tests!
		 //       (This is because the embedded tests override the Java default key/trust store locations using
		 //        system properties that will not be re-read)

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLoginAndAuthorizeReaderUsingLdapsOnEC2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToLoginAndAuthorizeReaderUsingLdapsOnEC2()
		 {
			  RestartNeo4NetServerWithOverriddenSettings( _useSystemAccountSettings.andThen( settings => settings.put( SecuritySettings.ldap_server, "ldaps://activedirectory.neohq.net:636" ) ) );

			  AssertAuth( "neo", "ProudListingsMedia1" );
			  AssertReadSucceeds();
			  AssertWriteFails( "'neo' with roles [reader]" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLoginAndAuthorizeReaderWithUserLdapContextUsingLDAPSOnEC2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToLoginAndAuthorizeReaderWithUserLdapContextUsingLDAPSOnEC2()
		 {
			  RestartNeo4NetServerWithOverriddenSettings( settings => settings.put( SecuritySettings.ldap_server, "ldaps://activedirectory.neohq.net:636" ) );

			  AssertAuth( "neo", "ProudListingsMedia1" );
			  AssertReadSucceeds();
			  AssertWriteFails( "'neo' with roles [reader]" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLoginAndAuthorizeReaderUsingStartTlsOnEC2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToLoginAndAuthorizeReaderUsingStartTlsOnEC2()
		 {
			  RestartNeo4NetServerWithOverriddenSettings( _useSystemAccountSettings.andThen( settings => settings.put( SecuritySettings.ldap_use_starttls, "true" ) ) );

			  AssertAuth( "neo", "ProudListingsMedia1" );
			  AssertReadSucceeds();
			  AssertWriteFails( "'neo' with roles [reader]" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToLoginAndAuthorizeReaderWithUserLdapContextUsingStartTlsOnEC2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToLoginAndAuthorizeReaderWithUserLdapContextUsingStartTlsOnEC2()
		 {
			  RestartNeo4NetServerWithOverriddenSettings( settings => settings.put( SecuritySettings.ldap_use_starttls, "true" ) );

			  AssertAuth( "neo", "ProudListingsMedia1" );
			  AssertReadSucceeds();
			  AssertWriteFails( "'neo' with roles [reader]" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToAccessEC2ActiveDirectoryInstance() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToAccessEC2ActiveDirectoryInstance()
		 {
			  RestartNeo4NetServerWithOverriddenSettings(settings =>
			  {
			  });

			  // When
			  AssertAuth( "tank", "ProudListingsMedia1" );

			  // Then
			  AssertReadSucceeds();
			  AssertWriteSucceeds();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertAuth(String username, String password) throws Exception
		 private void AssertAuth( string username, string password )
		 {
			  AssertAuth( username, password, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertAuth(String username, String password, String realm) throws Exception
		 private void AssertAuth( string username, string password, string realm )
		 {
			  _client.connect( _address ).send( _util.acceptedVersions( 1, 0, 0, 0 ) ).send( _util.chunk( new InitMessage( "TestClient/1.1", AuthToken( username, password, realm ) ) ) );

			  assertThat( _client, eventuallyReceives( new sbyte[]{ 0, 0, 0, 1 } ) );
			  assertThat( _client, _util.eventuallyReceives( msgSuccess() ) );
		 }

		 private IDictionary<string, object> AuthToken( string username, string password, string realm )
		 {
			  if ( !string.ReferenceEquals( realm, null ) && realm.Length > 0 )
			  {
					return map( "principal", username, "credentials", password, "scheme", "basic", "realm", realm );
			  }
			  else
			  {
					return map( "principal", username, "credentials", password, "scheme", "basic" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertAuthFail(String username, String password) throws Exception
		 private void AssertAuthFail( string username, string password )
		 {
			  _client.connect( _address ).send( _util.acceptedVersions( 1, 0, 0, 0 ) ).send( _util.chunk( new InitMessage( "TestClient/1.1", map( "principal", username, "credentials", password, "scheme", "basic" ) ) ) );

			  assertThat( _client, eventuallyReceives( new sbyte[]{ 0, 0, 0, 1 } ) );
			  assertThat( _client, _util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized, "The client is unauthorized due to authentication failure." ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void assertReadSucceeds() throws Exception
		 protected internal virtual void AssertReadSucceeds()
		 {
			  // When
			  _client.send( _util.chunk( new RunMessage( "MATCH (n) RETURN n" ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( _client, _util.eventuallyReceives( msgSuccess(), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void assertReadFails(String username) throws Exception
		 protected internal virtual void AssertReadFails( string username )
		 {
			  // When
			  _client.send( _util.chunk( new RunMessage( "MATCH (n) RETURN n" ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( _client, _util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Security.Forbidden, string.Format( "Read operations are not allowed for user {0}.", username ) ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void assertWriteSucceeds() throws Exception
		 protected internal virtual void AssertWriteSucceeds()
		 {
			  // When
			  _client.send( _util.chunk( new RunMessage( "CREATE ()" ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( _client, _util.eventuallyReceives( msgSuccess(), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void assertWriteFails(String username) throws Exception
		 protected internal virtual void AssertWriteFails( string username )
		 {
			  // When
			  _client.send( _util.chunk( new RunMessage( "CREATE ()" ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( _client, _util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Security.Forbidden, string.Format( "Write operations are not allowed for user {0}.", username ) ) ) );
		 }

	}

}