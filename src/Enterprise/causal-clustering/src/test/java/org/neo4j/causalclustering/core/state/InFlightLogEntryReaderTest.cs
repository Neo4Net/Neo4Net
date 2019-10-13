using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core.state
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using ConsecutiveInFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.ConsecutiveInFlightCache;
	using InFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCache;
	using RaftLogCursor = Neo4Net.causalclustering.core.consensus.log.RaftLogCursor;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using ReadableRaftLog = Neo4Net.causalclustering.core.consensus.log.ReadableRaftLog;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class InFlightLogEntryReaderTest
	public class InFlightLogEntryReaderTest
	{
		 private readonly ReadableRaftLog _raftLog = mock( typeof( ReadableRaftLog ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private final org.neo4j.causalclustering.core.consensus.log.cache.InFlightCache inFlightCache = mock(org.neo4j.causalclustering.core.consensus.log.cache.ConsecutiveInFlightCache.class);
		 private readonly InFlightCache _inFlightCache = mock( typeof( ConsecutiveInFlightCache ) );
		 private readonly long _logIndex = 42L;
		 private readonly RaftLogEntry _entry = mock( typeof( RaftLogEntry ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<bool[]> params()
		 public static ICollection<bool[]> Params()
		 {
			  return asList( new bool?[]{ true }, new bool?[]{ false } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public boolean clearCache;
		 public bool ClearCache;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseTheCacheWhenTheIndexIsPresent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseTheCacheWhenTheIndexIsPresent()
		 {
			  // given
			  InFlightLogEntryReader reader = new InFlightLogEntryReader( _raftLog, _inFlightCache, ClearCache );
			  StartingFromIndexReturnEntries( _inFlightCache, _logIndex, _entry );
			  StartingFromIndexReturnEntries( _raftLog, -1, null );

			  // when
			  RaftLogEntry raftLogEntry = reader.Get( _logIndex );

			  // then
			  assertEquals( _entry, raftLogEntry );
			  verify( _inFlightCache ).get( _logIndex );
			  AssertCacheIsUpdated( _inFlightCache, _logIndex );
			  verifyNoMoreInteractions( _inFlightCache );
			  verifyZeroInteractions( _raftLog );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseTheRaftLogWhenTheIndexIsNotPresent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseTheRaftLogWhenTheIndexIsNotPresent()
		 {
			  // given
			  InFlightLogEntryReader reader = new InFlightLogEntryReader( _raftLog, _inFlightCache, ClearCache );
			  StartingFromIndexReturnEntries( _inFlightCache, _logIndex, null );
			  StartingFromIndexReturnEntries( _raftLog, _logIndex, _entry );

			  // when
			  RaftLogEntry raftLogEntry = reader.Get( _logIndex );

			  // then
			  assertEquals( _entry, raftLogEntry );
			  verify( _inFlightCache ).get( _logIndex );
			  verify( _raftLog ).getEntryCursor( _logIndex );
			  AssertCacheIsUpdated( _inFlightCache, _logIndex );

			  verifyNoMoreInteractions( _inFlightCache );
			  verifyNoMoreInteractions( _raftLog );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNeverUseMapAgainAfterHavingFallenBackToTheRaftLog() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNeverUseMapAgainAfterHavingFallenBackToTheRaftLog()
		 {
			  // given
			  InFlightLogEntryReader reader = new InFlightLogEntryReader( _raftLog, _inFlightCache, ClearCache );
			  StartingFromIndexReturnEntries( _inFlightCache, _logIndex, _entry, null, mock( typeof( RaftLogEntry ) ) );
			  RaftLogEntry[] entries = new RaftLogEntry[] { _entry, mock( typeof( RaftLogEntry ) ), mock( typeof( RaftLogEntry ) ) };
			  StartingFromIndexReturnEntries( _raftLog, _logIndex + 1, entries[1], entries[2] );

			  for ( int offset = 0; offset < 3; offset++ )
			  {
					// when
					RaftLogEntry raftLogEntry = reader.Get( offset + _logIndex );

					// then
					assertEquals( entries[offset], raftLogEntry );

					if ( offset <= 1 )
					{
						 verify( _inFlightCache ).get( offset + _logIndex );
					}

					if ( offset == 1 )
					{
						 verify( _raftLog ).getEntryCursor( offset + _logIndex );
					}

					AssertCacheIsUpdated( _inFlightCache, offset + _logIndex );
			  }

			  verifyNoMoreInteractions( _inFlightCache );
			  verifyNoMoreInteractions( _raftLog );
		 }

		 private void StartingFromIndexReturnEntries( InFlightCache inFlightCache, long startIndex, RaftLogEntry entry, params RaftLogEntry[] otherEntries )
		 {
			  when( inFlightCache.Get( startIndex ) ).thenReturn( entry );
			  for ( int offset = 0; offset < otherEntries.Length; offset++ )
			  {
					when( inFlightCache.Get( startIndex + offset + 1L ) ).thenReturn( otherEntries[offset] );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startingFromIndexReturnEntries(org.neo4j.causalclustering.core.consensus.log.ReadableRaftLog raftLog, long startIndex, org.neo4j.causalclustering.core.consensus.log.RaftLogEntry entry, org.neo4j.causalclustering.core.consensus.log.RaftLogEntry... otherEntries) throws java.io.IOException
		 private void StartingFromIndexReturnEntries( ReadableRaftLog raftLog, long startIndex, RaftLogEntry entry, params RaftLogEntry[] otherEntries )
		 {
			  RaftLogCursor cursor = mock( typeof( RaftLogCursor ) );
			  when( raftLog.GetEntryCursor( startIndex ) ).thenReturn( cursor, ( RaftLogCursor ) null );

			  bool?[] bools = new bool?[otherEntries.Length + 1];
			  Arrays.fill( bools, true );
			  bools[otherEntries.Length] = false;

			  when( cursor.Next() ).thenReturn(true, bools);

			  long?[] indexes = new long?[otherEntries.Length + 1];
			  for ( int offset = 0; offset < indexes.Length; offset++ )
			  {
					indexes[offset] = startIndex + 1 + offset;
			  }
			  indexes[otherEntries.Length] = -1L;

			  when( cursor.Index() ).thenReturn(startIndex, indexes);

			  RaftLogEntry[] raftLogEntries = Arrays.copyOf( otherEntries, otherEntries.Length + 1 );
			  when( cursor.get() ).thenReturn(entry, raftLogEntries);
		 }

		 private void AssertCacheIsUpdated( InFlightCache inFlightCache, long key )
		 {
			  if ( ClearCache )
			  {
					verify( inFlightCache, times( 1 ) ).prune( key );
			  }
			  else
			  {
					verify( inFlightCache, never() ).prune(key);
			  }
		 }
	}

}