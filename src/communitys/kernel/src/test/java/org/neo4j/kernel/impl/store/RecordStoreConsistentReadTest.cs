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
namespace Neo4Net.Kernel.impl.store
{
	using After = org.junit.After;
	using AssumptionViolatedException = org.junit.AssumptionViolatedException;
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ReusableRecordsAllocator = Neo4Net.Kernel.impl.store.allocator.ReusableRecordsAllocator;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.impl.store.record.LabelTokenRecord;
	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using UTF8 = Neo4Net.Strings.UTF8;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.PageCacheRule.config;

	public abstract class RecordStoreConsistentReadTest<R, S> where R : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where S : RecordStore<R>
	{
		 // Constants for the contents of the existing record
		 protected internal const int ID = 1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule(config().withInconsistentReads(false));
		 public static readonly PageCacheRule PageCacheRule = new PageCacheRule( config().withInconsistentReads(false) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private FileSystemAbstraction _fs;
		 private AtomicBoolean _nextReadIsInconsistent;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _fs = new EphemeralFileSystemAbstraction();
			  _nextReadIsInconsistent = new AtomicBoolean();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _fs.Dispose();
		 }

		 private NeoStores StoreFixture()
		 {
			  PageCache pageCache = PageCacheRule.getPageCache( _fs, config().withInconsistentReads(_nextReadIsInconsistent) );
			  StoreFactory factory = new StoreFactory( TestDirectory.databaseLayout(), Config.defaults(), new DefaultIdGeneratorFactory(_fs), pageCache, _fs, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  NeoStores neoStores = factory.OpenAllNeoStores( true );
			  S store = InitializeStore( neoStores );

			  CommonAbstractStore commonAbstractStore = ( CommonAbstractStore ) store;
			  commonAbstractStore.rebuildIdGenerator();
			  return neoStores;
		 }

		 protected internal virtual S InitializeStore( NeoStores neoStores )
		 {
			  S store = GetStore( neoStores );
			  store.updateRecord( CreateExistingRecord( false ) );
			  return store;
		 }

		 protected internal abstract S GetStore( NeoStores neoStores );

		 protected internal abstract R CreateNullRecord( long id );

		 protected internal abstract R CreateExistingRecord( bool light );

		 protected internal abstract R GetLight( long id, S store );

		 protected internal abstract void AssertRecordsEqual( R actualRecord, R expectedRecord );

		 protected internal virtual R GetHeavy( S store, int id )
		 {
			  R record = store.getRecord( id, store.newRecord(), NORMAL );
			  store.ensureHeavy( record );
			  return record;
		 }

		 protected internal virtual R GetForce( S store, int id )
		 {
			  return store.getRecord( id, store.newRecord(), RecordLoad.FORCE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReadExistingRecord()
		 public virtual void MustReadExistingRecord()
		 {
			  using ( NeoStores neoStores = StoreFixture() )
			  {
					S store = GetStore( neoStores );
					R record = GetHeavy( store, ID );
					AssertRecordsEqual( record, CreateExistingRecord( false ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReadExistingLightRecord()
		 public virtual void MustReadExistingLightRecord()
		 {
			  using ( NeoStores neoStores = StoreFixture() )
			  {
					S store = GetStore( neoStores );
					R record = GetLight( ID, store );
					AssertRecordsEqual( record, CreateExistingRecord( true ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustForceReadExistingRecord()
		 public virtual void MustForceReadExistingRecord()
		 {
			  using ( NeoStores neoStores = StoreFixture() )
			  {
					S store = GetStore( neoStores );
					R record = GetForce( store, ID );
					AssertRecordsEqual( record, CreateExistingRecord( true ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = InvalidRecordException.class) public void readingNonExistingRecordMustThrow()
		 public virtual void ReadingNonExistingRecordMustThrow()
		 {
			  using ( NeoStores neoStores = StoreFixture() )
			  {
					S store = GetStore( neoStores );
					GetHeavy( store, ID + 1 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void forceReadingNonExistingRecordMustReturnEmptyRecordWithThatId()
		 public virtual void ForceReadingNonExistingRecordMustReturnEmptyRecordWithThatId()
		 {
			  using ( NeoStores neoStores = StoreFixture() )
			  {
					S store = GetStore( neoStores );
					R record = GetForce( store, ID + 1 );
					R nullRecord = CreateNullRecord( ID + 1 );
					AssertRecordsEqual( record, nullRecord );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustRetryInconsistentReads()
		 public virtual void MustRetryInconsistentReads()
		 {
			  using ( NeoStores neoStores = StoreFixture() )
			  {
					S store = GetStore( neoStores );
					_nextReadIsInconsistent.set( true );
					R record = GetHeavy( store, ID );
					AssertRecordsEqual( record, CreateExistingRecord( false ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustRetryInconsistentLightReads()
		 public virtual void MustRetryInconsistentLightReads()
		 {
			  using ( NeoStores neoStores = StoreFixture() )
			  {
					S store = GetStore( neoStores );
					_nextReadIsInconsistent.set( true );
					R record = GetLight( ID, store );
					AssertRecordsEqual( record, CreateExistingRecord( true ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustRetryInconsistentForcedReads()
		 public virtual void MustRetryInconsistentForcedReads()
		 {
			  using ( NeoStores neoStores = StoreFixture() )
			  {
					S store = GetStore( neoStores );
					_nextReadIsInconsistent.set( true );
					R record = GetForce( store, ID );
					AssertRecordsEqual( record, CreateExistingRecord( true ) );
			  }
		 }

		 public class RelationshipStoreConsistentReadTest : RecordStoreConsistentReadTest<RelationshipRecord, RelationshipStore>
		 {
			  // Constants for the contents of the existing record
			  internal const int FIRST_NODE = 2;
			  internal const int SECOND_NODE = 3;
			  internal const int TYPE = 4;
			  internal const int FIRST_PREV_REL = 5;
			  internal const int FIRST_NEXT_REL = 6;
			  internal const int SECOND_PREV_REL = 7;
			  internal const int SECOND_NEXT_REL = 8;

			  protected internal override RelationshipRecord CreateNullRecord( long id )
			  {
					RelationshipRecord record = new RelationshipRecord( id, false, 0, 0, 0, 0, 0, 0, 0, false, false );
					record.NextProp = 0;
					return record;
			  }

			  protected internal override RelationshipRecord CreateExistingRecord( bool light )
			  {
					return new RelationshipRecord( ID, true, FIRST_NODE, SECOND_NODE, TYPE, FIRST_PREV_REL, FIRST_NEXT_REL, SECOND_PREV_REL, SECOND_NEXT_REL, true, true );
			  }

			  protected internal override RelationshipRecord GetLight( long id, RelationshipStore store )
			  {
					return store.GetRecord( id, store.NewRecord(), NORMAL );
			  }

			  protected internal override void AssertRecordsEqual( RelationshipRecord actualRecord, RelationshipRecord expectedRecord )
			  {
					assertNotNull( "actualRecord", actualRecord );
					assertNotNull( "expectedRecord", expectedRecord );
					assertThat( "getFirstNextRel", actualRecord.FirstNextRel, @is( expectedRecord.FirstNextRel ) );
					assertThat( "getFirstNode", actualRecord.FirstNode, @is( expectedRecord.FirstNode ) );
					assertThat( "getFirstPrevRel", actualRecord.FirstPrevRel, @is( expectedRecord.FirstPrevRel ) );
					assertThat( "getSecondNextRel", actualRecord.SecondNextRel, @is( expectedRecord.SecondNextRel ) );
					assertThat( "getSecondNode", actualRecord.SecondNode, @is( expectedRecord.SecondNode ) );
					assertThat( "getSecondPrevRel", actualRecord.SecondPrevRel, @is( expectedRecord.SecondPrevRel ) );
					assertThat( "getType", actualRecord.Type, @is( expectedRecord.Type ) );
					assertThat( "isFirstInFirstChain", actualRecord.FirstInFirstChain, @is( expectedRecord.FirstInFirstChain ) );
					assertThat( "isFirstInSecondChain", actualRecord.FirstInSecondChain, @is( expectedRecord.FirstInSecondChain ) );
					assertThat( "getId", actualRecord.Id, @is( expectedRecord.Id ) );
					assertThat( "getLongId", actualRecord.Id, @is( expectedRecord.Id ) );
					assertThat( "getNextProp", actualRecord.NextProp, @is( expectedRecord.NextProp ) );
					assertThat( "inUse", actualRecord.InUse(), @is(expectedRecord.InUse()) );
			  }

			  protected internal override RelationshipStore GetStore( NeoStores neoStores )
			  {
					return neoStores.RelationshipStore;
			  }
		 }

		 public class LabelTokenStoreConsistentReadTest : RecordStoreConsistentReadTest<LabelTokenRecord, LabelTokenStore>
		 {

			  internal const int NAME_RECORD_ID = 2;
			  internal static readonly sbyte[] NameRecordData = UTF8.encode( "TheLabel" );

			  protected internal override LabelTokenStore GetStore( NeoStores neoStores )
			  {
					return neoStores.LabelTokenStore;
			  }

			  protected internal override LabelTokenStore InitializeStore( NeoStores neoStores )
			  {
					LabelTokenStore store = GetStore( neoStores );
					LabelTokenRecord record = CreateExistingRecord( false );
					DynamicRecord nameRecord = new DynamicRecord( NAME_RECORD_ID );
					record.NameRecords.Clear();
					nameRecord.Data = NameRecordData;
					nameRecord.InUse = true;
					record.AddNameRecord( nameRecord );
					store.UpdateRecord( record );
					return store;
			  }

			  protected internal override LabelTokenRecord CreateNullRecord( long id )
			  {
					return ( new LabelTokenRecord( ( int ) id ) ).initialize( false, 0 );
			  }

			  protected internal override LabelTokenRecord CreateExistingRecord( bool light )
			  {
					LabelTokenRecord record = new LabelTokenRecord( ID );
					record.NameId = NAME_RECORD_ID;
					record.InUse = true;
					if ( !light )
					{
						 DynamicRecord nameRecord = new DynamicRecord( NAME_RECORD_ID );
						 nameRecord.InUse = true;
						 nameRecord.Data = NameRecordData;
						 record.AddNameRecord( nameRecord );
					}
					return record;
			  }

			  protected internal override LabelTokenRecord GetLight( long id, LabelTokenStore store )
			  {
					throw new AssumptionViolatedException( "No light loading of LabelTokenRecords" );
			  }

			  protected internal override void AssertRecordsEqual( LabelTokenRecord actualRecord, LabelTokenRecord expectedRecord )
			  {
					assertNotNull( "actualRecord", actualRecord );
					assertNotNull( "expectedRecord", expectedRecord );
					assertThat( "getNameId", actualRecord.NameId, @is( expectedRecord.NameId ) );
					assertThat( "getId", actualRecord.Id, @is( expectedRecord.Id ) );
					assertThat( "getLongId", actualRecord.Id, @is( expectedRecord.Id ) );
					assertThat( "isLight", actualRecord.Light, @is( expectedRecord.Light ) );

					ICollection<DynamicRecord> actualNameRecords = actualRecord.NameRecords;
					ICollection<DynamicRecord> expectedNameRecords = expectedRecord.NameRecords;
					assertThat( "getNameRecords.size", actualNameRecords.Count, @is( expectedNameRecords.Count ) );
					IEnumerator<DynamicRecord> actualNRs = actualNameRecords.GetEnumerator();
					IEnumerator<DynamicRecord> expectedNRs = expectedNameRecords.GetEnumerator();
					int i = 0;
					while ( actualNRs.MoveNext() && expectedNRs.MoveNext() )
					{
						 DynamicRecord actualNameRecord = actualNRs.Current;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 DynamicRecord expectedNameRecord = expectedNRs.next();

						 assertThat( "[" + i + "]getData", actualNameRecord.Data, @is( expectedNameRecord.Data ) );
						 assertThat( "[" + i + "]getLength", actualNameRecord.Length, @is( expectedNameRecord.Length ) );
						 assertThat( "[" + i + "]getNextBlock", actualNameRecord.NextBlock, @is( expectedNameRecord.NextBlock ) );
						 assertThat( "[" + i + "]getType", actualNameRecord.getType(), @is(expectedNameRecord.getType()) );
						 assertThat( "[" + i + "]getId", actualNameRecord.Id, @is( expectedNameRecord.Id ) );
						 assertThat( "[" + i + "]getLongId", actualNameRecord.Id, @is( expectedNameRecord.Id ) );
						 assertThat( "[" + i + "]isStartRecord", actualNameRecord.StartRecord, @is( expectedNameRecord.StartRecord ) );
						 assertThat( "[" + i + "]inUse", actualNameRecord.InUse(), @is(expectedNameRecord.InUse()) );
						 i++;
					}
			  }
		 }

		 // This one might be good enough to cover all AbstractDynamicStore subclasses,
		 // including DynamicArrayStore and DynamicStringStore.
		 public class SchemaStoreConsistentReadTest : RecordStoreConsistentReadTest<DynamicRecord, SchemaStore>
		 {
			  internal static readonly sbyte[] ExistingRecordData = "Random bytes".Bytes;

			  protected internal override SchemaStore GetStore( NeoStores neoStores )
			  {
					return neoStores.SchemaStore;
			  }

			  protected internal override DynamicRecord CreateNullRecord( long id )
			  {
					DynamicRecord record = new DynamicRecord( id );
					record.NextBlock = 0;
					record.Data = new sbyte[0];
					return record;
			  }

			  protected internal override DynamicRecord CreateExistingRecord( bool light )
			  {
					DynamicRecord record = new DynamicRecord( ID );
					record.InUse = true;
					record.StartRecord = true;
					record.Length = ExistingRecordData.Length;
					record.Data = ExistingRecordData;
					return record;
			  }

			  protected internal override DynamicRecord GetLight( long id, SchemaStore store )
			  {
					throw new AssumptionViolatedException( "Light loading of DynamicRecords is a little different" );
			  }

			  protected internal override void AssertRecordsEqual( DynamicRecord actualRecord, DynamicRecord expectedRecord )
			  {
					assertNotNull( "actualRecord", actualRecord );
					assertNotNull( "expectedRecord", expectedRecord );
					assertThat( "getData", actualRecord.Data, @is( expectedRecord.Data ) );
					assertThat( "getLength", actualRecord.Length, @is( expectedRecord.Length ) );
					assertThat( "getNextBlock", actualRecord.NextBlock, @is( expectedRecord.NextBlock ) );
					assertThat( "getType", actualRecord.getType(), @is(expectedRecord.getType()) );
					assertThat( "getId", actualRecord.Id, @is( expectedRecord.Id ) );
					assertThat( "getLongId", actualRecord.Id, @is( expectedRecord.Id ) );
					assertThat( "isStartRecord", actualRecord.StartRecord, @is( expectedRecord.StartRecord ) );
			  }
		 }

		 public class PropertyStoreConsistentReadTest : RecordStoreConsistentReadTest<PropertyRecord, PropertyStore>
		 {
			  protected internal override PropertyStore GetStore( NeoStores neoStores )
			  {
					return neoStores.PropertyStore;
			  }

			  protected internal override PropertyRecord CreateNullRecord( long id )
			  {
					PropertyRecord record = new PropertyRecord( id );
					record.NextProp = 0;
					record.PrevProp = 0;
					return record;
			  }

			  protected internal override PropertyRecord CreateExistingRecord( bool light )
			  {
					PropertyRecord record = new PropertyRecord( ID );
					record.Id = ID;
					record.NextProp = 2;
					record.PrevProp = 4;
					record.InUse = true;
					PropertyBlock block = new PropertyBlock();
					DynamicRecordAllocator stringAllocator = new ReusableRecordsAllocator( 64, new DynamicRecord( 7 ) );
					Value value = Values.of( "a string too large to fit in the property block itself" );
					PropertyStore.EncodeValue( block, 6, value, stringAllocator, null, true );
					if ( light )
					{
						 block.ValueRecords.Clear();
					}
					record.PropertyBlock = block;
					return record;
			  }

			  protected internal override PropertyRecord GetLight( long id, PropertyStore store )
			  {
					throw new AssumptionViolatedException( "Getting a light non-existing property record will throw." );
			  }

			  protected internal override PropertyRecord GetHeavy( PropertyStore store, int id )
			  {
					PropertyRecord record = base.GetHeavy( store, id );
					EnsureHeavy( store, record );
					return record;
			  }

			  internal static void EnsureHeavy( PropertyStore store, PropertyRecord record )
			  {
					foreach ( PropertyBlock propertyBlock in record )
					{
						 store.EnsureHeavy( propertyBlock );
					}
			  }

			  protected internal override void AssertRecordsEqual( PropertyRecord actualRecord, PropertyRecord expectedRecord )
			  {
					assertNotNull( "actualRecord", actualRecord );
					assertNotNull( "expectedRecord", expectedRecord );
					assertThat( "getDeletedRecords", actualRecord.DeletedRecords, @is( expectedRecord.DeletedRecords ) );
					assertThat( "getNextProp", actualRecord.NextProp, @is( expectedRecord.NextProp ) );
					assertThat( "getEntityId", actualRecord.NodeId, @is( expectedRecord.NodeId ) );
					assertThat( "getPrevProp", actualRecord.PrevProp, @is( expectedRecord.PrevProp ) );
					assertThat( "getRelId", actualRecord.RelId, @is( expectedRecord.RelId ) );
					assertThat( "getId", actualRecord.Id, @is( expectedRecord.Id ) );
					assertThat( "getLongId", actualRecord.Id, @is( expectedRecord.Id ) );

					IList<PropertyBlock> actualBlocks = Iterables.asList( actualRecord );
					IList<PropertyBlock> expectedBlocks = Iterables.asList( expectedRecord );
					assertThat( "getPropertyBlocks().size", actualBlocks.Count, @is( expectedBlocks.Count ) );
					for ( int i = 0; i < actualBlocks.Count; i++ )
					{
						 PropertyBlock actualBlock = actualBlocks[i];
						 PropertyBlock expectedBlock = expectedBlocks[i];
						 AssertPropertyBlocksEqual( i, actualBlock, expectedBlock );
					}
			  }

			  internal static void AssertPropertyBlocksEqual( int index, PropertyBlock actualBlock, PropertyBlock expectedBlock )
			  {
					assertThat( "[" + index + "]getKeyIndexId", actualBlock.KeyIndexId, @is( expectedBlock.KeyIndexId ) );
					assertThat( "[" + index + "]getSingleValueBlock", actualBlock.SingleValueBlock, @is( expectedBlock.SingleValueBlock ) );
					assertThat( "[" + index + "]getSingleValueByte", actualBlock.SingleValueByte, @is( expectedBlock.SingleValueByte ) );
					assertThat( "[" + index + "]getSingleValueInt", actualBlock.SingleValueInt, @is( expectedBlock.SingleValueInt ) );
					assertThat( "[" + index + "]getSingleValueLong", actualBlock.SingleValueLong, @is( expectedBlock.SingleValueLong ) );
					assertThat( "[" + index + "]getSingleValueShort", actualBlock.SingleValueShort, @is( expectedBlock.SingleValueShort ) );
					assertThat( "[" + index + "]getSize", actualBlock.Size, @is( expectedBlock.Size ) );
					assertThat( "[" + index + "]getType", actualBlock.Type, @is( expectedBlock.Type ) );
					assertThat( "[" + index + "]isLight", actualBlock.Light, @is( expectedBlock.Light ) );

					IList<DynamicRecord> actualValueRecords = actualBlock.ValueRecords;
					IList<DynamicRecord> expectedValueRecords = expectedBlock.ValueRecords;
					assertThat( "[" + index + "]getValueRecords.size", actualValueRecords.Count, @is( expectedValueRecords.Count ) );

					for ( int i = 0; i < actualValueRecords.Count; i++ )
					{
						 DynamicRecord actualValueRecord = actualValueRecords[i];
						 DynamicRecord expectedValueRecord = expectedValueRecords[i];
						 assertThat( "[" + index + "]getValueRecords[" + i + "]getData", actualValueRecord.Data, @is( expectedValueRecord.Data ) );
						 assertThat( "[" + index + "]getValueRecords[" + i + "]getLength", actualValueRecord.Length, @is( expectedValueRecord.Length ) );
						 assertThat( "[" + index + "]getValueRecords[" + i + "]getNextBlock", actualValueRecord.NextBlock, @is( expectedValueRecord.NextBlock ) );
						 assertThat( "[" + index + "]getValueRecords[" + i + "]getType", actualValueRecord.getType(), @is(expectedValueRecord.getType()) );
						 assertThat( "[" + index + "]getValueRecords[" + i + "]getId", actualValueRecord.Id, @is( expectedValueRecord.Id ) );
						 assertThat( "[" + index + "]getValueRecords[" + i + "]getLongId", actualValueRecord.Id, @is( expectedValueRecord.Id ) );
						 assertThat( "[" + index + "]getValueRecords[" + i + "]isStartRecord", actualValueRecord.StartRecord, @is( expectedValueRecord.StartRecord ) );
						 assertThat( "[" + index + "]getValueRecords[" + i + "]inUse", actualValueRecord.InUse(), @is(expectedValueRecord.InUse()) );
					}
			  }
		 }
	}

}