using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.causalclustering.catchup
{
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandler = io.netty.channel.ChannelInboundHandler;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;
	using ReferenceCountUtil = io.netty.util.ReferenceCountUtil;


	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class RequestDecoderDispatcher<E> : ChannelInboundHandlerAdapter where E : Enum<E>
	{
		 private readonly IDictionary<E, ChannelInboundHandler> _decoders = new Dictionary<E, ChannelInboundHandler>();
		 private readonly Protocol<E> _protocol;
		 private readonly Log _log;

		 public RequestDecoderDispatcher( Protocol<E> protocol, LogProvider logProvider )
		 {
			  this._protocol = protocol;
			  this._log = logProvider.getLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void channelRead(io.netty.channel.ChannelHandlerContext ctx, Object msg) throws Exception
		 public override void ChannelRead( ChannelHandlerContext ctx, object msg )
		 {
			  ChannelInboundHandler @delegate = _protocol.select( _decoders );
			  if ( @delegate == null )
			  {
					_log.warn( "Unregistered handler for protocol %s", _protocol );

					/*
					 * Since we cannot process this message further we need to release the message as per netty doc
					 * see http://netty.io/wiki/reference-counted-objects.html#inbound-messages
					 */
					ReferenceCountUtil.release( msg );
					return;
			  }
			  @delegate.channelRead( ctx, msg );
		 }

		 public virtual void Register( E type, ChannelInboundHandler decoder )
		 {
			  Debug.Assert( !_decoders.ContainsKey( type ), "registering twice a decoder for the same type (" + type + ")?" );
			  _decoders[type] = decoder;
		 }
	}

}