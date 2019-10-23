using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.ha.com.master
{

	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using TransactionNotPresentOnMasterException = Neo4Net.com.TransactionNotPresentOnMasterException;
	using StoreWriter = Neo4Net.com.storecopy.StoreWriter;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IdAllocation = Neo4Net.Kernel.ha.id.IdAllocation;
	using LockResult = Neo4Net.Kernel.ha.@lock.LockResult;
	using LockStatus = Neo4Net.Kernel.ha.@lock.LockStatus;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using IllegalResourceException = Neo4Net.Kernel.impl.transaction.IllegalResourceException;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using ConcurrentAccessException = Neo4Net.Kernel.impl.util.collection.ConcurrentAccessException;
	using NoSuchEntryException = Neo4Net.Kernel.impl.util.collection.NoSuchEntryException;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;

	/// <summary>
	/// This is the real master code that executes on a master. The actual
	/// communication over network happens in <seealso cref="org.Neo4Net.kernel.ha.com.slave.MasterClient"/> and
	/// <seealso cref="MasterServer"/>.
	/// </summary>
	public class MasterImpl : LifecycleAdapter, Master
	{
		 public interface Monitor
		 {
			  void InitializeTx( RequestContext context );
		 }

		 // This is a bridge SPI that MasterImpl requires to function. Eventually this should be split
		 // up into many smaller APIs implemented by other services so that this is not needed.
		 // This SPI allows MasterImpl to have no direct dependencies, and instead puts those dependencies into the
		 // SPI implementation, thus making it easier to test this class by mocking the SPI.
		 public interface ISPI
		 {
			  bool Accessible { get; }

			  IdAllocation AllocateIds( IdType idType );

			  StoreId StoreId();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long applyPreparedTransaction(org.Neo4Net.kernel.impl.transaction.TransactionRepresentation preparedTransaction) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
			  long ApplyPreparedTransaction( TransactionRepresentation preparedTransaction );

			  int? CreateRelationshipType( string name );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long getTransactionChecksum(long txId) throws java.io.IOException;
			  long GetTransactionChecksum( long txId );

			  RequestContext FlushStoresAndStreamStoreFiles( StoreWriter writer );

			  Response<T> packEmptyResponse<T>( T response );

			  Response<T> packTransactionStreamResponse<T>( RequestContext context, T response );

			  Response<T> packTransactionObligationResponse<T>( RequestContext context, T response );

			  int GetOrCreateLabel( string name );

			  int GetOrCreateProperty( string name );

		 }

		 private readonly SPI _spi;
		 private readonly Config _config;
		 private readonly Monitor _monitor;
		 private readonly long _epoch;

		 private readonly ConversationManager _conversationManager;

		 public MasterImpl( SPI spi, ConversationManager conversationManager, Monitor monitor, Config config )
		 {
			  this._spi = spi;
			  this._config = config;
			  this._monitor = monitor;
			  this._conversationManager = conversationManager;
			  this._epoch = GenerateEpoch();
		 }

		 private long GenerateEpoch()
		 {
			  return ( ( ( ( long ) _config.get( ClusterSettings.server_id ).toIntegerIndex() ) ) << 48 ) | DateTimeHelper.CurrentUnixTimeMillis();
		 }

		 public override void Start()
		 {
			  _conversationManager.start();
		 }

		 public override void Stop()
		 {
			  _conversationManager.stop();
		 }

		 /// <summary>
		 /// Basically for all public methods call this assertion to verify that the caller meant to call this
		 /// master. The epoch is the one handed out from <seealso cref="handshake(long, StoreId)"/>.
		 /// Exceptions to the above are:
		 /// o <seealso cref="handshake(long, StoreId)"/>
		 /// o <seealso cref="copyStore(RequestContext, StoreWriter)"/>
		 /// o <seealso cref="pullUpdates(RequestContext)"/>
		 /// 
		 /// all other methods must have this. </summary>
		 /// <param name="context"> the request context containing the epoch the request thinks it's for. </param>
		 private void AssertCorrectEpoch( RequestContext context )
		 {
			  if ( this._epoch != context.Epoch )
			  {
					throw new InvalidEpochException( _epoch, context.Epoch );
			  }
		 }

		 public override Response<IdAllocation> AllocateIds( RequestContext context, IdType idType )
		 {
			  AssertCorrectEpoch( context );
			  IdAllocation result = _spi.allocateIds( idType );
			  return _spi.packEmptyResponse( result );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.com.Response<long> commit(org.Neo4Net.com.RequestContext context, org.Neo4Net.kernel.impl.transaction.TransactionRepresentation preparedTransaction) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 public override Response<long> Commit( RequestContext context, TransactionRepresentation preparedTransaction )
		 {
			  AssertCorrectEpoch( context );

			  // There are two constraints relating to locking during commit:
			  // 1) If the client is has grabbed locks, we need to ensure those locks remain live during commit
			  // 2) We must hold a schema read lock while committing, to not race with schema transactions on the master
			  //
			  // To satisfy this, we must determine if the client is holding locks, and if so, use that lock client, and if
			  // not, we use a one-off lock client. The way the client signals this is via the 'eventIdentifier' in the
			  // request. -1 means no locks are held, any other number means there should be a matching lock session.

			  if ( context.EventIdentifier == Neo4Net.Kernel.impl.locking.Locks_Client_Fields.NO_LOCK_SESSION_ID )
			  {
					// Client is not holding locks, use a temporary lock client
					using ( Conversation conversation = _conversationManager.acquire() )
					{
						 return Commit0( context, preparedTransaction );
					}
			  }
			  else
			  {
					// Client is holding locks, use the clients lock session
					try
					{
						 Conversation conversation = _conversationManager.acquire( context );
						 try
						 {
							  return Commit0( context, preparedTransaction );
						 }
						 finally
						 {
							  _conversationManager.release( context );
						 }
					}
					catch ( Exception e ) when ( e is NoSuchEntryException || e is ConcurrentAccessException )
					{
						 throw new TransactionNotPresentOnMasterException( context );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.com.Response<long> commit0(org.Neo4Net.com.RequestContext context, org.Neo4Net.kernel.impl.transaction.TransactionRepresentation preparedTransaction) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 private Response<long> Commit0( RequestContext context, TransactionRepresentation preparedTransaction )
		 {
			  long txId = _spi.applyPreparedTransaction( preparedTransaction );
			  return _spi.packTransactionObligationResponse( context, txId );
		 }

		 public override Response<int> CreateRelationshipType( RequestContext context, string name )
		 {
			  AssertCorrectEpoch( context );
			  return _spi.packTransactionObligationResponse( context, _spi.createRelationshipType( name ) );
		 }

		 public override Response<int> CreatePropertyKey( RequestContext context, string name )
		 {
			  AssertCorrectEpoch( context );
			  return _spi.packTransactionObligationResponse( context, _spi.getOrCreateProperty( name ) );
		 }

		 public override Response<int> CreateLabel( RequestContext context, string name )
		 {
			  AssertCorrectEpoch( context );
			  return _spi.packTransactionObligationResponse( context, _spi.getOrCreateLabel( name ) );
		 }

		 public override Response<Void> PullUpdates( RequestContext context )
		 {
			  return _spi.packTransactionStreamResponse( context, null );
		 }

		 public override Response<HandshakeResult> Handshake( long txId, StoreId storeId )
		 {
			  try
			  {
					long checksum = _spi.getTransactionChecksum( txId );
					return _spi.packEmptyResponse( new HandshakeResult( checksum, _epoch ) );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( "Couldn't get master ID for transaction id " + txId, e );
			  }
		 }

		 public override Response<Void> CopyStore( RequestContext requestContext, StoreWriter writer )
		 {
			  RequestContext context;
			  using ( StoreWriter storeWriter = writer )
			  {
					context = _spi.flushStoresAndStreamStoreFiles( storeWriter );
			  } // close the store writer

			  return _spi.packTransactionStreamResponse( context, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.com.Response<Void> newLockSession(org.Neo4Net.com.RequestContext context) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 public override Response<Void> NewLockSession( RequestContext context )
		 {
			  _monitor.initializeTx( context );

			  if ( !_spi.Accessible )
			  {
					throw new TransactionFailureException( Neo4Net.Kernel.Api.Exceptions.Status_General.DatabaseUnavailable, "Database is currently not available" );
			  }
			  AssertCorrectEpoch( context );

			  try
			  {
					_conversationManager.begin( context );
			  }
			  catch ( ConcurrentAccessException e )
			  {
					throw new TransactionFailureException( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionAccessedConcurrently, e, "The lock session requested to start is already in use. " + "Please retry your request in a few seconds." );
			  }
			  return _spi.packTransactionObligationResponse( context, null );
		 }

		 public override Response<Void> EndLockSession( RequestContext context, bool success )
		 {
			  AssertCorrectEpoch( context );
			  _conversationManager.end( context );
			  if ( !success )
			  {
					_conversationManager.stop( context );
			  }
			  return _spi.packTransactionObligationResponse( context, null );
		 }

		 public override Response<LockResult> AcquireExclusiveLock( RequestContext context, ResourceType type, params long[] resourceIds )
		 {
			  AssertCorrectEpoch( context );
			  Neo4Net.Kernel.impl.locking.Locks_Client session;
			  try
			  {
					session = _conversationManager.acquire( context ).Locks;
			  }
			  catch ( Exception e ) when ( e is NoSuchEntryException || e is ConcurrentAccessException )
			  {
					return _spi.packTransactionObligationResponse( context, new LockResult( LockStatus.NOT_LOCKED, "Unable to acquire exclusive lock: " + e.Message ) );
			  }
			  try
			  {
					foreach ( long resourceId in resourceIds )
					{
						 session.AcquireExclusive( LockTracer.NONE, type, resourceId );
					}
					return _spi.packTransactionObligationResponse( context, new LockResult( LockStatus.OK_LOCKED ) );
			  }
			  catch ( DeadlockDetectedException e )
			  {
					return _spi.packTransactionObligationResponse( context, new LockResult( LockStatus.DEAD_LOCKED, "Can't acquire exclusive lock, because it would have caused a deadlock: " + e.Message ) );
			  }
			  catch ( IllegalResourceException e )
			  {
					return _spi.packTransactionObligationResponse( context, new LockResult( LockStatus.NOT_LOCKED, "Attempted to lock illegal resource: " + e.Message ) );
			  }
			  finally
			  {
					_conversationManager.release( context );
			  }
		 }

		 public override Response<LockResult> AcquireSharedLock( RequestContext context, ResourceType type, params long[] resourceIds )
		 {
			  AssertCorrectEpoch( context );
			  Neo4Net.Kernel.impl.locking.Locks_Client session;
			  try
			  {
					session = _conversationManager.acquire( context ).Locks;
			  }
			  catch ( Exception e ) when ( e is NoSuchEntryException || e is ConcurrentAccessException )
			  {
					return _spi.packTransactionObligationResponse( context, new LockResult( LockStatus.NOT_LOCKED, "Unable to acquire shared lock: " + e.Message ) );
			  }
			  try
			  {
					foreach ( long resourceId in resourceIds )
					{
						 session.AcquireShared( LockTracer.NONE, type, resourceId );
					}

					return _spi.packTransactionObligationResponse( context, new LockResult( LockStatus.OK_LOCKED ) );
			  }
			  catch ( DeadlockDetectedException e )
			  {
					return _spi.packTransactionObligationResponse( context, new LockResult( LockStatus.DEAD_LOCKED, e.Message ) );
			  }
			  catch ( IllegalResourceException e )
			  {
					return _spi.packTransactionObligationResponse( context, new LockResult( LockStatus.NOT_LOCKED, "Attempted to lock illegal resource: " + e.Message ) );
			  }
			  finally
			  {
					_conversationManager.release( context );
			  }
		 }

		 // =====================================================================
		 // Just some methods which aren't really used when running a HA cluster,
		 // but exposed so that other tools can reach that information.
		 // =====================================================================

		 public virtual IDictionary<int, ICollection<RequestContext>> OngoingTransactions
		 {
			 get
			 {
				  IDictionary<int, ICollection<RequestContext>> result = new Dictionary<int, ICollection<RequestContext>>();
				  ISet<RequestContext> contexts = _conversationManager.ActiveContexts;
				  foreach ( RequestContext context in contexts.toArray( new RequestContext[contexts.Count] ) )
				  {
						ICollection<RequestContext> txs = result.computeIfAbsent( context.MachineId(), k => new List<RequestContext>() );
						txs.Add( context );
				  }
				  return result;
			 }
		 }
	}

}