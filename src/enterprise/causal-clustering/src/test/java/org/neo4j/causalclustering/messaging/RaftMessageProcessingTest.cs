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
namespace Neo4Net.causalclustering.messaging
{
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;


	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using ReplicatedInteger = Neo4Net.causalclustering.core.consensus.ReplicatedInteger;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using Neo4Net.causalclustering.core.state.storage;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.messaging.marshalling;
	using RaftMessageDecoder = Neo4Net.causalclustering.messaging.marshalling.v1.RaftMessageDecoder;
	using RaftMessageEncoder = Neo4Net.causalclustering.messaging.marshalling.v1.RaftMessageEncoder;
	using ReadPastEndException = Neo4Net.Storageengine.Api.ReadPastEndException;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class RaftMessageProcessingTest
	public class RaftMessageProcessingTest
	{
		 private static ChannelMarshal<ReplicatedContent> serializer = new SafeChannelMarshalAnonymousInnerClass();

		 private class SafeChannelMarshalAnonymousInnerClass : SafeChannelMarshal<ReplicatedContent>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(org.Neo4Net.causalclustering.core.replication.ReplicatedContent content, org.Neo4Net.storageengine.api.WritableChannel channel) throws java.io.IOException
			 public override void marshal( ReplicatedContent content, WritableChannel channel )
			 {
				  if ( content is ReplicatedInteger )
				  {
						channel.Put( ( sbyte ) 1 );
						channel.PutInt( ( ( ReplicatedInteger ) content ).get() );
				  }
				  else
				  {
						throw new System.ArgumentException( "Unknown content type " + content.GetType() );
				  }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.causalclustering.core.replication.ReplicatedContent unmarshal0(org.Neo4Net.storageengine.api.ReadableChannel channel) throws java.io.IOException
			 public override ReplicatedContent unmarshal0( ReadableChannel channel )
			 {
				  sbyte type = channel.Get();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.core.replication.ReplicatedContent content;
				  ReplicatedContent content;
				  switch ( type )
				  {
						case 1:
							 content = ReplicatedInteger.valueOf( channel.Int );
							 break;
						default:
							 throw new System.ArgumentException( string.Format( "Unknown content type 0x{0:x}", type ) );
				  }

				  try
				  {
						channel.Get();
						throw new System.ArgumentException( "Bytes remain in buffer after deserialization" );
				  }
				  catch ( ReadPastEndException )
				  {
						// expected
				  }
				  return content;
			 }
		 }

		 private EmbeddedChannel _channel;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _channel = new EmbeddedChannel( new RaftMessageEncoder( serializer ), new RaftMessageDecoder( serializer, Clock.systemUTC() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeAndDecodeVoteRequest()
		 public virtual void ShouldEncodeAndDecodeVoteRequest()
		 {
			  // given
			  MemberId member = new MemberId( System.Guid.randomUUID() );
			  Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Request request = new Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Request( member, 1, member, 1, 1 );

			  // when
			  _channel.writeOutbound( request );
			  object message = _channel.readOutbound();
			  _channel.writeInbound( message );

			  // then
			  assertEquals( request, _channel.readInbound() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeAndDecodeVoteResponse()
		 public virtual void ShouldEncodeAndDecodeVoteResponse()
		 {
			  // given
			  MemberId member = new MemberId( System.Guid.randomUUID() );
			  Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response response = new Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response( member, 1, true );

			  // when
			  _channel.writeOutbound( response );
			  object message = _channel.readOutbound();
			  _channel.writeInbound( message );

			  // then
			  assertEquals( response, _channel.readInbound() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeAndDecodeAppendEntriesRequest()
		 public virtual void ShouldEncodeAndDecodeAppendEntriesRequest()
		 {
			  // given
			  MemberId member = new MemberId( System.Guid.randomUUID() );
			  RaftLogEntry logEntry = new RaftLogEntry( 1, ReplicatedInteger.valueOf( 1 ) );
			  Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request = new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( member, 1, 1, 99, new RaftLogEntry[] { logEntry }, 1 );

			  // when
			  _channel.writeOutbound( request );
			  object message = _channel.readOutbound();
			  _channel.writeInbound( message );

			  // then
			  assertEquals( request, _channel.readInbound() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeAndDecodeAppendEntriesResponse()
		 public virtual void ShouldEncodeAndDecodeAppendEntriesResponse()
		 {
			  // given
			  MemberId member = new MemberId( System.Guid.randomUUID() );
			  Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response response = new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response( member, 1, false, -1, 0 );

			  // when
			  _channel.writeOutbound( response );
			  object message = _channel.readOutbound();
			  _channel.writeInbound( message );

			  // then
			  assertEquals( response, _channel.readInbound() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeAndDecodeNewEntryRequest()
		 public virtual void ShouldEncodeAndDecodeNewEntryRequest()
		 {
			  // given
			  MemberId member = new MemberId( System.Guid.randomUUID() );
			  Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request request = new Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request( member, ReplicatedInteger.valueOf( 12 ) );

			  // when
			  _channel.writeOutbound( request );
			  object message = _channel.readOutbound();
			  _channel.writeInbound( message );

			  // then
			  assertEquals( request, _channel.readInbound() );
		 }
	}

}