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
namespace Neo4Net.Bolt.v1
{
	using Channel = io.netty.channel.Channel;
	using ChannelHandler = io.netty.channel.ChannelHandler;
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using After = org.junit.After;
	using Test = org.junit.Test;


	using BoltConnection = Neo4Net.Bolt.runtime.BoltConnection;
	using BoltConnectionFactory = Neo4Net.Bolt.runtime.BoltConnectionFactory;
	using BoltStateMachineFactory = Neo4Net.Bolt.runtime.BoltStateMachineFactory;
	using ChunkDecoder = Neo4Net.Bolt.transport.pipeline.ChunkDecoder;
	using HouseKeeper = Neo4Net.Bolt.transport.pipeline.HouseKeeper;
	using MessageAccumulator = Neo4Net.Bolt.transport.pipeline.MessageAccumulator;
	using MessageDecoder = Neo4Net.Bolt.transport.pipeline.MessageDecoder;
	using NullLogService = Neo4Net.Logging.@internal.NullLogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class BoltProtocolV1Test
	{
		 private readonly EmbeddedChannel _channel = new EmbeddedChannel();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  _channel.finishAndReleaseAll();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInstallChannelHandlersInCorrectOrder() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInstallChannelHandlersInCorrectOrder()
		 {
			  // Given
			  BoltChannel boltChannel = NewBoltChannel( _channel );
			  BoltConnectionFactory connectionFactory = mock( typeof( BoltConnectionFactory ) );
			  when( connectionFactory.NewConnection( eq( boltChannel ), any() ) ).thenReturn(mock(typeof(BoltConnection)));
			  BoltProtocol boltProtocol = new BoltProtocolV1( boltChannel, connectionFactory, mock( typeof( BoltStateMachineFactory ) ), NullLogService.Instance );

			  // When
			  boltProtocol.Install();

			  IEnumerator<KeyValuePair<string, ChannelHandler>> handlers = _channel.pipeline().GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( handlers.next().Value, instanceOf(typeof(ChunkDecoder)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( handlers.next().Value, instanceOf(typeof(MessageAccumulator)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( handlers.next().Value, instanceOf(typeof(MessageDecoder)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( handlers.next().Value, instanceOf(typeof(HouseKeeper)) );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( handlers.hasNext() );
		 }

		 private static BoltChannel NewBoltChannel( Channel rawChannel )
		 {
			  return new BoltChannel( "bolt-1", "bolt", rawChannel );
		 }
	}

}