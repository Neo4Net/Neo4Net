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
namespace Neo4Net.causalclustering.core.state.machines.tx
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChunkedInput = io.netty.handler.stream.ChunkedInput;


	using ByteBufBacked = Neo4Net.causalclustering.messaging.ByteBufBacked;
	using ByteArrayChunkedEncoder = Neo4Net.causalclustering.messaging.marshalling.ByteArrayChunkedEncoder;
	using OutputStreamWritableChannel = Neo4Net.causalclustering.messaging.marshalling.OutputStreamWritableChannel;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransactionFactory.transactionalRepresentationWriter;

	public class ReplicatedTransactionSerializer
	{
		 private ReplicatedTransactionSerializer()
		 {
		 }

		 public static ReplicatedTransaction Decode( ByteBuf byteBuf )
		 {
			  int length = byteBuf.readableBytes();
			  sbyte[] bytes = new sbyte[length];
			  byteBuf.readBytes( bytes );
			  return ReplicatedTransaction.from( bytes );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static ReplicatedTransaction unmarshal(org.Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException
		 public static ReplicatedTransaction Unmarshal( ReadableChannel channel )
		 {
			  int txBytesLength = channel.Int;
			  sbyte[] txBytes = new sbyte[txBytesLength];
			  channel.Get( txBytes, txBytesLength );
			  return ReplicatedTransaction.from( txBytes );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void marshal(org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel writableChannel, ByteArrayReplicatedTransaction replicatedTransaction) throws java.io.IOException
		 public static void Marshal( WritableChannel writableChannel, ByteArrayReplicatedTransaction replicatedTransaction )
		 {
			  int length = replicatedTransaction.TxBytes.Length;
			  writableChannel.PutInt( length );
			  writableChannel.Put( replicatedTransaction.TxBytes, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void marshal(org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel writableChannel, TransactionRepresentationReplicatedTransaction replicatedTransaction) throws java.io.IOException
		 public static void Marshal( WritableChannel writableChannel, TransactionRepresentationReplicatedTransaction replicatedTransaction )
		 {
			  if ( writableChannel is ByteBufBacked )
			  {
					/*
					 * Marshals more efficiently if Channel is going over the network. In practice, this means maintaining support for
					 * RaftV1 without loosing performance
					 */
					ByteBuf buffer = ( ( ByteBufBacked ) writableChannel ).byteBuf();
					int metaDataIndex = buffer.writerIndex();
					int txStartIndex = metaDataIndex + Integer.BYTES;
					// leave room for length to be set later.
					buffer.writerIndex( txStartIndex );
					WriteTx( writableChannel, replicatedTransaction.Tx() );
					int txLength = buffer.writerIndex() - txStartIndex;
					buffer.setInt( metaDataIndex, txLength );
			  }
			  else
			  {
					/*
					 * Unknown length. This should only be reached in tests. When a ReplicatedTransaction is marshaled to file it has already passed over the network
					 * and is of a different type. More efficient marshalling is used in ByteArrayReplicatedTransaction.
					 */
					MemoryStream outputStream = new MemoryStream( 1024 );
					OutputStreamWritableChannel outputStreamWritableChannel = new OutputStreamWritableChannel( outputStream );
					WriteTx( outputStreamWritableChannel, replicatedTransaction.Tx() );
					int length = outputStream.size();
					writableChannel.PutInt( length );
					writableChannel.Put( outputStream.toByteArray(), length );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void writeTx(org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel writableChannel, org.Neo4Net.kernel.impl.transaction.TransactionRepresentation tx) throws java.io.IOException
		 private static void WriteTx( WritableChannel writableChannel, TransactionRepresentation tx )
		 {
			  ReplicatedTransactionFactory.TransactionRepresentationWriter txWriter = transactionalRepresentationWriter( tx );
			  while ( txWriter.CanWrite() )
			  {
					txWriter.Write( writableChannel );
			  }
		 }

		 public static ChunkedInput<ByteBuf> Encode( TransactionRepresentationReplicatedTransaction representationReplicatedTransaction )
		 {
			  return new ChunkedTransaction( representationReplicatedTransaction.Tx() );
		 }

		 public static ChunkedInput<ByteBuf> Encode( ByteArrayReplicatedTransaction byteArrayReplicatedTransaction )
		 {
			  return new ByteArrayChunkedEncoder( byteArrayReplicatedTransaction.TxBytes );
		 }
	}

}