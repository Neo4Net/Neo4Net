using System.Collections.Generic;
using System.Text;

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
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class TestIdGeneratorRebuilding
	{
		private bool InstanceFieldsInitialized = false;

		public TestIdGeneratorRebuilding()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fsRule.get() );
			RuleChain = RuleChain.outerRule( _fsRule ).around( _testDirectory );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public static readonly PageCacheRule PageCacheRule = new PageCacheRule();
		 private EphemeralFileSystemRule _fsRule = new EphemeralFileSystemRule();
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fsRule).around(testDirectory);
		 public RuleChain RuleChain;

		 private EphemeralFileSystemAbstraction _fs;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void doBefore()
		 public virtual void DoBefore()
		 {
			  _fs = _fsRule.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void verifyFixedSizeStoresCanRebuildIdGeneratorSlowly()
		 public virtual void VerifyFixedSizeStoresCanRebuildIdGeneratorSlowly()
		 {
			  // Given we have a store ...
			  Config config = Config.defaults( GraphDatabaseSettings.rebuild_idgenerators_fast, "false" );
			  File storeFile = _testDirectory.file( "nodes" );
			  File idFile = _testDirectory.file( "idNodes" );

			  DynamicArrayStore labelStore = mock( typeof( DynamicArrayStore ) );
			  NodeStore store = new NodeStore( storeFile, idFile, config, new DefaultIdGeneratorFactory( _fs ), PageCacheRule.getPageCache( _fs ), NullLogProvider.Instance, labelStore, RecordFormatSelector.defaultFormat() );
			  store.Initialize( true );
			  store.MakeStoreOk();

			  // ... that contain a number of records ...
			  NodeRecord record = new NodeRecord( 0 );
			  record.InUse = true;
			  int highestId = 50;
			  for ( int i = 0; i < highestId; i++ )
			  {
					assertThat( store.NextId(), @is((long) i) );
					record.Id = i;
					store.UpdateRecord( record );
			  }
			  store.HighestPossibleIdInUse = highestId;

			  // ... and some have been deleted
			  long?[] idsToFree = new long?[] { 2L, 3L, 5L, 7L };
			  record.InUse = false;
			  foreach ( long toDelete in idsToFree )
			  {
					record.Id = toDelete;
					store.UpdateRecord( record );
			  }

			  // Then when we rebuild the id generator
			  store.RebuildIdGenerator();
			  store.CloseIdGenerator();
			  store.OpenIdGenerator(); // simulate a restart to allow id reuse

			  // We should observe that the ids above got freed
			  IList<long> nextIds = new List<long>();
			  nextIds.Add( store.NextId() ); // 2
			  nextIds.Add( store.NextId() ); // 3
			  nextIds.Add( store.NextId() ); // 5
			  nextIds.Add( store.NextId() ); // 7
			  nextIds.Add( store.NextId() ); // 51
			  assertThat( nextIds, contains( 2L, 3L, 5L, 7L, 50L ) );
			  store.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void verifyDynamicSizedStoresCanRebuildIdGeneratorSlowly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void VerifyDynamicSizedStoresCanRebuildIdGeneratorSlowly()
		 {
			  // Given we have a store ...
			  Config config = Config.defaults( GraphDatabaseSettings.rebuild_idgenerators_fast, "false" );

			  StoreFactory storeFactory = new StoreFactory( _testDirectory.databaseLayout(), config, new DefaultIdGeneratorFactory(_fs), PageCacheRule.getPageCache(_fs), _fs, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  NeoStores neoStores = storeFactory.OpenAllNeoStores( true );
			  DynamicStringStore store = neoStores.PropertyStore.StringStore;

			  // ... that contain a number of records ...
			  DynamicRecord record = new DynamicRecord( 1 );
			  record.SetInUse( true, PropertyType.String.intValue() );
			  int highestId = 50;
			  for ( int i = 1; i <= highestId; i++ ) // id '0' is the dynamic store header
			  {
					assertThat( store.NextId(), @is((long) i) );
					record.Id = i;
					StringBuilder sb = new StringBuilder( i );
					for ( int j = 0; j < i; j++ )
					{
						 sb.Append( 'a' );
					}
					record.Data = sb.ToString().GetBytes(StandardCharsets.UTF_16);
					store.UpdateRecord( record );
			  }
			  store.HighestPossibleIdInUse = highestId;

			  // ... and some have been deleted
			  long?[] idsToFree = new long?[] { 2L, 3L, 5L, 7L };
			  record.InUse = false;
			  foreach ( long toDelete in idsToFree )
			  {
					record.Id = toDelete;
					store.UpdateRecord( record );
			  }

			  // Then when we rebuild the id generator
			  store.RebuildIdGenerator();

			  // We should observe that the ids above got freed
			  IList<long> nextIds = new List<long>();
			  nextIds.Add( store.NextId() ); // 2
			  nextIds.Add( store.NextId() ); // 3
			  nextIds.Add( store.NextId() ); // 5
			  nextIds.Add( store.NextId() ); // 7
			  nextIds.Add( store.NextId() ); // 51
			  assertThat( nextIds, contains( 2L, 3L, 5L, 7L, 51L ) );
			  neoStores.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rebuildingIdGeneratorMustNotMissOutOnFreeRecordsAtEndOfFilePage()
		 public virtual void RebuildingIdGeneratorMustNotMissOutOnFreeRecordsAtEndOfFilePage()
		 {
			  // Given we have a store ...
			  Config config = Config.defaults( GraphDatabaseSettings.rebuild_idgenerators_fast, "false" );
			  File storeFile = _testDirectory.file( "nodes" );
			  File idFile = _testDirectory.file( "idNodes" );

			  DynamicArrayStore labelStore = mock( typeof( DynamicArrayStore ) );
			  NodeStore store = new NodeStore( storeFile, idFile, config, new DefaultIdGeneratorFactory( _fs ), PageCacheRule.getPageCache( _fs ), NullLogProvider.Instance, labelStore, RecordFormatSelector.defaultFormat() );
			  store.Initialize( true );
			  store.MakeStoreOk();

			  // ... that contain enough records to fill several file pages ...
			  int recordsPerPage = store.RecordsPerPage;
			  NodeRecord record = new NodeRecord( 0 );
			  record.InUse = true;
			  int highestId = recordsPerPage * 3; // 3 pages worth of records
			  for ( int i = 0; i < highestId; i++ )
			  {
					assertThat( store.NextId(), @is((long) i) );
					record.Id = i;
					store.UpdateRecord( record );
			  }
			  store.HighestPossibleIdInUse = highestId;

			  // ... and some records at the end of a page have been deleted
			  long?[] idsToFree = new long?[] { recordsPerPage - 2L, recordsPerPage - 1L }; // id's are zero based, hence -2 and -1
			  record.InUse = false;
			  foreach ( long toDelete in idsToFree )
			  {
					record.Id = toDelete;
					store.UpdateRecord( record );
			  }

			  // Then when we rebuild the id generator
			  store.RebuildIdGenerator();
			  store.CloseIdGenerator();
			  store.OpenIdGenerator(); // simulate a restart to allow id reuse

			  // We should observe that the ids above got freed
			  IList<long> nextIds = new List<long>();
			  nextIds.Add( store.NextId() ); // recordsPerPage - 2
			  nextIds.Add( store.NextId() ); // recordsPerPage - 1
			  nextIds.Add( store.NextId() ); // recordsPerPage * 3 (we didn't use this id in the create-look above)
			  assertThat( nextIds, contains( recordsPerPage - 2L, recordsPerPage - 1L, recordsPerPage * 3L ) );
			  store.Close();
		 }
	}

}