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
namespace Neo4Net.causalclustering.core.consensus.shipping
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using InMemoryRaftLog = Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLog = Neo4Net.causalclustering.core.consensus.log.RaftLog;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using ConsecutiveInFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.ConsecutiveInFlightCache;
	using TimerService = Neo4Net.causalclustering.core.consensus.schedule.TimerService;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using Matchers = Neo4Net.Test.matchers.Matchers;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.identity.RaftTestMember.member;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.matchers.Matchers.hasMessage;

	public class RaftLogShipperTest
	{
		private bool InstanceFieldsInitialized = false;

		public RaftLogShipperTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_scheduler = Life.add( createScheduler() );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.kernel.lifecycle.LifeRule life = new org.Neo4Net.kernel.lifecycle.LifeRule(true);
		 public LifeRule Life = new LifeRule( true );
		 private IJobScheduler _scheduler;

		 private OutboundMessageCollector _outbound;
		 private RaftLog _raftLog;
		 private Clock _clock;
		 private TimerService _timerService;
		 private MemberId _leader;
		 private MemberId _follower;
		 private long _leaderTerm;
		 private long _leaderCommit;
		 private long _retryTimeMillis;
		 private int _catchupBatchSize = 64;
		 private int _maxAllowedShippingLag = 256;
		 private LogProvider _logProvider;
		 private Log _log;

		 private RaftLogShipper _logShipper;

		 private RaftLogEntry _entry0 = new RaftLogEntry( 0, ReplicatedInteger.valueOf( 1000 ) );
		 private RaftLogEntry _entry1 = new RaftLogEntry( 0, ReplicatedString.valueOf( "kedha" ) );
		 private RaftLogEntry _entry2 = new RaftLogEntry( 0, ReplicatedInteger.valueOf( 2000 ) );
		 private RaftLogEntry _entry3 = new RaftLogEntry( 0, ReplicatedString.valueOf( "chupchick" ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  // defaults
			  _outbound = new OutboundMessageCollector();
			  _raftLog = new InMemoryRaftLog();
			  _clock = Clocks.systemClock();
			  _leader = member( 0 );
			  _follower = member( 1 );
			  _leaderTerm = 0;
			  _leaderCommit = 0;
			  _retryTimeMillis = 100000;
			  _logProvider = mock( typeof( LogProvider ) );
			  _timerService = new TimerService( _scheduler, _logProvider );
			  _log = mock( typeof( Log ) );
			  when( _logProvider.getLog( typeof( RaftLogShipper ) ) ).thenReturn( _log );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void teardown()
		 public virtual void Teardown()
		 {
			  if ( _logShipper != null )
			  {
					_logShipper.stop();
					_logShipper = null;
			  }
		 }

		 private void StartLogShipper()
		 {
			  _logShipper = new RaftLogShipper( _outbound, _logProvider, _raftLog, _clock, _timerService, _leader, _follower, _leaderTerm, _leaderCommit, _retryTimeMillis, _catchupBatchSize, _maxAllowedShippingLag, new ConsecutiveInFlightCache() );
			  _logShipper.start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendLastEntryOnStart() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendLastEntryOnStart()
		 {
			  // given
			  _raftLog.append( _entry0 );
			  _raftLog.append( _entry1 );

			  // when
			  StartLogShipper();

			  // then
			  Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request expected = new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _leader, _leaderTerm, 0, _entry0.term(), RaftLogEntry.empty, _leaderCommit );
			  assertThat( _outbound.sentTo( _follower ), hasItem( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendPreviousEntryOnMismatch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendPreviousEntryOnMismatch()
		 {
			  // given
			  _raftLog.append( _entry0 );
			  _raftLog.append( _entry1 );
			  _raftLog.append( _entry2 );
			  StartLogShipper(); // ships entry2 on start

			  // when
			  _outbound.clear();
			  _logShipper.onMismatch( 0, new LeaderContext( 0, 0 ) );

			  // then: we expect it to ship (empty) entry1 next
			  Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request expected = new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _leader, _leaderTerm, 0, 0, RaftLogEntry.empty, _leaderCommit );
			  assertThat( _outbound.sentTo( _follower ), hasItem( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepSendingFirstEntryAfterSeveralMismatches() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepSendingFirstEntryAfterSeveralMismatches()
		 {
			  // given
			  _raftLog.append( _entry0 );
			  _raftLog.append( _entry1 );
			  StartLogShipper();

			  _logShipper.onMismatch( 0, new LeaderContext( 0, 0 ) );
			  _logShipper.onMismatch( 0, new LeaderContext( 0, 0 ) );

			  // when
			  _outbound.clear();
			  _logShipper.onMismatch( 0, new LeaderContext( 0, 0 ) );

			  // then
			  Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request expected = new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _leader, _leaderTerm, 0, 0, RaftLogEntry.empty, _leaderCommit );
			  assertThat( _outbound.sentTo( _follower ), hasItem( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendNextBatchAfterMatch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendNextBatchAfterMatch()
		 {
			  // given
			  _raftLog.append( _entry0 );
			  _raftLog.append( _entry1 );
			  _raftLog.append( _entry2 );
			  _raftLog.append( _entry3 );
			  StartLogShipper();

			  _logShipper.onMismatch( 0, new LeaderContext( 0, 0 ) );

			  // when
			  _outbound.clear();
			  _logShipper.onMatch( 0, new LeaderContext( 0, 0 ) );

			  // then
			  assertThat( _outbound.sentTo( _follower ), Matchers.hasRaftLogEntries( asList( _entry1, _entry2, _entry3 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendNewEntriesAfterMatchingLastEntry() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendNewEntriesAfterMatchingLastEntry()
		 {
			  // given
			  _raftLog.append( _entry0 );
			  StartLogShipper();

			  _logShipper.onMatch( 0, new LeaderContext( 0, 0 ) );

			  // when
			  _outbound.clear();

			  _raftLog.append( _entry1 );
			  _logShipper.onNewEntries( 0, 0, new RaftLogEntry[]{ _entry1 }, new LeaderContext( 0, 0 ) );
			  _raftLog.append( _entry2 );
			  _logShipper.onNewEntries( 1, 0, new RaftLogEntry[]{ _entry2 }, new LeaderContext( 0, 0 ) );

			  // then
			  assertThat( _outbound.sentTo( _follower ), Matchers.hasRaftLogEntries( asList( _entry1, _entry2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSendNewEntriesWhenNotMatched() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSendNewEntriesWhenNotMatched()
		 {
			  // given
			  _raftLog.append( _entry0 );
			  StartLogShipper();

			  // when
			  _outbound.clear();
			  _logShipper.onNewEntries( 0, 0, new RaftLogEntry[]{ _entry1 }, new LeaderContext( 0, 0 ) );
			  _logShipper.onNewEntries( 1, 0, new RaftLogEntry[]{ _entry2 }, new LeaderContext( 0, 0 ) );

			  // then
			  assertEquals( 0, _outbound.sentTo( _follower ).Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResendLastSentEntryOnFirstMismatch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResendLastSentEntryOnFirstMismatch()
		 {
			  // given
			  _raftLog.append( _entry0 );
			  StartLogShipper();
			  _raftLog.append( _entry1 );
			  _raftLog.append( _entry2 );

			  _logShipper.onMatch( 0, new LeaderContext( 0, 0 ) );
			  _logShipper.onNewEntries( 0, 0, new RaftLogEntry[]{ _entry1 }, new LeaderContext( 0, 0 ) );
			  _logShipper.onNewEntries( 1, 0, new RaftLogEntry[]{ _entry2 }, new LeaderContext( 0, 0 ) );

			  // when
			  _outbound.clear();
			  _logShipper.onMismatch( 1, new LeaderContext( 0, 0 ) );

			  // then
			  Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request expected = new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _leader, _leaderTerm, 1, _entry1.term(), RaftLogEntry.empty, _leaderCommit );
			  assertThat( _outbound.sentTo( _follower ), hasItem( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendAllEntriesAndCatchupCompletely() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendAllEntriesAndCatchupCompletely()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int ENTRY_COUNT = catchupBatchSize * 10;
			  int entryCount = _catchupBatchSize * 10;
			  ICollection<RaftLogEntry> entries = new List<RaftLogEntry>();
			  for ( int i = 0; i < entryCount; i++ )
			  {
					entries.Add( new RaftLogEntry( 0, ReplicatedInteger.valueOf( i ) ) );
			  }

			  foreach ( RaftLogEntry entry in entries )
			  {
					_raftLog.append( entry );
			  }

			  // then
			  StartLogShipper();

			  // back-tracking stage
			  Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request expected = new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _leader, _leaderTerm, 0, 0, RaftLogEntry.empty, _leaderCommit );
			  while ( !_outbound.sentTo( _follower ).Contains( expected ) )
			  {
					_logShipper.onMismatch( -1, new LeaderContext( 0, 0 ) );
			  }

			  // catchup stage
			  long matchIndex;

			  do
			  {
					Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request last = ( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request ) Iterables.last( _outbound.sentTo( _follower ) );
					matchIndex = last.PrevLogIndex() + last.Entries().Length;

					_outbound.clear();
					_logShipper.onMatch( matchIndex, new LeaderContext( 0, 0 ) );
			  } while ( _outbound.sentTo( _follower ).Count > 0 );

			  assertEquals( entryCount - 1, matchIndex );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendMostRecentlyAvailableEntryIfPruningHappened() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendMostRecentlyAvailableEntryIfPruningHappened()
		 {
			  //given
			  _raftLog.append( _entry0 );
			  _raftLog.append( _entry1 );
			  _raftLog.append( _entry2 );
			  _raftLog.append( _entry3 );

			  StartLogShipper();

			  //when
			  _raftLog.prune( 2 );
			  _outbound.clear();
			  _logShipper.onMismatch( 0, new LeaderContext( 0, 0 ) );

			  //then
			  Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request expected = new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _leader, _leaderTerm, 2, _entry2.term(), RaftLogEntry.empty, _leaderCommit );
			  assertThat( _outbound.sentTo( _follower ), hasItem( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendLogCompactionInfoToFollowerOnMatchIfEntryHasBeenPrunedAway() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendLogCompactionInfoToFollowerOnMatchIfEntryHasBeenPrunedAway()
		 {
			  //given
			  _raftLog.append( _entry0 );
			  _raftLog.append( _entry1 );
			  _raftLog.append( _entry2 );
			  _raftLog.append( _entry3 );

			  StartLogShipper();

			  //when
			  _outbound.clear();
			  _raftLog.prune( 2 );

			  _logShipper.onMatch( 1, new LeaderContext( 0, 0 ) );

			  //then
			  assertTrue( _outbound.hasAnyEntriesTo( _follower ) );
			  assertThat( _outbound.sentTo( _follower ), hasMessage( new Neo4Net.causalclustering.core.consensus.RaftMessages_LogCompactionInfo( _leader, 0, 2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickUpAfterMissedBatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPickUpAfterMissedBatch()
		 {
			  //given
			  _raftLog.append( _entry0 );
			  _raftLog.append( _entry1 );
			  _raftLog.append( _entry2 );
			  _raftLog.append( _entry3 );

			  StartLogShipper();
			  _logShipper.onMatch( 0, new LeaderContext( 0, 0 ) );
			  // we are now in PIPELINE mode, because we matched and the entire last batch was sent out

			  _logShipper.onTimeout();
			  // and now we should be in CATCHUP mode, awaiting a late response

			  // the response to the batch never came, so on timeout we enter MISMATCH mode and send a single entry based on
			  // the latest we knowingly sent (entry3)
			  _logShipper.onTimeout();
			  _outbound.clear();

			  // fake a match
			  _logShipper.onMatch( 0, new LeaderContext( 0, 0 ) );

			  assertThat( _outbound.sentTo( _follower ), Matchers.hasRaftLogEntries( asList( _entry1, _entry2, _entry3 ) ) );
		 }
	}

}