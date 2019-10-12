using System;
using System.Collections.Generic;

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
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;
	using Channel = org.jboss.netty.channel.Channel;
	using ChannelConfig = org.jboss.netty.channel.ChannelConfig;
	using ChannelFactory = org.jboss.netty.channel.ChannelFactory;
	using ChannelFuture = org.jboss.netty.channel.ChannelFuture;
	using ChannelFutureListener = org.jboss.netty.channel.ChannelFutureListener;
	using ChannelPipeline = org.jboss.netty.channel.ChannelPipeline;
	using BlockingReadHandler = org.jboss.netty.handler.queue.BlockingReadHandler;


	public class RecordingChannel : Channel
	{
		 private LinkedList<ChannelBuffer> _receivedMessages = new LinkedList<ChannelBuffer>();

		 public override ChannelFuture Write( object message )
		 {
			  if ( message is ChannelBuffer )
			  {
					ChannelBuffer buffer = ( ChannelBuffer ) message;
					_receivedMessages.AddLast( buffer.duplicate() );
			  }
			  return immediateFuture;
		 }

		 public override ChannelFuture Write( object message, SocketAddress remoteAddress )
		 {
			  Write( message );
			  return immediateFuture;
		 }

		 public override int? Id
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override ChannelFactory Factory
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override Channel Parent
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override ChannelConfig Config
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override ChannelPipeline Pipeline
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override bool Open
		 {
			 get
			 {
				  return true;
			 }
		 }

		 public override bool Bound
		 {
			 get
			 {
				  return true;
			 }
		 }

		 public override bool Connected
		 {
			 get
			 {
				  return true;
			 }
		 }

		 public override SocketAddress LocalAddress
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override SocketAddress RemoteAddress
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override ChannelFuture Bind( SocketAddress localAddress )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ChannelFuture Connect( SocketAddress remoteAddress )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ChannelFuture Disconnect()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ChannelFuture Unbind()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ChannelFuture Close()
		 {
			  return null;
		 }

		 public override ChannelFuture CloseFuture
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override int InterestOps
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override bool Readable
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public override bool Writable
		 {
			 get
			 {
				  return true;
			 }
		 }

		 public override ChannelFuture setInterestOps( int interestOps )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ChannelFuture setReadable( bool readable )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override object Attachment
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
			 set
			 {
				  throw new System.NotSupportedException();
			 }
		 }


		 public override int CompareTo( Channel o )
		 {
			  return 0;
		 }

		 // This is due to a tight coupling of the netty pipeline and message deserialization, we can't deserialize without
		 // this pipeline item yet. We should refactor the serialization/deserialization code appropriately such that it is
		 // not tied like this to components it should not be aware of.
		 public virtual BlockingReadHandler<ChannelBuffer> AsBlockingReadHandler()
		 {
			  return new BlockingReadHandlerAnonymousInnerClass( this );
		 }

		 private class BlockingReadHandlerAnonymousInnerClass : BlockingReadHandler<ChannelBuffer>
		 {
			 private readonly RecordingChannel _outerInstance;

			 public BlockingReadHandlerAnonymousInnerClass( RecordingChannel outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override ChannelBuffer read()
			 {
				  return _outerInstance.receivedMessages.RemoveFirst();
			 }

			 public override ChannelBuffer read( long timeout, TimeUnit unit )
			 {
				  return read();
			 }
		 }

		 private ChannelFuture immediateFuture = new ChannelFutureAnonymousInnerClass();

		 private class ChannelFutureAnonymousInnerClass : ChannelFuture
		 {
			 public override Channel Channel
			 {
				 get
				 {
					  return _outerInstance;
				 }
			 }

			 public override bool Done
			 {
				 get
				 {
					  return true;
				 }
			 }

			 public override bool Cancelled
			 {
				 get
				 {
					  return false;
				 }
			 }

			 public override bool Success
			 {
				 get
				 {
					  return true;
				 }
			 }

			 public override Exception Cause
			 {
				 get
				 {
					  return null;
				 }
			 }

			 public override bool cancel()
			 {
				  return false;
			 }

			 public override bool setSuccess()
			 {
				  return true;
			 }

			 public override bool setFailure( Exception cause )
			 {
				  return false;
			 }

			 public override bool setProgress( long amount, long current, long total )
			 {
				  return false;
			 }

			 public override void addListener( ChannelFutureListener listener )
			 {
				  try
				  {
						listener.operationComplete( this );
				  }
				  catch ( Exception e )
				  {
						throw new Exception( e );
				  }
			 }

			 public override void removeListener( ChannelFutureListener listener )
			 {
			 }

			 public override ChannelFuture rethrowIfFailed()
			 {
				  return null;
			 }

			 public override ChannelFuture sync()
			 {
				  return null;
			 }

			 public override ChannelFuture syncUninterruptibly()
			 {
				  return null;
			 }

			 public override ChannelFuture await()
			 {
				  return null;
			 }

			 public override ChannelFuture awaitUninterruptibly()
			 {
				  return null;
			 }

			 public override bool await( long timeout, TimeUnit unit )
			 {
				  return false;
			 }

			 public override bool await( long timeoutMillis )
			 {
				  return false;
			 }

			 public override bool awaitUninterruptibly( long timeout, TimeUnit unit )
			 {
				  return false;
			 }

			 public override bool awaitUninterruptibly( long timeoutMillis )
			 {
				  return false;
			 }
		 }
	}

}