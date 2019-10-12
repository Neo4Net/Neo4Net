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
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using MessageToMessageDecoder = io.netty.handler.codec.MessageToMessageDecoder;

	using Neo4Net.causalclustering.catchup;
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using Neo4Net.causalclustering.messaging.marshalling;

	public class ReplicatedContentDecoder : MessageToMessageDecoder<ContentBuilder<ReplicatedContent>>
	{
		 private readonly Protocol<ContentType> _protocol;
		 private ContentBuilder<ReplicatedContent> _contentBuilder = ContentBuilder.emptyUnfinished();

		 public ReplicatedContentDecoder( Protocol<ContentType> protocol )
		 {
			  this._protocol = protocol;
		 }

		 protected internal override void Decode( ChannelHandlerContext ctx, ContentBuilder<ReplicatedContent> msg, IList<object> @out )
		 {
			  _contentBuilder.combine( msg );
			  if ( _contentBuilder.Complete )
			  {
					@out.Add( _contentBuilder.build() );
					_contentBuilder = ContentBuilder.emptyUnfinished();
					_protocol.expect( ContentType.ContentType );
			  }
		 }
	}

}