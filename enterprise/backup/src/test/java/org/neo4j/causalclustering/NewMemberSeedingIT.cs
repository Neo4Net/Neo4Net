﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using BackupStore = Org.Neo4j.causalclustering.backup_stores.BackupStore;
	using BackupStoreWithSomeData = Org.Neo4j.causalclustering.backup_stores.BackupStoreWithSomeData;
	using BackupStoreWithSomeDataButNoTransactionLogs = Org.Neo4j.causalclustering.backup_stores.BackupStoreWithSomeDataButNoTransactionLogs;
	using EmptyBackupStore = Org.Neo4j.causalclustering.backup_stores.EmptyBackupStore;
	using ClusterLoad = Org.Neo4j.causalclustering.cluster_load.ClusterLoad;
	using NoLoad = Org.Neo4j.causalclustering.cluster_load.NoLoad;
	using SmallBurst = Org.Neo4j.causalclustering.cluster_load.SmallBurst;
	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using EnterpriseCluster = Org.Neo4j.causalclustering.discovery.EnterpriseCluster;
	using IpFamily = Org.Neo4j.causalclustering.discovery.IpFamily;
	using SharedDiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.SharedDiscoveryServiceFactory;
	using Standard = Org.Neo4j.Kernel.impl.store.format.standard.Standard;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.BackupUtil.restoreFromBackup;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.Cluster.dataMatchesEventually;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class NewMemberSeedingIT
	public class NewMemberSeedingIT
	{
		private bool InstanceFieldsInitialized = false;

		public NewMemberSeedingIT()
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
//ORIGINAL LINE: @Parameterized.Parameter() public org.neo4j.causalclustering.backup_stores.BackupStore seedStore;
		 public BackupStore SeedStore;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public org.neo4j.causalclustering.cluster_load.ClusterLoad intermediateLoad;
		 public ClusterLoad IntermediateLoad;

		 private SuppressOutput _suppressOutput = SuppressOutput.suppressAll();
		 private TestDirectory _testDir = TestDirectory.testDirectory();
		 private DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(testDir).around(suppressOutput);
		 public RuleChain Rules;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;
		 private FileCopyDetector _fileCopyDetector;
		 private File _baseBackupDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0} with {1}") public static Iterable<Object[]> data()
		 public static IEnumerable<object[]> Data()
		 {
			  return Combine( Stores(), Loads() );
		 }

		 private static IEnumerable<object[]> Combine( IEnumerable<BackupStore> stores, IEnumerable<ClusterLoad> loads )
		 {
			  List<object[]> @params = new List<object[]>();
			  foreach ( BackupStore store in stores )
			  {
					foreach ( ClusterLoad load in loads )
					{
						 @params.Add( new object[]{ store, load } );
					}
			  }
			  return @params;
		 }

		 private static IEnumerable<ClusterLoad> Loads()
		 {
			  return Arrays.asList( new NoLoad(), new SmallBurst() );
		 }

		 private static IEnumerable<BackupStore> Stores()
		 {
			  return Arrays.asList( new EmptyBackupStore(), new BackupStoreWithSomeData(), new BackupStoreWithSomeDataButNoTransactionLogs() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  this._fileCopyDetector = new FileCopyDetector();
			  _cluster = new EnterpriseCluster( _testDir.directory( "cluster-b" ), 3, 0, new SharedDiscoveryServiceFactory(), emptyMap(), emptyMap(), emptyMap(), emptyMap(), Standard.LATEST_NAME, IpFamily.IPV4, false );
			  _baseBackupDir = _testDir.directory( "backups" );
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
//ORIGINAL LINE: @Test public void shouldSeedNewMemberToCluster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeedNewMemberToCluster()
		 {
			  // given
			  _cluster.start();

			  // when
			  Optional<File> backup = SeedStore.generate( _baseBackupDir, _cluster );

			  // then
			  // possibly add load to cluster in between backup
			  IntermediateLoad.start( _cluster );

			  // when
			  CoreClusterMember newCoreClusterMember = _cluster.addCoreMemberWithId( 3 );
			  if ( backup.Present )
			  {
					restoreFromBackup( backup.get(), _fileSystemRule.get(), newCoreClusterMember );
			  }

			  // we want the new instance to seed from backup and not delete and re-download the store
			  newCoreClusterMember.Monitors().addMonitorListener(_fileCopyDetector);
			  newCoreClusterMember.Start();

			  // then
			  IntermediateLoad.stop();
			  dataMatchesEventually( newCoreClusterMember, _cluster.coreMembers() );
			  assertFalse( _fileCopyDetector.hasDetectedAnyFileCopied() );
		 }
	}

}