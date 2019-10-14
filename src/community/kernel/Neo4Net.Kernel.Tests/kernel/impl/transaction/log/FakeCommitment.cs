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
namespace Neo4Net.Kernel.impl.transaction.log
{
	public class FakeCommitment : Commitment
	{
		 public const int CHECKSUM = 3;
		 public const long TIMESTAMP = 8194639457389L;
		 private readonly long _id;
		 private readonly TransactionIdStore _transactionIdStore;
		 private bool _committed;
		 private bool _hasExplicitIndexChanges;

		 public FakeCommitment( long id, TransactionIdStore transactionIdStore ) : this( id, transactionIdStore, false )
		 {
		 }

		 public FakeCommitment( long id, TransactionIdStore transactionIdStore, bool markedAsCommitted )
		 {
			  this._id = id;
			  this._transactionIdStore = transactionIdStore;
			  this._committed = markedAsCommitted;
		 }

		 public override void PublishAsCommitted()
		 {
			  _committed = true;
			  _transactionIdStore.transactionCommitted( _id, CHECKSUM, TIMESTAMP );
		 }

		 public override void PublishAsClosed()
		 {
			  _transactionIdStore.transactionClosed( _id, 1, 2 );
		 }

		 public override bool MarkedAsCommitted()
		 {
			  return _committed;
		 }

		 public virtual bool HasExplicitIndexChanges
		 {
			 set
			 {
				  this._hasExplicitIndexChanges = value;
			 }
		 }

		 public override bool HasExplicitIndexChanges()
		 {
			  return _hasExplicitIndexChanges;
		 }
	}

}