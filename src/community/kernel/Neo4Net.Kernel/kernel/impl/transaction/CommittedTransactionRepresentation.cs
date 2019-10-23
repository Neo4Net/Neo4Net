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
namespace Neo4Net.Kernel.impl.transaction
{

	using Neo4Net.Helpers.Collections;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using LogEntryStart = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryStart;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;

	/// <summary>
	/// This class represents the concept of a TransactionRepresentation that has been
	/// committed to the TransactionStore. It contains, in addition to the TransactionRepresentation
	/// itself, a Start and Commit entry. This is the thing that <seealso cref="LogicalTransactionStore"/> returns when
	/// asked for a transaction via a cursor.
	/// </summary>
	public class CommittedTransactionRepresentation
	{
		 private readonly LogEntryStart _startEntry;
		 private readonly TransactionRepresentation _transactionRepresentation;
		 private readonly LogEntryCommit _commitEntry;

		 public CommittedTransactionRepresentation( LogEntryStart startEntry, TransactionRepresentation transactionRepresentation, LogEntryCommit commitEntry )
		 {
			  this._startEntry = startEntry;
			  this._transactionRepresentation = transactionRepresentation;
			  this._commitEntry = commitEntry;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void accept(org.Neo4Net.helpers.collection.Visitor<org.Neo4Net.Kernel.Api.StorageEngine.StorageCommand, java.io.IOException> visitor) throws java.io.IOException
		 public virtual void Accept( Visitor<StorageCommand, IOException> visitor )
		 {
			  _transactionRepresentation.accept( visitor );
		 }

		 public virtual LogEntryStart StartEntry
		 {
			 get
			 {
				  return _startEntry;
			 }
		 }

		 public virtual TransactionRepresentation TransactionRepresentation
		 {
			 get
			 {
				  return _transactionRepresentation;
			 }
		 }

		 public virtual LogEntryCommit CommitEntry
		 {
			 get
			 {
				  return _commitEntry;
			 }
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name +
						 "[" + _startEntry + ", " + _transactionRepresentation + ", " + _commitEntry + "]";
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  CommittedTransactionRepresentation that = ( CommittedTransactionRepresentation ) o;

			  return _commitEntry.Equals( that._commitEntry ) && _startEntry.Equals( that._startEntry ) && _transactionRepresentation.Equals( that._transactionRepresentation );
		 }

		 public override int GetHashCode()
		 {
			  int result = _startEntry.GetHashCode();
			  result = 31 * result + _transactionRepresentation.GetHashCode();
			  result = 31 * result + _commitEntry.GetHashCode();
			  return result;
		 }
	}

}