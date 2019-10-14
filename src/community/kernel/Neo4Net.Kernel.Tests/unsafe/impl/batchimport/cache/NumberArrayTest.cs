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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
	using AfterAll = org.junit.jupiter.api.AfterAll;
	using BeforeAll = org.junit.jupiter.api.BeforeAll;
	using DynamicTest = org.junit.jupiter.api.DynamicTest;
	using TestFactory = org.junit.jupiter.api.TestFactory;
	using ThrowingConsumer = org.junit.jupiter.api.function.ThrowingConsumer;


	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.AUTO_WITHOUT_PAGECACHE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.CHUNKED_FIXED_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.HEAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.OFF_HEAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.auto;

	internal class NumberArrayTest : NumberArrayPageCacheTestSupport
	{
		 private static readonly RandomRule _random = new RandomRule();
		 private const int INDEXES = 50_000;
		 private static readonly int _chunkSize = max( 1, INDEXES / 100 );
		 private static Fixture _fixture;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeAll static void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal static void SetUp()
		 {
			  _fixture = PrepareDirectoryAndPageCache( typeof( NumberArrayTest ) );
			  _random.reset();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterAll static void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal static void TearDown()
		 {
			  _fixture.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @TestFactory Stream<org.junit.jupiter.api.DynamicTest> shouldGetAndSetRandomItems()
		 internal virtual Stream<DynamicTest> ShouldGetAndSetRandomItems()
		 {
			  ThrowingConsumer<NumberArrayTestData> throwingConsumer = data =>
			  {
				using ( NumberArray array = data.array )
				{
					 IDictionary<int, object> key = new Dictionary<int, object>();
					 Reader reader = data.reader;
					 object defaultValue = reader.read( array, 0 );

					 // WHEN setting random items
					 for ( int i = 0; i < INDEXES * 2; i++ )
					 {
						  int index = _random.Next( INDEXES );
						  object value = data.valueGenerator.apply( _random );
						  data.writer.write( i % 2 == 0 ? array : array.at( index ), index, value );
						  key.put( index, value );
					 }

					 // THEN they should be read correctly
					 AssertAllValues( key, defaultValue, reader, array );

					 // AND WHEN swapping some
					 for ( int i = 0; i < INDEXES / 2; i++ )
					 {
						  int fromIndex = _random.Next( INDEXES );
						  int toIndex;
						  do
						  {
								toIndex = _random.Next( INDEXES );
						  } while ( toIndex == fromIndex );
						  object fromValue = reader.read( array, fromIndex );
						  object toValue = reader.read( array, toIndex );
						  key.put( fromIndex, toValue );
						  key.put( toIndex, fromValue );
						  array.swap( fromIndex, toIndex );
					 }

					 // THEN they should end up in the correct places
					 AssertAllValues( key, defaultValue, reader, array );
				}
			  };
			  return DynamicTest.stream( Arrays().GetEnumerator(), data => data.name, throwingConsumer );
		 }

		 public static ICollection<NumberArrayTestData> Arrays()
		 {
			  PageCache pageCache = _fixture.pageCache;
			  File dir = _fixture.directory;
			  ICollection<NumberArrayTestData> list = new List<NumberArrayTestData>();
			  IDictionary<string, NumberArrayFactory> factories = new Dictionary<string, NumberArrayFactory>();
			  factories["HEAP"] = HEAP;
			  factories["OFF_HEAP"] = OFF_HEAP;
			  factories["AUTO_WITHOUT_PAGECACHE"] = AUTO_WITHOUT_PAGECACHE;
			  factories["CHUNKED_FIXED_SIZE"] = CHUNKED_FIXED_SIZE;
			  factories["autoWithPageCacheFallback"] = auto( pageCache, dir, true, NO_MONITOR );
			  factories["PageCachedNumberArrayFactory"] = new PageCachedNumberArrayFactory( pageCache, dir );
			  foreach ( KeyValuePair<string, NumberArrayFactory> entry in factories.SetOfKeyValuePairs() )
			  {
					string name = entry.Key + " => ";
					NumberArrayFactory factory = entry.Value;
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					list.Add( ArrayData( name + "IntArray", factory.NewIntArray( INDEXES, -1 ), _random => _random.Next( 1_000_000_000 ), ( array, index, value ) => array.set( index, ( int? ) value ), IntArray::get ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					list.Add( ArrayData( name + "DynamicIntArray", factory.NewDynamicIntArray( _chunkSize, -1 ), _random => _random.Next( 1_000_000_000 ), ( array, index, value ) => array.set( index, ( int? ) value ), IntArray::get ) );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					list.Add( ArrayData( name + "LongArray", factory.NewLongArray( INDEXES, -1 ), _random => _random.nextLong( 1_000_000_000 ), ( array, index, value ) => array.set( index, ( long? ) value ), LongArray::get ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					list.Add( ArrayData( name + "DynamicLongArray", factory.NewDynamicLongArray( _chunkSize, -1 ), _random => _random.nextLong( 1_000_000_000 ), ( array, index, value ) => array.set( index, ( long? ) value ), LongArray::get ) );

					list.Add( ArrayData( name + "ByteArray5", factory.NewByteArray( INDEXES, DefaultByteArray( 5 ) ), _random => _random.Next( 1_000_000_000 ), ( array, index, value ) => array.setInt( index, 1, ( int? ) value ), ( array, index ) => array.getInt( index, 1 ) ) );
					list.Add( ArrayData( name + "DynamicByteArray5", factory.NewDynamicByteArray( _chunkSize, DefaultByteArray( 5 ) ), _random => _random.Next( 1_000_000_000 ), ( array, index, value ) => array.setInt( index, 1, ( int? ) value ), ( array, index ) => array.getInt( index, 1 ) ) );

					System.Func<RandomRule, object> valueGenerator = _random => new long[]{ _random.nextLong(), _random.Next(), (short) _random.Next(), (sbyte) _random.Next() };
					Writer<ByteArray> writer = ( array, index, value ) =>
					{
					 long[] values = ( long[] ) value;
					 array.setLong( index, 0, values[0] );
					 array.setInt( index, 8, ( int ) values[1] );
					 array.setShort( index, 12, ( short ) values[2] );
					 array.setByte( index, 14, ( sbyte ) values[3] );
					};
					Reader<ByteArray> reader = ( array, index ) => new long[]{ array.getLong( index, 0 ), array.getInt( index, 8 ), array.getShort( index, 12 ), array.getByte( index, 14 ) };
					list.Add( ArrayData( name + "ByteArray15", factory.NewByteArray( INDEXES, DefaultByteArray( 15 ) ), valueGenerator, writer, reader ) );
					list.Add( ArrayData( name + "DynamicByteArray15", factory.NewDynamicByteArray( _chunkSize, DefaultByteArray( 15 ) ), valueGenerator, writer, reader ) );
			  }
			  return list;
		 }

		 internal interface Writer<N> where N : NumberArray<N>
		 {
			  void Write( N array, int index, object value );
		 }

		 internal interface Reader<N> where N : NumberArray<N>
		 {
			  object Read( N array, int index );
		 }

		 private class NumberArrayTestData<T> where T : NumberArray<T>
		 {
			  internal string Name;
			  internal T Array;
			  internal System.Func<RandomRule, object> ValueGenerator;
			  internal Writer<T> Writer;
			  internal Reader<T> Reader;

			  internal NumberArrayTestData( string name, T array, System.Func<RandomRule, object> valueGenerator, Writer<T> writer, Reader<T> reader )
			  {
					this.Name = name;
					this.Array = array;
					this.ValueGenerator = valueGenerator;
					this.Writer = writer;
					this.Reader = reader;
			  }
		 }

		 private static sbyte[] DefaultByteArray( int length )
		 {
			  sbyte[] result = new sbyte[length];
			  Arrays.fill( result, ( sbyte ) - 1 );
			  return result;
		 }

		 private static NumberArrayTestData ArrayData<N>( string name, N array, System.Func<RandomRule, object> valueGenerator, Writer<N> writer, Reader<N> reader ) where N : NumberArray<N>
		 {
			  return new NumberArrayTestData( name, array, valueGenerator, writer, reader );
		 }

		 private static void AssertAllValues( IDictionary<int, object> key, object defaultValue, Reader reader, NumberArray array )
		 {
			  for ( int index = 0; index < INDEXES; index++ )
			  {
					object value = reader.read( index % 2 == 0 ? array : array.at( index ), index );
					object expectedValue = key.getOrDefault( index, defaultValue );
					if ( value is long[] )
					{
						 assertArrayEquals( ( long[] ) expectedValue, ( long[] ) value, "index " + index );
					}
					else
					{
						 assertEquals( expectedValue, value, "index " + index );
					}
			  }
		 }
	}

}