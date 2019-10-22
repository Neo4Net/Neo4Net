using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log.cache
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ConsecutiveCacheTest
	public class ConsecutiveCacheTest
	{
		 private readonly int _capacity;
		 private ConsecutiveCache<int> _cache;
		 private int?[] _evictions;

		 public ConsecutiveCacheTest( int capacity )
		 {
			  this._capacity = capacity;
			  this._evictions = new int?[capacity];
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "capacity={0}") public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { 1 },
				  new object[] { 2 },
				  new object[] { 3 },
				  new object[] { 4 },
				  new object[] { 8 },
				  new object[] { 1024 }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _cache = new ConsecutiveCache<int>( _capacity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEmptyInvariants()
		 public virtual void TestEmptyInvariants()
		 {
			  assertEquals( 0, _cache.size() );
			  for ( int i = 0; i < _capacity; i++ )
			  {
					assertNull( _cache.get( i ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCacheFill()
		 public virtual void TestCacheFill()
		 {
			  for ( int i = 0; i < _capacity; i++ )
			  {
					// when
					_cache.put( i, i, _evictions );
					assertTrue( Stream.of( _evictions ).allMatch( Objects.isNull ) );

					// then
					assertEquals( i + 1, _cache.size() );
			  }

			  // then
			  for ( int i = 0; i < _capacity; i++ )
			  {
					assertEquals( i, _cache.get( i ).intValue() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCacheMultipleFills()
		 public virtual void TestCacheMultipleFills()
		 {
			  // given
			  for ( int i = 0; i < _capacity; i++ )
			  {
					_cache.put( i, i, _evictions );
			  }

			  for ( int i = _capacity; i < 10 * _capacity; i++ )
			  {
					// when
					_cache.put( i, i, _evictions );

					// then
					assertEquals( i - _capacity, _evictions[0].Value );
					assertTrue( Stream.of( _evictions ).skip( 1 ).allMatch( Objects.isNull ) );
					assertEquals( _capacity, _cache.size() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCacheClearing()
		 public virtual void TestCacheClearing()
		 {
			  // given
			  for ( int i = 0; i < _capacity; i++ )
			  {
					_cache.put( i, i, _evictions );
			  }

			  // when
			  _cache.clear( _evictions );

			  // then
			  for ( int i = 0; i < _capacity; i++ )
			  {
					assertEquals( i, _evictions[i].Value );
					assertNull( _cache.get( i ) );
			  }

			  assertEquals( 0, _cache.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEntryOverride()
		 public virtual void TestEntryOverride()
		 {
			  // given
			  for ( int i = 0; i < _capacity; i++ )
			  {
					_cache.put( i, i, _evictions );
			  }

			  // when
			  _cache.put( _capacity / 2, 10000, _evictions );

			  // then
			  for ( int i = 0; i < _capacity; i++ )
			  {
					if ( i == _capacity / 2 )
					{
						 continue;
					}

					assertEquals( i, _evictions[i].Value );
					assertNull( _cache.get( i ) );
			  }

			  assertEquals( 10000, _cache.get( _capacity / 2 ).intValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEntrySkip()
		 public virtual void TestEntrySkip()
		 {
			  // given
			  for ( int i = 0; i < _capacity; i++ )
			  {
					_cache.put( i, i, _evictions );
			  }

			  // when
			  _cache.put( _capacity + 1, 10000, _evictions );

			  // then
			  for ( int i = 0; i < _capacity; i++ )
			  {
					assertEquals( i, _evictions[i].Value );
					assertNull( _cache.get( i ) );
			  }

			  assertEquals( 10000, _cache.get( _capacity + 1 ).intValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPruning()
		 public virtual void TestPruning()
		 {
			  // given
			  for ( int i = 0; i < _capacity; i++ )
			  {
					_cache.put( i, i, _evictions );
			  }

			  // when
			  int upToIndex = _capacity / 2;
			  _cache.prune( upToIndex, _evictions );

			  // then
			  for ( int i = 0; i <= upToIndex; i++ )
			  {
					assertNull( _cache.get( i ) );
					assertEquals( i, _evictions[i].Value );
			  }

			  for ( int i = upToIndex + 1; i < _capacity; i++ )
			  {
					assertEquals( i, _cache.get( i ).intValue() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoval()
		 public virtual void TestRemoval()
		 {
			  // given
			  for ( int i = 0; i < _capacity; i++ )
			  {
					_cache.put( i, i, _evictions );
			  }

			  // then
			  for ( int i = 0; i < _capacity; i++ )
			  {
					// when
					int? removed = _cache.remove();

					// then
					assertEquals( i, removed.Value );
			  }

			  assertNull( _cache.remove() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testTruncation()
		 public virtual void TestTruncation()
		 {
			  // given
			  for ( int i = 0; i < _capacity; i++ )
			  {
					_cache.put( i, i, _evictions );
			  }

			  // when
			  int fromIndex = _capacity / 2;
			  _cache.truncate( fromIndex, _evictions );

			  // then
			  for ( int i = 0; i < fromIndex; i++ )
			  {
					assertEquals( i, _cache.get( i ).intValue() );
			  }

			  for ( int i = fromIndex; i < _capacity; i++ )
			  {
					assertNull( _cache.get( i ) );
					assertEquals( i, _evictions[_capacity - i - 1].Value );
			  }
		 }
	}

}