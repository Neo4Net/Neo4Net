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
namespace Neo4Net.causalclustering.core.consensus.outcome
{
	using Test = org.junit.Test;

	using ConsecutiveInFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.ConsecutiveInFlightCache;
	using InFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCache;
	using InMemoryRaftLog = Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using Log = Neo4Net.Logging.Log;
	using NullLog = Neo4Net.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.ReplicatedInteger.ValueOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.log.RaftLogHelper.readLogEntry;

	public class BatchAppendLogEntriesTest
	{
		 private readonly Log _log = NullLog.Instance;
		 private RaftLogEntry _entryA = new RaftLogEntry( 0, ValueOf( 100 ) );
		 private RaftLogEntry _entryB = new RaftLogEntry( 1, ValueOf( 200 ) );
		 private RaftLogEntry _entryC = new RaftLogEntry( 2, ValueOf( 300 ) );
		 private RaftLogEntry _entryD = new RaftLogEntry( 3, ValueOf( 400 ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyMultipleEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyMultipleEntries()
		 {
			  // given
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  BatchAppendLogEntries batchAppendLogEntries = new BatchAppendLogEntries( 0, 0, new RaftLogEntry[]{ _entryA, _entryB, _entryC } );

			  // when
			  batchAppendLogEntries.ApplyTo( raftLog, _log );

			  // then
			  assertEquals( _entryA, readLogEntry( raftLog, 0 ) );
			  assertEquals( _entryB, readLogEntry( raftLog, 1 ) );
			  assertEquals( _entryC, readLogEntry( raftLog, 2 ) );
			  assertEquals( 2, raftLog.AppendIndex() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyFromOffsetOnly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyFromOffsetOnly()
		 {
			  // given
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  BatchAppendLogEntries batchAppendLogEntries = new BatchAppendLogEntries( 0, 2, new RaftLogEntry[]{ _entryA, _entryB, _entryC, _entryD } );

			  // when
			  batchAppendLogEntries.ApplyTo( raftLog, _log );

			  // then
			  assertEquals( _entryC, readLogEntry( raftLog, 0 ) );
			  assertEquals( _entryD, readLogEntry( raftLog, 1 ) );
			  assertEquals( 1, raftLog.AppendIndex() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void applyTo()
		 public virtual void ApplyTo()
		 {
			  //Test that batch commands apply entries to the cache.

			  //given
			  long baseIndex = 0;
			  int offset = 1;
			  RaftLogEntry[] entries = new RaftLogEntry[]
			  {
				  new RaftLogEntry( 0L, ValueOf( 0 ) ),
				  new RaftLogEntry( 1L, ValueOf( 1 ) ),
				  new RaftLogEntry( 2L, ValueOf( 2 ) )
			  };

			  BatchAppendLogEntries batchAppend = new BatchAppendLogEntries( baseIndex, offset, entries );

			  InFlightCache cache = new ConsecutiveInFlightCache();

			  //when
			  batchAppend.ApplyTo( cache, _log );

			  //then
			  assertNull( cache.Get( 0L ) );
			  assertNotNull( cache.Get( 1L ) );
			  assertNotNull( cache.Get( 2L ) );
		 }
	}

}