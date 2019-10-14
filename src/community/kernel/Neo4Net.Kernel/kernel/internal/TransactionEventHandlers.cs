using System;
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
namespace Neo4Net.Kernel.Internal
{

	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using LabelEntry = Neo4Net.Graphdb.@event.LabelEntry;
	using Neo4Net.Graphdb.@event;
	using TransactionData = Neo4Net.Graphdb.@event.TransactionData;
	using Neo4Net.Graphdb.@event;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Neo4Net.Kernel.api;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using TxStateTransactionDataSnapshot = Neo4Net.Kernel.impl.coreapi.TxStateTransactionDataSnapshot;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;
	using ReadableTransactionState = Neo4Net.Storageengine.Api.txstate.ReadableTransactionState;

	/// <summary>
	/// Handle the collection of transaction event handlers, and fire events as needed.
	/// </summary>
	public class TransactionEventHandlers : Lifecycle, TransactionHook<TransactionEventHandlers.TransactionHandlerState>
	{
		 private readonly CopyOnWriteArraySet<TransactionEventHandler> _transactionEventHandlers = new CopyOnWriteArraySet<TransactionEventHandler>();

		 private readonly EmbeddedProxySPI _proxySpi;

		 public TransactionEventHandlers( EmbeddedProxySPI spi )
		 {
			  this._proxySpi = spi;
		 }

		 public override void Init()
		 {
		 }

		 public override void Start()
		 {
		 }

		 public override void Stop()
		 {
		 }

		 public override void Shutdown()
		 {
		 }

		 public virtual TransactionEventHandler<T> RegisterTransactionEventHandler<T>( TransactionEventHandler<T> handler )
		 {
			  this._transactionEventHandlers.add( handler );
			  return handler;
		 }

		 public virtual TransactionEventHandler<T> UnregisterTransactionEventHandler<T>( TransactionEventHandler<T> handler )
		 {
			  return UnregisterHandler( this._transactionEventHandlers, handler );
		 }

		 private T UnregisterHandler<T, T1>( ICollection<T1> setOfHandlers, T handler )
		 {
			  if ( !setOfHandlers.remove( handler ) )
			  {
					throw new System.InvalidOperationException( handler + " isn't registered" );
			  }
			  return handler;
		 }

		 public override TransactionHandlerState BeforeCommit( ReadableTransactionState state, KernelTransaction transaction, StorageReader storageReader )
		 {
			  // The iterator grabs a snapshot of our list of handlers
			  IEnumerator<TransactionEventHandler> handlers = _transactionEventHandlers.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( !handlers.hasNext() )
			  {
					// Use 'null' as a signal that no event handlers were registered at beforeCommit time
					return null;
			  }

			  TransactionData txData = state == null ? EMPTY_DATA : new TxStateTransactionDataSnapshot( state, _proxySpi, storageReader, transaction );

			  TransactionHandlerState handlerStates = new TransactionHandlerState( txData );
			  while ( handlers.MoveNext() )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.graphdb.event.TransactionEventHandler<?> handler = handlers.Current;
					TransactionEventHandler<object> handler = handlers.Current;
					try
					{
						 handlerStates.Add( handler ).State = handler.BeforeCommit( txData );
					}
					catch ( Exception t )
					{
						 handlerStates.Failed( t );
					}
			  }

			  return handlerStates;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public void afterCommit(org.neo4j.storageengine.api.txstate.ReadableTransactionState state, org.neo4j.kernel.api.KernelTransaction transaction, TransactionHandlerState handlerState)
		 public override void AfterCommit( ReadableTransactionState state, KernelTransaction transaction, TransactionHandlerState handlerState )
		 {
			  if ( handlerState == null )
			  {
					// As per beforeCommit, 'null' means no handlers were registered in time for this transaction to
					// observe them.
					return;
			  }

			  foreach ( HandlerAndState handlerAndState in handlerState.States )
			  {
					handlerAndState.Handler.afterCommit( handlerState.TxData, handlerAndState.StateConflict );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public void afterRollback(org.neo4j.storageengine.api.txstate.ReadableTransactionState state, org.neo4j.kernel.api.KernelTransaction transaction, TransactionHandlerState handlerState)
		 public override void AfterRollback( ReadableTransactionState state, KernelTransaction transaction, TransactionHandlerState handlerState )
		 {
			  if ( handlerState == null )
			  {
					// For legacy reasons, we don't call transaction handlers on implicit rollback.
					return;
			  }

			  foreach ( HandlerAndState handlerAndState in handlerState.States )
			  {
					handlerAndState.Handler.afterRollback( handlerState.TxData, handlerAndState.StateConflict );
			  }
		 }

		 public class HandlerAndState
		 {
			  internal readonly TransactionEventHandler Handler;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal object StateConflict;

			  public HandlerAndState<T1>( TransactionEventHandler<T1> handler )
			  {
					this.Handler = handler;
			  }

			  internal virtual object State
			  {
				  set
				  {
						this.StateConflict = value;
				  }
			  }
		 }

		 public class TransactionHandlerState : Neo4Net.Kernel.api.TransactionHook_Outcome
		 {
			  internal readonly TransactionData TxData;
			  internal readonly IList<HandlerAndState> States = new LinkedList<HandlerAndState>();
			  internal Exception Error;

			  public TransactionHandlerState( TransactionData txData )
			  {
					this.TxData = txData;
			  }

			  public virtual void Failed( Exception error )
			  {
					this.Error = error;
			  }

			  public virtual bool Successful
			  {
				  get
				  {
						return Error == null;
				  }
			  }

			  public override Exception Failure()
			  {
					return Error;
			  }

			  public virtual HandlerAndState Add<T1>( TransactionEventHandler<T1> handler )
			  {
					HandlerAndState result = new HandlerAndState( handler );
					States.Add( result );
					return result;
			  }
		 }

		 private static readonly TransactionData EMPTY_DATA = new TransactionDataAnonymousInnerClass();

		 private class TransactionDataAnonymousInnerClass : TransactionData
		 {

			 public IEnumerable<Node> createdNodes()
			 {
				  return Iterables.empty();
			 }

			 public IEnumerable<Node> deletedNodes()
			 {
				  return Iterables.empty();
			 }

			 public bool isDeleted( Node node )
			 {
				  return false;
			 }

			 public IEnumerable<PropertyEntry<Node>> assignedNodeProperties()
			 {
				  return Iterables.empty();
			 }

			 public IEnumerable<PropertyEntry<Node>> removedNodeProperties()
			 {
				  return Iterables.empty();
			 }

			 public IEnumerable<LabelEntry> assignedLabels()
			 {
				  return Iterables.empty();
			 }

			 public IEnumerable<LabelEntry> removedLabels()
			 {
				  return Iterables.empty();
			 }

			 public IEnumerable<Relationship> createdRelationships()
			 {
				  return Iterables.empty();
			 }

			 public IEnumerable<Relationship> deletedRelationships()
			 {
				  return Iterables.empty();
			 }

			 public bool isDeleted( Relationship relationship )
			 {
				  return false;
			 }

			 public string username()
			 {
				  return "";
			 }

			 public IDictionary<string, object> metaData()
			 {
				  return Collections.emptyMap();
			 }

			 public IEnumerable<PropertyEntry<Relationship>> assignedRelationshipProperties()
			 {
				  return Iterables.empty();
			 }

			 public IEnumerable<PropertyEntry<Relationship>> removedRelationshipProperties()
			 {
				  return Iterables.empty();
			 }
		 }
	}

}