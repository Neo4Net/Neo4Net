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
namespace Org.Neo4j.causalclustering.core.state.machines.tx
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChunkedInput = io.netty.handler.stream.ChunkedInput;


	using ReplicatedContentHandler = Org.Neo4j.causalclustering.messaging.marshalling.ReplicatedContentHandler;
	using TransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.TransactionRepresentation;
	using WritableChannel = Org.Neo4j.Storageengine.Api.WritableChannel;

	public class ByteArrayReplicatedTransaction : ReplicatedTransaction
	{
		 private readonly sbyte[] _txBytes;

		 public override long? Size()
		 {
			  return long?.of( ( long ) _txBytes.Length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(org.neo4j.causalclustering.messaging.marshalling.ReplicatedContentHandler contentHandler) throws java.io.IOException
		 public override void Handle( ReplicatedContentHandler contentHandler )
		 {
			  contentHandler.Handle( this );
		 }

		 internal ByteArrayReplicatedTransaction( sbyte[] txBytes )
		 {
			  this._txBytes = txBytes;
		 }

		 internal virtual sbyte[] TxBytes
		 {
			 get
			 {
				  return _txBytes;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  ByteArrayReplicatedTransaction that = ( ByteArrayReplicatedTransaction ) o;
			  return Arrays.Equals( _txBytes, that._txBytes );
		 }

		 public override int GetHashCode()
		 {
			  return Arrays.GetHashCode( _txBytes );
		 }

		 public override ChunkedInput<ByteBuf> Encode()
		 {
			  return ReplicatedTransactionSerializer.Encode( this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
		 public override void Marshal( WritableChannel channel )
		 {
			  ReplicatedTransactionSerializer.Marshal( channel, this );
		 }

		 public override TransactionRepresentation Extract( TransactionRepresentationExtractor extractor )
		 {
			  return extractor.Extract( this );
		 }
	}

}