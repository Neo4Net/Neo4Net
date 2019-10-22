/*
 *
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * Modifications Copyright (c) 2019 "GraphFoundation" [https://graphfoundation.org]
 *
 * The included source code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html).
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Reference: https://raw.githubusercontent.com/Neo4Net/Neo4Net/3.4/enterprise/causal-clustering/src/test/java/org/Neo4Net/causalclustering/handlers/VoidPipelineWrapperFactoryTest.java
 */
namespace Neo4Net.causalclustering.handlers
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using Config = Neo4Net.Kernel.configuration.Config;
	using SslPolicyConfig = Neo4Net.Kernel.configuration.ssl.SslPolicyConfig;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PkiUtils = Neo4Net.Ssl.PkiUtils;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.Neo4Net_home;

	public class SecurePipelineWrapperFactoryTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private File _home;
		 private File _publicCertificateFile;
		 private File _privateKeyFile;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _home = TestDirectory.directory( "home" );
			  File baseDir = new File( _home, "certificates/default" );
			  _publicCertificateFile = new File( baseDir, "public.crt" );
			  _privateKeyFile = new File( baseDir, "private.key" );

			  ( new PkiUtils() ).createSelfSignedCertificate(_publicCertificateFile, _privateKeyFile, "localhost");

			  File trustedDir = new File( baseDir, "trusted" );
			  trustedDir.mkdir();
			  FileUtils.copyFile( _publicCertificateFile, new File( trustedDir, "public.crt" ) );
			  ( new File( baseDir, "revoked" ) ).mkdir();
		 }

		 private SecurePipelineWrapperFactory CreateSecurePipelineWrapperFactory()
		 {

			  SecurePipelineWrapperFactory pipelineWrapperFactory = new SecurePipelineWrapperFactory();

			  return pipelineWrapperFactory;
		 }

		 //@Test
		 public virtual void ClientSslEncryptionPoliciesThrowException()
		 {
			  // given
			  SecurePipelineWrapperFactory pipelineWrapperFactory = CreateSecurePipelineWrapperFactory();

			  // and
			  Config config = Config.defaults();
			  config.Augment( CausalClusteringSettings.ssl_policy, "cluster" );

			  // and
			  LogProvider logProvider = NullLogProvider.Instance;
			  Dependencies dependencies = null;

			  // then
			  ExpectedException.expectMessage( "Unexpected SSL policy causal_clustering.ssl_policy is a string" );

			  // when
			  pipelineWrapperFactory.ForClient( config, dependencies, logProvider, CausalClusteringSettings.ssl_policy );
		 }

		 //@Test
		 public virtual void ServerSslEncryptionPoliciesThrowException()
		 {
			  // given
			  SecurePipelineWrapperFactory pipelineWrapperFactory = CreateSecurePipelineWrapperFactory();

			  // and
			  Config config = Config.defaults();
			  config.Augment( OnlineBackupSettings.ssl_policy, "backup" );

			  // and
			  LogProvider logProvider = NullLogProvider.Instance;
			  Dependencies dependencies = null;

			  // then
			  ExpectedException.expectMessage( "Unexpected SSL policy dbms.backup.ssl_policy is a string" );

			  // when
			  pipelineWrapperFactory.ForServer( config, dependencies, logProvider, OnlineBackupSettings.ssl_policy );
		 }

		 //@Test
		 public virtual void ClientAndServersWithoutPoliciesFail()
		 {
			  // given
			  SecurePipelineWrapperFactory pipelineWrapperFactory = CreateSecurePipelineWrapperFactory();

			  SslPolicyConfig policyConfig = new SslPolicyConfig( "default" );
			  Config config = Config.defaults();

			  // Setup SslPolicy
			  config.augment( Neo4Net_home.name(), _home.AbsolutePath );
			  config.Augment( policyConfig.BaseDirectory.name(), "certificates/default" );

			  // and
			  LogProvider logProvider = NullLogProvider.Instance;
			  Dependencies dependencies = null;

			  // when
			  // expectedException.expect( java.lang.NullPointerException.class );
			  pipelineWrapperFactory.ForServer( config, dependencies, logProvider, CausalClusteringSettings.ssl_policy );
			  pipelineWrapperFactory.ForClient( config, dependencies, logProvider, CausalClusteringSettings.ssl_policy );
			  pipelineWrapperFactory.ForServer( config, dependencies, logProvider, OnlineBackupSettings.ssl_policy );
			  pipelineWrapperFactory.ForClient( config, dependencies, logProvider, OnlineBackupSettings.ssl_policy );
		 }
	}

}