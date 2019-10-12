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
namespace Org.Neo4j.causalclustering.catchup
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;
	using ReferenceCountUtil = io.netty.util.ReferenceCountUtil;

	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.ResponseMessageType.from;

	public class ClientMessageTypeHandler : ChannelInboundHandlerAdapter
	{
		 private readonly Log _log;
		 private readonly CatchupClientProtocol _protocol;

		 public ClientMessageTypeHandler( CatchupClientProtocol protocol, LogProvider logProvider )
		 {
			  this._protocol = protocol;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override void ChannelRead( ChannelHandlerContext ctx, object msg )
		 {
			  if ( _protocol.isExpecting( CatchupClientProtocol.State.MessageType ) )
			  {
					sbyte byteValue = ( ( ByteBuf ) msg ).readByte();
					ResponseMessageType responseMessageType = from( byteValue );

					switch ( responseMessageType.innerEnumValue )
					{
					case Org.Neo4j.causalclustering.catchup.ResponseMessageType.InnerEnum.STORE_ID:
						 _protocol.expect( CatchupClientProtocol.State.StoreId );
						 break;
					case Org.Neo4j.causalclustering.catchup.ResponseMessageType.InnerEnum.TX:
						 _protocol.expect( CatchupClientProtocol.State.TxPullResponse );
						 break;
					case Org.Neo4j.causalclustering.catchup.ResponseMessageType.InnerEnum.FILE:
						 _protocol.expect( CatchupClientProtocol.State.FileHeader );
						 break;
					case Org.Neo4j.causalclustering.catchup.ResponseMessageType.InnerEnum.STORE_COPY_FINISHED:
						 _protocol.expect( CatchupClientProtocol.State.StoreCopyFinished );
						 break;
					case Org.Neo4j.causalclustering.catchup.ResponseMessageType.InnerEnum.CORE_SNAPSHOT:
						 _protocol.expect( CatchupClientProtocol.State.CoreSnapshot );
						 break;
					case Org.Neo4j.causalclustering.catchup.ResponseMessageType.InnerEnum.TX_STREAM_FINISHED:
						 _protocol.expect( CatchupClientProtocol.State.TxStreamFinished );
						 break;
					case Org.Neo4j.causalclustering.catchup.ResponseMessageType.InnerEnum.PREPARE_STORE_COPY_RESPONSE:
						 _protocol.expect( CatchupClientProtocol.State.PrepareStoreCopyResponse );
						 break;
					case Org.Neo4j.causalclustering.catchup.ResponseMessageType.InnerEnum.INDEX_SNAPSHOT_RESPONSE:
						 _protocol.expect( CatchupClientProtocol.State.IndexSnapshotResponse );
						 break;
					default:
						 _log.warn( "No handler found for message type %s (%d)", responseMessageType.name(), byteValue );
					 break;
					}

					ReferenceCountUtil.release( msg );
			  }
			  else
			  {
					ctx.fireChannelRead( msg );
			  }
		 }
	}

}