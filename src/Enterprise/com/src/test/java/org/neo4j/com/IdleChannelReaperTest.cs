/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Neo4Net.com
{
	using Channel = org.jboss.netty.channel.Channel;
	using Test = org.junit.Test;

	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class IdleChannelReaperTest
	{
		 private const int THRESHOLD = 100;
		 private static readonly NullLogProvider _noLogging = NullLogProvider.Instance;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCloseAnyRecentlyActiveChannels()
		 public virtual void ShouldNotCloseAnyRecentlyActiveChannels()
		 {
			  // given
			  FakeClock clock = Clocks.fakeClock();
			  ChannelCloser channelCloser = mock( typeof( ChannelCloser ) );
			  IdleChannelReaper idleChannelReaper = new IdleChannelReaper( channelCloser, _noLogging, clock, THRESHOLD );

			  Channel channel = mock( typeof( Channel ) );
			  idleChannelReaper.Add( channel, DummyRequestContext() );

			  // when
			  idleChannelReaper.Run();

			  // then
			  verifyNoMoreInteractions( channelCloser );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseAnyChannelsThatHaveBeenIdleForLongerThanThreshold()
		 public virtual void ShouldCloseAnyChannelsThatHaveBeenIdleForLongerThanThreshold()
		 {
			  // given
			  FakeClock clock = Clocks.fakeClock();
			  ChannelCloser channelCloser = mock( typeof( ChannelCloser ) );
			  IdleChannelReaper idleChannelReaper = new IdleChannelReaper( channelCloser, _noLogging, clock, THRESHOLD );

			  Channel channel = mock( typeof( Channel ) );
			  idleChannelReaper.Add( channel, DummyRequestContext() );

			  // when
			  clock.Forward( THRESHOLD + 1, TimeUnit.MILLISECONDS );
			  idleChannelReaper.Run();

			  // then
			  verify( channelCloser ).tryToCloseChannel( channel );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCloseAChannelThatHasBeenIdleForMoreThanHalfThresholdButIsStillOpenConnectedAndBound()
		 public virtual void ShouldNotCloseAChannelThatHasBeenIdleForMoreThanHalfThresholdButIsStillOpenConnectedAndBound()
		 {
			  // given
			  FakeClock clock = Clocks.fakeClock();
			  ChannelCloser channelCloser = mock( typeof( ChannelCloser ) );
			  IdleChannelReaper idleChannelReaper = new IdleChannelReaper( channelCloser, _noLogging, clock, THRESHOLD );

			  Channel channel = mock( typeof( Channel ) );
			  idleChannelReaper.Add( channel, DummyRequestContext() );
			  when( channel.Open ).thenReturn( true );
			  when( channel.Connected ).thenReturn( true );
			  when( channel.Bound ).thenReturn( true );

			  // when
			  clock.Forward( THRESHOLD / 2 + 10, TimeUnit.MILLISECONDS );
			  idleChannelReaper.Run();

			  // then
			  verifyNoMoreInteractions( channelCloser );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTryToCloseAChannelThatHasBeenRemoved()
		 public virtual void ShouldNotTryToCloseAChannelThatHasBeenRemoved()
		 {
			  // given
			  FakeClock clock = Clocks.fakeClock();
			  ChannelCloser channelCloser = mock( typeof( ChannelCloser ) );
			  IdleChannelReaper idleChannelReaper = new IdleChannelReaper( channelCloser, _noLogging, clock, THRESHOLD );

			  Channel channel = mock( typeof( Channel ) );
			  RequestContext request = DummyRequestContext();

			  idleChannelReaper.Add( channel, request );

			  // when
			  idleChannelReaper.Remove( channel );
			  clock.Forward( THRESHOLD + 1, TimeUnit.MILLISECONDS );
			  idleChannelReaper.Run();

			  // then
			  verifyNoMoreInteractions( channelCloser );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTryToCloseAChannelThatWasRecentlyActive()
		 public virtual void ShouldNotTryToCloseAChannelThatWasRecentlyActive()
		 {
			  // given
			  FakeClock clock = Clocks.fakeClock();
			  ChannelCloser channelCloser = mock( typeof( ChannelCloser ) );
			  IdleChannelReaper idleChannelReaper = new IdleChannelReaper( channelCloser, _noLogging, clock, THRESHOLD );

			  Channel channel = mock( typeof( Channel ) );
			  RequestContext request = DummyRequestContext();

			  idleChannelReaper.Add( channel, request );

			  // when
			  clock.Forward( THRESHOLD + 100, TimeUnit.MILLISECONDS );
			  idleChannelReaper.Update( channel );
			  idleChannelReaper.Run();

			  // then
			  verifyNoMoreInteractions( channelCloser );
		 }

		 private RequestContext DummyRequestContext()
		 {
			  return new RequestContext( 1, 1, 1, 1, 1 );
		 }
	}

}