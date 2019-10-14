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
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using Neo4Net.Helpers.Collections;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using Bits = Neo4Net.Kernel.impl.util.Bits;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using UTF8 = Neo4Net.Strings.UTF8;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class TestArrayStore
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public static readonly PageCacheRule PageCacheRule = new PageCacheRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();

		 private DynamicArrayStore _arrayStore;
		 private NeoStores _neoStores;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  FileSystemAbstraction fs = FileSystemRule.get();
			  DefaultIdGeneratorFactory idGeneratorFactory = new DefaultIdGeneratorFactory( fs );
			  PageCache pageCache = PageCacheRule.getPageCache( fs );
			  StoreFactory factory = new StoreFactory( TestDirectory.databaseLayout(), Config.defaults(), idGeneratorFactory, pageCache, fs, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  _neoStores = factory.OpenAllNeoStores( true );
			  _arrayStore = _neoStores.PropertyStore.ArrayStore;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  if ( _neoStores != null )
			  {
					_neoStores.close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void intArrayPropertiesShouldBeBitPacked()
		 public virtual void IntArrayPropertiesShouldBeBitPacked()
		 {
			  AssertBitPackedArrayGetsCorrectlySerializedAndDeserialized( new int[] { 1, 2, 3, 4, 5, 6, 7 }, PropertyType.Int, 3 );
			  AssertBitPackedArrayGetsCorrectlySerializedAndDeserialized( new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }, PropertyType.Int, 4 );
			  AssertBitPackedArrayGetsCorrectlySerializedAndDeserialized( new int[] { 1000, 10000, 13000 }, PropertyType.Int, 14 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void longArrayPropertiesShouldBeBitPacked()
		 public virtual void LongArrayPropertiesShouldBeBitPacked()
		 {
			  AssertBitPackedArrayGetsCorrectlySerializedAndDeserialized( new long[] { 1, 2, 3, 4, 5, 6, 7 }, PropertyType.Long, 3 );
			  AssertBitPackedArrayGetsCorrectlySerializedAndDeserialized( new long[] { 1, 2, 3, 4, 5, 6, 7, 8 }, PropertyType.Long, 4 );
			  AssertBitPackedArrayGetsCorrectlySerializedAndDeserialized( new long[]{ 1000, 10000, 13000, 15000000000L }, PropertyType.Long, 34 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doubleArrayPropertiesShouldNotBeBitPacked()
		 public virtual void DoubleArrayPropertiesShouldNotBeBitPacked()
		 {
			  //TODO Enabling right-trim would allow doubles that are integers, like 42.0, to pack well
			  //While enabling the default left-trim would only allow some extreme doubles to pack, like Double.longBitsToDouble( 0x1L )

			  // Test doubles that pack well with right-trim
			  AssertBitPackedArrayGetsCorrectlySerializedAndDeserialized( new double[]{ 0.0, -100.0, 100.0, 0.5 }, PropertyType.Double, 64 );
			  // Test doubles that pack well with left-trim
			  AssertBitPackedArrayGetsCorrectlySerializedAndDeserialized( new double[]{ Double.longBitsToDouble( 0x1L ), Double.longBitsToDouble( 0x8L ) }, PropertyType.Double, 64 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void byteArrayPropertiesShouldNotBeBitPacked()
		 public virtual void ByteArrayPropertiesShouldNotBeBitPacked()
		 {
			  /* Byte arrays are always stored unpacked. For two reasons:
			   * - They are very unlikely to gain anything from bit packing
			   * - byte[] are often used for storing big arrays and the bigger the long
			   *   any bit analysis would take. For both writing and reading */
			  AssertBitPackedArrayGetsCorrectlySerializedAndDeserialized( new sbyte[] { 1, 2, 3, 4, 5 }, PropertyType.Byte, ( sizeof( sbyte ) * 8 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stringArrayGetsStoredAsUtf8()
		 public virtual void StringArrayGetsStoredAsUtf8()
		 {
			  string[] array = new string[] { "first", "second" };
			  ICollection<DynamicRecord> records = new List<DynamicRecord>();
			  _arrayStore.allocateRecords( records, array );
			  Pair<sbyte[], sbyte[]> loaded = LoadArray( records );
			  AssertStringHeader( loaded.First(), array.Length );
			  ByteBuffer buffer = ByteBuffer.wrap( loaded.Other() );
			  foreach ( string item in array )
			  {
					sbyte[] expectedData = UTF8.encode( item );
					assertEquals( expectedData.Length, buffer.Int );
					sbyte[] loadedItem = new sbyte[expectedData.Length];
					buffer.get( loadedItem );
					assertTrue( Arrays.Equals( expectedData, loadedItem ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pointArraysOfWgs84()
		 public virtual void PointArraysOfWgs84()
		 {
			  PointValue[] array = new PointValue[]{ Values.pointValue( CoordinateReferenceSystem.WGS84, -45.0, -45.0 ), Values.pointValue( CoordinateReferenceSystem.WGS84, 12.8, 56.3 ) };
			  int numberOfBitsUsedForDoubles = 64;

			  AssertPointArrayHasCorrectFormat( array, numberOfBitsUsedForDoubles );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pointArraysOfCartesian()
		 public virtual void PointArraysOfCartesian()
		 {
			  PointValue[] array = new PointValue[]{ Values.pointValue( CoordinateReferenceSystem.Cartesian, -100.0, -100.0 ), Values.pointValue( CoordinateReferenceSystem.Cartesian, 25.0, 50.5 ) };
			  int numberOfBitsUsedForDoubles = 64;

			  AssertPointArrayHasCorrectFormat( array, numberOfBitsUsedForDoubles );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void pointArraysOfMixedCRS()
		 public virtual void PointArraysOfMixedCRS()
		 {
			  PointValue[] array = new PointValue[]{ Values.pointValue( CoordinateReferenceSystem.Cartesian, Double.longBitsToDouble( 0x1L ), Double.longBitsToDouble( 0x7L ) ), Values.pointValue( CoordinateReferenceSystem.WGS84, Double.longBitsToDouble( 0x1L ), Double.longBitsToDouble( 0x1L ) ) };

			  ICollection<DynamicRecord> records = new List<DynamicRecord>();
			  _arrayStore.allocateRecords( records, array );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void pointArraysOfMixedDimension()
		 public virtual void PointArraysOfMixedDimension()
		 {
			  PointValue[] array = new PointValue[]{ Values.pointValue( CoordinateReferenceSystem.Cartesian, Double.longBitsToDouble( 0x1L ), Double.longBitsToDouble( 0x7L ) ), Values.pointValue( CoordinateReferenceSystem.Cartesian, Double.longBitsToDouble( 0x1L ), Double.longBitsToDouble( 0x1L ), Double.longBitsToDouble( 0x4L ) ) };

			  ICollection<DynamicRecord> records = new List<DynamicRecord>();
			  _arrayStore.allocateRecords( records, array );
		 }

		 private void AssertPointArrayHasCorrectFormat( PointValue[] array, int numberOfBitsUsedForDoubles )
		 {
			  ICollection<DynamicRecord> records = new List<DynamicRecord>();
			  _arrayStore.allocateRecords( records, array );
			  Pair<sbyte[], sbyte[]> loaded = LoadArray( records );
			  AssertGeometryHeader( loaded.First(), GeometryType.GeometryPoint.Gtype, 2, array[0].CoordinateReferenceSystem.Table.TableId, array[0].CoordinateReferenceSystem.Code );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int dimension = array[0].coordinate().length;
			  int dimension = array[0].Coordinate().Length;
			  double[] pointDoubles = new double[array.Length * dimension];
			  for ( int i = 0; i < pointDoubles.Length; i++ )
			  {
					pointDoubles[i] = array[i / dimension].Coordinate()[i % dimension];
			  }

			  sbyte[] doubleHeader = Arrays.copyOf( loaded.Other(), DynamicArrayStore.NUMBER_HEADER_SIZE );
			  sbyte[] doubleBody = Arrays.copyOfRange( loaded.Other(), DynamicArrayStore.NUMBER_HEADER_SIZE, loaded.Other().Length );
			  AssertNumericArrayHeaderAndContent( pointDoubles, PropertyType.Double, numberOfBitsUsedForDoubles, Pair.of( doubleHeader, doubleBody ) );
		 }

		 private void AssertStringHeader( sbyte[] header, int itemCount )
		 {
			  assertEquals( PropertyType.String.byteValue(), header[0] );
			  assertEquals( itemCount, ByteBuffer.wrap( header, 1, 4 ).Int );
		 }

		 private void AssertGeometryHeader( sbyte[] header, int geometryTpe, int dimension, int crsTableId, int crsCode )
		 {
			  assertEquals( PropertyType.Geometry.byteValue(), header[0] );
			  assertEquals( geometryTpe, header[1] );
			  assertEquals( dimension, header[2] );
			  assertEquals( crsTableId, header[3] );
			  assertEquals( crsCode, ByteBuffer.wrap( header, 4, 2 ).Short );
		 }

		 private void AssertBitPackedArrayGetsCorrectlySerializedAndDeserialized( object array, PropertyType type, int expectedBitsUsedPerItem )
		 {
			  ICollection<DynamicRecord> records = StoreArray( array );
			  Pair<sbyte[], sbyte[]> asBytes = LoadArray( records );
			  AssertNumericArrayHeaderAndContent( array, type, expectedBitsUsedPerItem, asBytes );
		 }

		 private void AssertNumericArrayHeaderAndContent( object array, PropertyType type, int expectedBitsUsedPerItem, Pair<sbyte[], sbyte[]> loadedBytesFromStore )
		 {
			  AssertArrayHeader( loadedBytesFromStore.First(), type, expectedBitsUsedPerItem );
			  Bits bits = Bits.bitsFromBytes( loadedBytesFromStore.Other() );
			  int length = Array.getLength( array );
			  for ( int i = 0; i < length; i++ )
			  {
					if ( array is double[] )
					{
						 assertEquals( System.BitConverter.DoubleToInt64Bits( Array.getDouble( array, i ) ), bits.GetLong( expectedBitsUsedPerItem ) );
					}
					else
					{
						 assertEquals( Array.getLong( array, i ), bits.GetLong( expectedBitsUsedPerItem ) );
					}
			  }
		 }

		 private void AssertArrayHeader( sbyte[] header, PropertyType type, int bitsPerItem )
		 {
			  assertEquals( type.byteValue(), header[0] );
			  assertEquals( bitsPerItem, header[2] );
		 }

		 private ICollection<DynamicRecord> StoreArray( object array )
		 {
			  ICollection<DynamicRecord> records = new List<DynamicRecord>();
			  _arrayStore.allocateRecords( records, array );
			  foreach ( DynamicRecord record in records )
			  {
					_arrayStore.updateRecord( record );
			  }
			  return records;
		 }

		 private Pair<sbyte[], sbyte[]> LoadArray( ICollection<DynamicRecord> records )
		 {
			  return _arrayStore.readFullByteArray( records, PropertyType.Array );
		 }
	}

}