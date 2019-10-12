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
namespace Neo4Net.causalclustering.messaging.marshalling
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using Test = org.junit.Test;


	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using ReplicatedInteger = Neo4Net.causalclustering.core.consensus.ReplicatedInteger;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using AppendEntriesRequestBuilder = Neo4Net.causalclustering.core.consensus.roles.AppendEntriesRequestBuilder;
	using AppendEntriesResponseBuilder = Neo4Net.causalclustering.core.consensus.roles.AppendEntriesResponseBuilder;
	using VoteRequestBuilder = Neo4Net.causalclustering.core.consensus.vote.VoteRequestBuilder;
	using VoteResponseBuilder = Neo4Net.causalclustering.core.consensus.vote.VoteResponseBuilder;
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using Neo4Net.causalclustering.core.state.storage;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using RaftMessageDecoder = Neo4Net.causalclustering.messaging.marshalling.v1.RaftMessageDecoder;
	using RaftMessageEncoder = Neo4Net.causalclustering.messaging.marshalling.v1.RaftMessageEncoder;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class RaftMessageEncodingDecodingTest
	{
		 private ClusterId _clusterId = new ClusterId( System.Guid.randomUUID() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAppendRequestWithMultipleEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAppendRequestWithMultipleEntries()
		 {
			  MemberId sender = new MemberId( System.Guid.randomUUID() );
			  Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request = ( new AppendEntriesRequestBuilder() ).from(sender).leaderCommit(2).leaderTerm(4).logEntry(new RaftLogEntry(1, ReplicatedInteger.valueOf(2))).logEntry(new RaftLogEntry(1, ReplicatedInteger.valueOf(3))).logEntry(new RaftLogEntry(1, ReplicatedInteger.valueOf(4))).build();
			  SerializeReadBackAndVerifyMessage( request );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAppendRequestWithNoEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAppendRequestWithNoEntries()
		 {
			  MemberId sender = new MemberId( System.Guid.randomUUID() );
			  Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request request = ( new AppendEntriesRequestBuilder() ).from(sender).leaderCommit(2).leaderTerm(4).build();
			  SerializeReadBackAndVerifyMessage( request );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAppendResponse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAppendResponse()
		 {
			  MemberId sender = new MemberId( System.Guid.randomUUID() );
			  Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response request = ( new AppendEntriesResponseBuilder() ).from(sender).success().matchIndex(12).build();
			  SerializeReadBackAndVerifyMessage( request );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeHeartbeats() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeHeartbeats()
		 {
			  // Given
			  Instant now = Instant.now();
			  Clock clock = Clock.@fixed( now, ZoneOffset.UTC );
			  RaftMessageEncoder encoder = new RaftMessageEncoder( marshal );
			  RaftMessageDecoder decoder = new RaftMessageDecoder( marshal, clock );

			  // Deserialization adds read objects in this list
			  List<object> thingsRead = new List<object>( 1 );

			  // When
			  MemberId sender = new MemberId( System.Guid.randomUUID() );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<?> message = org.neo4j.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage.of(now, clusterId, new org.neo4j.causalclustering.core.consensus.RaftMessages_Heartbeat(sender, 1, 2, 3));
			  Neo4Net.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<object> message = Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage.of( now, _clusterId, new Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat( sender, 1, 2, 3 ) );
			  ChannelHandlerContext ctx = SetupContext();
			  ByteBuf buffer = null;
			  try
			  {
					buffer = ctx.alloc().buffer();
					encoder.Encode( ctx, message, buffer );

					// When
					decoder.Decode( null, buffer, thingsRead );

					// Then
					assertEquals( 1, thingsRead.Count );
					assertEquals( message, thingsRead[0] );
			  }
			  finally
			  {
					if ( buffer != null )
					{
						 buffer.release();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeVoteRequest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeVoteRequest()
		 {
			  MemberId sender = new MemberId( System.Guid.randomUUID() );
			  Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Request request = ( new VoteRequestBuilder() ).candidate(sender).from(sender).lastLogIndex(2).lastLogTerm(1).term(3).build();
			  SerializeReadBackAndVerifyMessage( request );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeVoteResponse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeVoteResponse()
		 {
			  MemberId sender = new MemberId( System.Guid.randomUUID() );
			  Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response request = ( new VoteResponseBuilder() ).from(sender).grant().term(3).build();
			  SerializeReadBackAndVerifyMessage( request );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void serializeReadBackAndVerifyMessage(org.neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage message) throws Exception
		 private void SerializeReadBackAndVerifyMessage( Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage message )
		 {
			  // Given
			  Instant now = Instant.now();
			  Clock clock = Clock.@fixed( now, ZoneOffset.UTC );
			  RaftMessageEncoder encoder = new RaftMessageEncoder( marshal );
			  RaftMessageDecoder decoder = new RaftMessageDecoder( marshal, clock );

			  // Deserialization adds read objects in this list
			  List<object> thingsRead = new List<object>( 1 );

			  // When
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<?> decoratedMessage = org.neo4j.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage.of(now, clusterId, message);
			  Neo4Net.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<object> decoratedMessage = Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage.of( now, _clusterId, message );
			  ChannelHandlerContext ctx = SetupContext();
			  ByteBuf buffer = null;
			  try
			  {
					buffer = ctx.alloc().buffer();
					encoder.Encode( ctx, decoratedMessage, buffer );

					// When
					decoder.Decode( null, buffer, thingsRead );

					// Then
					assertEquals( 1, thingsRead.Count );
					assertEquals( decoratedMessage, thingsRead[0] );
			  }
			  finally
			  {
					if ( buffer != null )
					{
						 buffer.release();
					}
			  }
		 }

		 private static ChannelHandlerContext SetupContext()
		 {
			  ChannelHandlerContext context = mock( typeof( ChannelHandlerContext ) );
			  when( context.alloc() ).thenReturn(ByteBufAllocator.DEFAULT);
			  return context;
		 }

		 /*
		  * Serializer for ReplicatedIntegers. Differs form the one in RaftMessageProcessingTest in that it does not
		  * assume that there is only a single entry in the stream, which allows for asserting no remaining bytes once the
		  * first entry is read from the buffer.
		  */
		 private static readonly ChannelMarshal<ReplicatedContent> marshal = new SafeChannelMarshalAnonymousInnerClass();

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
				  return content;
			 }
		 }
	}

}