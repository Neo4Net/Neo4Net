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
namespace Neo4Net.causalclustering.scenarios
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Neo4Net.causalclustering.discovery;
	using EnterpriseCluster = Neo4Net.causalclustering.discovery.EnterpriseCluster;
	using IpFamily = Neo4Net.causalclustering.discovery.IpFamily;
	using SharedDiscoveryServiceFactory = Neo4Net.causalclustering.discovery.SharedDiscoveryServiceFactory;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using GraphDatabaseFactory = Neo4Net.GraphDb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using HighLimit = Neo4Net.Kernel.impl.store.format.highlimit.HighLimit;
	using DbRepresentation = Neo4Net.Test.DbRepresentation;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.discovery.Cluster.dataMatchesEventually;

	public class ClusterCommunityToEnterpriseIT
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private Neo4Net.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;
		 private FileSystemAbstraction _fsa;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.TestDirectory testDir = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDir = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.fs.DefaultFileSystemRule fileSystemRule = new Neo4Net.test.rule.fs.DefaultFileSystemRule();
		 public DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _fsa = FileSystemRule.get();

			  _cluster = new EnterpriseCluster( TestDir.directory( "cluster" ), 3, 0, new SharedDiscoveryServiceFactory(), emptyMap(), emptyMap(), emptyMap(), emptyMap(), HighLimit.NAME, IpFamily.IPV4, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  if ( _cluster != null )
			  {
					_cluster.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRestoreBySeedingAllMembers() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRestoreBySeedingAllMembers()
		 {
			  // given
			  IGraphDatabaseService database = ( new GraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDir.storeDir()).setConfig(GraphDatabaseSettings.allow_upgrade, Settings.TRUE).setConfig(GraphDatabaseSettings.record_format, HighLimit.NAME).setConfig(OnlineBackupSettings.online_backup_enabled, false.ToString()).newGraphDatabase();
			  database.Shutdown();
			  Config config = Config.defaults( OnlineBackupSettings.online_backup_enabled, Settings.FALSE );
			  DbRepresentation before = DbRepresentation.of( TestDir.storeDir(), config );

			  // when
			  _fsa.copyRecursively( TestDir.databaseDir(), _cluster.getCoreMemberById(0).databaseDirectory() );
			  _fsa.copyRecursively( TestDir.databaseDir(), _cluster.getCoreMemberById(1).databaseDirectory() );
			  _fsa.copyRecursively( TestDir.databaseDir(), _cluster.getCoreMemberById(2).databaseDirectory() );
			  _cluster.start();

			  // then
			  dataMatchesEventually( before, _cluster.coreMembers() );
		 }
	}

}