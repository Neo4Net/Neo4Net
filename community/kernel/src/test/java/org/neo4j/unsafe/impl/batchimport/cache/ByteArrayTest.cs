using System;
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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache
{
	using After = org.junit.After;
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using PageCache = Org.Neo4j.Io.pagecache.PageCache;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.AUTO_WITHOUT_PAGECACHE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.HEAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.OFF_HEAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.auto;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ByteArrayTest extends NumberArrayPageCacheTestSupport
	public class ByteArrayTest : NumberArrayPageCacheTestSupport
	{
		 private static readonly sbyte[] @default = new sbyte[50];
		 private const int LENGTH = 1_000;
		 private static Fixture _fixture;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters public static java.util.Collection<System.Func<ByteArray>> data() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static ICollection<System.Func<ByteArray>> Data()
		 {
			  _fixture = PrepareDirectoryAndPageCache( typeof( ByteArrayTest ) );
			  PageCache pageCache = _fixture.pageCache;
			  File dir = _fixture.directory;
			  NumberArrayFactory autoWithPageCacheFallback = auto( pageCache, dir, true, NO_MONITOR );
			  NumberArrayFactory pageCacheArrayFactory = new PageCachedNumberArrayFactory( pageCache, dir );
			  int chunkSize = LENGTH / ChunkedNumberArrayFactory.MAGIC_CHUNK_COUNT;
			  return Arrays.asList( () => HEAP.newByteArray(LENGTH, @default), () => HEAP.newDynamicByteArray(chunkSize, @default), () => OFF_HEAP.newByteArray(LENGTH, @default), () => OFF_HEAP.newDynamicByteArray(chunkSize, @default), () => AUTO_WITHOUT_PAGECACHE.newByteArray(LENGTH, @default), () => AUTO_WITHOUT_PAGECACHE.newDynamicByteArray(chunkSize, @default), () => autoWithPageCacheFallback.NewByteArray(LENGTH, @default), () => autoWithPageCacheFallback.NewDynamicByteArray(chunkSize, @default), () => pageCacheArrayFactory.NewByteArray(LENGTH, @default), () => pageCacheArrayFactory.NewDynamicByteArray(chunkSize, @default) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void closeFixture() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void CloseFixture()
		 {
			  _fixture.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public System.Func<ByteArray> factory;
		 public System.Func<ByteArray> Factory;
		 private ByteArray _array;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _array = Factory.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _array.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetAndGetBasicTypes()
		 public virtual void ShouldSetAndGetBasicTypes()
		 {
			  int index = 0;
			  sbyte[] actualBytes = new sbyte[@default.Length];
			  sbyte[] expectedBytes = new sbyte[actualBytes.Length];
			  ThreadLocalRandom.current().NextBytes(actualBytes);

			  int len = LENGTH - 1; // subtract one because we access TWO elements.
			  for ( int i = 0; i < len; i++ )
			  {
					try
					{
						 // WHEN
						 SimpleValues = index;
						 SetArray( index + 1, actualBytes );

						 // THEN
						 VerifySimpleValues( index );
						 VerifyArray( index + 1, actualBytes, expectedBytes );
					}
					catch ( Exception throwable )
					{
						 throw new AssertionError( "Failure at index " + i, throwable );
					}
			  }
		 }

		 private int SimpleValues
		 {
			 set
			 {
				  _array.setByte( value, 0, ( sbyte ) 123 );
				  _array.setShort( value, 1, ( short ) 1234 );
				  _array.setInt( value, 5, 12345 );
				  _array.setLong( value, 9, long.MaxValue - 100 );
				  _array.set3ByteInt( value, 17, 0b10101010_10101010_10101010 );
				  _array.set5ByteLong( value, 20, 0b10101010_10101010_10101010_10101010_10101010L );
				  _array.set6ByteLong( value, 25, 0b10101010_10101010_10101010_10101010_10101010_10101010L );
			 }
		 }

		 private void VerifySimpleValues( int index )
		 {
			  assertEquals( ( sbyte ) 123, _array.getByte( index, 0 ) );
			  assertEquals( ( short ) 1234, _array.getShort( index, 1 ) );
			  assertEquals( 12345, _array.getInt( index, 5 ) );
			  assertEquals( long.MaxValue - 100, _array.getLong( index, 9 ) );
			  assertEquals( 0b10101010_10101010_10101010, _array.get3ByteInt( index, 17 ) );
			  assertEquals( 0b10101010_10101010_10101010_10101010_10101010L, _array.get5ByteLong( index, 20 ) );
			  assertEquals( 0b10101010_10101010_10101010_10101010_10101010_10101010L, _array.get6ByteLong( index, 25 ) );
		 }

		 private void SetArray( int index, sbyte[] bytes )
		 {
			  _array.set( index, bytes );
		 }

		 private void VerifyArray( int index, sbyte[] actualBytes, sbyte[] scratchBuffer )
		 {
			  _array.get( index, scratchBuffer );
			  assertArrayEquals( actualBytes, scratchBuffer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectMinusOneFor3ByteInts()
		 public virtual void ShouldDetectMinusOneFor3ByteInts()
		 {
			  // WHEN
			  _array.set3ByteInt( 10, 2, -1 );
			  _array.set3ByteInt( 10, 5, -1 );

			  // THEN
			  assertEquals( -1L, _array.get3ByteInt( 10, 2 ) );
			  assertEquals( -1L, _array.get3ByteInt( 10, 5 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectMinusOneFor5ByteLongs()
		 public virtual void ShouldDetectMinusOneFor5ByteLongs()
		 {
			  // WHEN
			  _array.set5ByteLong( 10, 2, -1 );
			  _array.set5ByteLong( 10, 7, -1 );

			  // THEN
			  assertEquals( -1L, _array.get5ByteLong( 10, 2 ) );
			  assertEquals( -1L, _array.get5ByteLong( 10, 7 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectMinusOneFor6ByteLongs()
		 public virtual void ShouldDetectMinusOneFor6ByteLongs()
		 {
			  // WHEN
			  _array.set6ByteLong( 10, 2, -1 );
			  _array.set6ByteLong( 10, 8, -1 );

			  // THEN
			  assertEquals( -1L, _array.get6ByteLong( 10, 2 ) );
			  assertEquals( -1L, _array.get6ByteLong( 10, 8 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMultipleCallsToClose()
		 public virtual void ShouldHandleMultipleCallsToClose()
		 {
			  // WHEN
			  _array.close();

			  // THEN should also work
			  _array.close();
		 }
	}

}