using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Bolt.transport.pipeline
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ByteToMessageDecoder = io.netty.handler.codec.ByteToMessageDecoder;
	using DecoderException = io.netty.handler.codec.DecoderException;

	using FeatureToggles = Org.Neo4j.Util.FeatureToggles;

	public class MessageAccumulator : ByteToMessageDecoder
	{
		 private static readonly bool _useMergeCumulator = FeatureToggles.flag( typeof( MessageAccumulator ), "mergeCumulator", false );
		 private bool _readMessageBoundary;

		 public MessageAccumulator()
		 {
			  if ( _useMergeCumulator )
			  {
					Cumulator = MERGE_CUMULATOR;
			  }
			  else
			  {
					Cumulator = COMPOSITE_CUMULATOR;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void channelRead(io.netty.channel.ChannelHandlerContext ctx, Object msg) throws Exception
		 public override void ChannelRead( ChannelHandlerContext ctx, object msg )
		 {
			  ByteBuf buf = ( ByteBuf ) msg;

			  if ( buf.readableBytes() == 0 )
			  {
					AssertNonEmptyMessage();

					_readMessageBoundary = true;
			  }

			  base.ChannelRead( ctx, msg );
		 }

		 protected internal override void Decode( ChannelHandlerContext channelHandlerContext, ByteBuf @in, IList<object> @out )
		 {
			  if ( _readMessageBoundary )
			  {
					// now we have a complete message in the input buffer

					// increment ref count of the buffer and create it's duplicate that shares the content
					// duplicate will be the output of this decoded and input for the next one
					ByteBuf messageBuf = @in.retainedDuplicate();

					// signal that whole message was read by making input buffer seem like it was fully read/consumed
					@in.readerIndex( @in.readableBytes() );

					// pass the full message to the next handler in the pipeline
					@out.Add( messageBuf );

					_readMessageBoundary = false;
			  }
		 }

		 private void AssertNonEmptyMessage()
		 {
			  if ( actualReadableBytes() == 0 )
			  {
					throw new DecoderException( "Message boundary received when there's nothing to decode." );
			  }
		 }
	}

}