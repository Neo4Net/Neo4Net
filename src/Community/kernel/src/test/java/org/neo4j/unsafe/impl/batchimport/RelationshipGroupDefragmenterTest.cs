using System.Collections;
using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.impl.store;
	using RelationshipGroupStore = Neo4Net.Kernel.impl.store.RelationshipGroupStore;
	using ForcedSecondaryUnitRecordFormats = Neo4Net.Kernel.impl.store.format.ForcedSecondaryUnitRecordFormats;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using NullLogService = Neo4Net.Logging.@internal.NullLogService;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using Monitor = Neo4Net.@unsafe.Impl.Batchimport.RelationshipGroupDefragmenter.Monitor;
	using ExecutionMonitors = Neo4Net.@unsafe.Impl.Batchimport.staging.ExecutionMonitors;
	using BatchingNeoStores = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingNeoStores;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.atLeast;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.atMost;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.CHECK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.AUTO_WITHOUT_PAGECACHE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class RelationshipGroupDefragmenterTest
	public class RelationshipGroupDefragmenterTest
	{
		private bool InstanceFieldsInitialized = false;

		public RelationshipGroupDefragmenterTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _directory ).around( _random ).around( _fileSystemRule );
		}

		 private static readonly Configuration _config = Configuration.DEFAULT;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters public static java.util.Collection<Object[]> formats()
		 public static ICollection<object[]> Formats()
		 {
			  return asList( new object[] { Standard.LATEST_RECORD_FORMATS, 1 }, new object[] { new ForcedSecondaryUnitRecordFormats( Standard.LATEST_RECORD_FORMATS ), 2 } );
		 }

		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly RandomRule _random = new RandomRule();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(directory).around(random).around(fileSystemRule);
		 public RuleChain RuleChain;

		 [Parameter(0)]
		 public RecordFormats Format;
		 [Parameter(1)]
		 public int Units;

		 private BatchingNeoStores _stores;
		 private JobScheduler _jobScheduler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void start() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Start()
		 {
			  _jobScheduler = new ThreadPoolJobScheduler();
			  _stores = BatchingNeoStores.batchingNeoStores( _fileSystemRule.get(), _directory.absolutePath(), Format, _config, NullLogService.Instance, AdditionalInitialIds.EMPTY, Config.defaults(), _jobScheduler );
			  _stores.createNew();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void stop() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Stop()
		 {
			  _stores.close();
			  _jobScheduler.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDefragmentRelationshipGroupsWhenAllDense()
		 public virtual void ShouldDefragmentRelationshipGroupsWhenAllDense()
		 {
			  // GIVEN some nodes which has their groups scattered
			  int nodeCount = 100;
			  int relationshipTypeCount = 50;
			  RecordStore<RelationshipGroupRecord> groupStore = _stores.TemporaryRelationshipGroupStore;
			  RelationshipGroupRecord groupRecord = groupStore.NewRecord();
			  RecordStore<NodeRecord> nodeStore = _stores.NodeStore;
			  NodeRecord nodeRecord = nodeStore.NewRecord();
			  long cursor = 0;
			  for ( int typeId = relationshipTypeCount - 1; typeId >= 0; typeId-- )
			  {
					for ( long nodeId = 0; nodeId < nodeCount; nodeId++, cursor++ )
					{
						 // next doesn't matter at all, as we're rewriting it anyway
						 // firstOut/In/Loop we could use in verification phase later
						 groupRecord.Initialize( true, typeId, cursor, cursor + 1, cursor + 2, nodeId, 4 );
						 groupRecord.Id = groupStore.nextId();
						 groupStore.UpdateRecord( groupRecord );

						 if ( typeId == 0 )
						 {
							  // first round also create the nodes
							  nodeRecord.Initialize( true, -1, true, groupRecord.Id, 0 );
							  nodeRecord.Id = nodeId;
							  nodeStore.UpdateRecord( nodeRecord );
							  nodeStore.HighestPossibleIdInUse = nodeId;
						 }
					}
			  }

			  // WHEN
			  Defrag( nodeCount, groupStore );

			  // THEN all groups should sit sequentially in the store
			  VerifyGroupsAreSequentiallyOrderedByNode();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDefragmentRelationshipGroupsWhenSomeDense()
		 public virtual void ShouldDefragmentRelationshipGroupsWhenSomeDense()
		 {
			  // GIVEN some nodes which has their groups scattered
			  int nodeCount = 100;
			  int relationshipTypeCount = 50;
			  RecordStore<RelationshipGroupRecord> groupStore = _stores.TemporaryRelationshipGroupStore;
			  RelationshipGroupRecord groupRecord = groupStore.NewRecord();
			  RecordStore<NodeRecord> nodeStore = _stores.NodeStore;
			  NodeRecord nodeRecord = nodeStore.NewRecord();
			  long cursor = 0;
			  BitArray initializedNodes = new BitArray();
			  for ( int typeId = relationshipTypeCount - 1; typeId >= 0; typeId-- )
			  {
					for ( int nodeId = 0; nodeId < nodeCount; nodeId++, cursor++ )
					{
						 // Reasoning behind this thing is that we want to have roughly 10% of the nodes dense
						 // right from the beginning and then some stray dense nodes coming into this in the
						 // middle of the type range somewhere
						 double comparison = typeId == 0 || initializedNodes.Get( nodeId ) ? 0.1 : 0.001;

						 if ( _random.NextDouble() < comparison )
						 {
							  // next doesn't matter at all, as we're rewriting it anyway
							  // firstOut/In/Loop we could use in verification phase later
							  groupRecord.Initialize( true, typeId, cursor, cursor + 1, cursor + 2, nodeId, 4 );
							  groupRecord.Id = groupStore.nextId();
							  groupStore.UpdateRecord( groupRecord );

							  if ( !initializedNodes.Get( nodeId ) )
							  {
									nodeRecord.Initialize( true, -1, true, groupRecord.Id, 0 );
									nodeRecord.Id = nodeId;
									nodeStore.UpdateRecord( nodeRecord );
									nodeStore.HighestPossibleIdInUse = nodeId;
									initializedNodes.Set( nodeId, true );
							  }
						 }
					}
			  }

			  // WHEN
			  Defrag( nodeCount, groupStore );

			  // THEN all groups should sit sequentially in the store
			  VerifyGroupsAreSequentiallyOrderedByNode();
		 }

		 private void Defrag( int nodeCount, RecordStore<RelationshipGroupRecord> groupStore )
		 {
			  Monitor monitor = mock( typeof( Monitor ) );
			  RelationshipGroupDefragmenter defragmenter = new RelationshipGroupDefragmenter( _config, ExecutionMonitors.invisible(), monitor, AUTO_WITHOUT_PAGECACHE );

			  // Calculation below correlates somewhat to calculation in RelationshipGroupDefragmenter.
			  // Anyway we verify below that we exercise the multi-pass bit, which is what we want
			  long memory = groupStore.HighId * 15 + 200;
			  defragmenter.Run( memory, _stores, nodeCount );

			  // Verify that we exercise the multi-pass functionality
			  verify( monitor, atLeast( 2 ) ).defragmentingNodeRange( anyLong(), anyLong() );
			  verify( monitor, atMost( 10 ) ).defragmentingNodeRange( anyLong(), anyLong() );
		 }

		 private void VerifyGroupsAreSequentiallyOrderedByNode()
		 {
			  RelationshipGroupStore store = _stores.RelationshipGroupStore;
			  long firstId = store.NumberOfReservedLowIds;
			  long groupCount = store.HighId - firstId;
			  RelationshipGroupRecord groupRecord = store.NewRecord();
			  PageCursor groupCursor = store.OpenPageCursorForReading( firstId );
			  long highGroupId = store.HighId;
			  long currentNodeId = -1;
			  int currentTypeId = -1;
			  int newGroupCount = 0;
			  int currentGroupLength = 0;
			  for ( long id = firstId; id < highGroupId; id++, newGroupCount++ )
			  {
					store.GetRecordByCursor( id, groupRecord, CHECK, groupCursor );
					if ( !groupRecord.InUse() )
					{
						 // This will be the case if we have double record units, just assert that fact
						 assertTrue( Units > 1 );
						 assertTrue( currentGroupLength > 0 );
						 currentGroupLength--;
						 continue;
					}

					long nodeId = groupRecord.OwningNode;
					assertTrue( "Expected a group for node >= " + currentNodeId + ", but was " + nodeId + " in " + groupRecord, nodeId >= currentNodeId );
					if ( nodeId != currentNodeId )
					{
						 currentNodeId = nodeId;
						 currentTypeId = -1;
						 if ( Units > 1 )
						 {
							  assertEquals( 0, currentGroupLength );
						 }
						 currentGroupLength = 0;
					}
					currentGroupLength++;

					assertTrue( "Expected this group to have a next of current + " + Units + " OR NULL, " + "but was " + groupRecord.ToString(), groupRecord.Next == groupRecord.Id + 1 || groupRecord.Next == Record.NO_NEXT_RELATIONSHIP.intValue() );
					assertTrue( "Expected " + groupRecord + " to have type > " + currentTypeId, groupRecord.Type > currentTypeId );
					currentTypeId = groupRecord.Type;
			  }
			  assertEquals( groupCount, newGroupCount );
		 }
	}

}