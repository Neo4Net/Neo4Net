using System;
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
namespace Neo4Net.Kernel.api.impl.labelscan
{
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using Matcher = org.hamcrest.Matcher;
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;


	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using Neo4Net.GraphDb;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Neo4Net.Helpers.Collections;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using Neo4Net.Helpers.Collections;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using AllEntriesLabelScanReader = Neo4Net.Kernel.api.labelscan.AllEntriesLabelScanReader;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using LabelScanWriter = Neo4Net.Kernel.api.labelscan.LabelScanWriter;
	using NodeLabelRange = Neo4Net.Kernel.api.labelscan.NodeLabelRange;
	using NodeLabelUpdate = Neo4Net.Kernel.api.labelscan.NodeLabelUpdate;
	using FullStoreChangeStream = Neo4Net.Kernel.Impl.Api.scan.FullStoreChangeStream;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using LifecycleException = Neo4Net.Kernel.Lifecycle.LifecycleException;
	using LabelScanReader = Neo4Net.Kernel.Api.StorageEngine.schema.LabelScanReader;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.iterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.labelscan.NodeLabelUpdate.labelChanges;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.scan.FullStoreChangeStream.asStream;

	public abstract class LabelScanStoreTest
	{
		private bool InstanceFieldsInitialized = false;

		public LabelScanStoreTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( Random ).around( TestDirectory ).around( _expectedException ).around( FileSystemRule );
		}

		 protected internal readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private readonly ExpectedException _expectedException = ExpectedException.none();
		 protected internal readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();
		 internal readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(random).around(testDirectory).around(expectedException).around(fileSystemRule);
		 public RuleChain RuleChain;

		 private static readonly long[] _noLabels = new long[0];

		 private LifeSupport _life;
		 private TrackingMonitor _monitor;
		 private LabelScanStore _store;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutdown()
		 public virtual void Shutdown()
		 {
			  if ( _life != null )
			  {
					_life.shutdown();
			  }
		 }

		 protected internal abstract LabelScanStore CreateLabelScanStore( FileSystemAbstraction fileSystemAbstraction, DatabaseLayout databaseLayout, FullStoreChangeStream fullStoreChangeStream, bool usePersistentStore, bool readOnly, Neo4Net.Kernel.api.labelscan.LabelScanStore_Monitor monitor );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failToRetrieveWriterOnReadOnlyScanStore()
		 public virtual void FailToRetrieveWriterOnReadOnlyScanStore()
		 {
			  CreateAndStartReadOnly();
			  _expectedException.expect( typeof( System.NotSupportedException ) );
			  _store.newWriter();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void forceShouldNotCheckpointTreeOnReadOnlyScanStore()
		 public virtual void ForceShouldNotCheckpointTreeOnReadOnlyScanStore()
		 {
			  MutableBoolean ioLimiterCalled = new MutableBoolean();
			  CreateAndStartReadOnly();
			  _store.force((previousStamp, recentlyCompletedIOs, flushable) =>
			  {
			  ioLimiterCalled.setTrue();
			  return 0;
			  });
			  assertFalse( ioLimiterCalled.Value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotStartIfLabelScanStoreIndexDoesNotExistInReadOnlyMode()
		 public virtual void ShouldNotStartIfLabelScanStoreIndexDoesNotExistInReadOnlyMode()
		 {
			  try
			  {
					// WHEN
					Start( false, true );
					fail( "Should have failed" );
			  }
			  catch ( LifecycleException e )
			  {
					// THEN
					Exception rootCause = Exceptions.rootCause( e );
					assertTrue( rootCause is NoSuchFileException );
					assertTrue( e.Message.contains( "Cannot map non-existing file" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void snapshotReadOnlyLabelScanStore() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SnapshotReadOnlyLabelScanStore()
		 {
			  PrepareIndex();
			  CreateAndStartReadOnly();
			  using ( ResourceIterator<File> indexFiles = _store.snapshotStoreFiles() )
			  {
					IList<File> files = Iterators.asList( indexFiles );
					assertThat( "Should have at least index segment file.", files, HasLabelScanStore() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: protected abstract org.hamcrest.Matcher<Iterable<? super java.io.File>> hasLabelScanStore();
		 protected internal abstract Matcher<System.Collections.IEnumerable> HasLabelScanStore();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateIndexOnLabelChange() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateIndexOnLabelChange()
		 {
			  // GIVEN
			  int labelId = 1;
			  long nodeId = 10;
			  Start();

			  // WHEN
			  Write( iterator( labelChanges( nodeId, _noLabels, new long[]{ labelId } ) ) );

			  // THEN
			  AssertNodesForLabel( labelId, nodeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateIndexOnAddedLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateIndexOnAddedLabels()
		 {
			  // GIVEN
			  int labelId1 = 1;
			  int labelId2 = 2;
			  long nodeId = 10;
			  Start();
			  Write( iterator( labelChanges( nodeId, _noLabels, new long[]{ labelId1 } ) ) );
			  AssertNodesForLabel( labelId2 );

			  // WHEN
			  Write( iterator( labelChanges( nodeId, _noLabels, new long[]{ labelId1, labelId2 } ) ) );

			  // THEN
			  AssertNodesForLabel( labelId1, nodeId );
			  AssertNodesForLabel( labelId2, nodeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateIndexOnRemovedLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateIndexOnRemovedLabels()
		 {
			  // GIVEN
			  int labelId1 = 1;
			  int labelId2 = 2;
			  long nodeId = 10;
			  Start();
			  Write( iterator( labelChanges( nodeId, _noLabels, new long[]{ labelId1, labelId2 } ) ) );
			  AssertNodesForLabel( labelId1, nodeId );
			  AssertNodesForLabel( labelId2, nodeId );

			  // WHEN
			  Write( iterator( labelChanges( nodeId, new long[]{ labelId1, labelId2 }, new long[]{ labelId2 } ) ) );

			  // THEN
			  AssertNodesForLabel( labelId1 );
			  AssertNodesForLabel( labelId2, nodeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteFromIndexWhenDeletedNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeleteFromIndexWhenDeletedNode()
		 {
			  // GIVEN
			  int labelId = 1;
			  long nodeId = 10;
			  Start();
			  Write( iterator( labelChanges( nodeId, _noLabels, new long[]{ labelId } ) ) );

			  // WHEN
			  Write( iterator( labelChanges( nodeId, new long[]{ labelId }, _noLabels ) ) );

			  // THEN
			  AssertNodesForLabel( labelId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldScanSingleRange()
		 public virtual void ShouldScanSingleRange()
		 {
			  // GIVEN
			  int labelId1 = 1;
			  int labelId2 = 2;
			  long nodeId1 = 10;
			  long nodeId2 = 11;
			  Start( new IList<NodeLabelUpdate> { labelChanges( nodeId1, _noLabels, new long[]{ labelId1 } ), labelChanges( nodeId2, _noLabels, new long[]{ labelId1, labelId2 } ) } );

			  // WHEN
			  BoundedIterable<NodeLabelRange> reader = _store.allNodeLabelRanges();
			  NodeLabelRange range = single( reader.GetEnumerator() );

			  // THEN
			  assertArrayEquals( new long[]{ nodeId1, nodeId2 }, ReducedNodes( range ) );

			  assertArrayEquals( new long[]{ labelId1 }, Sorted( range.Labels( nodeId1 ) ) );
			  assertArrayEquals( new long[]{ labelId1, labelId2 }, Sorted( range.Labels( nodeId2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldScanMultipleRanges()
		 public virtual void ShouldScanMultipleRanges()
		 {
			  // GIVEN
			  int labelId1 = 1;
			  int labelId2 = 2;
			  long nodeId1 = 10;
			  long nodeId2 = 1280;
			  Start( new IList<NodeLabelUpdate> { labelChanges( nodeId1, _noLabels, new long[]{ labelId1 } ), labelChanges( nodeId2, _noLabels, new long[]{ labelId1, labelId2 } ) } );

			  // WHEN
			  BoundedIterable<NodeLabelRange> reader = _store.allNodeLabelRanges();
			  IEnumerator<NodeLabelRange> iterator = reader.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  NodeLabelRange range1 = iterator.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  NodeLabelRange range2 = iterator.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );

			  // THEN
			  assertArrayEquals( new long[]{ nodeId1 }, ReducedNodes( range1 ) );
			  assertArrayEquals( new long[]{ nodeId2 }, ReducedNodes( range2 ) );

			  assertArrayEquals( new long[]{ labelId1 }, Sorted( range1.Labels( nodeId1 ) ) );

			  assertArrayEquals( new long[]{ labelId1, labelId2 }, Sorted( range2.Labels( nodeId2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithAFullRange()
		 public virtual void ShouldWorkWithAFullRange()
		 {
			  // given
			  long labelId = 0;
			  IList<NodeLabelUpdate> updates = new List<NodeLabelUpdate>();
			  ISet<long> nodes = new HashSet<long>();
			  for ( int i = 0; i < 34; i++ )
			  {
					updates.Add( NodeLabelUpdate.labelChanges( i, new long[]{}, new long[]{labelId} ) );
					nodes.Add( ( long ) i );
			  }

			  Start( updates );

			  // when
			  LabelScanReader reader = _store.newReader();
			  ISet<long> nodesWithLabel = PrimitiveLongCollections.toSet( reader.NodesWithLabel( ( int ) labelId ) );

			  // then
			  assertEquals( nodes, nodesWithLabel );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateAFullRange() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateAFullRange()
		 {
			  // given
			  long label0Id = 0;
			  IList<NodeLabelUpdate> label0Updates = new List<NodeLabelUpdate>();
			  ISet<long> nodes = new HashSet<long>();
			  for ( int i = 0; i < 34; i++ )
			  {
					label0Updates.Add( NodeLabelUpdate.labelChanges( i, new long[]{}, new long[]{label0Id} ) );
					nodes.Add( ( long ) i );
			  }

			  Start( label0Updates );

			  // when
			  Write( Collections.emptyIterator() );

			  // then
			  LabelScanReader reader = _store.newReader();
			  ISet<long> nodesWithLabel0 = PrimitiveLongCollections.toSet( reader.NodesWithLabel( ( int ) label0Id ) );
			  assertEquals( nodes, nodesWithLabel0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeEntriesWhenOnlyLowestIsPresent()
		 public virtual void ShouldSeeEntriesWhenOnlyLowestIsPresent()
		 {
			  // given
			  long labelId = 0;
			  IList<NodeLabelUpdate> labelUpdates = new List<NodeLabelUpdate>();
			  labelUpdates.Add( NodeLabelUpdate.labelChanges( 0L, new long[]{}, new long[]{labelId} ) );

			  Start( labelUpdates );

			  // when
			  MutableInt count = new MutableInt();
			  AllEntriesLabelScanReader nodeLabelRanges = _store.allNodeLabelRanges();
			  nodeLabelRanges.forEach(nlr =>
			  {
				foreach ( long nodeId in nlr.nodes() )
				{
					 count.add( nlr.labels( nodeId ).length );
				}
			  });
			  assertThat( count.intValue(), @is(1) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void write(java.util.Iterator<org.Neo4Net.kernel.api.labelscan.NodeLabelUpdate> iterator) throws java.io.IOException
		 private void Write( IEnumerator<NodeLabelUpdate> iterator )
		 {
			  using ( LabelScanWriter writer = _store.newWriter() )
			  {
					while ( iterator.MoveNext() )
					{
						 writer.Write( iterator.Current );
					}
			  }
		 }

		 private long[] Sorted( long[] input )
		 {
			  Arrays.sort( input );
			  return input;
		 }

		 private long[] ReducedNodes( NodeLabelRange range )
		 {
			  long[] nodes = range.Nodes();
			  long[] result = new long[nodes.Length];
			  int cursor = 0;
			  foreach ( long node in nodes )
			  {
					if ( range.Labels( node ).Length > 0 )
					{
						 result[cursor++] = node;
					}
			  }
			  return Arrays.copyOf( result, cursor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRebuildFromScratchIfIndexMissing()
		 public virtual void ShouldRebuildFromScratchIfIndexMissing()
		 {
			  // GIVEN a start of the store with existing data in it
			  Start( new IList<NodeLabelUpdate> { labelChanges( 1, _noLabels, new long[]{ 1 } ), labelChanges( 2, _noLabels, new long[]{ 1, 2 } ) } );

			  // THEN
			  assertTrue( "Didn't rebuild the store on startup", _monitor.noIndexCalled & _monitor.rebuildingCalled & _monitor.rebuiltCalled );
			  AssertNodesForLabel( 1, 1, 2 );
			  AssertNodesForLabel( 2, 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rebuildCorruptedIndexIndexOnStartup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RebuildCorruptedIndexIndexOnStartup()
		 {
			  // GIVEN a start of the store with existing data in it
			  IList<NodeLabelUpdate> data = new IList<NodeLabelUpdate> { labelChanges( 1, _noLabels, new long[]{ 1 } ), labelChanges( 2, _noLabels, new long[]{ 1, 2 } ) };
			  Start( data, true, false );

			  // WHEN the index is corrupted and then started again
			  ScrambleIndexFilesAndRestart( data, true, false );

			  assertTrue( "Index corruption should be detected", _monitor.corruptedIndex );
			  assertTrue( "Index should be rebuild", _monitor.rebuildingCalled );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindDecentAmountOfNodesForALabel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindDecentAmountOfNodesForALabel()
		 {
			  // GIVEN
			  // 16 is the magic number of the page iterator
			  // 32 is the number of nodes in each lucene document
			  const int labelId = 1;
			  int nodeCount = 32 * 16 + 10;
			  Start();
			  Write( new PrefetchingIteratorAnonymousInnerClass( this, labelId, nodeCount ) );

			  // WHEN
			  ISet<long> nodeSet = new SortedSet<long>();
			  LabelScanReader reader = _store.newReader();
			  LongIterator nodes = reader.NodesWithLabel( labelId );
			  while ( nodes.hasNext() )
			  {
					nodeSet.Add( nodes.next() );
			  }
			  reader.Close();

			  // THEN
			  assertEquals( "Found gaps in node id range: " + Gaps( nodeSet, nodeCount ), nodeCount, nodeSet.Count );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<NodeLabelUpdate>
		 {
			 private readonly LabelScanStoreTest _outerInstance;

			 private int _labelId;
			 private int _nodeCount;

			 public PrefetchingIteratorAnonymousInnerClass( LabelScanStoreTest outerInstance, int labelId, int nodeCount )
			 {
				 this.outerInstance = outerInstance;
				 this._labelId = labelId;
				 this._nodeCount = nodeCount;
				 i = -1;
			 }

			 private int i;

			 protected internal override NodeLabelUpdate fetchNextOrNull()
			 {
				  return ++i < _nodeCount ? labelChanges( i, _noLabels, new long[]{ _labelId } ) : null;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindNodesWithAnyOfGivenLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindNodesWithAnyOfGivenLabels()
		 {
			  // GIVEN
			  int labelId1 = 3;
			  int labelId2 = 5;
			  int labelId3 = 13;
			  Start();

			  // WHEN
			  Write( iterator( labelChanges( 2, EMPTY_LONG_ARRAY, new long[] { labelId1, labelId2 } ), labelChanges( 1, EMPTY_LONG_ARRAY, new long[] { labelId1 } ), labelChanges( 4, EMPTY_LONG_ARRAY, new long[] { labelId1, labelId3 } ), labelChanges( 5, EMPTY_LONG_ARRAY, new long[] { labelId1, labelId2, labelId3 } ), labelChanges( 3, EMPTY_LONG_ARRAY, new long[] { labelId1 } ), labelChanges( 7, EMPTY_LONG_ARRAY, new long[] { labelId2 } ), labelChanges( 8, EMPTY_LONG_ARRAY, new long[] { labelId3 } ), labelChanges( 6, EMPTY_LONG_ARRAY, new long[] { labelId2 } ), labelChanges( 9, EMPTY_LONG_ARRAY, new long[] { labelId3 } ) ) );

			  // THEN
			  using ( LabelScanReader reader = _store.newReader() )
			  {
					assertArrayEquals( new long[] { 1, 2, 3, 4, 5, 6, 7 }, PrimitiveLongCollections.asArray( reader.NodesWithAnyOfLabels( new int[] { labelId1, labelId2 } ) ) );
					assertArrayEquals( new long[] { 1, 2, 3, 4, 5, 8, 9 }, PrimitiveLongCollections.asArray( reader.NodesWithAnyOfLabels( new int[] { labelId1, labelId3 } ) ) );
					assertArrayEquals( new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, PrimitiveLongCollections.asArray( reader.NodesWithAnyOfLabels( new int[] { labelId1, labelId2, labelId3 } ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindNodesWithAllGivenLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindNodesWithAllGivenLabels()
		 {
			  // GIVEN
			  int labelId1 = 3;
			  int labelId2 = 5;
			  int labelId3 = 13;
			  Start();

			  // WHEN
			  Write( iterator( labelChanges( 5, EMPTY_LONG_ARRAY, new long[] { labelId1, labelId2, labelId3 } ), labelChanges( 8, EMPTY_LONG_ARRAY, new long[] { labelId3 } ), labelChanges( 3, EMPTY_LONG_ARRAY, new long[] { labelId1 } ), labelChanges( 6, EMPTY_LONG_ARRAY, new long[] { labelId2 } ), labelChanges( 1, EMPTY_LONG_ARRAY, new long[] { labelId1 } ), labelChanges( 7, EMPTY_LONG_ARRAY, new long[] { labelId2 } ), labelChanges( 4, EMPTY_LONG_ARRAY, new long[] { labelId1, labelId3 } ), labelChanges( 2, EMPTY_LONG_ARRAY, new long[] { labelId1, labelId2 } ), labelChanges( 9, EMPTY_LONG_ARRAY, new long[] { labelId3 } ) ) );

			  // THEN
			  using ( LabelScanReader reader = _store.newReader() )
			  {
					assertArrayEquals( new long[] { 2, 5 }, PrimitiveLongCollections.asArray( reader.NodesWithAllLabels( new int[] { labelId1, labelId2 } ) ) );
					assertArrayEquals( new long[] { 4, 5 }, PrimitiveLongCollections.asArray( reader.NodesWithAllLabels( new int[] { labelId1, labelId3 } ) ) );
					assertArrayEquals( new long[] { 5 }, PrimitiveLongCollections.asArray( reader.NodesWithAllLabels( new int[] { labelId1, labelId2, labelId3 } ) ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void prepareIndex() throws java.io.IOException
		 private void PrepareIndex()
		 {
			  Start();
			  using ( LabelScanWriter labelScanWriter = _store.newWriter() )
			  {
					labelScanWriter.Write( NodeLabelUpdate.labelChanges( 1, new long[]{}, new long[]{1} ) );
			  }
			  _store.shutdown();
		 }

		 private ISet<long> Gaps( ISet<long> ids, int expectedCount )
		 {
			  ISet<long> gaps = new HashSet<long>();
			  for ( long i = 0; i < expectedCount; i++ )
			  {
					if ( !ids.Contains( i ) )
					{
						 gaps.Add( i );
					}
			  }
			  return gaps;
		 }

		 private void AssertNodesForLabel( int labelId, params long[] expectedNodeIds )
		 {
			  ISet<long> nodeSet = new HashSet<long>();
			  LongIterator nodes = _store.newReader().nodesWithLabel(labelId);
			  while ( nodes.hasNext() )
			  {
					nodeSet.Add( nodes.next() );
			  }

			  foreach ( long expectedNodeId in expectedNodeIds )
			  {
					assertTrue( "Expected node " + expectedNodeId + " not found in scan store", nodeSet.remove( expectedNodeId ) );
			  }
			  assertTrue( "Unexpected nodes in scan store " + nodeSet, nodeSet.Count == 0 );
		 }

		 private void CreateAndStartReadOnly()
		 {
			  // create label scan store and shutdown it
			  Start();
			  _life.shutdown();

			  Start( false, true );
		 }

		 private void Start()
		 {
			  Start( false, false );
		 }

		 private void Start( bool usePersistentStore, bool readOnly )
		 {
			  Start( Collections.emptyList(), usePersistentStore, readOnly );
		 }

		 private void Start( IList<NodeLabelUpdate> existingData )
		 {
			  Start( existingData, false, false );
		 }

		 private void Start( IList<NodeLabelUpdate> existingData, bool usePersistentStore, bool readOnly )
		 {
			  _life = new LifeSupport();
			  _monitor = new TrackingMonitor();

			  _store = CreateLabelScanStore( FileSystemRule.get(), TestDirectory.databaseLayout(), asStream(existingData), usePersistentStore, readOnly, _monitor );
			  _life.add( _store );

			  _life.start();
			  assertTrue( _monitor.initCalled );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void scrambleIndexFilesAndRestart(java.util.List<org.Neo4Net.kernel.api.labelscan.NodeLabelUpdate> data, boolean usePersistentStore, boolean readOnly) throws java.io.IOException
		 private void ScrambleIndexFilesAndRestart( IList<NodeLabelUpdate> data, bool usePersistentStore, bool readOnly )
		 {
			  Shutdown();
			  CorruptIndex( FileSystemRule.get(), TestDirectory.databaseLayout() );
			  Start( data, usePersistentStore, readOnly );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void corruptIndex(org.Neo4Net.io.fs.FileSystemAbstraction fileSystem, org.Neo4Net.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException;
		 protected internal abstract void CorruptIndex( FileSystemAbstraction fileSystem, DatabaseLayout databaseLayout );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void scrambleFile(java.io.File file) throws java.io.IOException
		 protected internal virtual void ScrambleFile( File file )
		 {
			  ScrambleFile( this.Random.random(), file );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void scrambleFile(java.util.Random random, java.io.File file) throws java.io.IOException
		 public static void ScrambleFile( Random random, File file )
		 {
			  using ( RandomAccessFile fileAccess = new RandomAccessFile( file, "rw" ), FileChannel channel = fileAccess.Channel )
			  {
					// The files will be small, so OK to allocate a buffer for the full size
					sbyte[] bytes = new sbyte[( int ) channel.size()];
					PutRandomBytes( random, bytes );
					ByteBuffer buffer = ByteBuffer.wrap( bytes );
					channel.position( 0 );
					channel.write( buffer );
			  }
		 }

		 private static void PutRandomBytes( Random random, sbyte[] bytes )
		 {
			  for ( int i = 0; i < bytes.Length; i++ )
			  {
					bytes[i] = ( sbyte ) random.Next();
			  }
		 }

		 public class TrackingMonitor : Neo4Net.Kernel.api.labelscan.LabelScanStore_Monitor_Adaptor
		 {
			  internal bool InitCalled;
			  public bool RebuildingCalled;
			  public bool RebuiltCalled;
			  public bool NoIndexCalled;
			  public bool CorruptedIndex;

			  public override void NoIndex()
			  {
					NoIndexCalled = true;
			  }

			  public override void NotValidIndex()
			  {
					CorruptedIndex = true;
			  }

			  public override void Rebuilding()
			  {
					RebuildingCalled = true;
			  }

			  public override void Rebuilt( long roughNodeCount )
			  {
					RebuiltCalled = true;
			  }

			  public override void Init()
			  {
					InitCalled = true;
			  }

			  public virtual void Reset()
			  {
					InitCalled = false;
					RebuildingCalled = false;
					RebuiltCalled = false;
					NoIndexCalled = false;
					CorruptedIndex = false;
			  }
		 }
	}

}