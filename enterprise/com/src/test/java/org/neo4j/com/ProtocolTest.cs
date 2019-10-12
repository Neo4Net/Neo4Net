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
namespace Org.Neo4j.com
{
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;
	using Test = org.junit.Test;


	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using TransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.TransactionRepresentation;
	using NodeCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.NodeCommand;
	using InMemoryClosableChannel = Org.Neo4j.Kernel.impl.transaction.log.InMemoryClosableChannel;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ProtocolTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeAndDeserializeTransactionRepresentation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeAndDeserializeTransactionRepresentation()
		 {
			  // GIVEN
			  PhysicalTransactionRepresentation transaction = new PhysicalTransactionRepresentation( JustOneNode() );
			  sbyte[] additionalHeader = "extra".GetBytes();
			  int masterId = 1;
			  int authorId = 2;
			  long timeStarted = 12345;
			  long lastTxWhenStarted = 12;
			  long timeCommitted = timeStarted + 10;
			  transaction.SetHeader( additionalHeader, masterId, authorId, timeStarted, lastTxWhenStarted, timeCommitted, -1 );
			  Protocol.TransactionSerializer serializer = new Protocol.TransactionSerializer( transaction );
			  ChannelBuffer buffer = new ChannelBufferWrapper( new InMemoryClosableChannel() );

			  // WHEN serializing the transaction
			  serializer.Write( buffer );

			  // THEN deserializing the same transaction should yield the same data.
			  // ... remember that this deserializer doesn't read the data source name string. Read it manually here
			  assertEquals( NeoStoreDataSource.DEFAULT_DATA_SOURCE_NAME, Protocol.ReadString( buffer ) );
			  VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel> reader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
			  TransactionRepresentation readTransaction = ( new Protocol.TransactionRepresentationDeserializer( reader ) ).Read( buffer, ByteBuffer.allocate( 1000 ) );
			  assertArrayEquals( additionalHeader, readTransaction.AdditionalHeader() );
			  assertEquals( masterId, readTransaction.MasterId );
			  assertEquals( authorId, readTransaction.AuthorId );
			  assertEquals( timeStarted, readTransaction.TimeStarted );
			  assertEquals( lastTxWhenStarted, readTransaction.LatestCommittedTxWhenStarted );
			  assertEquals( timeCommitted, readTransaction.TimeCommitted );
		 }

		 private ICollection<StorageCommand> JustOneNode()
		 {
			  NodeRecord node = new NodeRecord( 0 );
			  node.InUse = true;
			  return Arrays.asList( new NodeCommand( new NodeRecord( node.Id ), node ) );
		 }
	}

}