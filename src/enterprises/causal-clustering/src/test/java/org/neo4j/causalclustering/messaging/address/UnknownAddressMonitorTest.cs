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
namespace Neo4Net.causalclustering.messaging.address
{
	using Test = org.junit.Test;

	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Log = Neo4Net.Logging.Log;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;

	public class UnknownAddressMonitorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogFirstFailure()
		 public virtual void ShouldLogFirstFailure()
		 {
			  // given
			  Log log = mock( typeof( Log ) );
			  UnknownAddressMonitor logger = new UnknownAddressMonitor( log, TestClock(), 100 );

			  // when
			  MemberId to = member( 0 );
			  logger.LogAttemptToSendToMemberWithNoKnownAddress( to );

			  // then
			  verify( log ).info( format( "No address found for %s, probably because the member has been shut down.", to ) );
		 }

		 private FakeClock TestClock()
		 {
			  return Clocks.fakeClock( 1_000_000, MILLISECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrottleLogging()
		 public virtual void ShouldThrottleLogging()
		 {
			  // given
			  Log log = mock( typeof( Log ) );
			  FakeClock clock = TestClock();
			  UnknownAddressMonitor logger = new UnknownAddressMonitor( log, clock, 1000 );
			  MemberId to = member( 0 );

			  // when
			  logger.LogAttemptToSendToMemberWithNoKnownAddress( to );
			  clock.Forward( 1, MILLISECONDS );
			  logger.LogAttemptToSendToMemberWithNoKnownAddress( to );

			  // then
			  verify( log, times( 1 ) ).info( format( "No address found for %s, probably because the member has been shut " + "down.", to ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResumeLoggingAfterQuietPeriod()
		 public virtual void ShouldResumeLoggingAfterQuietPeriod()
		 {
			  // given
			  Log log = mock( typeof( Log ) );
			  FakeClock clock = TestClock();
			  UnknownAddressMonitor logger = new UnknownAddressMonitor( log, clock, 1000 );
			  MemberId to = member( 0 );

			  // when
			  logger.LogAttemptToSendToMemberWithNoKnownAddress( to );
			  clock.Forward( 20001, MILLISECONDS );
			  logger.LogAttemptToSendToMemberWithNoKnownAddress( to );
			  clock.Forward( 80001, MILLISECONDS );
			  logger.LogAttemptToSendToMemberWithNoKnownAddress( to );

			  // then
			  verify( log, times( 3 ) ).info( format( "No address found for %s, probably because the member has been shut " + "down.", to ) );
		 }
	}

}