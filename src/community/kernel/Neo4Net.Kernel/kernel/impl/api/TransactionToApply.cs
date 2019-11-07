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
namespace Neo4Net.Kernel.Impl.Api
{

	using Neo4Net.Collections.Helpers;
	using EmptyVersionContext = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContext;
	using VersionContext = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContext;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using Commitment = Neo4Net.Kernel.impl.transaction.log.Commitment;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using HexPrinter = Neo4Net.Kernel.impl.util.HexPrinter;
	using CommandsToApply = Neo4Net.Kernel.Api.StorageEngine.CommandsToApply;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;
	using TransactionApplicationMode = Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Format.date;

	/// <summary>
	/// A chain of transactions to apply. Transactions form a linked list, each pointing to the <seealso cref="next()"/>
	/// or {@code null}. This design chosen for less garbage and convenience, i.e. that we pass in a number of transactions
	/// while also expecting some results for each and every one of those transactions back. The results are
	/// written directly into each instance instead of creating another data structure which is then returned.
	/// This is an internal class so even if it mixes arguments with results it's easier to work with,
	/// requires less code... and less objects.
	/// 
	/// State and methods are divided up into two parts, one part being the responsibility of the user to manage,
	/// the other part up to the commit process to manage.
	/// 
	/// The access pattern looks like:
	/// <ol>
	/// <li>=== USER ===</li>
	/// <li>Construct instances</li>
	/// <li>Form the linked list using <seealso cref="next(TransactionToApply)"/></li>
	/// <li>Pass into <seealso cref="TransactionCommitProcess.commit(TransactionToApply, CommitEvent, TransactionApplicationMode)"/></li>
	/// <li>=== COMMIT PROCESS ===</li>
	/// <li>Commit, where <seealso cref="commitment(Commitment, long)"/> is called to store the <seealso cref="Commitment"/> and transaction id</li>
	/// <li>Apply, where <seealso cref="commitment()"/>,
	/// <seealso cref="transactionRepresentation()"/> and <seealso cref="next()"/> are called</li>
	/// <li>=== USER ===</li>
	/// <li>Data about the commit can now also be accessed using f.ex <seealso cref="commitment()"/> or <seealso cref="transactionId()"/></li>
	/// </ol>
	/// </summary>
	public class TransactionToApply : CommandsToApply, IDisposable
	{
		 public const long TRANSACTION_ID_NOT_SPECIFIED = 0;

		 // These fields are provided by user
		 private readonly TransactionRepresentation _transactionRepresentation;
		 private long _transactionId;
		 private readonly VersionContext _versionContext;
		 private TransactionToApply _nextTransactionInBatch;

		 // These fields are provided by commit process, storage engine, or recovery process
		 private Commitment _commitment;
		 private System.Action<long> _closedCallback;
		 private LogPosition _logPosition;

		 /// <summary>
		 /// Used when committing a transaction that hasn't already gotten a transaction id assigned.
		 /// </summary>
		 public TransactionToApply( TransactionRepresentation transactionRepresentation ) : this( transactionRepresentation, EmptyVersionContext.EMPTY )
		 {
		 }

		 /// <summary>
		 /// Used when committing a transaction that hasn't already gotten a transaction id assigned.
		 /// </summary>
		 public TransactionToApply( TransactionRepresentation transactionRepresentation, VersionContext versionContext ) : this( transactionRepresentation, TRANSACTION_ID_NOT_SPECIFIED, versionContext )
		 {
		 }

		 public TransactionToApply( TransactionRepresentation transactionRepresentation, long transactionId ) : this( transactionRepresentation, transactionId, EmptyVersionContext.EMPTY )
		 {
		 }

		 public TransactionToApply( TransactionRepresentation transactionRepresentation, long transactionId, VersionContext versionContext )
		 {
			  this._transactionRepresentation = transactionRepresentation;
			  this._transactionId = transactionId;
			  this._versionContext = versionContext;
		 }

		 // These methods are called by the user when building a batch
		 public virtual void Next( TransactionToApply next )
		 {
			  _nextTransactionInBatch = next;
		 }

		 // These methods are called by the commit process
		 public virtual Commitment Commitment()
		 {
			  return _commitment;
		 }

		 public override long TransactionId()
		 {
			  return _transactionId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean accept(Neo4Net.helpers.collection.Visitor<Neo4Net.Kernel.Api.StorageEngine.StorageCommand,java.io.IOException> visitor) throws java.io.IOException
		 public override bool Accept( Visitor<StorageCommand, IOException> visitor )
		 {
			  return _transactionRepresentation.accept( visitor );
		 }

		 public virtual TransactionRepresentation TransactionRepresentation()
		 {
			  return _transactionRepresentation;
		 }

		 public override bool RequiresApplicationOrdering()
		 {
			  return _commitment.hasExplicitIndexChanges();
		 }

		 public virtual void Commitment( Commitment commitment, long transactionId )
		 {
			  this._commitment = commitment;
			  this._transactionId = transactionId;
			  this._versionContext.initWrite( transactionId );
		 }

		 public virtual void LogPosition( LogPosition position )
		 {
			  this._logPosition = position;
		 }

		 public override TransactionToApply Next()
		 {
			  return _nextTransactionInBatch;
		 }

		 public virtual void OnClose( System.Action<long> closedCallback )
		 {
			  this._closedCallback = closedCallback;
		 }

		 public override void Close()
		 {
			  if ( _closedCallback != null )
			  {
					_closedCallback.accept( _transactionId );
			  }
		 }

		 public override string ToString()
		 {
			  TransactionRepresentation tr = this._transactionRepresentation;
			  return "Transaction #" + _transactionId + ( _logPosition != null ? " at log position " + _logPosition : " (no log position)" ) +
						" {started " + date( tr.TimeStarted ) +
						", committed " + date( tr.TimeCommitted ) +
						", with " + CountCommands() + " commands in this transaction" +
						", authored by " + tr.AuthorId +
						", with master id " + tr.MasterId +
						", lock session " + tr.LockSessionId +
						", latest committed transaction id when started was " + tr.LatestCommittedTxWhenStarted +
						", additional header bytes: " + HexPrinter.hex( tr.AdditionalHeader(), int.MaxValue, "" ) + "}";
		 }

		 private string CountCommands()
		 {
//JAVA TO C# CONVERTER TODO TASK: Local classes are not converted by Java to C# Converter:
//			  class Counter implements Neo4Net.helpers.collection.Visitor<Neo4Net.Kernel.Api.StorageEngine.StorageCommand, java.io.IOException>
	//		  {
	//				private int count;
	//
	//				@@Override public boolean visit(StorageCommand element)
	//				{
	//					 count++;
	//					 return false;
	//				}
	//		  }
			  try
			  {
					Counter counter = new Counter();
					Accept( counter );
					return counter.count.ToString();
			  }
			  catch ( Exception e )
			  {
					return "(unable to count: " + e.Message + ")";
			  }
		 }

		 public override IEnumerator<StorageCommand> Iterator()
		 {
			  return _transactionRepresentation.GetEnumerator();
		 }
	}

}