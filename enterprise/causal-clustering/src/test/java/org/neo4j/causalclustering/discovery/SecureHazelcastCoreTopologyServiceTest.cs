﻿using System;
using System.Collections.Generic;

/*
 *
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
 * Reference: https://raw.githubusercontent.com/neo4j/neo4j/3.4/enterprise/causal-clustering/src/test/java/org/neo4j/causalclustering/discovery/HazelcastCoreTopologyServiceTest.java
 */
namespace Org.Neo4j.causalclustering.discovery
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Org.Neo4j.Graphdb.factory.module.edition.AbstractEditionModule;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using SslPolicyConfig = Org.Neo4j.Kernel.configuration.ssl.SslPolicyConfig;
	using SslPolicyLoader = Org.Neo4j.Kernel.configuration.ssl.SslPolicyLoader;
	using EnterpriseEditionModule = Org.Neo4j.Kernel.impl.enterprise.EnterpriseEditionModule;
	using JobSchedulerFactory = Org.Neo4j.Kernel.impl.scheduler.JobSchedulerFactory;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PortAuthority = Org.Neo4j.Ports.Allocation.PortAuthority;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using PkiUtils = Org.Neo4j.Ssl.PkiUtils;
	using SslPolicy = Org.Neo4j.Ssl.SslPolicy;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.initial_discovery_members;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.facade.GraphDatabaseDependencies.newDependencies;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.neo4j_home;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.factory.DatabaseInfo.ENTERPRISE;

	public class SecureHazelcastCoreTopologyServiceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
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

			  @params[neo4j_home.name()] = _home.AbsolutePath;
			  @params[policyConfig.BaseDirectory.name()] = "certificates/default";

			  JobScheduler jobScheduler = JobSchedulerFactory.createInitialisedScheduler();
			  PlatformModule platformModule = new PlatformModule( TestDirectory.storeDir(), Config.defaults(), ENTERPRISE, newDependencies() );
			  AbstractEditionModule editionModule = new EnterpriseEditionModule( platformModule );
			  // Random members that does not exists, discovery will never succeed
			  string initialHosts = "localhost:" + PortAuthority.allocatePort() + ",localhost:" + PortAuthority.allocatePort();
			  Config config = config();
			  config.augment( initial_discovery_members, initialHosts );

			  // Setup SslPolicy
			  config.augment( neo4j_home.name(), _home.AbsolutePath );
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