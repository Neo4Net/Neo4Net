using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.store
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;

	public class TestDynamicStore
	{
		private bool InstanceFieldsInitialized = false;

		public TestDynamicStore()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fs );
			Chain = RuleChain.outerRule( _fs ).around( _testDirectory ).around( _pageCacheRule );
		}


		 private readonly EphemeralFileSystemRule _fs = new EphemeralFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain chain = org.junit.rules.RuleChain.outerRule(fs).around(testDirectory).around(pageCacheRule);
		 public RuleChain Chain;

		 private StoreFactory _storeFactory;
		 private NeoStores _neoStores;
		 private Config _config;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _config = _config();
			  _storeFactory = new StoreFactory( _testDirectory.databaseLayout(), _config, new DefaultIdGeneratorFactory(_fs.get()), _pageCacheRule.getPageCache(_fs.get()), _fs.get(), NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  if ( _neoStores != null )
			  {
					_neoStores.close();
			  }
		 }

		 private DynamicArrayStore CreateDynamicArrayStore()
		 {
			  _neoStores = _storeFactory.openAllNeoStores( true );
			  return _neoStores.PropertyStore.ArrayStore;
		 }

		 private Config Config()
		 {
			  return Config.defaults();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testClose()
		 public virtual void TestClose()
		 {
			  DynamicArrayStore store = CreateDynamicArrayStore();
			  ICollection<DynamicRecord> records = new List<DynamicRecord>();
			  store.AllocateRecordsFromBytes( records, new sbyte[10] );
			  long blockId = Iterables.first( records ).Id;
			  foreach ( DynamicRecord record in records )
			  {
					store.UpdateRecord( record );
			  }
			  _neoStores.close();
			  _neoStores = null;
			  try
			  {
					store.GetArrayFor( store.GetRecords( blockId, NORMAL ) );
					fail( "Closed store should throw exception" );
			  }
			  catch ( Exception )
			  { // good
			  }
			  try
			  {
					store.GetRecords( 0, NORMAL );
					fail( "Closed store should throw exception" );
			  }
			  catch ( Exception )
			  { // good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStoreGetCharsFromString()
		 public virtual void TestStoreGetCharsFromString()
		 {
			  const string str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			  DynamicArrayStore store = CreateDynamicArrayStore();
			  char[] chars = new char[str.Length];
			  str.CopyTo( 0, chars, 0, str.Length - 0 );
			  ICollection<DynamicRecord> records = new List<DynamicRecord>();
			  store.AllocateRecords( records, chars );
			  foreach ( DynamicRecord record in records )
			  {
					store.UpdateRecord( record );
			  }
			  // assertEquals( STR, new String( store.getChars( blockId ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRandomTest()
		 public virtual void TestRandomTest()
		 {
			  Random random = new Random( DateTimeHelper.CurrentUnixTimeMillis() );
			  DynamicArrayStore store = CreateDynamicArrayStore();
			  List<long> idsTaken = new List<long>();
			  IDictionary<long, sbyte[]> byteData = new Dictionary<long, sbyte[]>();
			  float deleteIndex = 0.2f;
			  float closeIndex = 0.1f;
			  int currentCount = 0;
			  int maxCount = 128;
			  ISet<long> set = new HashSet<long>();
			  while ( currentCount < maxCount )
			  {
					float rIndex = random.nextFloat();
					if ( rIndex < deleteIndex && currentCount > 0 )
					{
						 long blockId = idsTaken.Remove( random.Next( currentCount ) );
						 store.GetRecords( blockId, NORMAL );
						 sbyte[] bytes = ( sbyte[] ) store.GetArrayFor( store.GetRecords( blockId, NORMAL ) );
						 ValidateData( bytes, byteData.Remove( blockId ) );
						 ICollection<DynamicRecord> records = store.GetRecords( blockId, NORMAL );
						 foreach ( DynamicRecord record in records )
						 {
							  record.InUse = false;
							  store.UpdateRecord( record );
							  set.remove( record.Id );
						 }
						 currentCount--;
					}
					else
					{
						 sbyte[] bytes = CreateRandomBytes( random );
						 ICollection<DynamicRecord> records = new List<DynamicRecord>();
						 store.AllocateRecords( records, bytes );
						 foreach ( DynamicRecord record in records )
						 {
							  Debug.Assert( !set.Contains( record.Id ) );
							  store.UpdateRecord( record );
							  set.Add( record.Id );
						 }
						 long blockId = Iterables.first( records ).Id;
						 idsTaken.Add( blockId );
						 byteData[blockId] = bytes;
						 currentCount++;
					}
					if ( rIndex > ( 1.0f - closeIndex ) || rIndex < closeIndex )
					{
						 _neoStores.close();
						 store = CreateDynamicArrayStore();
					}
			  }
		 }

		 private sbyte[] CreateBytes( int length )
		 {
			  return new sbyte[length];
		 }

		 private sbyte[] CreateRandomBytes( Random r )
		 {
			  return new sbyte[r.Next( 1024 )];
		 }

		 private void ValidateData( sbyte[] data1, sbyte[] data2 )
		 {
			  assertEquals( data1.Length, data2.Length );
			  for ( int i = 0; i < data1.Length; i++ )
			  {
					assertEquals( data1[i], data2[i] );
			  }
		 }

		 private long Create( DynamicArrayStore store, object arrayToStore )
		 {
			  ICollection<DynamicRecord> records = new List<DynamicRecord>();
			  store.AllocateRecords( records, arrayToStore );
			  foreach ( DynamicRecord record in records )
			  {
					store.UpdateRecord( record );
			  }
			  return Iterables.first( records ).Id;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAddDeleteSequenceEmptyNumberArray()
		 public virtual void TestAddDeleteSequenceEmptyNumberArray()
		 {
			  DynamicArrayStore store = CreateDynamicArrayStore();
			  sbyte[] emptyToWrite = CreateBytes( 0 );
			  long blockId = Create( store, emptyToWrite );
			  store.GetRecords( blockId, NORMAL );
			  sbyte[] bytes = ( sbyte[] ) store.GetArrayFor( store.GetRecords( blockId, NORMAL ) );
			  assertEquals( 0, bytes.Length );

			  ICollection<DynamicRecord> records = store.GetRecords( blockId, NORMAL );
			  foreach ( DynamicRecord record in records )
			  {
					record.InUse = false;
					store.UpdateRecord( record );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAddDeleteSequenceEmptyStringArray()
		 public virtual void TestAddDeleteSequenceEmptyStringArray()
		 {
			  DynamicArrayStore store = CreateDynamicArrayStore();
			  long blockId = Create( store, new string[0] );
			  store.GetRecords( blockId, NORMAL );
			  string[] readBack = ( string[] ) store.GetArrayFor( store.GetRecords( blockId, NORMAL ) );
			  assertEquals( 0, readBack.Length );

			  ICollection<DynamicRecord> records = store.GetRecords( blockId, NORMAL );
			  foreach ( DynamicRecord record in records )
			  {
					record.InUse = false;
					store.UpdateRecord( record );
			  }
		 }
	}

}