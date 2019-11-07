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
namespace Neo4Net.causalclustering.messaging.marshalling.v2
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using NewLeaderBarrier = Neo4Net.causalclustering.core.consensus.NewLeaderBarrier;
	using MemberIdSet = Neo4Net.causalclustering.core.consensus.membership.MemberIdSet;
	using DistributedOperation = Neo4Net.causalclustering.core.replication.DistributedOperation;
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using GlobalSession = Neo4Net.causalclustering.core.replication.session.GlobalSession;
	using LocalOperationId = Neo4Net.causalclustering.core.replication.session.LocalOperationId;
	using DummyRequest = Neo4Net.causalclustering.core.state.machines.dummy.DummyRequest;
	using ReplicatedLockTokenRequest = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenRequest;
	using ReplicatedTokenRequest = Neo4Net.causalclustering.core.state.machines.token.ReplicatedTokenRequest;
	using TokenType = Neo4Net.causalclustering.core.state.machines.token.TokenType;
	using ReplicatedTransaction = Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransaction;
	using Buffers = Neo4Net.causalclustering.helpers.Buffers;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.messaging.marshalling;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class CoreReplicatedContentMarshallingTest
	public class CoreReplicatedContentMarshallingTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.causalclustering.helpers.Buffers buffers = new Neo4Net.causalclustering.helpers.Buffers();
		 public readonly Buffers Buffers = new Buffers();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public Neo4Net.causalclustering.core.replication.ReplicatedContent replicatedContent;
		 public ReplicatedContent ReplicatedContent;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Neo4Net.causalclustering.core.replication.ReplicatedContent[] data()
		 public static ReplicatedContent[] Data()
		 {
			  return new ReplicatedContent[]
			  {
				  new DummyRequest( new sbyte[]{ 1, 2, 3 } ),
				  ReplicatedTransaction.from( new sbyte[16 * 1024] ),
				  new MemberIdSet(new HashSet<MemberId>()
				  {
					  { add( new MemberId( System.Guid.randomUUID() ) ); }
				  }),
				  new ReplicatedTokenRequest( TokenType.LABEL, "token", new sbyte[]{ ( sbyte )'c', ( sbyte )'o', 5 } ),
				  new NewLeaderBarrier(),
				  new ReplicatedLockTokenRequest( new MemberId( System.Guid.randomUUID() ), 2 ),
				  new DistributedOperation( new DistributedOperation( ReplicatedTransaction.from( new sbyte[]{ 1, 2, 3, 4, 5, 6 } ), new GlobalSession( System.Guid.randomUUID(), new MemberId(System.Guid.randomUUID()) ), new LocalOperationId(1, 2) ), new GlobalSession(System.Guid.randomUUID(), new MemberId(System.Guid.randomUUID())), new LocalOperationId(4, 5) )
			  };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserialize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAndDeserialize()
		 {
			  ChannelMarshal<ReplicatedContent> coreReplicatedContentMarshal = CoreReplicatedContentMarshal.marshaller();
			  ByteBuf buffer = Buffers.buffer();
			  BoundedNetworkWritableChannel channel = new BoundedNetworkWritableChannel( buffer );
			  coreReplicatedContentMarshal.Marshal( ReplicatedContent, channel );

			  NetworkReadableClosableChannelNetty4 readChannel = new NetworkReadableClosableChannelNetty4( buffer );
			  ReplicatedContent result = coreReplicatedContentMarshal.Unmarshal( readChannel );

			  assertEquals( ReplicatedContent, result );
		 }
	}

}