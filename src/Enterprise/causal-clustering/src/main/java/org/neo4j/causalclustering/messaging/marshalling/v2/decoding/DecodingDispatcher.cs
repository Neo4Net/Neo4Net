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
	using Neo4Net.causalclustering.catchup;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class DecodingDispatcher : RequestDecoderDispatcher<ContentType>
	{
		 public DecodingDispatcher( Protocol<ContentType> protocol, LogProvider logProvider ) : base( protocol, logProvider )
		 {
			  Register( ContentType.ContentType, new ByteToMessageDecoderAnonymousInnerClass( this ) );
			  Register( ContentType.RaftLogEntryTerms, new RaftLogEntryTermsDecoder( protocol ) );
			  Register( ContentType.ReplicatedContent, new ReplicatedContentChunkDecoder() );
			  Register( ContentType.Message, new RaftMessageDecoder( protocol ) );
		 }

		 private class ByteToMessageDecoderAnonymousInnerClass : ByteToMessageDecoder
		 {
			 private readonly DecodingDispatcher _outerInstance;

			 public ByteToMessageDecoderAnonymousInnerClass( DecodingDispatcher outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void decode( ChannelHandlerContext ctx, ByteBuf @in, IList<object> @out )
			 {
				  if ( @in.readableBytes() > 0 )
				  {
						throw new System.InvalidOperationException( "Not expecting any data here" );
				  }
			 }
		 }
	}

}