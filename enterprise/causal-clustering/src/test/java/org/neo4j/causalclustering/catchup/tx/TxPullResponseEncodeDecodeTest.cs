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
namespace Org.Neo4j.causalclustering.catchup.tx
{
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using Test = org.junit.Test;

	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using LogEntryCommand = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using LogEntryCommit = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using LogEntryStart = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryStart;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;

	public class TxPullResponseEncodeDecodeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeAndDecodePullResponseMessage()
		 public virtual void ShouldEncodeAndDecodePullResponseMessage()
		 {
			  // given
			  EmbeddedChannel channel = new EmbeddedChannel( new TxPullResponseEncoder(), new TxPullResponseDecoder() );
			  TxPullResponse sent = new TxPullResponse( new StoreId( 1, 2, 3, 4 ), NewCommittedTransactionRepresentation() );

			  // when
			  channel.writeOutbound( sent );
			  object message = channel.readOutbound();
			  channel.writeInbound( message );

			  // then
			  TxPullResponse received = channel.readInbound();
			  assertNotSame( sent, received );
			  assertEquals( sent, received );
		 }

		 private CommittedTransactionRepresentation NewCommittedTransactionRepresentation()
		 {
			  const long arbitraryRecordId = 27L;
			  Command.NodeCommand command = new Command.NodeCommand( new NodeRecord( arbitraryRecordId ), new NodeRecord( arbitraryRecordId ) );

			  PhysicalTransactionRepresentation physicalTransactionRepresentation = new PhysicalTransactionRepresentation( singletonList( ( new LogEntryCommand( command ) ).Command ) );
			  physicalTransactionRepresentation.SetHeader( new sbyte[]{}, 0, 0, 0, 0, 0, 0 );

			  LogEntryStart startEntry = new LogEntryStart( 0, 0, 0L, 0L, new sbyte[]{}, LogPosition.UNSPECIFIED );
			  LogEntryCommit commitEntry = new LogEntryCommit( 42, 0 );

			  return new CommittedTransactionRepresentation( startEntry, physicalTransactionRepresentation, commitEntry );
		 }

	}

}