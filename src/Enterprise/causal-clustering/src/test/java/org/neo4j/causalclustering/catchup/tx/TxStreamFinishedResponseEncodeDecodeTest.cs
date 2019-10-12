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
namespace Neo4Net.causalclustering.catchup.tx
{
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.SUCCESS_END_OF_STREAM;

	public class TxStreamFinishedResponseEncodeDecodeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeAndDecodePullRequestMessage()
		 public virtual void ShouldEncodeAndDecodePullRequestMessage()
		 {
			  // given
			  EmbeddedChannel channel = new EmbeddedChannel( new TxStreamFinishedResponseEncoder(), new TxStreamFinishedResponseDecoder() );
			  TxStreamFinishedResponse sent = new TxStreamFinishedResponse( SUCCESS_END_OF_STREAM, 1000 );

			  // when
			  channel.writeOutbound( sent );
			  object message = channel.readOutbound();
			  channel.writeInbound( message );

			  // then
			  TxStreamFinishedResponse received = channel.readInbound();
			  assertNotSame( sent, received );
			  assertEquals( sent, received );
		 }

	}

}