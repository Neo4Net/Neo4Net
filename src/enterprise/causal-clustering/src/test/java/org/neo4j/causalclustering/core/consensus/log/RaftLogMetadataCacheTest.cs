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
namespace Neo4Net.causalclustering.core.consensus.log
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;

	using Test = org.junit.Test;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;

	public class RaftLogMetadataCacheTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNullWhenMissingAnEntryInTheCache()
		 public virtual void ShouldReturnNullWhenMissingAnEntryInTheCache()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RaftLogMetadataCache cache = new RaftLogMetadataCache(2);
			  RaftLogMetadataCache cache = new RaftLogMetadataCache( 2 );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RaftLogMetadataCache.RaftLogEntryMetadata metadata = cache.getMetadata(42);
			  RaftLogMetadataCache.RaftLogEntryMetadata metadata = cache.GetMetadata( 42 );

			  // then
			  assertNull( metadata );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTheTxValueTIfInTheCached()
		 public virtual void ShouldReturnTheTxValueTIfInTheCached()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RaftLogMetadataCache cache = new RaftLogMetadataCache(2);
			  RaftLogMetadataCache cache = new RaftLogMetadataCache( 2 );
			  const long index = 12L;
			  const long term = 12L;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.LogPosition position = new Neo4Net.kernel.impl.transaction.log.LogPosition(3, 4);
			  LogPosition position = new LogPosition( 3, 4 );

			  // when
			  cache.CacheMetadata( index, term, position );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RaftLogMetadataCache.RaftLogEntryMetadata metadata = cache.getMetadata(index);
			  RaftLogMetadataCache.RaftLogEntryMetadata metadata = cache.GetMetadata( index );

			  // then
			  assertEquals( new RaftLogMetadataCache.RaftLogEntryMetadata( term, position ), metadata );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearTheCache()
		 public virtual void ShouldClearTheCache()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RaftLogMetadataCache cache = new RaftLogMetadataCache(2);
			  RaftLogMetadataCache cache = new RaftLogMetadataCache( 2 );
			  const long index = 12L;
			  const long term = 12L;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.LogPosition position = new Neo4Net.kernel.impl.transaction.log.LogPosition(3, 4);
			  LogPosition position = new LogPosition( 3, 4 );

			  // when
			  cache.CacheMetadata( index, term, position );
			  cache.Clear();
			  RaftLogMetadataCache.RaftLogEntryMetadata metadata = cache.GetMetadata( index );

			  // then
			  assertNull( metadata );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveUpTo()
		 public virtual void ShouldRemoveUpTo()
		 {
			  // given
			  int cacheSize = 100;
			  RaftLogMetadataCache cache = new RaftLogMetadataCache( cacheSize );

			  for ( int i = 0; i < cacheSize; i++ )
			  {
					cache.CacheMetadata( i, i, new LogPosition( i, i ) );
			  }

			  // when
			  int upTo = 30;
			  cache.RemoveUpTo( upTo );

			  // then
			  long i = 0;
			  for ( ; i <= upTo; i++ )
			  {
					assertNull( cache.GetMetadata( i ) );
			  }
			  for ( ; i < cacheSize; i++ )
			  {
					RaftLogMetadataCache.RaftLogEntryMetadata metadata = cache.GetMetadata( i );
					assertNotNull( metadata );
					assertEquals( i, metadata.EntryTerm );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveUpwardsFrom()
		 public virtual void ShouldRemoveUpwardsFrom()
		 {
			  // given
			  int cacheSize = 100;
			  RaftLogMetadataCache cache = new RaftLogMetadataCache( cacheSize );

			  for ( int i = 0; i < cacheSize; i++ )
			  {
					cache.CacheMetadata( i, i, new LogPosition( i, i ) );
			  }

			  // when
			  int upFrom = 60;
			  cache.RemoveUpwardsFrom( upFrom );

			  // then
			  long i = 0;
			  for ( ; i < upFrom; i++ )
			  {
					RaftLogMetadataCache.RaftLogEntryMetadata metadata = cache.GetMetadata( i );
					assertNotNull( metadata );
					assertEquals( i, metadata.EntryTerm );
			  }
			  for ( ; i < cacheSize; i++ )
			  {
					assertNull( cache.GetMetadata( i ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptAndReturnIndexesInRangeJustDeleted()
		 public virtual void ShouldAcceptAndReturnIndexesInRangeJustDeleted()
		 {
			  // given
			  int cacheSize = 100;
			  RaftLogMetadataCache cache = new RaftLogMetadataCache( cacheSize );

			  for ( int i = 0; i < cacheSize; i++ )
			  {
					cache.CacheMetadata( i, i, new LogPosition( i, i ) );
			  }

			  // when
			  int upFrom = 60;
			  cache.RemoveUpwardsFrom( upFrom );

			  // and we add something in the deleted range
			  int insertedIndex = 70;
			  long insertedTerm = 150;
			  cache.CacheMetadata( insertedIndex, insertedTerm, new LogPosition( insertedIndex, insertedIndex ) );

			  // then
			  // nothing should be resurrected in the deleted range just because we inserted something there
			  int i = upFrom;
			  for ( ; i < insertedIndex; i++ )
			  {
					assertNull( cache.GetMetadata( i ) );
			  }
			  // i here should be insertedIndex
			  assertEquals( insertedTerm, cache.GetMetadata( i ).EntryTerm );
			  i++; // to continue iteration in the rest of the deleted range
			  for ( ; i < cacheSize; i++ )
			  {
					assertNull( cache.GetMetadata( i ) );
			  }
		 }
	}

}