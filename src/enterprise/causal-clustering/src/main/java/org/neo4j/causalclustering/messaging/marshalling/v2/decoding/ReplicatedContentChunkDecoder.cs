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
namespace Neo4Net.causalclustering.messaging.marshalling.v2.decoding
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ByteToMessageDecoder = io.netty.handler.codec.ByteToMessageDecoder;

	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using Neo4Net.causalclustering.messaging.marshalling;

	public class ReplicatedContentChunkDecoder : ByteToMessageDecoder
	{
		 private readonly Codec<ReplicatedContent> _codec = CoreReplicatedContentMarshal.codec();
		 private bool _expectingNewContent = true;
		 private bool _isLast;

		 internal ReplicatedContentChunkDecoder()
		 {
			  Cumulator = new ContentChunkCumulator( this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void decode(io.netty.channel.ChannelHandlerContext ctx, io.netty.buffer.ByteBuf in, java.util.List<Object> out) throws Exception
		 protected internal override void Decode( ChannelHandlerContext ctx, ByteBuf @in, IList<object> @out )
		 {
			  if ( _expectingNewContent )
			  {
					_isLast = @in.readBoolean();
					_expectingNewContent = false;
			  }
			  if ( _isLast )
			  {
					@out.Add( _codec.decode( @in ) );
					_isLast = false;
					_expectingNewContent = true;
			  }
		 }

		 private class ContentChunkCumulator : Cumulator
		 {
			 private readonly ReplicatedContentChunkDecoder _outerInstance;

			 public ContentChunkCumulator( ReplicatedContentChunkDecoder outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override ByteBuf Cumulate( ByteBufAllocator alloc, ByteBuf cumulation, ByteBuf @in )
			  {
					outerInstance.isLast = @in.readBoolean();
					return COMPOSITE_CUMULATOR.cumulate( alloc, cumulation, @in );
			  }
		 }
	}

}