﻿/*
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
namespace Org.Neo4j.causalclustering.core.consensus.membership
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;
	using Test = org.junit.Test;

	using BoundedNetworkWritableChannel = Org.Neo4j.causalclustering.messaging.BoundedNetworkWritableChannel;
	using NetworkReadableClosableChannelNetty4 = Org.Neo4j.causalclustering.messaging.NetworkReadableClosableChannelNetty4;
	using EndOfStreamException = Org.Neo4j.causalclustering.messaging.EndOfStreamException;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class MemberIdMarshalTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserialize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAndDeserialize()
		 {
			  // given
			  MemberId.Marshal marshal = new MemberId.Marshal();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.identity.MemberId member = new org.neo4j.causalclustering.identity.MemberId(java.util.UUID.randomUUID());
			  MemberId member = new MemberId( System.Guid.randomUUID() );

			  // when
			  ByteBuf buffer = Unpooled.buffer( 1_000 );
			  marshal.MarshalConflict( member, new BoundedNetworkWritableChannel( buffer ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.identity.MemberId recovered = marshal.unmarshal(new org.neo4j.causalclustering.messaging.NetworkReadableClosableChannelNetty4(buffer));
			  MemberId recovered = marshal.Unmarshal( new NetworkReadableClosableChannelNetty4( buffer ) );

			  // then
			  assertEquals( member, recovered );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionForHalfWrittenInstance() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionForHalfWrittenInstance()
		 {
			  // given
			  // a CoreMember and a ByteBuffer to write it to
			  MemberId.Marshal marshal = new MemberId.Marshal();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.identity.MemberId aRealMember = new org.neo4j.causalclustering.identity.MemberId(java.util.UUID.randomUUID());
			  MemberId aRealMember = new MemberId( System.Guid.randomUUID() );

			  ByteBuf buffer = Unpooled.buffer( 1000 );

			  // and the CoreMember is serialized but for 5 bytes at the end
			  marshal.MarshalConflict( aRealMember, new BoundedNetworkWritableChannel( buffer ) );
			  ByteBuf bufferWithMissingBytes = buffer.copy( 0, buffer.writerIndex() - 5 );

			  // when
			  try
			  {
					marshal.Unmarshal( new NetworkReadableClosableChannelNetty4( bufferWithMissingBytes ) );
					fail( "Should have thrown exception" );
			  }
			  catch ( EndOfStreamException )
			  {
					// expected
			  }
		 }
	}

}