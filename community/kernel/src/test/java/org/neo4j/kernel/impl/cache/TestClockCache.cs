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
namespace Org.Neo4j.Kernel.impl.cache
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class TestClockCache
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCreate()
		 public virtual void TestCreate()
		 {
			  try
			  {
					new ClockCache<>( "TestCache", 0 );
					fail( "Illegal maxSize should throw exception" );
			  }
			  catch ( System.ArgumentException )
			  { // good
			  }
			  ClockCache<object, object> cache = new ClockCache<object, object>( "TestCache", 70 );
			  try
			  {
					cache.Put( null, new object() );
					fail( "Null key should throw exception" );
			  }
			  catch ( System.ArgumentException )
			  { // good
			  }
			  try
			  {
					cache.Put( new object(), null );
					fail( "Null element should throw exception" );
			  }
			  catch ( System.ArgumentException )
			  { // good
			  }
			  try
			  {
					cache.Get( null );
					fail( "Null key should throw exception" );
			  }
			  catch ( System.ArgumentException )
			  { // good
			  }
			  try
			  {
					cache.Remove( null );
					fail( "Null key should throw exception" );
			  }
			  catch ( System.ArgumentException )
			  { // good
			  }
			  cache.Put( new object(), new object() );
			  cache.Clear();
		 }

		 private class ClockCacheTest<K, E> : ClockCache<K, E>
		 {
			  internal E CleanedElement;

			  internal ClockCacheTest( string name, int maxSize ) : base( name, maxSize )
			  {
			  }

			  public override void ElementCleaned( E element )
			  {
					CleanedElement = element;
			  }

			  internal virtual E LastCleanedElement
			  {
				  get
				  {
						return CleanedElement;
				  }
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimple()
		 public virtual void TestSimple()
		 {
			  ClockCacheTest<int, string> cache = new ClockCacheTest<int, string>( "TestCache", 3 );
			  IDictionary<string, int> valueToKey = new Dictionary<string, int>();
			  IDictionary<int, string> keyToValue = new Dictionary<int, string>();

			  string s1 = "1";
			  int? key1 = 1;
			  valueToKey[s1] = key1.Value;
			  keyToValue[key1] = s1;

			  string s2 = "2";
			  int? key2 = 2;
			  valueToKey[s2] = key2.Value;
			  keyToValue[key2] = s2;

			  string s3 = "3";
			  int? key3 = 3;
			  valueToKey[s3] = key3.Value;
			  keyToValue[key3] = s3;

			  string s4 = "4";
			  int? key4 = 4;
			  valueToKey[s4] = key4.Value;
			  keyToValue[key4] = s4;

			  string s5 = "5";
			  int? key5 = 5;
			  valueToKey[s5] = key5.Value;
			  keyToValue[key5] = s5;

			  IList<int> cleanedElements = new LinkedList<int>();
			  IList<int> existingElements = new LinkedList<int>();

			  cache.Put( key1, s1 );
			  cache.Put( key2, s2 );
			  cache.Put( key3, s3 );
			  assertNull( cache.LastCleanedElement );

			  string fromKey2 = cache.Get( key2 );
			  assertEquals( s2, fromKey2 );
			  string fromKey1 = cache.Get( key1 );
			  assertEquals( s1, fromKey1 );
			  string fromKey3 = cache.Get( key3 );
			  assertEquals( s3, fromKey3 );

			  cache.Put( key4, s4 );
			  assertNotEquals( s4, cache.LastCleanedElement );
			  cleanedElements.Add( valueToKey[cache.LastCleanedElement] );
			  existingElements.RemoveAt( valueToKey[cache.LastCleanedElement] );

			  cache.Put( key5, s5 );
			  assertNotEquals( s4, cache.LastCleanedElement );
			  assertNotEquals( s5, cache.LastCleanedElement );
			  cleanedElements.Add( valueToKey[cache.LastCleanedElement] );
			  existingElements.RemoveAt( valueToKey[cache.LastCleanedElement] );

			  int size = cache.Size();
			  assertEquals( 3, size );
			  foreach ( int? key in cleanedElements )
			  {
					assertNull( cache.Get( key ) );
			  }
			  foreach ( int? key in existingElements )
			  {
					assertEquals( keyToValue[key], cache.Get( key ) );
			  }
			  cache.Clear();
			  assertEquals( 0, cache.Size() );
			  foreach ( int? key in keyToValue.Keys )
			  {
					assertNull( cache.Get( key ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateSizeWhenRemoving()
		 public virtual void ShouldUpdateSizeWhenRemoving()
		 {
			  ClockCache<string, int> cache = new ClockCache<string, int>( "foo", 3 );
			  cache.Put( "bar", 42 );
			  cache.Put( "baz", 87 );

			  cache.Remove( "bar" );

			  assertThat( cache.Size(), @is(1) );
		 }
	}

}