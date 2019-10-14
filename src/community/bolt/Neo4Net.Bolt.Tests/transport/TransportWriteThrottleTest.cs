using System;
using System.Threading;

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
namespace Neo4Net.Bolt.transport
{
	using Channel = io.netty.channel.Channel;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandler = io.netty.channel.ChannelInboundHandler;
	using ChannelPipeline = io.netty.channel.ChannelPipeline;
	using WriteBufferWaterMark = io.netty.channel.WriteBufferWaterMark;
	using SocketChannel = io.netty.channel.socket.SocketChannel;
	using SocketChannelConfig = io.netty.channel.socket.SocketChannelConfig;
	using Attribute = io.netty.util.Attribute;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Answers = org.mockito.Answers;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;


	using Neo4Net.Test.rule.concurrent;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.atLeast;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class TransportWriteThrottleTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.concurrent.OtherThreadRule<Void> otherThread = new org.neo4j.test.rule.concurrent.OtherThreadRule<>(1, java.util.concurrent.TimeUnit.MINUTES);
		 public OtherThreadRule<Void> OtherThread = new OtherThreadRule<Void>( 1, TimeUnit.MINUTES );

		 private ChannelHandlerContext _context;
		 private Channel _channel;
		 private SocketChannelConfig _config;
		 private ThrottleLock @lock;
		 private Attribute _lockAttribute;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  @lock = NewThrottleLockMock();

			  _config = mock( typeof( SocketChannelConfig ) );

			  _lockAttribute = mock( typeof( Attribute ) );
			  when( _lockAttribute.get() ).thenReturn(@lock);

			  Attribute durationExceedAttribute = mock( typeof( Attribute ) );
			  when( durationExceedAttribute.get() ).thenReturn(null);

			  _channel = mock( typeof( SocketChannel ), Answers.RETURNS_MOCKS );
			  when( _channel.config() ).thenReturn(_config);
			  when( _channel.Open ).thenReturn( true );
			  when( _channel.remoteAddress() ).thenReturn(InetSocketAddress.createUnresolved("localhost", 32000));
			  when( _channel.attr( TransportWriteThrottle.LockKey ) ).thenReturn( _lockAttribute );
			  when( _channel.attr( TransportWriteThrottle.MaxDurationExceededKey ) ).thenReturn( durationExceedAttribute );

			  ChannelPipeline pipeline = _channel.pipeline();
			  when( _channel.pipeline() ).thenReturn(pipeline);

			  _context = mock( typeof( ChannelHandlerContext ), Answers.RETURNS_MOCKS );
			  when( _context.channel() ).thenReturn(_channel);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetWriteBufferWatermarkOnChannelConfigWhenInstalled()
		 public virtual void ShouldSetWriteBufferWatermarkOnChannelConfigWhenInstalled()
		 {
			  // given
			  TransportThrottle throttle = NewThrottle();

			  // when
			  throttle.Install( _channel );

			  // expect
			  ArgumentCaptor<WriteBufferWaterMark> argument = ArgumentCaptor.forClass( typeof( WriteBufferWaterMark ) );
			  verify( _config, times( 1 ) ).WriteBufferWaterMark = argument.capture();

			  assertEquals( 64, argument.Value.low() );
			  assertEquals( 256, argument.Value.high() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLockWhenWritable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLockWhenWritable()
		 {
			  // given
			  TestThrottleLock lockOverride = new TestThrottleLock();
			  TransportThrottle throttle = NewThrottleAndInstall( _channel, lockOverride );
			  when( _channel.Writable ).thenReturn( true );

			  // when
			  Future future = OtherThread.execute(state =>
			  {
				throttle.Acquire( _channel );
				return null;
			  });

			  // expect
			  try
			  {
					future.get( 2000, TimeUnit.MILLISECONDS );
			  }
			  catch ( Exception )
			  {
					fail( "should not throw" );
			  }

			  assertTrue( future.Done );
			  assertThat( lockOverride.LockCallCount(), @is(0) );
			  assertThat( lockOverride.UnlockCallCount(), @is(0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLockWhenNotWritable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLockWhenNotWritable()
		 {
			  // given
			  TestThrottleLock lockOverride = new TestThrottleLock();
			  TransportThrottle throttle = NewThrottleAndInstall( _channel, lockOverride );
			  when( _channel.Writable ).thenReturn( false );

			  // when
			  Future<Void> future = OtherThread.execute(state =>
			  {
				throttle.Acquire( _channel );
				return null;
			  });

			  // expect
			  try
			  {
					future.get( 2000, TimeUnit.MILLISECONDS );

					fail( "should timeout" );
			  }
			  catch ( TimeoutException )
			  {
					// expected
			  }

			  assertFalse( future.Done );
			  assertThat( lockOverride.LockCallCount(), greaterThan(0) );
			  assertThat( lockOverride.UnlockCallCount(), @is(0) );

			  // stop the thread that is trying to acquire the lock
			  // otherwise it remains actively spinning even after the test
			  future.cancel( true );
			  try
			  {
					OtherThread.get().awaitFuture(future);
					fail( "Exception expected" );
			  }
			  catch ( CancellationException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResumeWhenWritableOnceAgain() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResumeWhenWritableOnceAgain()
		 {
			  // given
			  TransportThrottle throttle = NewThrottleAndInstall( _channel );
			  when( _channel.Writable ).thenReturn( false ).thenReturn( true );

			  // when
			  throttle.Acquire( _channel );

			  // expect
			  verify( @lock, atLeast( 1 ) ).@lock( any(), anyLong() );
			  verify( @lock, never() ).unlock(any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResumeWhenWritabilityChanged() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResumeWhenWritabilityChanged()
		 {
			  TestThrottleLock lockOverride = new TestThrottleLock();

			  // given
			  TransportThrottle throttle = NewThrottleAndInstall( _channel, lockOverride );
			  when( _channel.Writable ).thenReturn( false );

			  Future<Void> completionFuture = OtherThread.execute(state =>
			  {
				throttle.Acquire( _channel );
				return null;
			  });

			  OtherThread.get().waitUntilWaiting();

			  // when
			  when( _channel.Writable ).thenReturn( true );
			  ArgumentCaptor<ChannelInboundHandler> captor = ArgumentCaptor.forClass( typeof( ChannelInboundHandler ) );
			  verify( _channel.pipeline() ).addLast(captor.capture());
			  captor.Value.channelWritabilityChanged( _context );

			  OtherThread.get().awaitFuture(completionFuture);

			  assertThat( lockOverride.LockCallCount(), greaterThan(0) );
			  assertThat( lockOverride.UnlockCallCount(), @is(1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowThrottleExceptionWhenMaxDurationIsReached() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowThrottleExceptionWhenMaxDurationIsReached()
		 {
			  // given
			  TestThrottleLock lockOverride = new TestThrottleLock();
			  FakeClock clock = Clocks.fakeClock( 1, TimeUnit.SECONDS );
			  TransportThrottle throttle = NewThrottleAndInstall( _channel, lockOverride, clock, Duration.ofSeconds( 5 ) );
			  when( _channel.Writable ).thenReturn( false );

			  // when
			  Future<Void> future = OtherThread.execute(state =>
			  {
				throttle.Acquire( _channel );
				return null;
			  });

			  OtherThread.get().waitUntilWaiting();
			  clock.Forward( 6, TimeUnit.SECONDS );

			  // expect
			  try
			  {
					future.get( 1, TimeUnit.MINUTES );

					fail( "expecting ExecutionException" );
			  }
			  catch ( ExecutionException ex )
			  {
					assertThat( ex.InnerException, instanceOf( typeof( TransportThrottleException ) ) );
					assertThat( ex.Message, containsString( "will be closed because the client did not consume outgoing buffers for" ) );
			  }
		 }

		 private TransportThrottle NewThrottle()
		 {
			  return NewThrottle( null, Clocks.systemClock(), Duration.ZERO );
		 }

		 private TransportThrottle NewThrottle( ThrottleLock lockOverride, Clock clock, Duration maxLockDuration )
		 {
			  if ( lockOverride != null )
			  {
					@lock = lockOverride;

					when( _lockAttribute.get() ).thenReturn(lockOverride);
			  }

			  return new TransportWriteThrottle( 64, 256, clock, maxLockDuration, () => @lock );
		 }

		 private TransportThrottle NewThrottleAndInstall( Channel channel )
		 {
			  return NewThrottleAndInstall( channel, null );
		 }

		 private TransportThrottle NewThrottleAndInstall( Channel channel, ThrottleLock lockOverride )
		 {
			  return NewThrottleAndInstall( channel, lockOverride, Clocks.systemClock(), Duration.ZERO );
		 }

		 private TransportThrottle NewThrottleAndInstall( Channel channel, ThrottleLock lockOverride, Clock clock, Duration maxLockDuration )
		 {
			  TransportThrottle throttle = NewThrottle( lockOverride, clock, maxLockDuration );

			  throttle.Install( channel );

			  return throttle;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static ThrottleLock newThrottleLockMock() throws InterruptedException
		 private static ThrottleLock NewThrottleLockMock()
		 {
			  ThrottleLock @lock = mock( typeof( ThrottleLock ) );
			  doAnswer(invocation =>
			  {
				// sleep a bit to prevent the caller thread spinning in a tight loop
				// every mock invocation is recorded and generates objects, like the stacktrace
				Thread.Sleep( 500 );
				return null;
			  }).when( @lock ).@lock( any(), anyLong() );
			  return @lock;
		 }

		 private class TestThrottleLock : ThrottleLock
		 {
			  internal AtomicInteger LockCount = new AtomicInteger( 0 );
			  internal AtomicInteger UnlockCount = new AtomicInteger( 0 );
			  internal ThrottleLock ActualLock = new DefaultThrottleLock();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void lock(io.netty.channel.Channel channel, long timeout) throws InterruptedException
			  public override void Lock( Channel channel, long timeout )
			  {
					ActualLock.@lock( channel, timeout );
					LockCount.incrementAndGet();
			  }

			  public override void Unlock( Channel channel )
			  {
					ActualLock.unlock( channel );
					UnlockCount.incrementAndGet();
			  }

			  public virtual int LockCallCount()
			  {
					return LockCount.get();
			  }

			  public virtual int UnlockCallCount()
			  {
					return UnlockCount.get();
			  }

		 }

	}

}