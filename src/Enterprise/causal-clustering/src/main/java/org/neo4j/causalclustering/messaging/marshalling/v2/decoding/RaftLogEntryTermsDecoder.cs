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
namespace Neo4Net.causalclustering.messaging.marshalling.v2.decoding
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ByteToMessageDecoder = io.netty.handler.codec.ByteToMessageDecoder;

	using Neo4Net.causalclustering.catchup;

	internal class RaftLogEntryTermsDecoder : ByteToMessageDecoder
	{
		 private readonly Protocol<ContentType> _protocol;

		 internal RaftLogEntryTermsDecoder( Protocol<ContentType> protocol )
		 {
			  this._protocol = protocol;
		 }

		 protected internal override void Decode( ChannelHandlerContext ctx, ByteBuf @in, IList<object> @out )
		 {
			  int size = @in.readInt();
			  long[] terms = new long[size];
			  for ( int i = 0; i < size; i++ )
			  {
				  terms[i] = @in.readLong();
			  }
			  @out.Add( new RaftLogEntryTerms( this, terms ) );
			  _protocol.expect( ContentType.ContentType );
		 }

		 internal class RaftLogEntryTerms
		 {
			 private readonly RaftLogEntryTermsDecoder _outerInstance;

			  internal readonly long[] Term;

			  internal RaftLogEntryTerms( RaftLogEntryTermsDecoder outerInstance, long[] term )
			  {
				  this._outerInstance = outerInstance;
					this.Term = term;
			  }

			  public virtual long[] Terms()
			  {
					return Term;
			  }
		 }
	}

}