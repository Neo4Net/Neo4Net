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
namespace Neo4Net.causalclustering
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using BackupStore = Neo4Net.causalclustering.backup_stores.BackupStore;
	using BackupStoreWithSomeData = Neo4Net.causalclustering.backup_stores.BackupStoreWithSomeData;
	using BackupStoreWithSomeDataButNoTransactionLogs = Neo4Net.causalclustering.backup_stores.BackupStoreWithSomeDataButNoTransactionLogs;
	using EmptyBackupStore = Neo4Net.causalclustering.backup_stores.EmptyBackupStore;
	using NoStore = Neo4Net.causalclustering.backup_stores.NoStore;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using EnterpriseCluster = Neo4Net.causalclustering.discovery.EnterpriseCluster;
	using IpFamily = Neo4Net.causalclustering.discovery.IpFamily;
	using SharedDiscoveryServiceFactory = Neo4Net.causalclustering.discovery.SharedDiscoveryServiceFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using DbRepresentation = Neo4Net.Test.DbRepresentation;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.BackupUtil.restoreFromBackup;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.Cluster.dataMatchesEventually;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ClusterSeedingIT
	public class ClusterSeedingIT
	{
		private bool InstanceFieldsInitialized = false;

		public ClusterSeedingIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( _fileSystemRule ).around( _testDir ).around( _suppressOutput );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public org.neo4j.causalclustering.backup_stores.BackupStore initialStore;
		 public BackupStore InitialStore;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public boolean shouldStoreCopy;
		 public bool ShouldStoreCopy;

		 private SuppressOutput _suppressOutput = SuppressOutput.suppressAll();
		 private TestDirectory _testDir = TestDirectory.testDirectory();
		 private DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(testDir).around(suppressOutput);
		 public RuleChain Rules;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> backupCluster;
		 private Cluster<object> _backupCluster;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;
		 private FileCopyDetector _fileCopyDetector;
		 private File _baseBackupDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Object[][] data()
		 public static object[][] Data()
		 {
			  return new object[][]
			  {
				  new object[]
				  {
					  new NoStore(),
					  true
				  },
				  new object[]
				  {
					  new EmptyBackupStore(),
					  false
				  },
				  new object[]
				  {
					  new BackupStoreWithSomeData(),
					  false
				  },
				  new object[]
				  {
					  new BackupStoreWithSomeDataButNoTransactionLogs(),
					  false
				  }
			  };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  this._fileCopyDetector = new FileCopyDetector();
			  _backupCluster = new EnterpriseCluster( _testDir.directory( "cluster-for-backup" ), 3, 0, new SharedDiscoveryServiceFactory(), emptyMap(), emptyMap(), emptyMap(), emptyMap(), Standard.LATEST_NAME, IpFamily.IPV4, false );

			  _cluster = new EnterpriseCluster( _testDir.directory( "cluster-b" ), 3, 0, new SharedDiscoveryServiceFactory(), emptyMap(), emptyMap(), emptyMap(), emptyMap(), Standard.LATEST_NAME, IpFamily.IPV4, false );

			  _baseBackupDir = _testDir.directory( "backups" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  if ( _backupCluster != null )
			  {
					_backupCluster.shutdown();
			  }
			  if ( _cluster != null )
			  {
					_cluster.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeedNewCluster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeedNewCluster()
		 {
			  // given
			  _backupCluster.start();
			  Optional<File> backup = InitialStore.generate( _baseBackupDir, _backupCluster );
			  _backupCluster.shutdown();

			  if ( backup.Present )
			  {
					foreach ( CoreClusterMember coreClusterMember in _cluster.coreMembers() )
					{
						 restoreFromBackup( backup.get(), _fileSystemRule.get(), coreClusterMember );
					}
			  }

			  // we want the cluster to seed from backup. No instance should delete and re-copy the store.
			  _cluster.coreMembers().forEach(ccm => ccm.monitors().addMonitorListener(_fileCopyDetector));

			  // when
			  _cluster.start();

			  // then
			  if ( backup.Present )
			  {
					Config config = Config.defaults( GraphDatabaseSettings.active_database, backup.get().Name );
					dataMatchesEventually( DbRepresentation.of( backup.get(), config ), _cluster.coreMembers() );
			  }
			  assertEquals( ShouldStoreCopy, _fileCopyDetector.hasDetectedAnyFileCopied() );
		 }
	}

}