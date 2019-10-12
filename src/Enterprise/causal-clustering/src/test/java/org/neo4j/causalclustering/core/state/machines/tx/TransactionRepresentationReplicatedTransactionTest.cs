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
namespace Neo4Net.causalclustering.core.state.machines.tx
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Assertions = org.junit.jupiter.api.Assertions;


	using Buffers = Neo4Net.causalclustering.helpers.Buffers;
	using BoundedNetworkWritableChannel = Neo4Net.causalclustering.messaging.BoundedNetworkWritableChannel;
	using NetworkReadableClosableChannelNetty4 = Neo4Net.causalclustering.messaging.NetworkReadableClosableChannelNetty4;
	using NetworkWritableChannel = Neo4Net.causalclustering.messaging.NetworkWritableChannel;
	using OutputStreamWritableChannel = Neo4Net.causalclustering.messaging.marshalling.OutputStreamWritableChannel;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class TransactionRepresentationReplicatedTransactionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.causalclustering.helpers.Buffers buffers = new org.neo4j.causalclustering.helpers.Buffers();
		 public readonly Buffers Buffers = new Buffers();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMarshalToSameByteIfByteBufBackedOrNot() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMarshalToSameByteIfByteBufBackedOrNot()
		 {
			  PhysicalTransactionRepresentation expectedTx = new PhysicalTransactionRepresentation( Collections.singleton( new Command.NodeCommand( new NodeRecord( 1 ), new NodeRecord( 2 ) ) ) );

			  expectedTx.SetHeader( new sbyte[0], 1, 2, 3, 4, 5, 6 );
			  TransactionRepresentationReplicatedTransaction replicatedTransaction = ReplicatedTransaction.from( expectedTx );

			  MemoryStream stream = new MemoryStream();
			  ByteBuf buffer = Buffers.buffer();
			  OutputStreamWritableChannel outputStreamWritableChannel = new OutputStreamWritableChannel( stream );
			  NetworkWritableChannel networkWritableChannel = new NetworkWritableChannel( buffer );

			  replicatedTransaction.Marshal( outputStreamWritableChannel );
			  replicatedTransaction.Marshal( networkWritableChannel );

			  sbyte[] bufferArray = Arrays.copyOf( buffer.array(), buffer.writerIndex() );

			  Assertions.assertArrayEquals( bufferArray, stream.toByteArray() );
		 }
	}

}