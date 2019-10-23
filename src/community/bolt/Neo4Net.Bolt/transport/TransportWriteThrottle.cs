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
	using ChannelHandler = io.netty.channel.ChannelHandler;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandler = io.netty.channel.ChannelInboundHandler;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;
	using WriteBufferWaterMark = io.netty.channel.WriteBufferWaterMark;
	using AttributeKey = io.netty.util.AttributeKey;
	using DurationFormatUtils = org.apache.commons.lang3.time.DurationFormatUtils;


	/// <summary>
	/// Throttle that blocks write operations to the channel based on channel's isWritable
	/// property. Buffer sizes based on which the channel will change its isWritable property
	/// and whether to apply this throttle are configurable through GraphDatabaseSettings.
	/// </summary>
	public class TransportWriteThrottle : TransportThrottle
	{
		 internal static readonly AttributeKey<ThrottleLock> LockKey = AttributeKey.ValueOf( "BOLT.WRITE_THROTTLE.LOCK" );
		 internal static readonly AttributeKey<bool> MaxDurationExceededKey = AttributeKey.ValueOf( "BOLT.WRITE_THROTTLE.MAX_DURATION_EXCEEDED" );
		 private readonly int _lowWaterMark;
		 private readonly int _highWaterMark;
		 private readonly Clock _clock;
		 private readonly long _maxLockDuration;
		 private readonly System.Func<ThrottleLock> _lockSupplier;
		 private readonly ChannelInboundHandler _listener;

		 public TransportWriteThrottle( int lowWaterMark, int highWaterMark, Clock clock, Duration maxLockDuration ) : this( lowWaterMark, highWaterMark, clock, maxLockDuration, DefaultThrottleLock::new )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 }

		 public TransportWriteThrottle( int lowWaterMark, int highWaterMark, Clock clock, Duration maxLockDuration, System.Func<ThrottleLock> lockSupplier )
		 {
			  this._lowWaterMark = lowWaterMark;
			  this._highWaterMark = highWaterMark;
			  this._clock = clock;
			  this._maxLockDuration = maxLockDuration.toMillis();
			  this._lockSupplier = lockSupplier;
			  this._listener = new ChannelStatusListener( this );
		 }

		 public override void Install( Channel channel )
		 {
			  ThrottleLock @lock = _lockSupplier.get();

			  channel.attr( LockKey ).set( @lock );
			  channel.config().WriteBufferWaterMark = new WriteBufferWaterMark(_lowWaterMark, _highWaterMark);
			  channel.pipeline().addLast(_listener);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquire(io.netty.channel.Channel channel) throws TransportThrottleException
		 public override void Acquire( Channel channel )
		 {
			  // if this channel's max lock duration is already exceeded, we'll allow the protocol to
			  // (at least) try to communicate the error to the client before aborting the connection
			  if ( !IsDurationAlreadyExceeded( channel ) )
			  {
					ThrottleLock @lock = channel.attr( LockKey ).get();

					long startTimeMillis = 0;
					while ( channel.Open && !channel.Writable )
					{
						 if ( _maxLockDuration > 0 )
						 {
							  long currentTimeMillis = _clock.millis();
							  if ( startTimeMillis == 0 )
							  {
									startTimeMillis = currentTimeMillis;
							  }
							  else
							  {
									if ( currentTimeMillis - startTimeMillis > _maxLockDuration )
									{
										 DurationExceeded = channel;

										 throw new TransportThrottleException( string.Format( "Bolt connection [{0}] will be closed because the client did not consume outgoing buffers for {1} which is not expected.", channel.remoteAddress(), DurationFormatUtils.formatDurationHMS(_maxLockDuration) ) );
									}
							  }
						 }

						 try
						 {
							  @lock.Lock( channel, 1000 );
						 }
						 catch ( InterruptedException ex )
						 {
							  Thread.CurrentThread.Interrupt();
							  throw new Exception( ex );
						 }
					}
			  }
		 }

		 public override void Release( Channel channel )
		 {
			  if ( channel.Writable )
			  {
					ThrottleLock @lock = channel.attr( LockKey ).get();

					@lock.Unlock( channel );
			  }
		 }

		 public override void Uninstall( Channel channel )
		 {
			  channel.attr( LockKey ).set( null );
		 }

		 private static bool IsDurationAlreadyExceeded( Channel channel )
		 {
			  bool? marker = channel.attr( MaxDurationExceededKey ).get();
			  return marker != null && marker;
		 }

		 private static Channel DurationExceeded
		 {
			 set
			 {
				  value.attr( MaxDurationExceededKey ).set( true );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ChannelHandler.Sharable private class ChannelStatusListener extends io.netty.channel.ChannelInboundHandlerAdapter
		 private class ChannelStatusListener : ChannelInboundHandlerAdapter
		 {
			 private readonly TransportWriteThrottle _outerInstance;

			 public ChannelStatusListener( TransportWriteThrottle outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }


			  public override void ChannelWritabilityChanged( ChannelHandlerContext ctx )
			  {
					outerInstance.Release( ctx.channel() );
			  }
		 }
	}

}