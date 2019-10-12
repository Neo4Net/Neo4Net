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
namespace Org.Neo4j.Kernel.ha.cluster
{

	using RequestContext = Org.Neo4j.com.RequestContext;
	using Org.Neo4j.com;
	using ResponsePacker = Org.Neo4j.com.storecopy.ResponsePacker;
	using StoreCopyServer = Org.Neo4j.com.storecopy.StoreCopyServer;
	using StoreWriter = Org.Neo4j.com.storecopy.StoreWriter;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using MasterImpl = Org.Neo4j.Kernel.ha.com.master.MasterImpl;
	using IdAllocation = Org.Neo4j.Kernel.ha.id.IdAllocation;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using IdGenerator = Org.Neo4j.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using TransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.TransactionRepresentation;
	using LogicalTransactionStore = Org.Neo4j.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using CommitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.CommitEvent;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;

	public class DefaultMasterImplSPI : MasterImpl.SPI
	{
		 private const int ID_GRAB_SIZE = 1000;
		 internal const string STORE_COPY_CHECKPOINT_TRIGGER = "store copy";

		 private readonly GraphDatabaseAPI _graphDb;
		 private readonly TransactionChecksumLookup _txChecksumLookup;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly TokenHolders _tokenHolders;
		 private readonly IdGeneratorFactory _idGeneratorFactory;
		 private readonly NeoStoreDataSource _neoStoreDataSource;
		 private readonly File _databaseDirectory;
		 private readonly ResponsePacker _responsePacker;
		 private readonly Monitors _monitors;

		 private readonly TransactionCommitProcess _transactionCommitProcess;
		 private readonly CheckPointer _checkPointer;

		 public DefaultMasterImplSPI( GraphDatabaseAPI graphDb, FileSystemAbstraction fileSystemAbstraction, Monitors monitors, TokenHolders tokenHolders, IdGeneratorFactory idGeneratorFactory, TransactionCommitProcess transactionCommitProcess, CheckPointer checkPointer, TransactionIdStore transactionIdStore, LogicalTransactionStore logicalTransactionStore, NeoStoreDataSource neoStoreDataSource, LogProvider logProvider )
		 {
			  this._graphDb = graphDb;
			  this._fileSystem = fileSystemAbstraction;
			  this._tokenHolders = tokenHolders;
			  this._idGeneratorFactory = idGeneratorFactory;
			  this._transactionCommitProcess = transactionCommitProcess;
			  this._checkPointer = checkPointer;
			  this._neoStoreDataSource = neoStoreDataSource;
			  this._databaseDirectory = graphDb.DatabaseLayout().databaseDirectory();
			  this._txChecksumLookup = new TransactionChecksumLookup( transactionIdStore, logicalTransactionStore );
			  this._responsePacker = new ResponsePacker( logicalTransactionStore, transactionIdStore, graphDb.storeId );
			  this._monitors = monitors;
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  monitors.AddMonitorListener( new LoggingStoreCopyServerMonitor( logProvider.GetLog( typeof( StoreCopyServer ) ) ), typeof( StoreCopyServer ).FullName );
		 }

		 public virtual bool Accessible
		 {
			 get
			 {
				  // Wait for 5s for the database to become available, if not already so
				  return _graphDb.isAvailable( 5000 );
			 }
		 }

		 public override int GetOrCreateLabel( string name )
		 {
			  return _tokenHolders.labelTokens().getOrCreateId(name);
		 }

		 public override int GetOrCreateProperty( string name )
		 {
			  return _tokenHolders.propertyKeyTokens().getOrCreateId(name);
		 }

		 public override IdAllocation AllocateIds( IdType idType )
		 {
			  IdGenerator generator = _idGeneratorFactory.get( idType );
			  return new IdAllocation( generator.NextIdBatch( ID_GRAB_SIZE ), generator.HighId, generator.DefragCount );
		 }

		 public override StoreId StoreId()
		 {
			  return _graphDb.storeId();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long applyPreparedTransaction(org.neo4j.kernel.impl.transaction.TransactionRepresentation preparedTransaction) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 public override long ApplyPreparedTransaction( TransactionRepresentation preparedTransaction )
		 {
			  return _transactionCommitProcess.commit( new TransactionToApply( preparedTransaction ), CommitEvent.NULL, TransactionApplicationMode.EXTERNAL );
		 }

		 public override int? CreateRelationshipType( string name )
		 {
			  return _tokenHolders.relationshipTypeTokens().getOrCreateId(name);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getTransactionChecksum(long txId) throws java.io.IOException
		 public override long GetTransactionChecksum( long txId )
		 {
			  return _txChecksumLookup.lookup( txId );
		 }

		 public override RequestContext FlushStoresAndStreamStoreFiles( StoreWriter writer )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  StoreCopyServer streamer = new StoreCopyServer( _neoStoreDataSource, _checkPointer, _fileSystem, _databaseDirectory, _monitors.newMonitor( typeof( StoreCopyServer.Monitor ), typeof( StoreCopyServer ).FullName ) );
			  return streamer.FlushStoresAndStreamStoreFiles( STORE_COPY_CHECKPOINT_TRIGGER, writer, false );
		 }

		 public override Response<T> PackTransactionStreamResponse<T>( RequestContext context, T response )
		 {
			  return _responsePacker.packTransactionStreamResponse( context, response );
		 }

		 public override Response<T> PackTransactionObligationResponse<T>( RequestContext context, T response )
		 {
			  return _responsePacker.packTransactionObligationResponse( context, response );
		 }

		 public override Response<T> PackEmptyResponse<T>( T response )
		 {
			  return _responsePacker.packEmptyResponse( response );
		 }

		 private class LoggingStoreCopyServerMonitor : StoreCopyServer.Monitor
		 {
			  internal Log Log;

			  internal LoggingStoreCopyServerMonitor( Log log )
			  {
					this.Log = log;
			  }

			  public override void StartTryCheckPoint( string storeCopyIdentifier )
			  {
					Log.debug( "%s: try to checkpoint before sending store.", storeCopyIdentifier );
			  }

			  public override void FinishTryCheckPoint( string storeCopyIdentifier )
			  {
					Log.debug( "%s: checkpoint before sending store completed.", storeCopyIdentifier );
			  }

			  public override void StartStreamingStoreFile( File file, string storeCopyIdentifier )
			  {
					Log.debug( "%s: start streaming file %s.", storeCopyIdentifier, file );
			  }

			  public override void FinishStreamingStoreFile( File file, string storeCopyIdentifier )
			  {
					Log.debug( "%s: finish streaming file %s.", storeCopyIdentifier, file );
			  }

			  public override void StartStreamingStoreFiles( string storeCopyIdentifier )
			  {
					Log.debug( "%s: start streaming store files.", storeCopyIdentifier );
			  }

			  public override void FinishStreamingStoreFiles( string storeCopyIdentifier )
			  {
					Log.debug( "%s: finish streaming store files.", storeCopyIdentifier );
			  }

			  public override void StartStreamingTransactions( long startTxId, string storeCopyIdentifier )
			  {
					Log.debug( "%s: start streaming transaction starting %d.", storeCopyIdentifier, startTxId );
			  }

			  public override void FinishStreamingTransactions( long endTxId, string storeCopyIdentifier )
			  {
					Log.debug( "%s: finish streaming transactions at %d.", storeCopyIdentifier, endTxId );
			  }
		 }
	}

}