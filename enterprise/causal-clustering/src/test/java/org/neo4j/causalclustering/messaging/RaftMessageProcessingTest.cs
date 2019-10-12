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
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;


	using RaftMessages = Org.Neo4j.causalclustering.core.consensus.RaftMessages;
	using ReplicatedInteger = Org.Neo4j.causalclustering.core.consensus.ReplicatedInteger;
	using RaftLogEntry = Org.Neo4j.causalclustering.core.consensus.log.RaftLogEntry;
	using ReplicatedContent = Org.Neo4j.causalclustering.core.replication.ReplicatedContent;
	using Org.Neo4j.causalclustering.core.state.storage;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Org.Neo4j.causalclustering.messaging.marshalling;
	using RaftMessageDecoder = Org.Neo4j.causalclustering.messaging.marshalling.v1.RaftMessageDecoder;
	using RaftMessageEncoder = Org.Neo4j.causalclustering.messaging.marshalling.v1.RaftMessageEncoder;
	using ReadPastEndException = Org.Neo4j.Storageengine.Api.ReadPastEndException;
	using ReadableChannel = Org.Neo4j.Storageengine.Api.ReadableChannel;
	using WritableChannel = Org.Neo4j.Storageengine.Api.WritableChannel;

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
//ORIGINAL LINE: public void marshal(org.neo4j.causalclustering.core.replication.ReplicatedContent content, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
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
//ORIGINAL LINE: public org.neo4j.causalclustering.core.replication.ReplicatedContent unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
			 public override ReplicatedContent unmarshal0( ReadableChannel channel )
			 {
				  sbyte type = channel.Get();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.core.replication.ReplicatedContent content;
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
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Request request = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Request( member, 1, member, 1, 1 );

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
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Response response = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Response( member, 1, true );

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
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( member, 1, 1, 99, new RaftLogEntry[] { logEntry }, 1 );

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
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response response = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response( member, 1, false, -1, 0 );

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
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_NewEntry_Request request = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_NewEntry_Request( member, ReplicatedInteger.valueOf( 12 ) );

			  // when
			  _channel.writeOutbound( request );
			  object message = _channel.readOutbound();
			  _channel.writeInbound( message );

			  // then
			  assertEquals( request, _channel.readInbound() );
		 }
	}

}