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
namespace Neo4Net.@unsafe.Impl.Batchimport.store
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Predicates = Neo4Net.Functions.Predicates;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.GraphDb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using Config = Neo4Net.Kernel.configuration.Config;
	using MyRelTypes = Neo4Net.Kernel.impl.MyRelTypes;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using Neo4Net.Kernel.impl.store;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using ForcedSecondaryUnitRecordFormats = Neo4Net.Kernel.impl.store.format.ForcedSecondaryUnitRecordFormats;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using NullLogService = Neo4Net.Logging.Internal.NullLogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using Input_Estimates = Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates;
	using Inputs = Neo4Net.@unsafe.Impl.Batchimport.input.Inputs;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
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
//	import static Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.format.standard.Standard.LATEST_RECORD_FORMATS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.Record.NULL_REFERENCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.AdditionalInitialIds.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.Configuration.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.store.BatchingNeoStores.DOUBLE_RELATIONSHIP_RECORD_UNIT_THRESHOLD;

	public class BatchingNeoStoresTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.PageCacheAndDependenciesRule storage = new Neo4Net.test.rule.PageCacheAndDependenciesRule();
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotOpenStoreWithNodesOrRelationshipsInIt() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotOpenStoreWithNodesOrRelationshipsInIt()
		 {
			  // GIVEN
			  SomeDataInTheDatabase();

			  // WHEN
			  try
			  {
					  using ( IJobScheduler jobScheduler = new ThreadPoolJobScheduler() )
					  {
						RecordFormats recordFormats = RecordFormatSelector.selectForConfig( Config.defaults(), NullLogProvider.Instance );
						using ( BatchingNeoStores store = BatchingNeoStores.BatchingNeoStoresConflict( Storage.fileSystem(), Storage.directory().databaseDir(), recordFormats, DEFAULT, NullLogService.Instance, EMPTY, Config.defaults(), jobScheduler ) )
						{
							 store.CreateNew();
							 fail( "Should fail on existing data" );
						}
					  }
			  }
			  catch ( System.InvalidOperationException e )
			  {
					// THEN
					assertThat( e.Message, containsString( "already contains" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectDbConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectDbConfig()
		 {
			  // GIVEN
			  int size = 10;
			  Config config = Config.defaults( stringMap( GraphDatabaseSettings.array_block_size.name(), size.ToString(), GraphDatabaseSettings.string_block_size.name(), size.ToString() ) );

			  // WHEN
			  RecordFormats recordFormats = LATEST_RECORD_FORMATS;
			  int headerSize = recordFormats.Dynamic().RecordHeaderSize;
			  using ( IJobScheduler jobScheduler = new ThreadPoolJobScheduler(), BatchingNeoStores store = BatchingNeoStores.BatchingNeoStoresConflict(Storage.fileSystem(), Storage.directory().absolutePath(), recordFormats, DEFAULT, NullLogService.Instance, EMPTY, config, jobScheduler) )
			  {
					store.CreateNew();

					// THEN
					assertEquals( size + headerSize, store.PropertyStore.ArrayStore.RecordSize );
					assertEquals( size + headerSize, store.PropertyStore.StringStore.RecordSize );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPruneAndOpenExistingDatabase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPruneAndOpenExistingDatabase()
		 {
			  // given
			  foreach ( StoreType typeToTest in RelevantRecordStores() )
			  {
					// given all the stores with some records in them
					using ( PageCache pageCache = Storage.pageCache() )
					{
						 Storage.directory().cleanup();
						 using ( BatchingNeoStores stores = BatchingNeoStores.BatchingNeoStoresWithExternalPageCache( Storage.fileSystem(), pageCache, PageCacheTracer.NULL, Storage.directory().absolutePath(), LATEST_RECORD_FORMATS, DEFAULT, NullLogService.Instance, EMPTY, Config.defaults() ) )
						 {
							  stores.CreateNew();
							  foreach ( StoreType type in RelevantRecordStores() )
							  {
									CreateRecordIn( stores.NeoStores.getRecordStore( type ) );
							  }
						 }

						 // when opening and pruning all except the one we test
						 using ( BatchingNeoStores stores = BatchingNeoStores.BatchingNeoStoresWithExternalPageCache( Storage.fileSystem(), pageCache, PageCacheTracer.NULL, Storage.directory().absolutePath(), LATEST_RECORD_FORMATS, DEFAULT, NullLogService.Instance, EMPTY, Config.defaults() ) )
						 {
							  stores.PruneAndOpenExistingStore( type => type == typeToTest, Predicates.alwaysFalse() );

							  // then only the one we kept should have data in it
							  foreach ( StoreType type in RelevantRecordStores() )
							  {
									RecordStore<AbstractBaseRecord> store = stores.NeoStores.getRecordStore( type );
									if ( type == typeToTest )
									{
										 assertThat( store.ToString(), (int) store.HighId, greaterThan(store.NumberOfReservedLowIds) );
									}
									else
									{
										 assertEquals( store.ToString(), store.NumberOfReservedLowIds, store.HighId );
									}
							  }
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDecideToAllocateDoubleRelationshipRecordUnitsOnLargeAmountOfRelationshipsOnSupportedFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDecideToAllocateDoubleRelationshipRecordUnitsOnLargeAmountOfRelationshipsOnSupportedFormat()
		 {
			  // given
			  RecordFormats formats = new ForcedSecondaryUnitRecordFormats( LATEST_RECORD_FORMATS );
			  using ( PageCache pageCache = Storage.pageCache(), BatchingNeoStores stores = BatchingNeoStores.BatchingNeoStoresWithExternalPageCache(Storage.fileSystem(), pageCache, PageCacheTracer.NULL, Storage.directory().absolutePath(), formats, DEFAULT, NullLogService.Instance, EMPTY, Config.defaults()) )
			  {
					stores.CreateNew();
					Input_Estimates estimates = Inputs.knownEstimates( 0, DOUBLE_RELATIONSHIP_RECORD_UNIT_THRESHOLD << 1, 0, 0, 0, 0, 0 );

					// when
					bool doubleUnits = stores.DetermineDoubleRelationshipRecordUnits( estimates );

					// then
					assertTrue( doubleUnits );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDecideToAllocateDoubleRelationshipRecordUnitsonLowAmountOfRelationshipsOnSupportedFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDecideToAllocateDoubleRelationshipRecordUnitsonLowAmountOfRelationshipsOnSupportedFormat()
		 {
			  // given
			  RecordFormats formats = new ForcedSecondaryUnitRecordFormats( LATEST_RECORD_FORMATS );
			  using ( BatchingNeoStores stores = BatchingNeoStores.BatchingNeoStoresWithExternalPageCache( Storage.fileSystem(), Storage.pageCache(), PageCacheTracer.NULL, Storage.directory().absolutePath(), formats, DEFAULT, NullLogService.Instance, EMPTY, Config.defaults() ) )
			  {
					stores.CreateNew();
					Input_Estimates estimates = Inputs.knownEstimates( 0, DOUBLE_RELATIONSHIP_RECORD_UNIT_THRESHOLD >> 1, 0, 0, 0, 0, 0 );

					// when
					bool doubleUnits = stores.DetermineDoubleRelationshipRecordUnits( estimates );

					// then
					assertFalse( doubleUnits );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDecideToAllocateDoubleRelationshipRecordUnitsonLargeAmountOfRelationshipsOnUnsupportedFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDecideToAllocateDoubleRelationshipRecordUnitsonLargeAmountOfRelationshipsOnUnsupportedFormat()
		 {
			  // given
			  RecordFormats formats = LATEST_RECORD_FORMATS;
			  using ( BatchingNeoStores stores = BatchingNeoStores.BatchingNeoStoresWithExternalPageCache( Storage.fileSystem(), Storage.pageCache(), PageCacheTracer.NULL, Storage.directory().absolutePath(), formats, DEFAULT, NullLogService.Instance, EMPTY, Config.defaults() ) )
			  {
					stores.CreateNew();
					Input_Estimates estimates = Inputs.knownEstimates( 0, DOUBLE_RELATIONSHIP_RECORD_UNIT_THRESHOLD << 1, 0, 0, 0, 0, 0 );

					// when
					bool doubleUnits = stores.DetermineDoubleRelationshipRecordUnits( estimates );

					// then
					assertFalse( doubleUnits );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteIdGeneratorsWhenOpeningExistingStore() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeleteIdGeneratorsWhenOpeningExistingStore()
		 {
			  // given
			  long expectedHighId;
			  using ( BatchingNeoStores stores = BatchingNeoStores.BatchingNeoStoresWithExternalPageCache( Storage.fileSystem(), Storage.pageCache(), PageCacheTracer.NULL, Storage.directory().absolutePath(), LATEST_RECORD_FORMATS, DEFAULT, NullLogService.Instance, EMPTY, Config.defaults() ) )
			  {
					stores.CreateNew();
					RelationshipStore relationshipStore = stores.RelationshipStore;
					RelationshipRecord record = relationshipStore.NewRecord();
					long no = NULL_REFERENCE.longValue();
					record.Initialize( true, no, 1, 2, 0, no, no, no, no, true, true );
					record.Id = relationshipStore.NextId();
					expectedHighId = relationshipStore.HighId;
					relationshipStore.UpdateRecord( record );
					// fiddle with the highId
					relationshipStore.HighId = record.Id + 999;
			  }

			  // when
			  using ( BatchingNeoStores stores = BatchingNeoStores.BatchingNeoStoresWithExternalPageCache( Storage.fileSystem(), Storage.pageCache(), PageCacheTracer.NULL, Storage.directory().absolutePath(), LATEST_RECORD_FORMATS, DEFAULT, NullLogService.Instance, EMPTY, Config.defaults() ) )
			  {
					stores.PruneAndOpenExistingStore( Predicates.alwaysTrue(), Predicates.alwaysTrue() );

					// then
					assertEquals( expectedHighId, stores.RelationshipStore.HighId );
			  }
		 }

		 private StoreType[] RelevantRecordStores()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Stream.of( StoreType.values() ).filter(type => type.RecordStore && type != StoreType.META_DATA).toArray(StoreType[]::new);
		 }

		 private void CreateRecordIn<RECORD>( RecordStore<RECORD> store ) where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  RECORD record = store.NewRecord();
			  record.Id = store.nextId();
			  record.InUse = true;
			  if ( record is PropertyRecord )
			  {
					// Special hack for property store, since it's not enough to simply set a record as in use there
					PropertyBlock block = new PropertyBlock();
					( ( PropertyStore )store ).encodeValue( block, 0, Values.of( 10 ) );
					( ( PropertyRecord ) record ).addPropertyBlock( block );
			  }
			  store.UpdateRecord( record );
		 }

		 private void SomeDataInTheDatabase()
		 {
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(new UncloseableDelegatingFileSystemAbstraction(Storage.fileSystem())).newImpermanentDatabase(Storage.directory().databaseDir());
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						Db.createNode().createRelationshipTo(Db.createNode(), MyRelTypes.TEST);
						tx.Success();
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }
	}

}