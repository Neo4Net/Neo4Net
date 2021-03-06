﻿/*
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
namespace Org.Neo4j.Bolt
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using SslPolicyConfig = Org.Neo4j.Kernel.configuration.ssl.SslPolicyConfig;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using SecureClient = Org.Neo4j.Ssl.SecureClient;
	using SslContextFactory = Org.Neo4j.Ssl.SslContextFactory;
	using SslResource = Org.Neo4j.Ssl.SslResource;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ssl.SslContextFactory.SslParameters.protocols;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ssl.SslContextFactory.makeSslPolicy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ssl.SslResourceBuilder.selfSignedKeyId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.PortUtils.getBoltPort;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class BoltTlsIT
	public class BoltTlsIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private readonly LogProvider _logProvider = NullLogProvider.Instance;

		 private SslPolicyConfig _sslPolicy = new SslPolicyConfig( "bolt" );

		 private GraphDatabaseAPI _db;
		 private SslResource _sslResource;

		 private BoltConnector _bolt = new BoltConnector( "bolt" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  File sslObjectsDir = new File( TestDirectory.storeDir(), "certificates" );
			  assertTrue( sslObjectsDir.mkdirs() );

			  _sslResource = selfSignedKeyId( 0 ).trustKeyId( 0 ).install( sslObjectsDir );

			  CreateAndStartDb();
		 }

		 internal class TestSetup
		 {
			  internal readonly string ClientTlsVersions;
			  internal readonly string BoltTlsVersions;
			  internal readonly bool ShouldSucceed;

			  internal TestSetup( string clientTlsVersions, string boltTlsVersion, bool shouldSucceed )
			  {
					this.ClientTlsVersions = clientTlsVersions;
					this.BoltTlsVersions = boltTlsVersion;
					this.ShouldSucceed = shouldSucceed;
			  }

			  public override string ToString()
			  {
					return "TestSetup{"
							  + "clientTlsVersions='" + ClientTlsVersions + '\'' + ", boltTlsVersions='" + BoltTlsVersions + '\'' + ", shouldSucceed=" + ShouldSucceed + '}';
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Object[] params()
		 public static object[] Params()
		 {
			  return new TestSetup[]
			  {
				  new TestSetup( "TLSv1.1", "TLSv1.2", false ),
				  new TestSetup( "TLSv1.2", "TLSv1.1", false ),
				  new TestSetup( "TLSv1", "TLSv1.1", false ),
				  new TestSetup( "TLSv1.1", "TLSv1.2", false ),
				  new TestSetup( "TLSv1", "TLSv1", true ),
				  new TestSetup( "TLSv1.1", "TLSv1.1", true ),
				  new TestSetup( "TLSv1.2", "TLSv1.2", true ),
				  new TestSetup( "SSLv3,TLSv1", "TLSv1.1,TLSv1.2", false ),
				  new TestSetup( "TLSv1.1,TLSv1.2", "TLSv1.1,TLSv1.2", true )
			  };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public TestSetup setup;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public TestSetup SetupConflict;

		 private void CreateAndStartDb()
		 {
			  _db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder(TestDirectory.databaseDir()).setConfig(_bolt.enabled, "true").setConfig(_bolt.listen_address, "localhost:0").setConfig(GraphDatabaseSettings.bolt_ssl_policy, "bolt").setConfig(_sslPolicy.allow_key_generation, "true").setConfig(_sslPolicy.base_directory, "certificates").setConfig(_sslPolicy.tls_versions, SetupConflict.boltTlsVersions).setConfig(_sslPolicy.client_auth, "none").setConfig(_sslPolicy.verify_hostname, "false").newGraphDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void teardown()
		 public virtual void Teardown()
		 {
			  if ( _db != null )
			  {
					_db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectProtocolSelection() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectProtocolSelection()
		 {
			  // given
			  SslContextFactory.SslParameters @params = protocols( SetupConflict.clientTlsVersions ).ciphers();
			  SecureClient client = new SecureClient( makeSslPolicy( _sslResource, @params ) );

			  // when
			  client.Connect( getBoltPort( _db ) );

			  // then
			  try
			  {
					assertTrue( client.SslHandshakeFuture().get(1, TimeUnit.MINUTES).Active );
			  }
			  catch ( ExecutionException )
			  {
					assertFalse( SetupConflict.shouldSucceed );
			  }
		 }
	}

}