using System;

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
namespace Neo4Net.Kernel.Impl.@event
{
	using TransactionData = Neo4Net.GraphDb.Events.TransactionData;
	using Neo4Net.GraphDb.Events;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;

	public class VerifyingTransactionEventHandler : TransactionEventHandler<object>
	{
		 private readonly ExpectedTransactionData _expectedData;
		 private bool _hasBeenCalled;
		 private Exception _failure;

		 public VerifyingTransactionEventHandler( ExpectedTransactionData expectedData )
		 {
			  this._expectedData = expectedData;
		 }

		 public override void AfterCommit( TransactionData data, object state )
		 {
			  Verify( data );
		 }

		 public override void AfterRollback( TransactionData data, object state )
		 {
		 }

		 public override object BeforeCommit( TransactionData data )
		 {
			  return Verify( data );
		 }

		 private object Verify( TransactionData data )
		 {
			  // TODO Hmm, makes me think... should we really call transaction event handlers
			  // for these relationship type / property index transactions?
			  if ( Iterables.count( data.CreatedNodes() ) == 0 )
			  {
					return null;
			  }

			  try
			  {
					this._expectedData.compareTo( data );
					this._hasBeenCalled = true;
					return null;
			  }
			  catch ( Exception e )
			  {
					_failure = e;
					throw e;
			  }
		 }

		 internal virtual bool HasBeenCalled()
		 {
			  return this._hasBeenCalled;
		 }

		 internal virtual Exception Failure()
		 {
			  return this._failure;
		 }
	}

}