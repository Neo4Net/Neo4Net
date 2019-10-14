using System.Collections.Generic;

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
namespace Neo4Net.Server.security.enterprise.auth.integration.bolt
{
	using AbstractLdapTestUnit = org.apache.directory.server.core.integ.AbstractLdapTestUnit;
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using Matchers = org.hamcrest.Matchers;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using RuleChain = org.junit.rules.RuleChain;


	using AuthToken = Neo4Net.driver.v1.AuthToken;
	using AuthTokens = Neo4Net.driver.v1.AuthTokens;
	using Config = Neo4Net.driver.v1.Config;
	using Driver = Neo4Net.driver.v1.Driver;
	using GraphDatabase = Neo4Net.driver.v1.GraphDatabase;
	using Record = Neo4Net.driver.v1.Record;
	using Session = Neo4Net.driver.v1.Session;
	using StatementResult = Neo4Net.driver.v1.StatementResult;
	using Value = Neo4Net.driver.v1.Value;
	using AuthenticationException = Neo4Net.driver.v1.exceptions.AuthenticationException;
	using ClientException = Neo4Net.driver.v1.exceptions.ClientException;
	using ServiceUnavailableException = Neo4Net.driver.v1.exceptions.ServiceUnavailableException;
	using Neo4Net.Graphdb.config;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using AuthSubject = Neo4Net.Internal.Kernel.Api.security.AuthSubject;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using Neo4Net.Server.security.enterprise.auth;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EnterpriseDatabaseRule = Neo4Net.Test.rule.EnterpriseDatabaseRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.driver.Internal.logging.DevNullLogging.DEV_NULL_LOGGING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.BoltConnector.EncryptionLevel.OPTIONAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.BasicAuthManagerTest.password;

	public abstract class EnterpriseAuthenticationTestBase : AbstractLdapTestUnit
	{
		private bool InstanceFieldsInitialized = false;

		public EnterpriseAuthenticationTestBase()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			DbRule = GetDatabaseTestRule( _testDirectory );
			Chain = RuleChain.outerRule( _testDirectory ).around( DbRule );
		}

		 private static readonly Config _config = Config.build().withLogging(DEV_NULL_LOGGING).toConfig();

		 private TestDirectory _testDirectory = TestDirectory.testDirectory( this.GetType() );

		 protected internal DatabaseRule DbRule;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain chain = org.junit.rules.RuleChain.outerRule(testDirectory).around(dbRule);
		 public RuleChain Chain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  DbRule.withSetting( GraphDatabaseSettings.auth_enabled, "true" ).withSetting( ( new BoltConnector( "bolt" ) ).type, "BOLT" ).withSetting( ( new BoltConnector( "bolt" ) ).enabled, "true" ).withSetting( ( new BoltConnector( "bolt" ) ).encryption_level, OPTIONAL.name() ).withSetting((new BoltConnector("bolt")).listen_address, "localhost:0");
			  DbRule.withSettings( Settings );
			  DbRule.ensureStarted();
			  DbRule.resolveDependency( typeof( Procedures ) ).registerProcedure( typeof( ProcedureInteractionTestBase.ClassWithProcedures ) );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected abstract java.util.Map<org.neo4j.graphdb.config.Setting<?>,String> getSettings();
		 protected internal abstract IDictionary<Setting<object>, string> Settings { get; }

		 protected internal virtual DatabaseRule GetDatabaseTestRule( TestDirectory testDirectory )
		 {
			  return ( new EnterpriseDatabaseRule( testDirectory ) ).startLazily();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void restartServerWithOverriddenSettings(String... configChanges) throws java.io.IOException
		 internal virtual void RestartServerWithOverriddenSettings( params string[] configChanges )
		 {
			  DbRule.restartDatabase( configChanges );
		 }

		 internal virtual void AssertAuth( string username, string password )
		 {
			  AssertAuth( username, password, null );
		 }

		 internal virtual void AssertAuth( string username, string password, string realm )
		 {
			  using ( Driver driver = ConnectDriver( username, password, realm ), Session session = driver.session() )
			  {
					Value single = session.run( "RETURN 1" ).single().get(0);
					assertThat( single.asLong(), CoreMatchers.equalTo(1L) );
			  }
		 }

		 internal virtual void AssertAuth( AuthToken authToken )
		 {
			  using ( Driver driver = ConnectDriver( authToken ), Session session = driver.session() )
			  {
					Value single = session.run( "RETURN 1" ).single().get(0);
					assertThat( single.asLong(), CoreMatchers.equalTo(1L) );
			  }
		 }

		 internal virtual void AssertAuthFail( string username, string password )
		 {
			  AssertAuthFail( username, password, null );
		 }

		 internal virtual void AssertAuthFail( string username, string password, string realm )
		 {
			  try
			  {
					  using ( Driver ignored = ConnectDriver( username, password, realm ) )
					  {
						fail( "Should not have authenticated" );
					  }
			  }
			  catch ( AuthenticationException e )
			  {
					assertThat( e.code(), CoreMatchers.equalTo("Neo.ClientError.Security.Unauthorized") );
			  }
		 }

		 internal virtual void AssertReadSucceeds( Driver driver )
		 {
			  using ( Session session = driver.session() )
			  {
					Value single = session.run( "MATCH (n) RETURN count(n)" ).single().get(0);
					assertThat( single.asLong(), Matchers.greaterThanOrEqualTo(0L) );
			  }
		 }

		 internal virtual void AssertReadFails( string username, string password )
		 {
			  using ( Driver driver = ConnectDriver( username, password ) )
			  {
					AssertReadFails( driver );
			  }
		 }

		 internal virtual void AssertReadFails( Driver driver )
		 {
			  try
			  {
					  using ( Session session = driver.session() )
					  {
						session.run( "MATCH (n) RETURN count(n)" ).single().get(0);
						fail( "Should not be allowed read operation" );
					  }
			  }
			  catch ( ClientException e )
			  {
					assertThat( e.Message, containsString( "Read operations are not allowed for user " ) );
			  }
		 }

		 internal virtual void AssertWriteSucceeds( Driver driver )
		 {
			  using ( Session session = driver.session() )
			  {
					StatementResult result = session.run( "CREATE ()" );
					assertThat( result.summary().counters().nodesCreated(), CoreMatchers.equalTo(1) );
			  }
		 }

		 internal virtual void AssertWriteFails( Driver driver )
		 {
			  try
			  {
					  using ( Session session = driver.session() )
					  {
						session.run( "CREATE ()" ).consume();
						fail( "Should not be allowed write operation" );
					  }
			  }
			  catch ( ClientException e )
			  {
					assertThat( e.Message, containsString( "Write operations are not allowed for user " ) );
			  }
		 }

		 internal virtual void AssertProcSucceeds( Driver driver )
		 {
			  using ( Session session = driver.session() )
			  {
					Value single = session.run( "CALL test.staticReadProcedure()" ).single().get(0);
					assertThat( single.asString(), CoreMatchers.equalTo("static") );
			  }
		 }

		 internal virtual void AssertAuthorizationExpired( Driver driver )
		 {
			  try
			  {
					  using ( Session session = driver.session() )
					  {
						session.run( "MATCH (n) RETURN n" ).single();
						fail( "should have gotten authorization expired exception" );
					  }
			  }
			  catch ( ServiceUnavailableException )
			  {
					// TODO Bolt should handle the AuthorizationExpiredException better
					//assertThat( e.getMessage(), equalTo( "Plugin 'plugin-TestCombinedAuthPlugin' authorization info expired: " +
					//        "authorization_expired_user needs to re-authenticate." ) );
			  }
		 }

		 internal virtual void ClearAuthCacheFromDifferentConnection()
		 {
			  ClearAuthCacheFromDifferentConnection( "neo4j", "abc123", null );
		 }

		 internal virtual void ClearAuthCacheFromDifferentConnection( string username, string password, string realm )
		 {
			  using ( Driver driver = ConnectDriver( username, password, realm ), Session session = driver.session() )
			  {
					session.run( "CALL dbms.security.clearAuthCache()" );
			  }
		 }

		 internal virtual Driver ConnectDriver( string username, string password )
		 {
			  return ConnectDriver( username, password, null );
		 }

		 internal virtual Driver ConnectDriver( string username, string password, string realm )
		 {
			  AuthToken token;
			  if ( string.ReferenceEquals( realm, null ) || realm.Length == 0 )
			  {
					token = AuthTokens.basic( username, password );
			  }
			  else
			  {
					token = AuthTokens.basic( username, password, realm );
			  }
			  return ConnectDriver( token );
		 }

		 private Driver ConnectDriver( AuthToken token )
		 {
			  return GraphDatabase.driver( "bolt://" + DbRule.resolveDependency( typeof( ConnectorPortRegister ) ).getLocalAddress( "bolt" ).ToString(), token, _config );
		 }

		 internal virtual void AssertRoles( Driver driver, params string[] roles )
		 {
			  using ( Session session = driver.session() )
			  {
					Record record = session.run( "CALL dbms.showCurrentUser() YIELD roles" ).single();
					assertThat( record.get( "roles" ).asList(), containsInAnyOrder(roles) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void assertSecurityLogContains(String message) throws java.io.IOException
		 internal virtual void AssertSecurityLogContains( string message )
		 {
			  FileSystemAbstraction fileSystem = _testDirectory.FileSystem;
			  File workingDirectory = _testDirectory.directory();
			  File logFile = new File( workingDirectory, "logs/security.log" );

			  Reader reader = fileSystem.OpenAsReader( logFile, UTF_8 );
			  StreamReader bufferedReader = new StreamReader( reader );
			  string line;
			  bool foundError = false;

			  while ( !string.ReferenceEquals( ( line = bufferedReader.ReadLine() ), null ) )
			  {
					if ( line.Contains( message ) )
					{
						 foundError = true;
					}
			  }
			  bufferedReader.Close();
			  reader.close();

			  assertThat( "Security log should contain message '" + message + "'", foundError );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void assertSecurityLogDoesNotContain(String message) throws java.io.IOException
		 internal virtual void AssertSecurityLogDoesNotContain( string message )
		 {
			  FileSystemAbstraction fileSystem = _testDirectory.FileSystem;
			  File workingDirectory = _testDirectory.directory();
			  File logFile = new File( workingDirectory, "logs/security.log" );

			  Reader reader = fileSystem.OpenAsReader( logFile, UTF_8 );
			  StreamReader bufferedReader = new StreamReader( reader );
			  string line;

			  while ( !string.ReferenceEquals( ( line = bufferedReader.ReadLine() ), null ) )
			  {
					assertThat( "Security log should not contain message '" + message + "'", !line.Contains( message ) );
			  }
			  bufferedReader.Close();
			  reader.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createNativeUser(String username, String password, String... roles) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 internal virtual void CreateNativeUser( string username, string password, params string[] roles )
		 {
			  EnterpriseAuthAndUserManager authManager = DbRule.resolveDependency( typeof( EnterpriseAuthAndUserManager ) );

			  authManager.GetUserManager( AuthSubject.AUTH_DISABLED, true ).newUser( username, password( password ), false );

			  foreach ( string role in roles )
			  {
					authManager.GetUserManager( AuthSubject.AUTH_DISABLED, true ).addRoleToUser( role, username );
			  }
		 }

		 //-------------------------------------------------------------------------
		 // TLS helper
		 internal class EmbeddedTestCertificates : AutoCloseable
		 {
			  internal const string KEY_STORE = "javax.net.ssl.keyStore";
			  internal const string KEY_STORE_PASSWORD = "javax.net.ssl.keyStorePassword";
			  internal const string TRUST_STORE = "javax.net.ssl.trustStore";
			  internal const string TRUST_STORE_PASSWORD = "javax.net.ssl.trustStorePassword";

			  internal readonly string KeyStore = System.getProperty( KEY_STORE );
			  internal readonly string KeyStorePassword = System.getProperty( KEY_STORE_PASSWORD );
			  internal readonly string TrustStore = System.getProperty( TRUST_STORE );
			  internal readonly string TrustStorePassword = System.getProperty( TRUST_STORE_PASSWORD );

			  internal EmbeddedTestCertificates()
			  {
					URL url = this.GetType().getResource("/neo4j_ldap_test_keystore.jks");
					File keyStoreFile = new File( url.File );
					string keyStorePath = keyStoreFile.AbsolutePath;

					System.setProperty( KEY_STORE, keyStorePath );
					System.setProperty( KEY_STORE_PASSWORD, "secret" );
					System.setProperty( TRUST_STORE, keyStorePath );
					System.setProperty( TRUST_STORE_PASSWORD, "secret" );
			  }

			  public override void Close()
			  {
					ResetProperty( KEY_STORE, KeyStore );
					ResetProperty( KEY_STORE_PASSWORD, KeyStorePassword );
					ResetProperty( TRUST_STORE, TrustStore );
					ResetProperty( TRUST_STORE_PASSWORD, TrustStorePassword );
			  }

			  internal virtual void ResetProperty( string property, string value )
			  {
					if ( string.ReferenceEquals( value, null ) )
					{
						 System.clearProperty( property );
					}
					else
					{
						 System.setProperty( property, value );
					}
			  }
		 }
	}

}