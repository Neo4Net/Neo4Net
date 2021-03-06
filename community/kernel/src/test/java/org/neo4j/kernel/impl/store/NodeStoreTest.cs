﻿using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.impl.store
{
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using After = org.junit.After;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using DelegatingFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.DelegatingFileSystemAbstraction;
	using DelegatingStoreChannel = Org.Neo4j.Graphdb.mockfs.DelegatingStoreChannel;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using Org.Neo4j.Helpers.Collection;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ReusableRecordsAllocator = Org.Neo4j.Kernel.impl.store.allocator.ReusableRecordsAllocator;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGenerator = Org.Neo4j.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.DynamicArrayStore.allocateFromNumbers;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NodeStore.readOwnerFromDynamicLabelsRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NO_NEXT_PROPERTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;

	public class NodeStoreTest
	{
		private bool InstanceFieldsInitialized = false;

		public NodeStoreTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _efs );
			RuleChain = RuleChain.outerRule( _efs ).around( _testDirectory );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public static readonly PageCacheRule PageCacheRule = new PageCacheRule();

		 private readonly EphemeralFileSystemRule _efs = new EphemeralFileSystemRule();
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(efs).around(testDirectory);
		 public RuleChain RuleChain;

		 private NodeStore _nodeStore;
		 private NeoStores _neoStores;
		 private IdGeneratorFactory _idGeneratorFactory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  if ( _neoStores != null )
			  {
					_neoStores.close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadFirstFromSingleRecordDynamicLongArray()
		 public virtual void ShouldReadFirstFromSingleRecordDynamicLongArray()
		 {
			  // GIVEN
			  long? expectedId = 12L;
			  long[] ids = new long[]{ expectedId.Value, 23L, 42L };
			  DynamicRecord firstRecord = new DynamicRecord( 0L );
			  allocateFromNumbers( new List<>(), ids, new ReusableRecordsAllocator(60, firstRecord) );

			  // WHEN
			  long? firstId = readOwnerFromDynamicLabelsRecord( firstRecord );

			  // THEN
			  assertEquals( expectedId, firstId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadFirstAsNullFromEmptyDynamicLongArray()
		 public virtual void ShouldReadFirstAsNullFromEmptyDynamicLongArray()
		 {
			  // GIVEN
			  long? expectedId = null;
			  long[] ids = new long[]{};
			  DynamicRecord firstRecord = new DynamicRecord( 0L );
			  allocateFromNumbers( new List<>(), ids, new ReusableRecordsAllocator(60, firstRecord) );

			  // WHEN
			  long? firstId = readOwnerFromDynamicLabelsRecord( firstRecord );

			  // THEN
			  assertEquals( expectedId, firstId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadFirstFromTwoRecordDynamicLongArray()
		 public virtual void ShouldReadFirstFromTwoRecordDynamicLongArray()
		 {
			  // GIVEN
			  long? expectedId = 12L;
			  long[] ids = new long[]{ expectedId.Value, 1L, 2L, 3L, 4L, 5L, 6L, 7L, 8L, 9L, 10L, 11L };
			  DynamicRecord firstRecord = new DynamicRecord( 0L );
			  allocateFromNumbers( new List<>(), ids, new ReusableRecordsAllocator(8, firstRecord, new DynamicRecord(1L)) );

			  // WHEN
			  long? firstId = readOwnerFromDynamicLabelsRecord( firstRecord );

			  // THEN
			  assertEquals( expectedId, firstId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCombineProperFiveByteLabelField() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCombineProperFiveByteLabelField()
		 {
			  // GIVEN
			  // -- a store
			  EphemeralFileSystemAbstraction fs = _efs.get();
			  _nodeStore = NewNodeStore( fs );

			  // -- a record with the msb carrying a negative value
			  long nodeId = 0;
			  long labels = 0x8000000001L;
			  NodeRecord record = new NodeRecord( nodeId, false, NO_NEXT_RELATIONSHIP.intValue(), NO_NEXT_PROPERTY.intValue() );
			  record.InUse = true;
			  record.SetLabelField( labels, Collections.emptyList() );
			  _nodeStore.updateRecord( record );

			  // WHEN
			  // -- reading that record back
			  NodeRecord readRecord = _nodeStore.getRecord( nodeId, _nodeStore.newRecord(), NORMAL );

			  // THEN
			  // -- the label field must be the same
			  assertEquals( labels, readRecord.LabelField );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepRecordLightWhenSettingLabelFieldWithoutDynamicRecords()
		 public virtual void ShouldKeepRecordLightWhenSettingLabelFieldWithoutDynamicRecords()
		 {
			  // GIVEN
			  NodeRecord record = new NodeRecord( 0, false, NO_NEXT_RELATIONSHIP.intValue(), NO_NEXT_PROPERTY.intValue() );

			  // WHEN
			  record.SetLabelField( 0, Collections.emptyList() );

			  // THEN
			  assertTrue( record.Light );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMarkRecordHeavyWhenSettingLabelFieldWithDynamicRecords()
		 public virtual void ShouldMarkRecordHeavyWhenSettingLabelFieldWithDynamicRecords()
		 {
			  // GIVEN
			  NodeRecord record = new NodeRecord( 0, false, NO_NEXT_RELATIONSHIP.intValue(), NO_NEXT_PROPERTY.intValue() );

			  // WHEN
			  DynamicRecord dynamicRecord = new DynamicRecord( 1 );
			  record.SetLabelField( 0x8000000001L, asList( dynamicRecord ) );

			  // THEN
			  assertFalse( record.Light );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTellNodeInUse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTellNodeInUse()
		 {
			  // Given
			  EphemeralFileSystemAbstraction fs = _efs.get();
			  NodeStore store = NewNodeStore( fs );

			  long exists = store.NextId();
			  store.UpdateRecord( new NodeRecord( exists, false, 10, 20, true ) );

			  long deleted = store.NextId();
			  store.UpdateRecord( new NodeRecord( deleted, false, 10, 20, true ) );
			  store.UpdateRecord( new NodeRecord( deleted, false, 10, 20, false ) );

			  // When & then
			  assertTrue( store.IsInUse( exists ) );
			  assertFalse( store.IsInUse( deleted ) );
			  assertFalse( store.IsInUse( _nodeStore.recordFormat.MaxId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void scanningRecordsShouldVisitEachInUseRecordOnce() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ScanningRecordsShouldVisitEachInUseRecordOnce()
		 {
			  // GIVEN we have a NodeStore with data that spans several pages...
			  EphemeralFileSystemAbstraction fs = _efs.get();
			  _nodeStore = NewNodeStore( fs );

			  ThreadLocalRandom rng = ThreadLocalRandom.current();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet nextRelSet = new org.eclipse.collections.impl.set.mutable.primitive.LongHashSet();
			  MutableLongSet nextRelSet = new LongHashSet();
			  for ( int i = 0; i < 10_000; i++ )
			  {
					// Enough records to span several pages
					int nextRelCandidate = rng.Next( 0, int.MaxValue );
					if ( nextRelSet.add( nextRelCandidate ) )
					{
						 long nodeId = _nodeStore.nextId();
						 NodeRecord record = new NodeRecord( nodeId, false, nextRelCandidate, 20, true );
						 _nodeStore.updateRecord( record );
						 if ( rng.Next( 0, 10 ) < 3 )
						 {
							  nextRelSet.remove( nextRelCandidate );
							  record.InUse = false;
							  _nodeStore.updateRecord( record );
						 }
					}
			  }

			  // ...WHEN we now have an interesting set of node records, and we
			  // visit each and remove that node from our nextRelSet...

			  Visitor<NodeRecord, IOException> scanner = record =>
			  {
				// ...THEN we should observe that no nextRel is ever removed twice...
				assertTrue( nextRelSet.remove( record.NextRel ) );
				return false;
			  };
			  _nodeStore.scanAllRecords( scanner );

			  // ...NOR do we have anything left in the set afterwards.
			  assertTrue( nextRelSet.Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseStoreFileOnFailureToOpen()
		 public virtual void ShouldCloseStoreFileOnFailureToOpen()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableBoolean fired = new org.apache.commons.lang3.mutable.MutableBoolean();
			  MutableBoolean fired = new MutableBoolean();
			  FileSystemAbstraction fs = new DelegatingFileSystemAbstractionAnonymousInnerClass( this, _efs.get(), fired );

			  // WHEN
			  try
			  {
					  using ( PageCache pageCache = PageCacheRule.getPageCache( fs ) )
					  {
						NewNodeStore( fs );
						fail( "Should fail" );
					  }
			  } // Close the page cache here so that we can see failure to close (due to still mapped files)
			  catch ( Exception e )
			  {
					// THEN
					assertTrue( contains( e, typeof( IOException ) ) );
					assertTrue( fired.booleanValue() );
			  }
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass : DelegatingFileSystemAbstraction
		 {
			 private readonly NodeStoreTest _outerInstance;

			 private MutableBoolean _fired;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass( NodeStoreTest outerInstance, UnknownType get, MutableBoolean fired ) : base( get )
			 {
				 this.outerInstance = outerInstance;
				 this._fired = fired;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreChannel open(java.io.File fileName, org.neo4j.io.fs.OpenMode openMode) throws java.io.IOException
			 public override StoreChannel open( File fileName, OpenMode openMode )
			 {
				  return new DelegatingStoreChannelAnonymousInnerClass( this, base.open( fileName, openMode ) );
			 }

			 private class DelegatingStoreChannelAnonymousInnerClass : DelegatingStoreChannel
			 {
				 private readonly DelegatingFileSystemAbstractionAnonymousInnerClass _outerInstance;

				 public DelegatingStoreChannelAnonymousInnerClass( DelegatingFileSystemAbstractionAnonymousInnerClass outerInstance, UnknownType open ) : base( open )
				 {
					 this.outerInstance = outerInstance;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readAll(ByteBuffer dst) throws java.io.IOException
				 public override void readAll( ByteBuffer dst )
				 {
					  _outerInstance.fired.Value = true;
					  throw new IOException( "Proving a point here" );
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFreeSecondaryUnitIdOfDeletedRecord() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFreeSecondaryUnitIdOfDeletedRecord()
		 {
			  // GIVEN
			  EphemeralFileSystemAbstraction fs = _efs.get();
			  _nodeStore = NewNodeStore( fs );
			  NodeRecord record = new NodeRecord( 5L );
			  record.RequiresSecondaryUnit = true;
			  record.SecondaryUnitId = 10L;
			  record.InUse = true;
			  _nodeStore.updateRecord( record );
			  _nodeStore.HighestPossibleIdInUse = 10L;

			  // WHEN
			  record.InUse = false;
			  _nodeStore.updateRecord( record );

			  // THEN
			  IdGenerator idGenerator = _idGeneratorFactory.get( IdType.NODE );
			  verify( idGenerator ).freeId( 5L );
			  verify( idGenerator ).freeId( 10L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFreeSecondaryUnitIdOfShrunkRecord() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFreeSecondaryUnitIdOfShrunkRecord()
		 {
			  // GIVEN
			  EphemeralFileSystemAbstraction fs = _efs.get();
			  _nodeStore = NewNodeStore( fs );
			  NodeRecord record = new NodeRecord( 5L );
			  record.RequiresSecondaryUnit = true;
			  record.SecondaryUnitId = 10L;
			  record.InUse = true;
			  _nodeStore.updateRecord( record );
			  _nodeStore.HighestPossibleIdInUse = 10L;

			  // WHEN
			  record.RequiresSecondaryUnit = false;
			  _nodeStore.updateRecord( record );

			  // THEN
			  IdGenerator idGenerator = _idGeneratorFactory.get( IdType.NODE );
			  verify( idGenerator, never() ).freeId(5L);
			  verify( idGenerator ).freeId( 10L );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private NodeStore newNodeStore(org.neo4j.io.fs.FileSystemAbstraction fs) throws java.io.IOException
		 private NodeStore NewNodeStore( FileSystemAbstraction fs )
		 {
			  return NewNodeStore( fs, PageCacheRule.getPageCache( fs ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private NodeStore newNodeStore(org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.io.pagecache.PageCache pageCache) throws java.io.IOException
		 private NodeStore NewNodeStore( FileSystemAbstraction fs, PageCache pageCache )
		 {
			  _idGeneratorFactory = spy( new DefaultIdGeneratorFactoryAnonymousInnerClass( this, fs ) );
			  StoreFactory factory = new StoreFactory( _testDirectory.databaseLayout( "new" ), Config.defaults(), _idGeneratorFactory, pageCache, fs, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  _neoStores = factory.OpenAllNeoStores( true );
			  _nodeStore = _neoStores.NodeStore;
			  return _nodeStore;
		 }

		 private class DefaultIdGeneratorFactoryAnonymousInnerClass : DefaultIdGeneratorFactory
		 {
			 private readonly NodeStoreTest _outerInstance;

			 private FileSystemAbstraction _fs;

			 public DefaultIdGeneratorFactoryAnonymousInnerClass( NodeStoreTest outerInstance, FileSystemAbstraction fs ) : base( fs )
			 {
				 this.outerInstance = outerInstance;
				 this._fs = fs;
			 }

			 protected internal override IdGenerator instantiate( FileSystemAbstraction fs, File fileName, int grabSize, long maxValue, bool aggressiveReuse, IdType idType, System.Func<long> highId )
			 {
				  return spy( base.instantiate( fs, fileName, grabSize, maxValue, aggressiveReuse, idType, highId ) );
			 }
		 }
	}

}