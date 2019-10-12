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
namespace Org.Neo4j.causalclustering.messaging
{
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using Test = org.junit.Test;

	using NullLog = Org.Neo4j.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class SimpleNettyChannelTest
	{
		 private EmbeddedChannel _nettyChannel = new EmbeddedChannel();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteOnNettyChannel()
		 public virtual void ShouldWriteOnNettyChannel()
		 {
			  // given
			  SimpleNettyChannel channel = new SimpleNettyChannel( _nettyChannel, NullLog.Instance );

			  // when
			  object msg = new object();
			  Future<Void> writeComplete = channel.Write( msg );

			  // then
			  assertNull( _nettyChannel.readOutbound() );
			  assertFalse( writeComplete.Done );

			  // when
			  _nettyChannel.flush();

			  // then
			  assertTrue( writeComplete.Done );
			  assertEquals( msg, _nettyChannel.readOutbound() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteAndFlushOnNettyChannel()
		 public virtual void ShouldWriteAndFlushOnNettyChannel()
		 {
			  // given
			  SimpleNettyChannel channel = new SimpleNettyChannel( _nettyChannel, NullLog.Instance );

			  // when
			  object msg = new object();
			  Future<Void> writeComplete = channel.WriteAndFlush( msg );

			  // then
			  assertTrue( writeComplete.Done );
			  assertEquals( msg, _nettyChannel.readOutbound() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldThrowWhenWritingOnDisposedChannel()
		 public virtual void ShouldThrowWhenWritingOnDisposedChannel()
		 {
			  // given
			  SimpleNettyChannel channel = new SimpleNettyChannel( _nettyChannel, NullLog.Instance );
			  channel.Dispose();

			  // when
			  channel.Write( new object() );

			  // then expected to throw
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldThrowWhenWriteAndFlushingOnDisposedChannel()
		 public virtual void ShouldThrowWhenWriteAndFlushingOnDisposedChannel()
		 {
			  // given
			  SimpleNettyChannel channel = new SimpleNettyChannel( _nettyChannel, NullLog.Instance );
			  channel.Dispose();

			  // when
			  channel.WriteAndFlush( new object() );

			  // then expected to throw
		 }
	}

}