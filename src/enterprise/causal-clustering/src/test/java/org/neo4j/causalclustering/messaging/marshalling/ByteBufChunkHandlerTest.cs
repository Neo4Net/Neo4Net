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
namespace Neo4Net.causalclustering.messaging.marshalling
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChunkedInput = io.netty.handler.stream.ChunkedInput;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Buffers = Neo4Net.causalclustering.helpers.Buffers;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;

	public class ByteBufChunkHandlerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.causalclustering.helpers.Buffers buffers = new Neo4Net.causalclustering.helpers.Buffers();
		 public readonly Buffers Buffers = new Buffers();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.causalclustering.messaging.MessageTooBigException.class) public void shouldThrowExceptioIfToLarge() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptioIfToLarge()
		 {
			  MaxTotalSize maxTotalSize = new MaxTotalSize( new PredictableChunkedInput( this, 10, 1 ), 10 );

			  maxTotalSize.ReadChunk( Buffers );
			  maxTotalSize.ReadChunk( Buffers );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowIfNotTooLarge() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowIfNotTooLarge()
		 {
			  MaxTotalSize maxTotalSize = new MaxTotalSize( new PredictableChunkedInput( this, 10, 1 ), 11 );

			  maxTotalSize.ReadChunk( Buffers );
			  maxTotalSize.ReadChunk( Buffers );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void shouldThrowIfIllegalSizeValue()
		 public virtual void ShouldThrowIfIllegalSizeValue()
		 {
			  new MaxTotalSize( new PredictableChunkedInput( this ), -1 );
		 }

		 private class PredictableChunkedInput : ChunkedInput<ByteBuf>
		 {
			 private readonly ByteBufChunkHandlerTest _outerInstance;

			  internal readonly IEnumerator<int> Sizes;

			  internal PredictableChunkedInput( ByteBufChunkHandlerTest outerInstance, params int[] sizes )
			  {
				  this._outerInstance = outerInstance;
					this.Sizes = Iterators.asIterator( sizes );
			  }

			  public override bool EndOfInput
			  {
				  get
				  {
	//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						return !Sizes.hasNext();
				  }
			  }

			  public override void Close()
			  {

			  }

			  public override ByteBuf ReadChunk( ChannelHandlerContext ctx )
			  {
					return ReadChunk( ctx.alloc() );
			  }

			  public override ByteBuf ReadChunk( ByteBufAllocator allocator )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					int? size = Sizes.next();
					if ( size == null )
					{
						 return null;
					}
					return allocator.buffer( size ).writerIndex( size );
			  }

			  public override long Length()
			  {
					return 0;
			  }

			  public override long Progress()
			  {
					return 0;
			  }
		 }
	}

}