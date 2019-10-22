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
namespace Neo4Net.Kernel.impl.transaction.state
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using CloneableInPublic = Neo4Net.Helpers.CloneableInPublic;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Neo4Net.Helpers.Collections;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DynamicNodeLabels = Neo4Net.Kernel.impl.store.DynamicNodeLabels;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeLabels = Neo4Net.Kernel.impl.store.NodeLabels;
	using NodeLabelsField = Neo4Net.Kernel.impl.store.NodeLabelsField;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using Bits = Neo4Net.Kernel.impl.util.Bits;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Numbers.safeCastLongToInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.util.Bits.bits;

	public class NodeLabelsFieldTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.Neo4Net.test.rule.PageCacheRule pageCacheRule = new org.Neo4Net.test.rule.PageCacheRule();
		 public static readonly PageCacheRule PageCacheRule = new PageCacheRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.fs.EphemeralFileSystemRule fs = new org.Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.RandomRule random = new org.Neo4Net.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private NeoStores _neoStores;
		 private NodeStore _nodeStore;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void startUp()
		 public virtual void StartUp()
		 {
			  Config config = Config.defaults( GraphDatabaseSettings.label_block_size, "60" );
			  StoreFactory storeFactory = new StoreFactory( TestDirectory.databaseLayout(), config, new DefaultIdGeneratorFactory(Fs.get()), PageCacheRule.getPageCache(Fs.get()), Fs.get(), NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  _neoStores = storeFactory.OpenAllNeoStores( true );
			  _nodeStore = _neoStores.NodeStore;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanUp()
		 public virtual void CleanUp()
		 {
			  _neoStores.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInlineOneLabel()
		 public virtual void ShouldInlineOneLabel()
		 {
			  // GIVEN
			  long labelId = 10;
			  NodeRecord node = NodeRecordWithInlinedLabels();
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  nodeLabels.Add( labelId, null, null );

			  // THEN
			  assertEquals( InlinedLabelsLongRepresentation( labelId ), node.LabelField );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInlineOneLabelWithHighId()
		 public virtual void ShouldInlineOneLabelWithHighId()
		 {
			  // GIVEN
			  long labelId = 10000;
			  NodeRecord node = NodeRecordWithInlinedLabels();
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  nodeLabels.Add( labelId, null, null );

			  // THEN
			  assertEquals( InlinedLabelsLongRepresentation( labelId ), node.LabelField );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInlineTwoSmallLabels()
		 public virtual void ShouldInlineTwoSmallLabels()
		 {
			  // GIVEN
			  long labelId1 = 10;
			  long labelId2 = 30;
			  NodeRecord node = NodeRecordWithInlinedLabels( labelId1 );
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  nodeLabels.Add( labelId2, null, null );

			  // THEN
			  assertEquals( InlinedLabelsLongRepresentation( labelId1, labelId2 ), node.LabelField );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInlineThreeSmallLabels()
		 public virtual void ShouldInlineThreeSmallLabels()
		 {
			  // GIVEN
			  long labelId1 = 10;
			  long labelId2 = 30;
			  long labelId3 = 4095;
			  NodeRecord node = NodeRecordWithInlinedLabels( labelId1, labelId2 );
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  nodeLabels.Add( labelId3, null, null );

			  // THEN
			  assertEquals( InlinedLabelsLongRepresentation( labelId1, labelId2, labelId3 ), node.LabelField );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInlineFourSmallLabels()
		 public virtual void ShouldInlineFourSmallLabels()
		 {
			  // GIVEN
			  long labelId1 = 10;
			  long labelId2 = 30;
			  long labelId3 = 45;
			  long labelId4 = 60;
			  NodeRecord node = NodeRecordWithInlinedLabels( labelId1, labelId2, labelId3 );
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  nodeLabels.Add( labelId4, null, null );

			  // THEN
			  assertEquals( InlinedLabelsLongRepresentation( labelId1, labelId2, labelId3, labelId4 ), node.LabelField );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInlineFiveSmallLabels()
		 public virtual void ShouldInlineFiveSmallLabels()
		 {
			  // GIVEN
			  long labelId1 = 10;
			  long labelId2 = 30;
			  long labelId3 = 45;
			  long labelId4 = 60;
			  long labelId5 = 61;
			  NodeRecord node = NodeRecordWithInlinedLabels( labelId1, labelId2, labelId3, labelId4 );
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  nodeLabels.Add( labelId5, null, null );

			  // THEN
			  assertEquals( InlinedLabelsLongRepresentation( labelId1, labelId2, labelId3, labelId4, labelId5 ), node.LabelField );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSpillOverToDynamicRecordIfExceedsInlinedSpace()
		 public virtual void ShouldSpillOverToDynamicRecordIfExceedsInlinedSpace()
		 {
			  // GIVEN -- the upper limit for a label ID for 3 labels would be 36b/3 - 1 = 12b - 1 = 4095
			  long labelId1 = 10;
			  long labelId2 = 30;
			  long labelId3 = 4096;
			  NodeRecord node = NodeRecordWithInlinedLabels( labelId1, labelId2 );
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  ICollection<DynamicRecord> changedDynamicRecords = nodeLabels.Add( labelId3, _nodeStore, _nodeStore.DynamicLabelStore );

			  // THEN
			  assertEquals( 1, Iterables.count( changedDynamicRecords ) );
			  assertEquals( DynamicLabelsLongRepresentation( changedDynamicRecords ), node.LabelField );
			  assertTrue( Arrays.Equals( new long[] { labelId1, labelId2, labelId3 }, DynamicNodeLabels.getDynamicLabelsArray( changedDynamicRecords, _nodeStore.DynamicLabelStore ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void oneDynamicRecordShouldExtendIntoAnAdditionalIfTooManyLabels()
		 public virtual void OneDynamicRecordShouldExtendIntoAnAdditionalIfTooManyLabels()
		 {
			  // GIVEN
			  // will occupy 60B of data, i.e. one dynamic record
			  NodeRecord node = nodeRecordWithDynamicLabels( _nodeStore, OneByteLongs( 56 ) );
			  ICollection<DynamicRecord> initialRecords = node.DynamicLabelRecords;
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  ISet<DynamicRecord> changedDynamicRecords = Iterables.asSet( nodeLabels.Add( 1, _nodeStore, _nodeStore.DynamicLabelStore ) );

			  // THEN
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			  assertTrue( changedDynamicRecords.containsAll( initialRecords ) );
			  assertEquals( initialRecords.Count + 1, changedDynamicRecords.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void oneDynamicRecordShouldStoreItsOwner()
		 public virtual void OneDynamicRecordShouldStoreItsOwner()
		 {
			  // GIVEN
			  // will occupy 60B of data, i.e. one dynamic record
			  long? nodeId = 24L;
			  NodeRecord node = nodeRecordWithDynamicLabels( nodeId, _nodeStore, OneByteLongs( 56 ) );
			  ICollection<DynamicRecord> initialRecords = node.DynamicLabelRecords;

			  // WHEN
			  Pair<long, long[]> pair = DynamicNodeLabels.getDynamicLabelsArrayAndOwner( initialRecords, _nodeStore.DynamicLabelStore );

			  // THEN
			  assertEquals( nodeId, pair.First() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoDynamicRecordsShouldShrinkToOneWhenRemoving()
		 public virtual void TwoDynamicRecordsShouldShrinkToOneWhenRemoving()
		 {
			  // GIVEN
			  // will occupy 61B of data, i.e. just two dynamic records
			  NodeRecord node = nodeRecordWithDynamicLabels( _nodeStore, OneByteLongs( 57 ) );
			  ICollection<DynamicRecord> initialRecords = node.DynamicLabelRecords;
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  IList<DynamicRecord> changedDynamicRecords = Iterables.addToCollection( nodeLabels.Remove( 255, _nodeStore ), new List<DynamicRecord>() );

			  // THEN
			  assertEquals( initialRecords, changedDynamicRecords );
			  assertTrue( changedDynamicRecords[0].InUse() );
			  assertFalse( changedDynamicRecords[1].InUse() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoDynamicRecordsShouldShrinkToOneWhenRemovingWithoutChangingItsOwner()
		 public virtual void TwoDynamicRecordsShouldShrinkToOneWhenRemovingWithoutChangingItsOwner()
		 {
			  // GIVEN
			  // will occupy 61B of data, i.e. just two dynamic records
			  long? nodeId = 42L;
			  NodeRecord node = nodeRecordWithDynamicLabels( nodeId, _nodeStore, OneByteLongs( 57 ) );
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  IList<DynamicRecord> changedDynamicRecords = Iterables.addToCollection( nodeLabels.Remove( 255, _nodeStore ), new List<DynamicRecord>() );

			  // WHEN
			  Pair<long, long[]> changedPair = DynamicNodeLabels.getDynamicLabelsArrayAndOwner( changedDynamicRecords, _nodeStore.DynamicLabelStore );

			  // THEN
			  assertEquals( nodeId, changedPair.First() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void oneDynamicRecordShouldShrinkIntoInlinedWhenRemoving()
		 public virtual void OneDynamicRecordShouldShrinkIntoInlinedWhenRemoving()
		 {
			  // GIVEN
			  NodeRecord node = nodeRecordWithDynamicLabels( _nodeStore, OneByteLongs( 5 ) );
			  ICollection<DynamicRecord> initialRecords = node.DynamicLabelRecords;
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  ICollection<DynamicRecord> changedDynamicRecords = Iterables.asCollection( nodeLabels.Remove( 255, _nodeStore ) );

			  // THEN
			  assertEquals( initialRecords, changedDynamicRecords );
			  assertFalse( Iterables.single( changedDynamicRecords ).inUse() );
			  assertEquals( InlinedLabelsLongRepresentation( 251, 252, 253, 254 ), node.LabelField );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadIdOfDynamicRecordFromDynamicLabelsField()
		 public virtual void ShouldReadIdOfDynamicRecordFromDynamicLabelsField()
		 {
			  // GIVEN
			  NodeRecord node = nodeRecordWithDynamicLabels( _nodeStore, OneByteLongs( 5 ) );
			  DynamicRecord dynamicRecord = node.DynamicLabelRecords.GetEnumerator().next();

			  // WHEN
			  long dynRecordId = NodeLabelsField.firstDynamicLabelRecordId( node.LabelField );

			  // THEN
			  assertEquals( dynamicRecord.Id, dynRecordId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadNullDynamicRecordFromInlineLabelsField()
		 public virtual void ShouldReadNullDynamicRecordFromInlineLabelsField()
		 {
			  // GIVEN
			  NodeRecord node = NodeRecordWithInlinedLabels( 23L );

			  // WHEN
			  bool isDynamicReference = NodeLabelsField.fieldPointsToDynamicRecordOfLabels( node.LabelField );

			  // THEN
			  assertFalse( isDynamicReference );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void maximumOfSevenInlinedLabels()
		 public virtual void MaximumOfSevenInlinedLabels()
		 {
			  // GIVEN
			  long[] labels = new long[] { 0, 1, 2, 3, 4, 5, 6 };
			  NodeRecord node = NodeRecordWithInlinedLabels( labels );
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  IEnumerable<DynamicRecord> changedDynamicRecords = nodeLabels.Add( 23, _nodeStore, _nodeStore.DynamicLabelStore );

			  // THEN
			  assertEquals( DynamicLabelsLongRepresentation( changedDynamicRecords ), node.LabelField );
			  assertEquals( 1, Iterables.count( changedDynamicRecords ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingAnAlreadyAddedLabelWhenLabelsAreInlinedShouldFail()
		 public virtual void AddingAnAlreadyAddedLabelWhenLabelsAreInlinedShouldFail()
		 {
			  // GIVEN
			  int labelId = 1;
			  NodeRecord node = NodeRecordWithInlinedLabels( labelId );
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  try
			  {
					nodeLabels.Add( labelId, _nodeStore, _nodeStore.DynamicLabelStore );
					fail( "Should have thrown exception" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// THEN
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingAnAlreadyAddedLabelWhenLabelsAreInDynamicRecordsShouldFail()
		 public virtual void AddingAnAlreadyAddedLabelWhenLabelsAreInDynamicRecordsShouldFail()
		 {
			  // GIVEN
			  long[] labels = OneByteLongs( 20 );
			  NodeRecord node = nodeRecordWithDynamicLabels( _nodeStore, labels );
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  try
			  {
					nodeLabels.Add( safeCastLongToInt( labels[0] ), _nodeStore, _nodeStore.DynamicLabelStore );
					fail( "Should have thrown exception" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// THEN
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removingNonExistentInlinedLabelShouldFail()
		 public virtual void RemovingNonExistentInlinedLabelShouldFail()
		 {
			  // GIVEN
			  int labelId1 = 1;
			  int labelId2 = 2;
			  NodeRecord node = NodeRecordWithInlinedLabels( labelId1 );
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  try
			  {
					nodeLabels.Remove( labelId2, _nodeStore );
					fail( "Should have thrown exception" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// THEN
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removingNonExistentLabelInDynamicRecordsShouldFail()
		 public virtual void RemovingNonExistentLabelInDynamicRecordsShouldFail()
		 {
			  // GIVEN
			  long[] labels = OneByteLongs( 20 );
			  NodeRecord node = nodeRecordWithDynamicLabels( _nodeStore, labels );
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  try
			  {
					nodeLabels.Remove( 123456, _nodeStore );
					fail( "Should have thrown exception" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// THEN
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReallocateSomeOfPreviousDynamicRecords()
		 public virtual void ShouldReallocateSomeOfPreviousDynamicRecords()
		 {
			  // GIVEN
			  NodeRecord node = nodeRecordWithDynamicLabels( _nodeStore, OneByteLongs( 5 ) );
			  ISet<DynamicRecord> initialRecords = Iterables.asUniqueSet( node.DynamicLabelRecords );
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  ISet<DynamicRecord> reallocatedRecords = Iterables.asUniqueSet( nodeLabels.Put( FourByteLongs( 100 ), _nodeStore, _nodeStore.DynamicLabelStore ) );

			  // THEN
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			  assertTrue( reallocatedRecords.containsAll( initialRecords ) );
			  assertTrue( reallocatedRecords.Count > initialRecords.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReallocateAllOfPreviousDynamicRecordsAndThenSome()
		 public virtual void ShouldReallocateAllOfPreviousDynamicRecordsAndThenSome()
		 {
			  // GIVEN
			  NodeRecord node = nodeRecordWithDynamicLabels( _nodeStore, FourByteLongs( 100 ) );
			  ISet<DynamicRecord> initialRecords = Iterables.asSet( Cloned( node.DynamicLabelRecords, typeof( DynamicRecord ) ) );
			  NodeLabels nodeLabels = NodeLabelsField.parseLabelsField( node );

			  // WHEN
			  ISet<DynamicRecord> reallocatedRecords = Iterables.asUniqueSet( nodeLabels.Put( FourByteLongs( 5 ), _nodeStore, _nodeStore.DynamicLabelStore ) );

			  // THEN
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			  assertTrue( "initial:" + initialRecords + ", reallocated:" + reallocatedRecords, initialRecords.containsAll( Used( reallocatedRecords ) ) );
			  assertTrue( Used( reallocatedRecords ).Count < initialRecords.Count );
		 }

		 /*
		  * There was this issue that DynamicNodeLabels#add would consider even unused dynamic records when
		  * reading existing label ids before making the change. Previously this would create a duplicate
		  * last label id (the one formerly being in the second record).
		  *
		  * This randomized test found this issue every time when it existed and it will potentially find other
		  * unforeseen issues as well.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleRandomAddsAndRemoves()
		 public virtual void ShouldHandleRandomAddsAndRemoves()
		 {
			  // GIVEN
			  ISet<int> key = new HashSet<int>();
			  NodeRecord node = new NodeRecord( 0 );
			  node.InUse = true;

			  // WHEN
			  for ( int i = 0; i < 100_000; i++ )
			  {
					NodeLabels labels = NodeLabelsField.parseLabelsField( node );
					int labelId = Random.Next( 200 );
					if ( Random.nextBoolean() )
					{
						 if ( !key.Contains( labelId ) )
						 {
							  labels.Add( labelId, _nodeStore, _nodeStore.DynamicLabelStore );
							  key.Add( labelId );
						 }
					}
					else
					{
						 if ( key.remove( labelId ) )
						 {
							  labels.Remove( labelId, _nodeStore );
						 }
					}
			  }

			  // THEN
			  NodeLabels labels = NodeLabelsField.parseLabelsField( node );
			  long[] readLabelIds = labels.Get( _nodeStore );
			  foreach ( long labelId in readLabelIds )
			  {
					assertTrue( "Found an unexpected label " + labelId, key.remove( ( int ) labelId ) );
			  }
			  assertTrue( key.Count == 0 );
		 }

		 private long DynamicLabelsLongRepresentation( IEnumerable<DynamicRecord> records )
		 {
			  return 0x8000000000L | Iterables.first( records ).Id;
		 }

		 private long InlinedLabelsLongRepresentation( params long[] labelIds )
		 {
			  long header = ( long ) labelIds.Length << 36;
			  sbyte bitsPerLabel = ( sbyte )( 36 / labelIds.Length );
			  Bits bits = bits( 5 );
			  foreach ( long labelId in labelIds )
			  {
					bits.Put( labelId, bitsPerLabel );
			  }
			  return header | bits.Longs[0];
		 }

		 private NodeRecord NodeRecordWithInlinedLabels( params long[] labels )
		 {
			  NodeRecord node = new NodeRecord( 0, false, 0, 0 );
			  if ( labels.Length > 0 )
			  {
					node.SetLabelField( InlinedLabelsLongRepresentation( labels ), Collections.emptyList() );
			  }
			  return node;
		 }

		 private NodeRecord NodeRecordWithDynamicLabels( NodeStore nodeStore, params long[] labels )
		 {
			  return nodeRecordWithDynamicLabels( 0, nodeStore, labels );
		 }

		 private NodeRecord NodeRecordWithDynamicLabels( long nodeId, NodeStore nodeStore, params long[] labels )
		 {
			  NodeRecord node = new NodeRecord( nodeId, false, 0, 0 );
			  ICollection<DynamicRecord> initialRecords = AllocateAndApply( nodeStore, node.Id, labels );
			  node.SetLabelField( DynamicLabelsLongRepresentation( initialRecords ), initialRecords );
			  return node;
		 }

		 private ICollection<DynamicRecord> AllocateAndApply( NodeStore nodeStore, long nodeId, long[] longs )
		 {
			  ICollection<DynamicRecord> records = DynamicNodeLabels.allocateRecordsForDynamicLabels( nodeId, longs, nodeStore.DynamicLabelStore );
			  nodeStore.UpdateDynamicLabelRecords( records );
			  return records;
		 }

		 private long[] OneByteLongs( int numberOfLongs )
		 {
			  long[] result = new long[numberOfLongs];
			  for ( int i = 0; i < numberOfLongs; i++ )
			  {
					result[i] = 255 - i;
			  }
			  Arrays.sort( result );
			  return result;
		 }

		 private long[] FourByteLongs( int numberOfLongs )
		 {
			  long[] result = new long[numberOfLongs];
			  for ( int i = 0; i < numberOfLongs; i++ )
			  {
					result[i] = int.MaxValue - i;
			  }
			  Arrays.sort( result );
			  return result;
		 }

		 private ISet<DynamicRecord> Used( ISet<DynamicRecord> reallocatedRecords )
		 {
			  ISet<DynamicRecord> used = new HashSet<DynamicRecord>();
			  foreach ( DynamicRecord record in reallocatedRecords )
			  {
					if ( record.InUse() )
					{
						 used.Add( record );
					}
			  }
			  return used;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static <T extends org.Neo4Net.helpers.CloneableInPublic> Iterable<T> cloned(Iterable<T> items, final Class<T> itemClass)
		 private static IEnumerable<T> Cloned<T>( IEnumerable<T> items, Type itemClass ) where T : Neo4Net.Helpers.CloneableInPublic
		 {
				 itemClass = typeof( T );
			  return Iterables.map( from => itemClass.cast( from.clone() ), items );
		 }
	}

}