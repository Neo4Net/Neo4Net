using System;
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
namespace Neo4Net.causalclustering.messaging
{
	using ChannelDuplexHandler = io.netty.channel.ChannelDuplexHandler;
	using ChannelHandler = io.netty.channel.ChannelHandler;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelPromise = io.netty.channel.ChannelPromise;


	using GateEvent = Neo4Net.causalclustering.protocol.handshake.GateEvent;

	/// <summary>
	/// Gates messages and keeps them on a queue until the gate is either
	/// opened successfully or closed forever.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ChannelHandler.Sharable public class MessageGate extends io.netty.channel.ChannelDuplexHandler
	public class MessageGate : ChannelDuplexHandler
	{
		 private readonly System.Predicate<object> _gated;

		 private IList<GatedWrite> _pending = new List<GatedWrite>();

		 public MessageGate( System.Predicate<object> gated )
		 {
			  this._gated = gated;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void userEventTriggered(io.netty.channel.ChannelHandlerContext ctx, Object evt) throws Exception
		 public override void UserEventTriggered( ChannelHandlerContext ctx, object evt )
		 {
			  if ( evt is GateEvent )
			  {
					if ( GateEvent.Success.Equals( evt ) )
					{
						 foreach ( GatedWrite write in _pending )
						 {
							  ctx.write( write.Msg, write.Promise );
						 }

						 ctx.channel().pipeline().remove(this);
					}

					_pending.Clear();
					_pending = null;
			  }
			  else
			  {
					base.UserEventTriggered( ctx, evt );
			  }
		 }

		 public override void Write( ChannelHandlerContext ctx, object msg, ChannelPromise promise )
		 {
			  if ( !_gated.test( msg ) )
			  {
					ctx.write( msg, promise );
			  }
			  else if ( _pending != null )
			  {
					_pending.Add( new GatedWrite( msg, promise ) );
			  }
			  else
			  {
					promise.Failure = new Exception( "Gate failed and has been permanently closed." );
			  }
		 }

		 internal class GatedWrite
		 {
			  internal readonly object Msg;
			  internal readonly ChannelPromise Promise;

			  internal GatedWrite( object msg, ChannelPromise promise )
			  {
					this.Msg = msg;
					this.Promise = promise;
			  }
		 }
	}

}