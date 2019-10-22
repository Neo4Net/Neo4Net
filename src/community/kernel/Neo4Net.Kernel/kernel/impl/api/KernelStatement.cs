using System;
using System.Collections.Generic;
using System.Threading;

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
	using StringUtils = org.apache.commons.lang3.StringUtils;


	using NotInTransactionException = Neo4Net.GraphDb.NotInTransactionException;
	using TransactionTerminatedException = Neo4Net.GraphDb.TransactionTerminatedException;
	using AccessMode = Neo4Net.Internal.Kernel.Api.security.AccessMode;
	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;
	using VersionContext = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContext;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using AssertOpen = Neo4Net.Kernel.api.AssertOpen;
	using QueryRegistryOperations = Neo4Net.Kernel.api.QueryRegistryOperations;
	using Statement = Neo4Net.Kernel.api.Statement;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using ExplicitIndexTransactionState = Neo4Net.Kernel.api.txstate.ExplicitIndexTransactionState;
	using TransactionState = Neo4Net.Kernel.api.txstate.TransactionState;
	using TxStateHolder = Neo4Net.Kernel.api.txstate.TxStateHolder;
	using AuxiliaryTransactionState = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionState;
	using StatementLocks = Neo4Net.Kernel.impl.locking.StatementLocks;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.util.FeatureToggles.flag;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.util.FeatureToggles.toggle;

	/// <summary>
	/// A resource efficient implementation of <seealso cref="Statement"/>. Designed to be reused within a
	/// <seealso cref="KernelTransactionImplementation"/> instance, even across transactions since this instances itself
	/// doesn't hold essential state. Usage:
	/// 
	/// <ol>
	/// <li>Construct <seealso cref="KernelStatement"/> when <seealso cref="KernelTransactionImplementation"/> is constructed</li>
	/// <li>For every transaction...</li>
	/// <li>Call <seealso cref="initialize(StatementLocks, PageCursorTracer)"/> which makes this instance
	/// full available and ready to use. Call when the <seealso cref="KernelTransactionImplementation"/> is initialized.</li>
	/// <li>Alternate <seealso cref="acquire()"/> / <seealso cref="close()"/> when acquiring / closing a statement for the transaction...
	/// Temporarily asymmetric number of calls to <seealso cref="acquire()"/> / <seealso cref="close()"/> is supported, although in
	/// the end an equal number of calls must have been issued.</li>
	/// <li>To be safe call <seealso cref="forceClose()"/> at the end of a transaction to force a close of the statement,
	/// even if there are more than one current call to <seealso cref="acquire()"/>. This instance is now again ready
	/// to be <seealso cref="initialize(StatementLocks, PageCursorTracer)"/>  initialized} and used for the transaction
	/// instance again, when it's initialized.</li>
	/// </ol>
	/// </summary>
	public class KernelStatement : CloseableResourceManager, TxStateHolder, Statement, AssertOpen
	{
		 private static readonly bool _trackStatements = flag( typeof( KernelStatement ), "trackStatements", false );
		 private static readonly bool _recordStatementsTraces = flag( typeof( KernelStatement ), "recordStatementsTraces", false );
		 private const int STATEMENT_TRACK_HISTORY_MAX_SIZE = 100;
		 private static readonly Deque<StackTraceElement[]> _emptyStatementHistory = new LinkedList<StackTraceElement[]>( 0 );

		 private readonly TxStateHolder _txStateHolder;
		 private readonly StorageReader _storageReader;
		 private readonly KernelTransactionImplementation _transaction;
		 private readonly OperationsFacade _facade;
		 private StatementLocks _statementLocks;
		 private PageCursorTracer _pageCursorTracer = PageCursorTracer.NULL;
		 private int _referenceCount;
		 private volatile ExecutingQueryList _executingQueryList;
		 private readonly LockTracer _systemLockTracer;
		 private readonly Deque<StackTraceElement[]> _statementOpenCloseCalls;
		 private readonly ClockContext _clockContext;
		 private readonly VersionContextSupplier _versionContextSupplier;

		 public KernelStatement( KernelTransactionImplementation transaction, TxStateHolder txStateHolder, StorageReader storageReader, LockTracer systemLockTracer, StatementOperationParts statementOperations, ClockContext clockContext, VersionContextSupplier versionContextSupplier )
		 {
			  this._transaction = transaction;
			  this._txStateHolder = txStateHolder;
			  this._storageReader = storageReader;
			  this._facade = new OperationsFacade( this, statementOperations );
			  this._executingQueryList = ExecutingQueryList.EMPTY;
			  this._systemLockTracer = systemLockTracer;
			  this._statementOpenCloseCalls = _recordStatementsTraces ? new LinkedList<StackTraceElement[]>() : _emptyStatementHistory;
			  this._clockContext = clockContext;
			  this._versionContextSupplier = versionContextSupplier;
		 }

		 public override QueryRegistryOperations QueryRegistration()
		 {
			  return _facade;
		 }

		 public override TransactionState TxState()
		 {
			  return _txStateHolder.txState();
		 }

		 public override AuxiliaryTransactionState AuxiliaryTxState( object providerIdentityKey )
		 {
			  return _txStateHolder.auxiliaryTxState( providerIdentityKey );
		 }

		 public override ExplicitIndexTransactionState ExplicitIndexTxState()
		 {
			  return _txStateHolder.explicitIndexTxState();
		 }

		 public override bool HasTxStateWithChanges()
		 {
			  return _txStateHolder.hasTxStateWithChanges();
		 }

		 public override void Close()
		 {
			  // Check referenceCount > 0 since we allow multiple close calls,
			  // i.e. ignore closing already closed statements
			  if ( _referenceCount > 0 && ( --_referenceCount == 0 ) )
			  {
					CleanupResources();
			  }
			  RecordOpenCloseMethods();
		 }

		 public override void AssertOpen()
		 {
			  if ( _referenceCount == 0 )
			  {
					throw new NotInTransactionException( "The statement has been closed." );
			  }

			  Optional<Status> terminationReason = _transaction.ReasonIfTerminated;
			  terminationReason.ifPresent(status =>
			  {
				throw new TransactionTerminatedException( status );
			  });
		 }

		 public virtual void Initialize( StatementLocks statementLocks, PageCursorTracer pageCursorCounters )
		 {
			  this._statementLocks = statementLocks;
			  this._pageCursorTracer = pageCursorCounters;
			  this._clockContext.initializeTransaction();
		 }

		 public virtual StatementLocks Locks()
		 {
			  return _statementLocks;
		 }

		 public virtual LockTracer LockTracer()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  LockTracer tracer = _executingQueryList.top( ExecutingQuery::lockTracer );
			  return tracer == null ? _systemLockTracer : _systemLockTracer.combine( tracer );
		 }

		 public virtual PageCursorTracer PageCursorTracer
		 {
			 get
			 {
				  return _pageCursorTracer;
			 }
		 }

		 public void Acquire()
		 {
			  if ( _referenceCount++ == 0 )
			  {
					_storageReader.acquire();
					_clockContext.initializeStatement();
			  }
			  RecordOpenCloseMethods();
		 }

		 internal bool Acquired
		 {
			 get
			 {
				  return _referenceCount > 0;
			 }
		 }

		 internal void ForceClose()
		 {
			  if ( _referenceCount > 0 )
			  {
					int leakedStatements = _referenceCount;
					_referenceCount = 0;
					CleanupResources();
					if ( _trackStatements && _transaction.Success )
					{
						 string message = GetStatementNotClosedMessage( leakedStatements );
						 throw new StatementNotClosedException( message, _statementOpenCloseCalls );
					}
			  }
			  _pageCursorTracer.reportEvents();
		 }

		 private string GetStatementNotClosedMessage( int leakedStatements )
		 {
			  string additionalInstruction = _recordStatementsTraces ? StringUtils.EMPTY : format( " To see statement open/close stack traces please pass '%s' to your JVM" + " or enable corresponding feature toggle.", toggle( typeof( KernelStatement ), "recordStatementsTraces", true ) );
			  return format( "Statements were not correctly closed. Number of leaked statements: %d.%s", leakedStatements, additionalInstruction );
		 }

		 internal string Username()
		 {
			  return _transaction.securityContext().subject().username();
		 }

		 internal ExecutingQueryList ExecutingQueryList()
		 {
			  return _executingQueryList;
		 }

		 internal void StartQueryExecution( ExecutingQuery query )
		 {
			  this._executingQueryList = _executingQueryList.push( query );
		 }

		 internal void StopQueryExecution( ExecutingQuery executingQuery )
		 {
			  this._executingQueryList = _executingQueryList.remove( executingQuery );
			  _transaction.getStatistics().addWaitingTime(executingQuery.ReportedWaitingTimeNanos());
		 }

		 private void CleanupResources()
		 {
			  // closing is done by KTI
			  _storageReader.release();
			  _executingQueryList = ExecutingQueryList.EMPTY;
			  CloseAllCloseableResources();
		 }

		 public virtual KernelTransactionImplementation Transaction
		 {
			 get
			 {
				  return _transaction;
			 }
		 }

		 public virtual VersionContext VersionContext
		 {
			 get
			 {
				  return _versionContextSupplier.VersionContext;
			 }
		 }

		 internal virtual void AssertAllows( System.Func<AccessMode, bool> allows, string mode )
		 {
			_transaction.assertAllows( allows, mode );
		 }

		 private void RecordOpenCloseMethods()
		 {
			  if ( _recordStatementsTraces )
			  {
					if ( _statementOpenCloseCalls.size() > STATEMENT_TRACK_HISTORY_MAX_SIZE )
					{
						 _statementOpenCloseCalls.pop();
					}
					StackTraceElement[] stackTrace = Thread.CurrentThread.StackTrace;
					_statementOpenCloseCalls.add( Arrays.copyOfRange( stackTrace, 2, stackTrace.Length ) );
			  }
		 }

		 public virtual ClockContext Clocks()
		 {
			  return _clockContext;
		 }

		 internal class StatementNotClosedException : System.InvalidOperationException
		 {

			  internal StatementNotClosedException( string s, Deque<StackTraceElement[]> openCloseTraces ) : base( s )
			  {
					this.addSuppressed( new StatementTraceException( BuildMessage( openCloseTraces ) ) );
			  }

			  internal static string BuildMessage( Deque<StackTraceElement[]> openCloseTraces )
			  {
					if ( openCloseTraces.Empty )
					{
						 return StringUtils.EMPTY;
					}
					int separatorLength = 80;
					string paddingString = "=";

					MemoryStream @out = new MemoryStream();
					PrintStream printStream = new PrintStream( @out );
					printStream.println();
					printStream.println( "Last " + STATEMENT_TRACK_HISTORY_MAX_SIZE + " statements open/close stack traces are:" );
					int element = 0;
					foreach ( StackTraceElement[] traceElements in openCloseTraces )
					{
						 printStream.println( StringUtils.center( "*StackTrace " + element + "*", separatorLength, paddingString ) );
						 foreach ( StackTraceElement traceElement in traceElements )
						 {
							  printStream.println( "\tat " + traceElement );
						 }
						 printStream.println( StringUtils.center( "", separatorLength, paddingString ) );
						 printStream.println();
						 element++;
					}
					printStream.println( "All statement open/close stack traces printed." );
					return @out.ToString();
			  }

			  private class StatementTraceException : Exception
			  {
					internal StatementTraceException( string message ) : base( message )
					{
					}

					public override Exception FillInStackTrace()
					{
						lock ( this )
						{
							 return this;
						}
					}
			  }
		 }
	}

}