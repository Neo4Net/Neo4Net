using System;
using System.Collections.Generic;

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
 * Reference: https://raw.githubusercontent.com/Neo4Net/Neo4Net/3.4/enterprise/causal-clustering/src/test/java/org/Neo4Net/causalclustering/discovery/HazelcastCoreTopologyServiceTest.java
 */
namespace Neo4Net.causalclustering.discovery
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using PlatformModule = Neo4Net.GraphDb.factory.module.PlatformModule;
	using AbstractEditionModule = Neo4Net.GraphDb.factory.module.edition.AbstractEditionModule;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using SslPolicyConfig = Neo4Net.Kernel.configuration.ssl.SslPolicyConfig;
	using SslPolicyLoader = Neo4Net.Kernel.configuration.ssl.SslPolicyLoader;
	using EnterpriseEditionModule = Neo4Net.Kernel.impl.enterprise.EnterpriseEditionModule;
	using IJobSchedulerFactory = Neo4Net.Kernel.impl.scheduler.JobSchedulerFactory;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using PkiUtils = Neo4Net.Ssl.PkiUtils;
	using SslPolicy = Neo4Net.Ssl.SslPolicy;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.CausalClusteringSettings.initial_discovery_members;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.facade.GraphDatabaseDependencies.newDependencies;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.Neo4Net_home;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.factory.DatabaseInfo.ENTERPRISE;

	public class SecureHazelcastCoreTopologyServiceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private File _home;
		 private File _publicCertificateFile;
		 private File _privateKeyFile;

		 private static Config Config()
		 {
			  return Config.defaults( stringMap( CausalClusteringSettings.raft_advertised_address.name(), "127.0.0.1:7000", CausalClusteringSettings.transaction_advertised_address.name(), "127.0.0.1:7001", (new BoltConnector("bolt")).enabled.name(), "true", (new BoltConnector("bolt")).advertised_address.name(), "127.0.0.1:7002" ) );
		 }

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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 120_000) public void shouldBeAbleToStartAndStoreWithoutSuccessfulJoin()
		 public virtual void ShouldBeAbleToStartAndStoreWithoutSuccessfulJoin()
		 {
			  // given
			  IDictionary<string, string> @params = stringMap();

			  SslPolicyConfig policyConfig = new SslPolicyConfig( "default" );

			  @params[Neo4Net_home.name()] = _home.AbsolutePath;
			  @params[policyConfig.BaseDirectory.name()] = "certificates/default";

			  IJobScheduler jobScheduler = IJobSchedulerFactory.createInitializedScheduler();
			  PlatformModule platformModule = new PlatformModule( TestDirectory.storeDir(), Config.defaults(), ENTERPRISE, newDependencies() );
			  AbstractEditionModule editionModule = new EnterpriseEditionModule( platformModule );
			  // Random members that does not exists, discovery will never succeed
			  string initialHosts = "localhost:" + PortAuthority.allocatePort() + ",localhost:" + PortAuthority.allocatePort();
			  Config config = config();
			  config.augment( initial_discovery_members, initialHosts );

			  // Setup SslPolicy
			  config.augment( Neo4Net_home.name(), _home.AbsolutePath );
			  config.Augment( policyConfig.BaseDirectory.name(), "certificates/default" );

			  SslPolicyLoader sslPolicyLoader = SslPolicyLoader.create( config, NullLogProvider.Instance );

			  RemoteMembersResolver remoteMembersResolver = ResolutionResolverFactory.ChooseResolver( config, platformModule.Logging );

			  // then
			  SslPolicy sslPolicy = sslPolicyLoader.GetPolicy( "default" );
			  Monitors monitors = new Monitors();
			  SecureHazelcastCoreTopologyService service = new SecureHazelcastCoreTopologyService( config, sslPolicy, new MemberId( System.Guid.randomUUID() ), jobScheduler, NullLogProvider.Instance, NullLogProvider.Instance, remoteMembersResolver, new TopologyServiceNoRetriesStrategy(), monitors );
			  try
			  {
					service.Init();
					service.Start();
					service.Stop();
			  }
			  catch ( Exception )
			  {
					fail( "Caught an Exception" );
			  }
		 }
	}

}