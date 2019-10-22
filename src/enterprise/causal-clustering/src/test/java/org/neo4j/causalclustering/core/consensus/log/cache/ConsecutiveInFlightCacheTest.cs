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
	using Test = org.junit.Test;

	using DummyRequest = Neo4Net.causalclustering.core.state.machines.dummy.DummyRequest;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ConsecutiveInFlightCacheTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTrackUsedMemory()
		 public virtual void ShouldTrackUsedMemory()
		 {
			  int capacity = 4;
			  ConsecutiveInFlightCache cache = new ConsecutiveInFlightCache( capacity, 1000, InFlightCacheMonitor.VOID, true );

			  for ( int i = 0; i < capacity; i++ )
			  {
					// when
					cache.Put( i, Content( 100 ) );

					// then
					assertEquals( ( i + 1 ) * 100, cache.TotalBytes() );
			  }

			  // when
			  cache.Put( capacity, Content( 100 ) );

			  // then
			  assertEquals( capacity, cache.ElementCount() );
			  assertEquals( capacity * 100, cache.TotalBytes() );

			  // when
			  cache.Put( capacity + 1, Content( 500 ) );
			  assertEquals( capacity, cache.ElementCount() );
			  assertEquals( 800, cache.TotalBytes() );

			  // when
			  cache.Put( capacity + 2, Content( 500 ) );
			  assertEquals( 2, cache.ElementCount() );
			  assertEquals( 1000, cache.TotalBytes() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnLatestItems()
		 public virtual void ShouldReturnLatestItems()
		 {
			  // given
			  int capacity = 4;
			  ConsecutiveInFlightCache cache = new ConsecutiveInFlightCache( capacity, 1000, InFlightCacheMonitor.VOID, true );

			  // when
			  for ( int i = 0; i < 3 * capacity; i++ )
			  {
					cache.Put( i, Content( i ) );
			  }

			  // then
			  for ( int i = 0; i < 3 * capacity; i++ )
			  {
					RaftLogEntry entry = cache.Get( i );
					if ( i < 2 * capacity )
					{
						 assertNull( entry );
					}
					else
					{
						 assertTrue( entry.Content().size().HasValue );
						 assertEquals( i, entry.Content().size().Value );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemovePrunedItems()
		 public virtual void ShouldRemovePrunedItems()
		 {
			  // given
			  int capacity = 20;
			  ConsecutiveInFlightCache cache = new ConsecutiveInFlightCache( capacity, 1000, InFlightCacheMonitor.VOID, true );

			  for ( int i = 0; i < capacity; i++ )
			  {
					cache.Put( i, Content( i ) );
			  }

			  // when
			  int upToIndex = capacity / 2 - 1;
			  cache.Prune( upToIndex );

			  // then
			  assertEquals( capacity / 2, cache.ElementCount() );

			  for ( int i = 0; i < capacity; i++ )
			  {
					RaftLogEntry entry = cache.Get( i );
					if ( i <= upToIndex )
					{
						 assertNull( entry );
					}
					else
					{
						 assertTrue( entry.Content().size().HasValue );
						 assertEquals( i, entry.Content().size().Value );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveTruncatedItems()
		 public virtual void ShouldRemoveTruncatedItems()
		 {
			  // given
			  int capacity = 20;
			  ConsecutiveInFlightCache cache = new ConsecutiveInFlightCache( capacity, 1000, InFlightCacheMonitor.VOID, true );

			  for ( int i = 0; i < capacity; i++ )
			  {
					cache.Put( i, Content( i ) );
			  }

			  // when
			  int fromIndex = capacity / 2;
			  cache.Truncate( fromIndex );

			  // then
			  assertEquals( fromIndex, cache.ElementCount() );
			  assertEquals( ( fromIndex * ( fromIndex - 1 ) ) / 2, cache.TotalBytes() );

			  for ( int i = fromIndex; i < capacity; i++ )
			  {
					assertNull( cache.Get( i ) );
			  }
		 }

		 private RaftLogEntry Content( int size )
		 {
			  return new RaftLogEntry( 0, new DummyRequest( new sbyte[size] ) );
		 }
	}

}