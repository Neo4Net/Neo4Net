/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
	using ChunkedInput = io.netty.handler.stream.ChunkedInput;


	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

	public interface ReplicatedTransaction : CoreReplicatedContent
	{
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static TransactionRepresentationReplicatedTransaction from(org.neo4j.kernel.impl.transaction.TransactionRepresentation tx)
	//	 {
	//		  return new TransactionRepresentationReplicatedTransaction(tx);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static ByteArrayReplicatedTransaction from(byte[] bytes)
	//	 {
	//		  return new ByteArrayReplicatedTransaction(bytes);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void dispatch(org.neo4j.causalclustering.core.state.CommandDispatcher commandDispatcher, long commandIndex, System.Action<org.neo4j.causalclustering.core.state.Result> callback)
	//	 {
	//		  commandDispatcher.dispatch(this, commandIndex, callback);
	//	 }

		 ChunkedInput<ByteBuf> Encode();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void marshal(org.neo4j.storageengine.api.WritableChannel writableChannel) throws java.io.IOException;
		 void Marshal( WritableChannel writableChannel );

		 TransactionRepresentation Extract( TransactionRepresentationExtractor extractor );
	}

}