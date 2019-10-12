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
namespace Org.Neo4j.causalclustering.core.replication
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;
	using Test = org.junit.Test;


	using MemberIdSet = Org.Neo4j.causalclustering.core.consensus.membership.MemberIdSet;
	using ReplicatedIdAllocationRequest = Org.Neo4j.causalclustering.core.state.machines.id.ReplicatedIdAllocationRequest;
	using ReplicatedTokenRequest = Org.Neo4j.causalclustering.core.state.machines.token.ReplicatedTokenRequest;
	using ReplicatedTokenRequestSerializer = Org.Neo4j.causalclustering.core.state.machines.token.ReplicatedTokenRequestSerializer;
	using TokenType = Org.Neo4j.causalclustering.core.state.machines.token.TokenType;
	using ReplicatedTransaction = Org.Neo4j.causalclustering.core.state.machines.tx.ReplicatedTransaction;
	using ReplicatedTransactionFactory = Org.Neo4j.causalclustering.core.state.machines.tx.ReplicatedTransactionFactory;
	using TransactionRepresentationReplicatedTransaction = Org.Neo4j.causalclustering.core.state.machines.tx.TransactionRepresentationReplicatedTransaction;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using EndOfStreamException = Org.Neo4j.causalclustering.messaging.EndOfStreamException;
	using NetworkWritableChannel = Org.Neo4j.causalclustering.messaging.NetworkWritableChannel;
	using NetworkReadableClosableChannelNetty4 = Org.Neo4j.causalclustering.messaging.NetworkReadableClosableChannelNetty4;
	using Org.Neo4j.causalclustering.messaging.marshalling;
	using CoreReplicatedContentMarshal = Org.Neo4j.causalclustering.messaging.marshalling.CoreReplicatedContentMarshal;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using TransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.TransactionRepresentation;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class CoreReplicatedContentMarshalTest
	{
		 private readonly ChannelMarshal<ReplicatedContent> _marshal = CoreReplicatedContentMarshal.marshaller();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMarshalTransactionReference() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMarshalTransactionReference()
		 {
			  ByteBuf buffer = Unpooled.buffer();
			  PhysicalTransactionRepresentation representation = new PhysicalTransactionRepresentation( Collections.emptyList() );
			  representation.SetHeader( new sbyte[]{ 0 }, 1, 1, 1, 1, 1, 1 );

			  TransactionRepresentationReplicatedTransaction replicatedTx = ReplicatedTransaction.from( representation );

			  AssertMarshalingEquality( buffer, replicatedTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMarshalTransactionReferenceWithMissingHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMarshalTransactionReferenceWithMissingHeader()
		 {
			  ByteBuf buffer = Unpooled.buffer();
			  PhysicalTransactionRepresentation representation = new PhysicalTransactionRepresentation( Collections.emptyList() );

			  TransactionRepresentationReplicatedTransaction replicatedTx = ReplicatedTransaction.from( representation );

			  AssertMarshalingEquality( buffer, replicatedTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMarshalMemberSet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMarshalMemberSet()
		 {
			  ByteBuf buffer = Unpooled.buffer();
			  ReplicatedContent message = new MemberIdSet(asSet(new MemberId(System.Guid.randomUUID()), new MemberId(System.Guid.randomUUID())
			 ));

			  AssertMarshalingEquality( buffer, message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMarshalIdRangeRequest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMarshalIdRangeRequest()
		 {
			  ByteBuf buffer = Unpooled.buffer();
			  ReplicatedContent message = new ReplicatedIdAllocationRequest( new MemberId( System.Guid.randomUUID() ), IdType.PROPERTY, 100, 200 );

			  AssertMarshalingEquality( buffer, message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMarshalTokenRequest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMarshalTokenRequest()
		 {
			  ByteBuf buffer = Unpooled.buffer();

			  List<StorageCommand> commands = new List<StorageCommand>();
			  LabelTokenRecord before = new LabelTokenRecord( 0 );
			  LabelTokenRecord after = new LabelTokenRecord( 0 );
			  after.InUse = true;
			  after.SetCreated();
			  after.NameId = 3232;
			  commands.Add( new Command.LabelTokenCommand( before, after ) );
			  ReplicatedContent message = new ReplicatedTokenRequest( TokenType.LABEL, "theLabel", ReplicatedTokenRequestSerializer.commandBytes( commands ) );

			  AssertMarshalingEquality( buffer, message );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertMarshalingEquality(io.netty.buffer.ByteBuf buffer, ReplicatedContent replicatedTx) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
		 private void AssertMarshalingEquality( ByteBuf buffer, ReplicatedContent replicatedTx )
		 {
			  _marshal.marshal( replicatedTx, new NetworkWritableChannel( buffer ) );

			  assertThat( _marshal.unmarshal( new NetworkReadableClosableChannelNetty4( buffer ) ), equalTo( replicatedTx ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertMarshalingEquality(io.netty.buffer.ByteBuf buffer, org.neo4j.causalclustering.core.state.machines.tx.TransactionRepresentationReplicatedTransaction replicatedTx) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
		 private void AssertMarshalingEquality( ByteBuf buffer, TransactionRepresentationReplicatedTransaction replicatedTx )
		 {
			  _marshal.marshal( replicatedTx, new NetworkWritableChannel( buffer ) );

			  ReplicatedContent unmarshal = _marshal.unmarshal( new NetworkReadableClosableChannelNetty4( buffer ) );

			  TransactionRepresentation tx = replicatedTx.Tx();
			  sbyte[] extraHeader = tx.AdditionalHeader();
			  if ( extraHeader == null )
			  {
					// hackishly set additional header to empty array...
					( ( PhysicalTransactionRepresentation ) tx ).setHeader( new sbyte[0], tx.MasterId, tx.AuthorId, tx.TimeStarted, tx.LatestCommittedTxWhenStarted, tx.TimeCommitted, tx.LockSessionId );
					extraHeader = tx.AdditionalHeader();
			  }
			  TransactionRepresentation representation = ReplicatedTransactionFactory.extractTransactionRepresentation( ( ReplicatedTransaction ) unmarshal, extraHeader );
			  assertThat( representation, equalTo( tx ) );
		 }
	}

}