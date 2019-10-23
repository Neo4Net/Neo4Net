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

	using ReplicatedContentHandler = Neo4Net.causalclustering.messaging.marshalling.ReplicatedContentHandler;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

	public class TransactionRepresentationReplicatedTransaction : ReplicatedTransaction
	{
		 private readonly TransactionRepresentation _tx;

		 internal TransactionRepresentationReplicatedTransaction( TransactionRepresentation tx )
		 {
			  this._tx = tx;
		 }

		 public override ChunkedInput<ByteBuf> Encode()
		 {
			  return ReplicatedTransactionSerializer.Encode( this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel writableChannel) throws java.io.IOException
		 public override void Marshal( WritableChannel writableChannel )
		 {
			  ReplicatedTransactionSerializer.Marshal( writableChannel, this );
		 }

		 public override TransactionRepresentation Extract( TransactionRepresentationExtractor extractor )
		 {
			  return extractor.Extract( this );
		 }

		 public virtual TransactionRepresentation Tx()
		 {
			  return _tx;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(org.Neo4Net.causalclustering.messaging.marshalling.ReplicatedContentHandler contentHandler) throws java.io.IOException
		 public override void Handle( ReplicatedContentHandler contentHandler )
		 {
			  contentHandler.Handle( this );
		 }
	}

}