/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class PositionCacheTest
	{
		 private readonly PositionCache _cache = new PositionCache();
		 private readonly LogPosition _beginning = new LogPosition( 0, SegmentHeader.Size );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnSaneDefaultPosition()
		 public virtual void ShouldReturnSaneDefaultPosition()
		 {
			  // when
			  LogPosition position = _cache.lookup( 5 );

			  // then
			  assertEquals( _beginning, position );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnBestPosition()
		 public virtual void ShouldReturnBestPosition()
		 {
			  // given
			  _cache.put( Pos( 4 ) );
			  _cache.put( Pos( 6 ) );

			  // when
			  LogPosition lookup = _cache.lookup( 7 );

			  // then
			  assertEquals( Pos( 6 ), lookup );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnExactMatch()
		 public virtual void ShouldReturnExactMatch()
		 {
			  // given
			  _cache.put( Pos( 4 ) );
			  _cache.put( Pos( 6 ) );
			  _cache.put( Pos( 8 ) );

			  // when
			  LogPosition lookup = _cache.lookup( 6 );

			  // then
			  assertEquals( Pos( 6 ), lookup );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnPositionAhead()
		 public virtual void ShouldNotReturnPositionAhead()
		 {
			  // given
			  _cache.put( Pos( 4 ) );
			  _cache.put( Pos( 6 ) );
			  _cache.put( Pos( 8 ) );

			  // when
			  LogPosition lookup = _cache.lookup( 7 );

			  // then
			  assertEquals( Pos( 6 ), lookup );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPushOutOldEntries()
		 public virtual void ShouldPushOutOldEntries()
		 {
			  // given
			  int count = PositionCache.CACHE_SIZE + 4;
			  for ( int i = 0; i < count; i++ )
			  {
					_cache.put( Pos( i ) );
			  }

			  // then
			  for ( int i = 0; i < PositionCache.CACHE_SIZE; i++ )
			  {
					int index = count - i - 1;
					assertEquals( Pos( index ), _cache.lookup( index ) );
			  }

			  int index = count - PositionCache.CACHE_SIZE - 1;
			  assertEquals( _beginning, _cache.lookup( index ) );
		 }

		 private LogPosition Pos( int i )
		 {
			  return new LogPosition( i, 100 * i );
		 }
	}

}