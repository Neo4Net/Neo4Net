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
namespace Neo4Net.com
{

	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using LogEntryStart = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryStart;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

	public class MadeUpServerImplementation : MadeUpCommunicationInterface
	{
		 private readonly StoreId _storeIdToRespondWith;
		 private bool _gotCalled;

		 public MadeUpServerImplementation( StoreId storeIdToRespondWith )
		 {
			  this._storeIdToRespondWith = storeIdToRespondWith;
		 }

		 public override Response<int> Multiply( int value1, int value2 )
		 {
			  _gotCalled = true;
			  return new TransactionStreamResponse<int>( value1 * value2, _storeIdToRespondWith, TransactionStream_Fields.Empty, ResourceReleaser_Fields.NoOp );
		 }

		 public override Response<Void> FetchDataStream( MadeUpWriter writer, int dataSize )
		 {
			  // Reversed on the server side. This will send data back to the client.
			  writer.Write( new KnownDataByteChannel( dataSize ) );
			  return EmptyResponse();
		 }

		 private Response<Void> EmptyResponse()
		 {
			  return new TransactionStreamResponse<Void>( null, _storeIdToRespondWith, TransactionStream_Fields.Empty, ResourceReleaser_Fields.NoOp );
		 }

		 public override Response<Void> SendDataStream( ReadableByteChannel data )
		 {
			  // TOOD Verify as well?
			  ReadFully( data );
			  return EmptyResponse();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Response<int> streamBackTransactions(int responseToSendBack, final int txCount)
		 public override Response<int> StreamBackTransactions( int responseToSendBack, int txCount )
		 {
			  TransactionStream transactions = visitor =>
			  {
				for ( int i = 1; i <= txCount; i++ )
				{
					 CommittedTransactionRepresentation transaction = CreateTransaction( TransactionIdStore.BASE_TX_ID + i );
					 visitor.visit( transaction );
				}
			  };

			  return new TransactionStreamResponse<int>( responseToSendBack, _storeIdToRespondWith, transactions, ResourceReleaser_Fields.NoOp );
		 }

		 public override Response<int> InformAboutTransactionObligations( int responseToSendBack, long desiredObligation )
		 {
			  return new TransactionObligationResponse<int>( responseToSendBack, _storeIdToRespondWith, desiredObligation, ResourceReleaser_Fields.NoOp );
		 }

		 protected internal virtual CommittedTransactionRepresentation CreateTransaction( long txId )
		 {
			  return new CommittedTransactionRepresentation( new LogEntryStart( 0, 0, 0, 0, new sbyte[0], null ), Transaction( txId ), new LogEntryCommit( txId, 0 ) );
		 }

		 private TransactionRepresentation Transaction( long txId )
		 {
			  ICollection<StorageCommand> commands = new List<StorageCommand>();
			  NodeRecord node = new NodeRecord( txId );
			  node.InUse = true;
			  commands.Add( new NodeCommand( new NodeRecord( txId ), node ) );
			  PhysicalTransactionRepresentation transaction = new PhysicalTransactionRepresentation( commands );
			  transaction.SetHeader( new sbyte[0], 0, 0, 0, 0, 0, 0 );
			  return transaction;
		 }

		 private void ReadFully( ReadableByteChannel data )
		 {
			  ByteBuffer buffer = ByteBuffer.allocate( 1000 );
			  try
			  {
					while ( true )
					{
						 buffer.clear();
						 if ( data.read( buffer ) == -1 )
						 {
							  break;
						 }
					}
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 public override Response<int> ThrowException( string messageInException )
		 {
			  throw new MadeUpException( messageInException, new Exception( "The cause of it" ) );
		 }

		 public virtual bool GotCalled()
		 {
			  return this._gotCalled;
		 }
	}

}