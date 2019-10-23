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
namespace Neo4Net.causalclustering.core.replication
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;
	using Test = org.junit.Test;


	using MemberIdSet = Neo4Net.causalclustering.core.consensus.membership.MemberIdSet;
	using ReplicatedIdAllocationRequest = Neo4Net.causalclustering.core.state.machines.id.ReplicatedIdAllocationRequest;
	using ReplicatedTokenRequest = Neo4Net.causalclustering.core.state.machines.token.ReplicatedTokenRequest;
	using ReplicatedTokenRequestSerializer = Neo4Net.causalclustering.core.state.machines.token.ReplicatedTokenRequestSerializer;
	using TokenType = Neo4Net.causalclustering.core.state.machines.token.TokenType;
	using ReplicatedTransaction = Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransaction;
	using ReplicatedTransactionFactory = Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransactionFactory;
	using TransactionRepresentationReplicatedTransaction = Neo4Net.causalclustering.core.state.machines.tx.TransactionRepresentationReplicatedTransaction;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using NetworkWritableChannel = Neo4Net.causalclustering.messaging.NetworkWritableChannel;
	using NetworkReadableClosableChannelNetty4 = Neo4Net.causalclustering.messaging.NetworkReadableClosableChannelNetty4;
	using Neo4Net.causalclustering.messaging.marshalling;
	using CoreReplicatedContentMarshal = Neo4Net.causalclustering.messaging.marshalling.CoreReplicatedContentMarshal;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;

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
//ORIGINAL LINE: private void assertMarshalingEquality(io.netty.buffer.ByteBuf buffer, ReplicatedContent replicatedTx) throws java.io.IOException, org.Neo4Net.causalclustering.messaging.EndOfStreamException
		 private void AssertMarshalingEquality( ByteBuf buffer, ReplicatedContent replicatedTx )
		 {
			  _marshal.marshal( replicatedTx, new NetworkWritableChannel( buffer ) );

			  assertThat( _marshal.unmarshal( new NetworkReadableClosableChannelNetty4( buffer ) ), equalTo( replicatedTx ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertMarshalingEquality(io.netty.buffer.ByteBuf buffer, org.Neo4Net.causalclustering.core.state.machines.tx.TransactionRepresentationReplicatedTransaction replicatedTx) throws java.io.IOException, org.Neo4Net.causalclustering.messaging.EndOfStreamException
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