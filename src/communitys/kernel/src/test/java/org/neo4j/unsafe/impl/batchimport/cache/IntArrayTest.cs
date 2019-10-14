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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
	using AfterAll = org.junit.jupiter.api.AfterAll;
	using BeforeAll = org.junit.jupiter.api.BeforeAll;
	using DynamicTest = org.junit.jupiter.api.DynamicTest;
	using TestFactory = org.junit.jupiter.api.TestFactory;
	using ThrowingConsumer = org.junit.jupiter.api.function.ThrowingConsumer;


	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using PageCache = Neo4Net.Io.pagecache.PageCache;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.DynamicTest.stream;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.HEAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.OFF_HEAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.auto;

	internal class IntArrayTest : NumberArrayPageCacheTestSupport
	{
		private bool InstanceFieldsInitialized = false;

		public IntArrayTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_random = new Random( _seed );
		}

		 private static Fixture _fixture;
		 private readonly long _seed = currentTimeMillis();
		 private Random _random;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeAll static void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal static void SetUp()
		 {
			  _fixture = PrepareDirectoryAndPageCache( typeof( IntArrayTest ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterAll static void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal static void TearDown()
		 {
			  _fixture.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @TestFactory Stream<org.junit.jupiter.api.DynamicTest> shouldHandleSomeRandomSetAndGet()
		 internal virtual Stream<DynamicTest> ShouldHandleSomeRandomSetAndGet()
		 {
			  // GIVEN
			  ThrowingConsumer<NumberArrayFactory> arrayFactoryConsumer = factory =>
			  {
				int length = _random.Next( 100_000 ) + 100;
				int defaultValue = _random.Next( 2 ) - 1; // 0 or -1
				using ( IntArray array = factory.newIntArray( length, defaultValue ) )
				{
					 int[] expected = new int[length];
					 Arrays.Fill( expected, defaultValue );

					 // WHEN
					 int operations = _random.Next( 1_000 ) + 10;
					 for ( int i = 0; i < operations; i++ )
					 {
						  // THEN
						  int index = _random.Next( length );
						  int value = _random.Next();
						  switch ( _random.Next( 3 ) )
						  {
						  case 0: // set
								array.Set( index, value );
								expected[index] = value;
								break;
						  case 1: // get
								assertEquals( expected[index], array.Get( index ), "Seed:" + _seed );
								break;
						  default: // swap
								int toIndex = _random.Next( length );
								array.Swap( index, toIndex );
								Swap( expected, index, toIndex );
								break;
						  }
					 }
				}
			  };
			  return stream( ArrayFactories(), NumberArrayFactoryName, arrayFactoryConsumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @TestFactory Stream<org.junit.jupiter.api.DynamicTest> shouldHandleMultipleCallsToClose()
		 internal virtual Stream<DynamicTest> ShouldHandleMultipleCallsToClose()
		 {
			  return DynamicTest.stream(ArrayFactories(), NumberArrayFactoryName, factory =>
			  {
				// GIVEN
				NumberArray<object> array = factory.newIntArray( 10, -1 );

				// WHEN
				array.Close();

				// THEN should also work
				array.Close();
			  });
		 }

		 private static System.Func<NumberArrayFactory, string> NumberArrayFactoryName
		 {
			 get
			 {
	//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  return factory => factory.GetType().FullName;
			 }
		 }

		 private static void Swap( int[] expected, int fromIndex, int toIndex )
		 {
			  int fromValue = expected[fromIndex];
			  expected[fromIndex] = expected[toIndex];
			  expected[toIndex] = fromValue;
		 }

		 private static IEnumerator<NumberArrayFactory> ArrayFactories()
		 {
			  PageCache pageCache = _fixture.pageCache;
			  File dir = _fixture.directory;
			  NumberArrayFactory autoWithPageCacheFallback = auto( pageCache, dir, true, NO_MONITOR );
			  NumberArrayFactory pageCacheArrayFactory = new PageCachedNumberArrayFactory( pageCache, dir );
			  return Iterators.iterator( HEAP, OFF_HEAP, autoWithPageCacheFallback, pageCacheArrayFactory );
		 }
	}

}