using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Server.rest.transactional
{

	using Predicates = Neo4Net.Functions.Predicates;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using InvalidConcurrentTransactionAccess = Neo4Net.Server.rest.transactional.error.InvalidConcurrentTransactionAccess;
	using InvalidTransactionId = Neo4Net.Server.rest.transactional.error.InvalidTransactionId;
	using TransactionLifecycleException = Neo4Net.Server.rest.transactional.error.TransactionLifecycleException;

	public class TransactionHandleRegistry : TransactionRegistry
	{
		 private readonly AtomicLong _idGenerator = new AtomicLong( 0L );
		 private readonly ConcurrentDictionary<long, TransactionMarker> _registry = new ConcurrentDictionary<long, TransactionMarker>( 64 );

		 private readonly Clock _clock;

		 private readonly Log _log;
		 private readonly long _timeoutMillis;

		 public TransactionHandleRegistry( Clock clock, long timeoutMillis, LogProvider logProvider )
		 {
			  this._clock = clock;
			  this._timeoutMillis = timeoutMillis;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 private abstract class TransactionMarker
		 {
			  internal abstract ActiveTransaction ActiveTransaction { get; }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract SuspendedTransaction getSuspendedTransaction() throws Neo4Net.server.rest.transactional.error.InvalidConcurrentTransactionAccess;
			  internal abstract SuspendedTransaction SuspendedTransaction { get; }

			  internal abstract bool Suspended { get; }
		 }

		 private class ActiveTransaction : TransactionMarker
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly TransactionTerminationHandle TerminationHandleConflict;

			  internal ActiveTransaction( TransactionTerminationHandle terminationHandle )
			  {
					this.TerminationHandleConflict = terminationHandle;
			  }

			  internal virtual TransactionTerminationHandle TerminationHandle
			  {
				  get
				  {
						return TerminationHandleConflict;
				  }
			  }

			  internal override ActiveTransaction GetActiveTransaction()
			  {
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: SuspendedTransaction getSuspendedTransaction() throws Neo4Net.server.rest.transactional.error.InvalidConcurrentTransactionAccess
			  internal override SuspendedTransaction SuspendedTransaction
			  {
				  get
				  {
						throw new InvalidConcurrentTransactionAccess();
				  }
			  }

			  internal override bool Suspended
			  {
				  get
				  {
						return false;
				  }
			  }
		 }

		 private class SuspendedTransaction : TransactionMarker
		 {
			 private readonly TransactionHandleRegistry _outerInstance;

			  internal readonly ActiveTransaction ActiveMarker;
			  internal readonly TransactionHandle TransactionHandle;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long LastActiveTimestampConflict;

			  internal SuspendedTransaction( TransactionHandleRegistry outerInstance, ActiveTransaction activeMarker, TransactionHandle transactionHandle )
			  {
				  this._outerInstance = outerInstance;
					this.ActiveMarker = activeMarker;
					this.TransactionHandle = transactionHandle;
					this.LastActiveTimestampConflict = outerInstance.clock.millis();
			  }

			  internal override ActiveTransaction ActiveTransaction
			  {
				  get
				  {
						return ActiveMarker;
				  }
			  }

			  internal override SuspendedTransaction GetSuspendedTransaction()
			  {
					return this;
			  }

			  internal override bool Suspended
			  {
				  get
				  {
						return true;
				  }
			  }

			  internal virtual long LastActiveTimestamp
			  {
				  get
				  {
						return LastActiveTimestampConflict;
				  }
			  }
		 }

		 public override long Begin( TransactionHandle handle )
		 {
			  long id = _idGenerator.incrementAndGet();
			  if ( null == _registry.GetOrAdd( id, new ActiveTransaction( handle ) ) )
			  {
					return id;
			  }
			  else
			  {
					throw new System.InvalidOperationException( "Attempt to begin transaction for id that was already registered" );
			  }
		 }

		 public override long Release( long id, TransactionHandle transactionHandle )
		 {
			  TransactionMarker marker = _registry[id];

			  if ( null == marker )
			  {
					throw new System.InvalidOperationException( "Trying to suspend unregistered transaction" );
			  }

			  if ( marker.Suspended )
			  {
					throw new System.InvalidOperationException( "Trying to suspend transaction that was already suspended" );
			  }

			  SuspendedTransaction suspendedTx = new SuspendedTransaction( this, marker.ActiveTransaction, transactionHandle );
			  if ( !_registry.replace( id, marker, suspendedTx ) )
			  {
					throw new System.InvalidOperationException( "Trying to suspend transaction that has been concurrently suspended" );
			  }
			  return ComputeNewExpiryTime( suspendedTx.LastActiveTimestamp );
		 }

		 private long ComputeNewExpiryTime( long lastActiveTimestamp )
		 {
			  return lastActiveTimestamp + _timeoutMillis;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionHandle acquire(long id) throws Neo4Net.server.rest.transactional.error.TransactionLifecycleException
		 public override TransactionHandle Acquire( long id )
		 {
			  TransactionMarker marker = _registry[id];

			  if ( null == marker )
			  {
					throw new InvalidTransactionId();
			  }

			  SuspendedTransaction transaction = marker.SuspendedTransaction;
			  if ( _registry.replace( id, marker, marker.ActiveTransaction ) )
			  {
					return transaction.TransactionHandle;
			  }
			  else
			  {
					throw new InvalidConcurrentTransactionAccess();
			  }
		 }

		 public override void Forget( long id )
		 {
			  TransactionMarker marker = _registry[id];

			  if ( null == marker )
			  {
					throw new System.InvalidOperationException( "Could not finish unregistered transaction" );
			  }

			  if ( marker.Suspended )
			  {
					throw new System.InvalidOperationException( "Cannot finish suspended registered transaction" );
			  }

			  if ( !_registry.Remove( id, marker ) )
			  {
					throw new System.InvalidOperationException( "Trying to finish transaction that has been concurrently finished or suspended" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionHandle terminate(long id) throws Neo4Net.server.rest.transactional.error.TransactionLifecycleException
		 public override TransactionHandle Terminate( long id )
		 {
			  TransactionMarker marker = _registry[id];
			  if ( null == marker )
			  {
					throw new InvalidTransactionId();
			  }

			  TransactionTerminationHandle handle = marker.ActiveTransaction.TerminationHandle;
			  handle.Terminate();

			  try
			  {
					SuspendedTransaction transaction = marker.SuspendedTransaction;
					if ( _registry.replace( id, marker, marker.ActiveTransaction ) )
					{
						 return transaction.TransactionHandle;
					}
			  }
			  catch ( InvalidConcurrentTransactionAccess )
			  {
					// We could not acquire the transaction. Let the other request clean up.
			  }
			  return null;
		 }

		 public override void RollbackAllSuspendedTransactions()
		 {
			  RollbackSuspended( Predicates.alwaysTrue() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void rollbackSuspendedTransactionsIdleSince(final long oldestLastActiveTime)
		 public virtual void RollbackSuspendedTransactionsIdleSince( long oldestLastActiveTime )
		 {
			  RollbackSuspended(item =>
			  {
				try
				{
					 SuspendedTransaction transaction = item.SuspendedTransaction;
					 return transaction.LastActiveTimestampConflict < oldestLastActiveTime;
				}
				catch ( InvalidConcurrentTransactionAccess concurrentTransactionAccessError )
				{
					 throw new Exception( concurrentTransactionAccessError );
				}
			  });
		 }

		 private void RollbackSuspended( System.Predicate<TransactionMarker> predicate )
		 {
			  ISet<long> candidateTransactionIdsToRollback = new HashSet<long>();

			  foreach ( KeyValuePair<long, TransactionMarker> entry in _registry.SetOfKeyValuePairs() )
			  {
					TransactionMarker marker = entry.Value;
					if ( marker.Suspended && predicate( marker ) )
					{
						 candidateTransactionIdsToRollback.Add( entry.Key );
					}
			  }

			  foreach ( long id in candidateTransactionIdsToRollback )
			  {
					TransactionHandle handle;
					try
					{
						 handle = Acquire( id );
					}
					catch ( TransactionLifecycleException )
					{
						 // Allow this - someone snatched the transaction from under our feet,
						 continue;
					}
					try
					{
						 handle.ForceRollback();
						 _log.info( format( "Transaction with id %d has been automatically rolled back due to transaction timeout.", id ) );
					}
					catch ( Exception e )
					{
						 _log.error( format( "Transaction with id %d failed to roll back.", id ), e );
					}
					finally
					{
						 Forget( id );
					}
			  }
		 }
	}

}