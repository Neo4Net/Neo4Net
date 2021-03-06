﻿using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.Impl.Api
{

	using Org.Neo4j.Helpers.Collection;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Org.Neo4j.Kernel.api;
	using TransactionHook_Outcome = Org.Neo4j.Kernel.api.TransactionHook_Outcome;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using ReadableTransactionState = Org.Neo4j.Storageengine.Api.txstate.ReadableTransactionState;

	public class TransactionHooks
	{
		 protected internal readonly ISet<TransactionHook> Hooks = new CopyOnWriteArraySet<TransactionHook>();

		 public virtual void Register( TransactionHook hook )
		 {
			  Hooks.Add( hook );
		 }

		 public virtual void Unregister( TransactionHook hook )
		 {
			  Hooks.remove( hook );
		 }

		 public virtual TransactionHooksState BeforeCommit( ReadableTransactionState state, KernelTransaction tx, StorageReader storageReader )
		 {
			  if ( Hooks.Count == 0 )
			  {
					return null;
			  }

			  TransactionHooksState hookState = new TransactionHooksState();
			  foreach ( TransactionHook hook in Hooks )
			  {
					TransactionHook_Outcome outcome = hook.beforeCommit( state, tx, storageReader );
					hookState.Add( hook, outcome );
			  }
			  return hookState;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public void afterCommit(org.neo4j.storageengine.api.txstate.ReadableTransactionState state, org.neo4j.kernel.api.KernelTransaction tx, TransactionHooksState hooksState)
		 public virtual void AfterCommit( ReadableTransactionState state, KernelTransaction tx, TransactionHooksState hooksState )
		 {
			  if ( hooksState == null )
			  {
					return;
			  }
			  foreach ( Pair<TransactionHook, TransactionHook_Outcome> hookAndOutcome in hooksState.HooksWithOutcome() )
			  {
					TransactionHook hook = hookAndOutcome.First();
					TransactionHook_Outcome outcome = hookAndOutcome.Other();
					hook.afterCommit( state, tx, outcome );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public void afterRollback(org.neo4j.storageengine.api.txstate.ReadableTransactionState state, org.neo4j.kernel.api.KernelTransaction tx, TransactionHooksState hooksState)
		 public virtual void AfterRollback( ReadableTransactionState state, KernelTransaction tx, TransactionHooksState hooksState )
		 {
			  if ( hooksState == null )
			  {
					return;
			  }
			  foreach ( Pair<TransactionHook, TransactionHook_Outcome> hookAndOutcome in hooksState.HooksWithOutcome() )
			  {
					hookAndOutcome.First().afterRollback(state, tx, hookAndOutcome.Other());
			  }
		 }

		 public class TransactionHooksState
		 {
			  internal readonly IList<Pair<TransactionHook, TransactionHook_Outcome>> HooksWithAttachment = new List<Pair<TransactionHook, TransactionHook_Outcome>>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal Exception FailureConflict;

			  public virtual void Add( TransactionHook hook, TransactionHook_Outcome outcome )
			  {
					HooksWithAttachment.Add( Pair.of( hook, outcome ) );
					if ( outcome != null && !outcome.Successful )
					{
						 FailureConflict = outcome.Failure();
					}
			  }

			  internal virtual IEnumerable<Pair<TransactionHook, TransactionHook_Outcome>> HooksWithOutcome()
			  {
					return HooksWithAttachment;
			  }

			  public virtual bool Failed()
			  {
					return FailureConflict != null;
			  }

			  public virtual Exception Failure()
			  {
					return FailureConflict;
			  }
		 }
	}

}