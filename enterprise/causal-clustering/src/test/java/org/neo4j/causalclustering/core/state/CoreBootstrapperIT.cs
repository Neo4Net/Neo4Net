using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.core.state
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using GlobalSessionTrackerState = Org.Neo4j.causalclustering.core.replication.session.GlobalSessionTrackerState;
	using IdAllocationState = Org.Neo4j.causalclustering.core.state.machines.id.IdAllocationState;
	using ReplicatedLockTokenState = Org.Neo4j.causalclustering.core.state.machines.locks.ReplicatedLockTokenState;
	using LastCommittedIndexFinder = Org.Neo4j.causalclustering.core.state.machines.tx.LastCommittedIndexFinder;
	using CoreSnapshot = Org.Neo4j.causalclustering.core.state.snapshot.CoreSnapshot;
	using CoreStateType = Org.Neo4j.causalclustering.core.state.snapshot.CoreStateType;
	using RaftCoreState = Org.Neo4j.causalclustering.core.state.snapshot.RaftCoreState;
	using ClassicNeo4jStore = Org.Neo4j.causalclustering.helpers.ClassicNeo4jStore;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using ReadOnlyTransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.ReadOnlyTransactionIdStore;
	using ReadOnlyTransactionStore = Org.Neo4j.Kernel.impl.transaction.log.ReadOnlyTransactionStore;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.record_id_batch_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class CoreBootstrapperIT
	{
		private bool InstanceFieldsInitialized = false;

		public CoreBootstrapperIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _pageCacheRule ).around( _pageCacheRule ).around( _testDirectory );
		}

		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(pageCacheRule).around(pageCacheRule).around(testDirectory);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetAllCoreState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetAllCoreState()
		 {
			  // given
			  int nodeCount = 100;
			  FileSystemAbstraction fileSystem = _fileSystemRule.get();
			  File classicNeo4jStore = ClassicNeo4jStore.builder( _testDirectory.directory(), fileSystem ).amountOfNodes(nodeCount).build().StoreDir;

			  PageCache pageCache = _pageCacheRule.getPageCache( fileSystem );
			  DatabaseLayout databaseLayout = DatabaseLayout.of( classicNeo4jStore );
			  CoreBootstrapper bootstrapper = new CoreBootstrapper( databaseLayout, pageCache, fileSystem, Config.defaults(), NullLogProvider.Instance, new Monitors() );
			  BootstrapAndVerify( nodeCount, fileSystem, databaseLayout, pageCache, Config.defaults(), bootstrapper );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setAllCoreStateOnDatabaseWithCustomLogFilesLocation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetAllCoreStateOnDatabaseWithCustomLogFilesLocation()
		 {
			  // given
			  int nodeCount = 100;
			  FileSystemAbstraction fileSystem = _fileSystemRule.get();
			  string customTransactionLogsLocation = "transaction-logs";
			  File classicNeo4jStore = ClassicNeo4jStore.builder( _testDirectory.directory(), fileSystem ).amountOfNodes(nodeCount).logicalLogsLocation(customTransactionLogsLocation).build().StoreDir;

			  PageCache pageCache = _pageCacheRule.getPageCache( fileSystem );
			  DatabaseLayout databaseLayout = DatabaseLayout.of( classicNeo4jStore );
			  Config config = Config.defaults( GraphDatabaseSettings.logical_logs_location, customTransactionLogsLocation );
			  CoreBootstrapper bootstrapper = new CoreBootstrapper( databaseLayout, pageCache, fileSystem, config, NullLogProvider.Instance, new Monitors() );

			  BootstrapAndVerify( nodeCount, fileSystem, databaseLayout, pageCache, config, bootstrapper );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToBootstrapIfClusterIsInNeedOfRecovery() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToBootstrapIfClusterIsInNeedOfRecovery()
		 {
			  // given
			  int nodeCount = 100;
			  FileSystemAbstraction fileSystem = _fileSystemRule.get();
			  File storeInNeedOfRecovery = ClassicNeo4jStore.builder( _testDirectory.directory(), fileSystem ).amountOfNodes(nodeCount).needToRecover().build().StoreDir;
			  AssertableLogProvider assertableLogProvider = new AssertableLogProvider();

			  PageCache pageCache = _pageCacheRule.getPageCache( fileSystem );
			  DatabaseLayout databaseLayout = DatabaseLayout.of( storeInNeedOfRecovery );
			  CoreBootstrapper bootstrapper = new CoreBootstrapper( databaseLayout, pageCache, fileSystem, Config.defaults(), assertableLogProvider, new Monitors() );

			  // when
			  ISet<MemberId> membership = asSet( RandomMember(), RandomMember(), RandomMember() );
			  try
			  {
					bootstrapper.Bootstrap( membership );
					fail();
			  }
			  catch ( Exception e )
			  {
					string errorMessage = "Cannot bootstrap. Recovery is required. Please ensure that the store being seeded comes from a cleanly shutdown " +
							  "instance of Neo4j or a Neo4j backup";
					assertEquals( e.Message, errorMessage );
					assertableLogProvider.AssertExactly( AssertableLogProvider.inLog( typeof( CoreBootstrapper ) ).error( errorMessage ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToBootstrapIfClusterIsInNeedOfRecoveryWithCustomLogicalLogsLocation() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToBootstrapIfClusterIsInNeedOfRecoveryWithCustomLogicalLogsLocation()
		 {
			  // given
			  int nodeCount = 100;
			  FileSystemAbstraction fileSystem = _fileSystemRule.get();
			  string customTransactionLogsLocation = "transaction-logs";
			  File storeInNeedOfRecovery = ClassicNeo4jStore.builder( _testDirectory.directory(), fileSystem ).amountOfNodes(nodeCount).logicalLogsLocation(customTransactionLogsLocation).needToRecover().build().StoreDir;
			  AssertableLogProvider assertableLogProvider = new AssertableLogProvider();

			  PageCache pageCache = _pageCacheRule.getPageCache( fileSystem );
			  DatabaseLayout databaseLayout = DatabaseLayout.of( storeInNeedOfRecovery );
			  Config config = Config.defaults( GraphDatabaseSettings.logical_logs_location, customTransactionLogsLocation );
			  CoreBootstrapper bootstrapper = new CoreBootstrapper( databaseLayout, pageCache, fileSystem, config, assertableLogProvider, new Monitors() );

			  // when
			  ISet<MemberId> membership = asSet( RandomMember(), RandomMember(), RandomMember() );
			  try
			  {
					bootstrapper.Bootstrap( membership );
					fail();
			  }
			  catch ( Exception e )
			  {
					string errorMessage = "Cannot bootstrap. Recovery is required. Please ensure that the store being seeded comes from a cleanly shutdown " +
							  "instance of Neo4j or a Neo4j backup";
					assertEquals( e.Message, errorMessage );
					assertableLogProvider.AssertExactly( AssertableLogProvider.inLog( typeof( CoreBootstrapper ) ).error( errorMessage ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void bootstrapAndVerify(long nodeCount, org.neo4j.io.fs.FileSystemAbstraction fileSystem, org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.kernel.configuration.Config config, CoreBootstrapper bootstrapper) throws Exception
		 private static void BootstrapAndVerify( long nodeCount, FileSystemAbstraction fileSystem, DatabaseLayout databaseLayout, PageCache pageCache, Config config, CoreBootstrapper bootstrapper )
		 {
			  // when
			  ISet<MemberId> membership = asSet( RandomMember(), RandomMember(), RandomMember() );
			  CoreSnapshot snapshot = bootstrapper.Bootstrap( membership );

			  // then
			  int recordIdBatchSize = parseInt( record_id_batch_size.DefaultValue );
			  assertThat( ( ( IdAllocationState ) snapshot.Get( CoreStateType.ID_ALLOCATION ) ).firstUnallocated( IdType.NODE ), allOf( greaterThanOrEqualTo( nodeCount ), lessThanOrEqualTo( nodeCount + recordIdBatchSize ) ) );

			  /* Bootstrapped state is created in RAFT land at index -1 and term -1. */
			  assertEquals( 0, snapshot.PrevIndex() );
			  assertEquals( 0, snapshot.PrevTerm() );

			  /* Lock is initially not taken. */
			  assertEquals( new ReplicatedLockTokenState(), snapshot.Get(CoreStateType.LOCK_TOKEN) );

			  /* Raft has the bootstrapped set of members initially. */
			  assertEquals( membership, ( ( RaftCoreState ) snapshot.Get( CoreStateType.RAFT_CORE_STATE ) ).committed().members() );

			  /* The session state is initially empty. */
			  assertEquals( new GlobalSessionTrackerState(), snapshot.Get(CoreStateType.SESSION_TRACKER) );

			  ReadOnlyTransactionStore transactionStore = new ReadOnlyTransactionStore( pageCache, fileSystem, databaseLayout, config, new Monitors() );
			  LastCommittedIndexFinder lastCommittedIndexFinder = new LastCommittedIndexFinder( new ReadOnlyTransactionIdStore( pageCache, databaseLayout ), transactionStore, NullLogProvider.Instance );

			  long lastCommittedIndex = lastCommittedIndexFinder.LastCommittedIndex;
			  assertEquals( -1, lastCommittedIndex );
		 }

		 private static MemberId RandomMember()
		 {
			  return new MemberId( randomUUID() );
		 }
	}

}