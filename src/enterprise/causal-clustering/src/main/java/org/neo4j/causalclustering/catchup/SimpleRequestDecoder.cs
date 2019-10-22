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
namespace Neo4Net.causalclustering.catchup
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using MessageToMessageDecoder = io.netty.handler.codec.MessageToMessageDecoder;

	using Message = Neo4Net.causalclustering.messaging.Message;
	using Neo4Net.Functions;

	/// <summary>
	/// This class extends <seealso cref="MessageToMessageDecoder"/> because if it extended
	/// <seealso cref="io.netty.handler.codec.ByteToMessageDecoder"/> instead the decode method would fail as no
	/// bytes are consumed from the ByteBuf but an object is added in the out list.
	/// </summary>
	public class SimpleRequestDecoder : MessageToMessageDecoder<ByteBuf>
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.function.Factory<? extends org.Neo4Net.causalclustering.messaging.Message> factory;
		 private IFactory<Message> _factory;

		 public SimpleRequestDecoder<T1>( IFactory<T1> factory ) where T1 : Neo4Net.causalclustering.messaging.Message
		 {
			  this._factory = factory;
		 }

		 protected internal override void Decode( ChannelHandlerContext ctx, ByteBuf msg, IList<object> @out )
		 {
			  @out.Add( _factory.newInstance() );
		 }
	}

}