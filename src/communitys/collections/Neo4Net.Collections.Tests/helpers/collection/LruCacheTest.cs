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
namespace Neo4Net.Helpers.Collections
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class LruCacheTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowWhenMaxSizeIsNotGreaterThanZero()
		 internal virtual void ShouldThrowWhenMaxSizeIsNotGreaterThanZero()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => new LruCache<>("TestCache", 0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowWhenPuttingEntryWithNullKey()
		 internal virtual void ShouldThrowWhenPuttingEntryWithNullKey()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => (new LruCache<>("TestCache", 70)).put(null, new object()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowWhenPuttingEntryWithNullValue()
		 internal virtual void ShouldThrowWhenPuttingEntryWithNullValue()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => (new LruCache<>("TestCache", 70)).put(new object(), null) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowWhenGettingWithANullKey()
		 internal virtual void ShouldThrowWhenGettingWithANullKey()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => (new LruCache<>("TestCache", 70)).Get(null) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowWhenRemovingWithANullKey()
		 internal virtual void ShouldThrowWhenRemovingWithANullKey()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => (new LruCache<>("TestCache", 70)).Remove(null) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWork()
		 internal virtual void ShouldWork()
		 {
			  LruCache<int, string> cache = new LruCache<int, string>( "TestCache", 3 );

			  string s1 = "1";
			  int? key1 = 1;
			  string s2 = "2";
			  int? key2 = 2;
			  string s3 = "3";
			  int? key3 = 3;
			  string s4 = "4";
			  int? key4 = 4;
			  string s5 = "5";
			  int? key5 = 5;

			  cache.Put( key1.Value, s1 );
			  cache.Put( key2.Value, s2 );
			  cache.Put( key3.Value, s3 );
			  cache.Get( key2.Value );

			  assertEquals( new HashSet<>( Arrays.asList( key1, key2, key3 ) ), cache.Keys );

			  cache.Put( key4.Value, s4 );

			  assertEquals( new HashSet<>( Arrays.asList( key2, key3, key4 ) ), cache.Keys );

			  cache.Put( key5.Value, s5 );

			  assertEquals( new HashSet<>( Arrays.asList( key2, key4, key5 ) ), cache.Keys );

			  int size = cache.Size();

			  assertEquals( 3, size );
			  assertNull( cache.Get( key1.Value ) );
			  assertEquals( s2, cache.Get( key2.Value ) );
			  assertNull( cache.Get( key3.Value ) );
			  assertEquals( s4, cache.Get( key4.Value ) );
			  assertEquals( s5, cache.Get( key5.Value ) );

			  cache.Clear();

			  assertEquals( 0, cache.Size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldResizeTheCache()
		 internal virtual void ShouldResizeTheCache()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<String> cleaned = new java.util.HashSet<>();
			  ISet<string> cleaned = new HashSet<string>();
			  LruCache<int, string> cache = new LruCacheAnonymousInnerClass( this, cleaned );

			  string s1 = "1";
			  int? key1 = 1;
			  string s2 = "2";
			  int? key2 = 2;
			  string s3 = "3";
			  int? key3 = 3;
			  string s4 = "4";
			  int? key4 = 4;
			  string s5 = "5";
			  int? key5 = 5;

			  cache.Put( key1.Value, s1 );
			  cache.Put( key2.Value, s2 );
			  cache.Put( key3.Value, s3 );
			  cache.Get( key2.Value );

			  assertEquals( Set( key1, key2, key3 ), cache.Keys );
			  assertEquals( cache.MaxSize(), cache.Size() );

			  cache.Resize( 5 );

			  assertEquals( 5, cache.MaxSize() );
			  assertEquals( 3, cache.Size() );
			  assertTrue( cleaned.Count == 0 );

			  cache.Put( key4.Value, s4 );

			  assertEquals( Set( key1, key2, key3, key4 ), cache.Keys );

			  cache.Put( key5.Value, s5 );

			  assertEquals( Set( key1, key2, key3, key4, key5 ), cache.Keys );
			  assertEquals( cache.MaxSize(), cache.Size() );

			  cache.Resize( 4 );

			  assertEquals( Set( key2, key3, key4, key5 ), cache.Keys );
			  assertEquals( cache.MaxSize(), cache.Size() );
			  assertEquals( Set( s1 ), cleaned );

			  cleaned.Clear();

			  cache.Resize( 3 );

			  assertEquals( Set( key2, key4, key5 ), cache.Keys );
			  assertEquals( 3, cache.MaxSize() );
			  assertEquals( 3, cache.Size() );
			  assertEquals( Set( s3 ), cleaned );
		 }

		 private class LruCacheAnonymousInnerClass : LruCache<int, string>
		 {
			 private readonly LruCacheTest _outerInstance;

			 private ISet<string> _cleaned;

			 public LruCacheAnonymousInnerClass( LruCacheTest outerInstance, ISet<string> cleaned ) : base( "TestCache", 3 )
			 {
				 this.outerInstance = outerInstance;
				 this._cleaned = cleaned;
			 }

			 public override void elementCleaned( string element )
			 {
				  _cleaned.Add( element );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldClear()
		 internal virtual void ShouldClear()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<String> cleaned = new java.util.HashSet<>();
			  ISet<string> cleaned = new HashSet<string>();
			  LruCache<int, string> cache = new LruCacheAnonymousInnerClass2( this, cleaned );

			  string s1 = "1";
			  int? key1 = 1;
			  string s2 = "2";
			  int? key2 = 2;
			  string s3 = "3";
			  int? key3 = 3;
			  string s4 = "4";
			  int? key4 = 4;
			  string s5 = "5";
			  int? key5 = 5;

			  cache.Put( key1.Value, s1 );
			  cache.Put( key2.Value, s2 );
			  cache.Put( key3.Value, s3 );
			  cache.Get( key2.Value );

			  assertEquals( Set( key1, key2, key3 ), cache.Keys );
			  assertEquals( cache.MaxSize(), cache.Size() );

			  cache.Resize( 5 );

			  assertEquals( 5, cache.MaxSize() );
			  assertEquals( 3, cache.Size() );

			  cache.Put( key4.Value, s4 );

			  assertEquals( Set( key1, key2, key3, key4 ), cache.Keys );

			  cache.Put( key5.Value, s5 );

			  assertEquals( Set( key1, key2, key3, key4, key5 ), cache.Keys );
			  assertEquals( cache.MaxSize(), cache.Size() );

			  cache.Clear();

			  assertEquals( 0, cache.Size() );
			  assertEquals( Set( s1, s2, s3, s4, s5 ), cleaned );
		 }

		 private class LruCacheAnonymousInnerClass2 : LruCache<int, string>
		 {
			 private readonly LruCacheTest _outerInstance;

			 private ISet<string> _cleaned;

			 public LruCacheAnonymousInnerClass2( LruCacheTest outerInstance, ISet<string> cleaned ) : base( "TestCache", 3 )
			 {
				 this.outerInstance = outerInstance;
				 this._cleaned = cleaned;
			 }

			 public override void elementCleaned( string element )
			 {
				  _cleaned.Add( element );
			 }
		 }

		 private static ISet<E> Set<E>( params E[] elems )
		 {
			  return new HashSet<E>( Arrays.asList( elems ) );
		 }
	}

}