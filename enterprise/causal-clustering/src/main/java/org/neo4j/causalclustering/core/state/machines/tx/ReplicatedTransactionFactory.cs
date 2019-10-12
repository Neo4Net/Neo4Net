using System;
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
namespace Org.Neo4j.causalclustering.core.state.machines.tx
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;


	using NetworkReadableClosableChannelNetty4 = Org.Neo4j.causalclustering.messaging.NetworkReadableClosableChannelNetty4;
	using Org.Neo4j.Function;
	using RecordStorageCommandReaderFactory = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageCommandReaderFactory;
	using TransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.TransactionRepresentation;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using InvalidLogEntryHandler = Org.Neo4j.Kernel.impl.transaction.log.entry.InvalidLogEntryHandler;
	using LogEntryCommand = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using StorageCommandSerializer = Org.Neo4j.Kernel.impl.transaction.log.entry.StorageCommandSerializer;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;
	using WritableChannel = Org.Neo4j.Storageengine.Api.WritableChannel;

	public class ReplicatedTransactionFactory
	{

		 private ReplicatedTransactionFactory()
		 {
			  throw new AssertionError( "Should not be instantiated" );
		 }

		 public static TransactionRepresentation ExtractTransactionRepresentation( ReplicatedTransaction transactionCommand, sbyte[] extraHeader )
		 {
			  return transactionCommand.Extract( new TransactionRepresentationReader( extraHeader ) );
		 }

		 public static TransactionRepresentationWriter TransactionalRepresentationWriter( TransactionRepresentation transactionCommand )
		 {
			  return new TransactionRepresentationWriter( transactionCommand );
		 }

		 private class TransactionRepresentationReader : TransactionRepresentationExtractor
		 {
			  internal readonly sbyte[] ExtraHeader;

			  internal TransactionRepresentationReader( sbyte[] extraHeader )
			  {
					this.ExtraHeader = extraHeader;
			  }

			  public override TransactionRepresentation Extract( TransactionRepresentationReplicatedTransaction replicatedTransaction )
			  {
					return replicatedTransaction.Tx();
			  }

			  public override TransactionRepresentation Extract( ByteArrayReplicatedTransaction replicatedTransaction )
			  {
					ByteBuf buffer = Unpooled.wrappedBuffer( replicatedTransaction.TxBytes );
					NetworkReadableClosableChannelNetty4 channel = new NetworkReadableClosableChannelNetty4( buffer );
					return Read( channel );
			  }

			  internal virtual TransactionRepresentation Read( NetworkReadableClosableChannelNetty4 channel )
			  {
					try
					{
						 LogEntryReader<ReadableClosablePositionAwareChannel> reader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>( new RecordStorageCommandReaderFactory(), InvalidLogEntryHandler.STRICT );

						 int authorId = channel.Int;
						 int masterId = channel.Int;
						 long latestCommittedTxWhenStarted = channel.Long;
						 long timeStarted = channel.Long;
						 long timeCommitted = channel.Long;
						 int lockSessionId = channel.Int;

						 int headerLength = channel.Int;
						 sbyte[] header;
						 if ( headerLength == 0 )
						 {
							  header = ExtraHeader;
						 }
						 else
						 {
							  header = new sbyte[headerLength];
						 }

						 channel.Get( header, headerLength );

						 LogEntryCommand entryRead;
						 IList<StorageCommand> commands = new LinkedList<StorageCommand>();

						 while ( ( entryRead = ( LogEntryCommand ) reader.ReadLogEntry( channel ) ) != null )
						 {
							  commands.Add( entryRead.Command );
						 }

						 PhysicalTransactionRepresentation tx = new PhysicalTransactionRepresentation( commands );
						 tx.SetHeader( header, masterId, authorId, timeStarted, latestCommittedTxWhenStarted, timeCommitted, lockSessionId );

						 return tx;
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }
		 }

		 internal class TransactionRepresentationWriter
		 {
			  internal readonly IEnumerator<StorageCommand> Commands;
			  internal ThrowingConsumer<WritableChannel, IOException> NextJob;

			  internal TransactionRepresentationWriter( TransactionRepresentation tx )
			  {
					NextJob = channel =>
					{
					 channel.putInt( tx.AuthorId );
					 channel.putInt( tx.MasterId );
					 channel.putLong( tx.LatestCommittedTxWhenStarted );
					 channel.putLong( tx.TimeStarted );
					 channel.putLong( tx.TimeCommitted );
					 channel.putInt( tx.LockSessionId );

					 sbyte[] additionalHeader = tx.AdditionalHeader();
					 if ( additionalHeader != null )
					 {
						  channel.putInt( additionalHeader.Length );
						  channel.put( additionalHeader, additionalHeader.Length );
					 }
					 else
					 {
						  channel.putInt( 0 );
					 }
					};
					Commands = tx.GetEnumerator();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void write(org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  internal virtual void Write( WritableChannel channel )
			  {
					NextJob.accept( channel );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( Commands.hasNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 StorageCommand storageCommand = Commands.next();
						 NextJob = c => ( new StorageCommandSerializer( c ) ).visit( storageCommand );
					}
					else
					{
						 NextJob = null;
					}
			  }

			  internal virtual bool CanWrite()
			  {
					return NextJob != null;
			  }
		 }
	}

}