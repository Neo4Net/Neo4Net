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
namespace Neo4Net.causalclustering.messaging.marshalling.v2
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;
	using UnpooledByteBufAllocator = io.netty.buffer.UnpooledByteBufAllocator;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using SimpleChannelInboundHandler = io.netty.channel.SimpleChannelInboundHandler;
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using ChunkedInput = io.netty.handler.stream.ChunkedInput;
	using ReferenceCountUtil = io.netty.util.ReferenceCountUtil;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using RaftProtocolClientInstallerV1 = Neo4Net.causalclustering.core.consensus.protocol.v1.RaftProtocolClientInstallerV1;
	using RaftProtocolServerInstallerV1 = Neo4Net.causalclustering.core.consensus.protocol.v1.RaftProtocolServerInstallerV1;
	using RaftProtocolClientInstallerV2 = Neo4Net.causalclustering.core.consensus.protocol.v2.RaftProtocolClientInstallerV2;
	using RaftProtocolServerInstallerV2 = Neo4Net.causalclustering.core.consensus.protocol.v2.RaftProtocolServerInstallerV2;
	using DistributedOperation = Neo4Net.causalclustering.core.replication.DistributedOperation;
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using GlobalSession = Neo4Net.causalclustering.core.replication.session.GlobalSession;
	using LocalOperationId = Neo4Net.causalclustering.core.replication.session.LocalOperationId;
	using DummyRequest = Neo4Net.causalclustering.core.state.machines.dummy.DummyRequest;
	using ReplicatedLockTokenRequest = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenRequest;
	using ReplicatedTokenRequest = Neo4Net.causalclustering.core.state.machines.token.ReplicatedTokenRequest;
	using TokenType = Neo4Net.causalclustering.core.state.machines.token.TokenType;
	using ReplicatedTransaction = Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransaction;
	using VoidPipelineWrapperFactory = Neo4Net.causalclustering.handlers.VoidPipelineWrapperFactory;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using NettyPipelineBuilderFactory = Neo4Net.causalclustering.protocol.NettyPipelineBuilderFactory;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;

	/// <summary>
	/// Warning! This test ensures that all raft protocol work as expected in their current implementation. However, it does not know about changes to the
	/// protocols that breaks backward compatibility.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class RaftMessageEncoderDecoderTest
	public class RaftMessageEncoderDecoderTest
	{
		private bool InstanceFieldsInitialized = false;

		public RaftMessageEncoderDecoderTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_handler = new RaftMessageHandler( this );
		}

		 private static readonly MemberId _memberId = new MemberId( System.Guid.randomUUID() );
		 private static readonly int[] _protocols = new int[] { 1, 2 };
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public org.neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage raftMessage;
		 public Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage RaftMessage;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public int raftProtocol;
		 public int RaftProtocol;
		 private RaftMessageHandler _handler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "Raft v{1} with message {0}") public static Object[] data()
		 public static object[] Data()
		 {
			  return setUpParams(new Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage[]
			  {
				  new Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat( _memberId, 1, 2, 3 ),
				  new Neo4Net.causalclustering.core.consensus.RaftMessages_HeartbeatResponse( _memberId ),
				  new Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request( _memberId, new DummyRequest( new sbyte[]{ 1, 2, 3, 4, 5, 6, 7, 8 } ) ),
				  new Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request( _memberId, ReplicatedTransaction.from( new sbyte[]{ 1, 2, 3, 4, 5, 6, 7, 8 } ) ),
				  new Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request( _memberId, ReplicatedTransaction.from( new PhysicalTransactionRepresentation( Collections.emptyList() ) ) ),
				  new Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request( _memberId, new DistributedOperation( new DistributedOperation( ReplicatedTransaction.from( new sbyte[]{ 1, 2, 3, 4, 5 } ), new GlobalSession( System.Guid.randomUUID(), _memberId ), new LocalOperationId(1, 2) ), new GlobalSession(System.Guid.randomUUID(), _memberId), new LocalOperationId(3, 4) ) ),
				  new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request(_memberId, 1, 2, 3, new RaftLogEntry[]
				  {
					  new RaftLogEntry( 0, new ReplicatedTokenRequest( TokenType.LABEL, "name", new sbyte[]{ 2, 3, 4 } ) ),
					  new RaftLogEntry( 1, new ReplicatedLockTokenRequest( _memberId, 2 ) )
				  }, 5),
				  new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response( _memberId, 1, true, 2, 3 ),
				  new Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Request( _memberId, long.MaxValue, _memberId, long.MinValue, 1 ),
				  new Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Response( _memberId, 1, true ),
				  new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request( _memberId, long.MaxValue, _memberId, long.MinValue, 1 ),
				  new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response( _memberId, 1, true ),
				  new Neo4Net.causalclustering.core.consensus.RaftMessages_LogCompactionInfo( _memberId, long.MaxValue, long.MinValue )
			  });
		 }

		 private static object[] setUpParams( Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage[] messages )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return java.util.Arrays.stream(messages).flatMap((System.Func<org.neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage,java.util.stream.Stream<?>>) RaftMessageEncoderDecoderTest::params).toArray();
			  return Arrays.stream( messages ).flatMap( ( System.Func<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage, Stream<object>> ) RaftMessageEncoderDecoderTest.@params ).toArray();
		 }

		 private static Stream<object[]> Params( Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage raftMessage )
		 {
			  return Arrays.stream( _protocols ).mapToObj( p => new object[]{ raftMessage, p } );
		 }

		 private EmbeddedChannel _outbound;
		 private EmbeddedChannel _inbound;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupChannels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetupChannels()
		 {
			  _outbound = new EmbeddedChannel();
			  _inbound = new EmbeddedChannel();

			  if ( RaftProtocol == 2 )
			  {
					( new RaftProtocolClientInstallerV2( new NettyPipelineBuilderFactory( VoidPipelineWrapperFactory.VOID_WRAPPER ), Collections.emptyList(), FormattedLogProvider.toOutputStream(System.out) ) ).install(_outbound);
					( new RaftProtocolServerInstallerV2( _handler, new NettyPipelineBuilderFactory( VoidPipelineWrapperFactory.VOID_WRAPPER ), Collections.emptyList(), FormattedLogProvider.toOutputStream(System.out) ) ).install(_inbound);
			  }
			  else if ( RaftProtocol == 1 )
			  {
					( new RaftProtocolClientInstallerV1( new NettyPipelineBuilderFactory( VoidPipelineWrapperFactory.VOID_WRAPPER ), Collections.emptyList(), FormattedLogProvider.toOutputStream(System.out) ) ).install(_outbound);
					( new RaftProtocolServerInstallerV1( _handler, new NettyPipelineBuilderFactory( VoidPipelineWrapperFactory.VOID_WRAPPER ), Collections.emptyList(), FormattedLogProvider.toOutputStream(System.out) ) ).install(_inbound);
			  }
			  else
			  {
					throw new System.ArgumentException( "Unknown raft protocol " + RaftProtocol );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanUp()
		 public virtual void CleanUp()
		 {
			  if ( _outbound != null )
			  {
					_outbound.close();
			  }
			  if ( _inbound != null )
			  {
					_inbound.close();
			  }
			  _outbound = _inbound = null;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeDecodeRaftMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEncodeDecodeRaftMessage()
		 {
			  ClusterId clusterId = new ClusterId( System.Guid.randomUUID() );
			  Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> idAwareMessage = Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage.of( Instant.now(), clusterId, RaftMessage );

			  _outbound.writeOutbound( idAwareMessage );

			  object o;
			  while ( ( o = _outbound.readOutbound() ) != null )
			  {
					_inbound.writeInbound( o );
			  }
			  Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> message = _handler.RaftMessage;
			  assertEquals( clusterId, message.clusterId() );
			  RaftMessageEquals( RaftMessage, message.message() );
			  assertNull( _inbound.readInbound() );
			  ReferenceCountUtil.release( _handler.msg );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void raftMessageEquals(org.neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage raftMessage, org.neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage message) throws Exception
		 private void RaftMessageEquals( Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage raftMessage, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage message )
		 {
			  if ( raftMessage is Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request )
			  {
					assertEquals( message.From(), raftMessage.From() );
					assertEquals( message.Type(), raftMessage.Type() );
					ContentEquals( ( ( Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request ) raftMessage ).Content(), ((Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request) raftMessage).Content() );
			  }
			  else if ( raftMessage is Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request )
			  {
					assertEquals( message.From(), raftMessage.From() );
					assertEquals( message.Type(), raftMessage.Type() );
					RaftLogEntry[] entries1 = ( ( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request ) raftMessage ).Entries();
					RaftLogEntry[] entries2 = ( ( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request ) message ).Entries();
					for ( int i = 0; i < entries1.Length; i++ )
					{
						 RaftLogEntry raftLogEntry1 = entries1[i];
						 RaftLogEntry raftLogEntry2 = entries2[i];
						 assertEquals( raftLogEntry1.Term(), raftLogEntry2.Term() );
						 ContentEquals( raftLogEntry1.Content(), raftLogEntry2.Content() );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void contentEquals(org.neo4j.causalclustering.core.replication.ReplicatedContent one, org.neo4j.causalclustering.core.replication.ReplicatedContent two) throws Exception
		 private void ContentEquals( ReplicatedContent one, ReplicatedContent two )
		 {
			  if ( one is ReplicatedTransaction )
			  {
					ByteBuf buffer1 = Unpooled.buffer();
					ByteBuf buffer2 = Unpooled.buffer();
					Encode( buffer1, ( ( ReplicatedTransaction ) one ).encode() );
					Encode( buffer2, ( ( ReplicatedTransaction ) two ).encode() );
					assertEquals( buffer1, buffer2 );
			  }
			  else if ( one is DistributedOperation )
			  {
					assertEquals( ( ( DistributedOperation ) one ).globalSession(), ((DistributedOperation) two).globalSession() );
					assertEquals( ( ( DistributedOperation ) one ).operationId(), ((DistributedOperation) two).operationId() );
					ContentEquals( ( ( DistributedOperation ) one ).content(), ((DistributedOperation) two).content() );
			  }
			  else
			  {
					assertEquals( one, two );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void encode(io.netty.buffer.ByteBuf buffer, io.netty.handler.stream.ChunkedInput<io.netty.buffer.ByteBuf> marshal) throws Exception
		 private static void Encode( ByteBuf buffer, ChunkedInput<ByteBuf> marshal )
		 {
			  while ( !marshal.EndOfInput )
			  {
					ByteBuf tmp = marshal.readChunk( UnpooledByteBufAllocator.DEFAULT );
					if ( tmp != null )
					{
						 buffer.writeBytes( tmp );
						 tmp.release();
					}
			  }
		 }

		 internal class RaftMessageHandler : SimpleChannelInboundHandler<Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>>
		 {
			 private readonly RaftMessageEncoderDecoderTest _outerInstance;

			 public RaftMessageHandler( RaftMessageEncoderDecoderTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }


			  internal Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> Msg;

			  protected internal override void ChannelRead0( ChannelHandlerContext ctx, Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> msg )
			  {
					this.Msg = msg;
			  }

			  internal virtual Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> RaftMessage
			  {
				  get
				  {
						return Msg;
				  }
			  }
		 }
	}

}