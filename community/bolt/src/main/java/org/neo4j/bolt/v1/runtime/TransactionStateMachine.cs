using System;
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
namespace Org.Neo4j.Bolt.v1.runtime
{

	using BoltResult = Org.Neo4j.Bolt.runtime.BoltResult;
	using BoltResultHandle = Org.Neo4j.Bolt.runtime.BoltResultHandle;
	using StatementMetadata = Org.Neo4j.Bolt.runtime.StatementMetadata;
	using StatementProcessor = Org.Neo4j.Bolt.runtime.StatementProcessor;
	using TransactionStateMachineSPI = Org.Neo4j.Bolt.runtime.TransactionStateMachineSPI;
	using AuthenticationResult = Org.Neo4j.Bolt.security.auth.AuthenticationResult;
	using Bookmark = Org.Neo4j.Bolt.v1.runtime.bookmarking.Bookmark;
	using BookmarkResult = Org.Neo4j.Bolt.v1.runtime.spi.BookmarkResult;
	using InvalidSemanticsException = Org.Neo4j.Cypher.InvalidSemanticsException;
	using Org.Neo4j.Function;
	using TransactionTerminatedException = Org.Neo4j.Graphdb.TransactionTerminatedException;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using QueryExecutionKernelException = Org.Neo4j.Kernel.impl.query.QueryExecutionKernelException;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkState;

	public class TransactionStateMachine : StatementProcessor
	{
		 internal readonly TransactionStateMachineSPI Spi;
		 internal readonly MutableTransactionState Ctx;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal State StateConflict = State.AutoCommit;

		 internal TransactionStateMachine( TransactionStateMachineSPI spi, AuthenticationResult authenticationResult, Clock clock )
		 {
			  this.Spi = spi;
			  Ctx = new MutableTransactionState( authenticationResult, clock );
		 }

		 public virtual State State()
		 {
			  return StateConflict;
		 }

		 private void Before()
		 {
			  if ( Ctx.currentTransaction != null )
			  {
					Spi.bindTransactionToCurrentThread( Ctx.currentTransaction );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void beginTransaction(org.neo4j.bolt.v1.runtime.bookmarking.Bookmark bookmark) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override void BeginTransaction( Bookmark bookmark )
		 {
			  BeginTransaction( bookmark, null, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void beginTransaction(org.neo4j.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String,Object> txMetadata) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override void BeginTransaction( Bookmark bookmark, Duration txTimeout, IDictionary<string, object> txMetadata )
		 {
			  Before();
			  try
			  {
					EnsureNoPendingTerminationNotice();

					StateConflict = StateConflict.beginTransaction( Ctx, Spi, bookmark, txTimeout, txMetadata );
			  }
			  finally
			  {
					After();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.runtime.StatementMetadata run(String statement, org.neo4j.values.virtual.MapValue params) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override StatementMetadata Run( string statement, MapValue @params )
		 {
			  return Run( statement, @params, null, null, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.runtime.StatementMetadata run(String statement, org.neo4j.values.virtual.MapValue params, org.neo4j.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String,Object> txMetaData) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override StatementMetadata Run( string statement, MapValue @params, Bookmark bookmark, Duration txTimeout, IDictionary<string, object> txMetaData )
		 {
			  Before();
			  try
			  {
					EnsureNoPendingTerminationNotice();

					StateConflict = StateConflict.run( Ctx, Spi, statement, @params, bookmark, txTimeout, txMetaData );

					return Ctx.currentStatementMetadata;
			  }
			  finally
			  {
					After();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.v1.runtime.bookmarking.Bookmark streamResult(org.neo4j.function.ThrowingConsumer<org.neo4j.bolt.runtime.BoltResult, Exception> resultConsumer) throws Exception
		 public override Bookmark StreamResult( ThrowingConsumer<BoltResult, Exception> resultConsumer )
		 {
			  Before();
			  try
			  {
					EnsureNoPendingTerminationNotice();

					return StateConflict.streamResult( Ctx, Spi, resultConsumer );
			  }
			  finally
			  {
					After();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.v1.runtime.bookmarking.Bookmark commitTransaction() throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override Bookmark CommitTransaction()
		 {
			  Before();
			  try
			  {
					EnsureNoPendingTerminationNotice();

					StateConflict = StateConflict.commitTransaction( Ctx, Spi );
					return NewestBookmark( Spi );
			  }
			  catch ( TransactionFailureException ex )
			  {
					StateConflict = State.AutoCommit;
					throw ex;
			  }
			  finally
			  {
					After();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void rollbackTransaction() throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override void RollbackTransaction()
		 {
			  Before();
			  try
			  {
					EnsureNoPendingTerminationNotice();
					StateConflict = StateConflict.rollbackTransaction( Ctx, Spi );
			  }
			  finally
			  {
					After();
			  }
		 }

		 public override bool HasOpenStatement()
		 {
			  return Ctx.currentResultHandle != null;
		 }

		 /// <summary>
		 /// Rollback and close transaction. Move back to <seealso cref="State.AUTO_COMMIT"/>.
		 /// <para>
		 /// <b>Warning:</b>This method should only be called by the bolt worker thread during it's regular message
		 /// processing. It is wrong to call it from a different thread because kernel transactions are not thread-safe.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <exception cref="TransactionFailureException"> when transaction fails to close. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void reset() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 public override void Reset()
		 {
			  StateConflict.terminateQueryAndRollbackTransaction( Ctx );
			  StateConflict = State.AutoCommit;
		 }

		 private void After()
		 {
			  Spi.unbindTransactionFromCurrentThread();
		 }

		 public override void MarkCurrentTransactionForTermination()
		 {
			  KernelTransaction tx = Ctx.currentTransaction;
			  if ( tx != null )
			  {
					tx.MarkForTermination( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.Terminated );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateTransaction() throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override void ValidateTransaction()
		 {
			  KernelTransaction tx = Ctx.currentTransaction;

			  if ( tx != null )
			  {
					Optional<Status> statusOpt = tx.ReasonIfTerminated;

					if ( statusOpt.Present )
					{
						 if ( statusOpt.get().code().classification().rollbackTransaction() )
						 {
							  Ctx.pendingTerminationNotice = statusOpt.get();

							  Reset();
						 }
					}
			  }
		 }

		 private void EnsureNoPendingTerminationNotice()
		 {
			  if ( Ctx.pendingTerminationNotice != null )
			  {
					Status status = Ctx.pendingTerminationNotice;

					Ctx.pendingTerminationNotice = null;

					throw new TransactionTerminatedException( status );
			  }
		 }

		 public override bool HasTransaction()
		 {
			  return StateConflict == State.ExplicitTransaction;
		 }

		 internal abstract class State
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           AUTO_COMMIT { State beginTransaction(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi, org.neo4j.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String, Object> txMetadata) throws org.neo4j.internal.kernel.api.exceptions.KernelException { waitForBookmark(spi, bookmark); ctx.currentResult = org.neo4j.bolt.runtime.BoltResult.EMPTY; ctx.currentTransaction = spi.beginTransaction(ctx.loginContext, txTimeout, txMetadata); return EXPLICIT_TRANSACTION; } State run(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi, String statement, org.neo4j.values.virtual.MapValue params, org.neo4j.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String, Object> txMetadata) throws org.neo4j.internal.kernel.api.exceptions.KernelException { statement = parseStatement(ctx, statement); waitForBookmark(spi, bookmark); execute(ctx, spi, statement, params, spi.isPeriodicCommit(statement), txTimeout, txMetadata); return AUTO_COMMIT; } private String parseStatement(MutableTransactionState ctx, String statement) { if(statement.isEmpty()) { statement = ctx.lastStatement; } else { ctx.lastStatement = statement; } return statement; } void execute(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi, String statement, org.neo4j.values.virtual.MapValue params, boolean isPeriodicCommit, java.time.Duration txTimeout, java.util.Map<String, Object> txMetadata) throws org.neo4j.internal.kernel.api.exceptions.KernelException { if(!isPeriodicCommit) { ctx.currentTransaction = spi.beginTransaction(ctx.loginContext, txTimeout, txMetadata); } boolean failed = true; try { org.neo4j.bolt.runtime.BoltResultHandle resultHandle = spi.executeQuery(ctx.loginContext, statement, params, txTimeout, txMetadata); startExecution(ctx, resultHandle); failed = false; } finally { if(!isPeriodicCommit) { if(failed) { closeTransaction(ctx, false); } } else { ctx.currentTransaction = spi.beginTransaction(ctx.loginContext, txTimeout, txMetadata); } } } Bookmark streamResult(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi, org.neo4j.function.ThrowingConsumer<org.neo4j.bolt.runtime.BoltResult, Exception> resultConsumer) throws Exception { assert ctx.currentResult != null; try { consumeResult(ctx, resultConsumer); closeTransaction(ctx, true); return newestBookmark(spi); } finally { closeTransaction(ctx, false); } } State commitTransaction(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi) throws org.neo4j.internal.kernel.api.exceptions.KernelException { throw new org.neo4j.kernel.impl.query.QueryExecutionKernelException(new org.neo4j.cypher.InvalidSemanticsException("No current transaction to commit.")); } State rollbackTransaction(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi) { ctx.currentResult = org.neo4j.bolt.runtime.BoltResult.EMPTY; return AUTO_COMMIT; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           EXPLICIT_TRANSACTION { State beginTransaction(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi, org.neo4j.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String, Object> txMetadata) throws org.neo4j.internal.kernel.api.exceptions.KernelException { throw new org.neo4j.kernel.impl.query.QueryExecutionKernelException(new org.neo4j.cypher.InvalidSemanticsException("Nested transactions are not supported.")); } State run(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi, String statement, org.neo4j.values.virtual.MapValue params, org.neo4j.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration ignored1, java.util.Map<String, Object> ignored2) throws org.neo4j.internal.kernel.api.exceptions.KernelException { checkState(ignored1 == null, "Explicit Transaction should not run with tx_timeout"); checkState(ignored2 == null, "Explicit Transaction should not run with tx_metadata"); if(statement.isEmpty()) { statement = ctx.lastStatement; } else { ctx.lastStatement = statement; } if(spi.isPeriodicCommit(statement)) { throw new org.neo4j.kernel.impl.query.QueryExecutionKernelException(new org.neo4j.cypher.InvalidSemanticsException("Executing queries that use periodic commit in an " + "open transaction is not possible.")); } else { org.neo4j.bolt.runtime.BoltResultHandle resultHandle = spi.executeQuery(ctx.loginContext, statement, params, null, null); startExecution(ctx, resultHandle); return EXPLICIT_TRANSACTION; } } Bookmark streamResult(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi, org.neo4j.function.ThrowingConsumer<org.neo4j.bolt.runtime.BoltResult, Exception> resultConsumer) throws Exception { assert ctx.currentResult != null; consumeResult(ctx, resultConsumer); return null; } State commitTransaction(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi) throws org.neo4j.internal.kernel.api.exceptions.KernelException { closeTransaction(ctx, true); org.neo4j.bolt.v1.runtime.bookmarking.Bookmark bookmark = newestBookmark(spi); ctx.currentResult = new org.neo4j.bolt.v1.runtime.spi.BookmarkResult(bookmark); return AUTO_COMMIT; } State rollbackTransaction(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi) throws org.neo4j.internal.kernel.api.exceptions.KernelException { closeTransaction(ctx, false); ctx.currentResult = org.neo4j.bolt.runtime.BoltResult.EMPTY; return AUTO_COMMIT; } };

			  private static readonly IList<State> valueList = new List<State>();

			  static State()
			  {
				  valueList.Add( AUTO_COMMIT );
				  valueList.Add( EXPLICIT_TRANSACTION );
			  }

			  public enum InnerEnum
			  {
				  AUTO_COMMIT,
				  EXPLICIT_TRANSACTION
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private State( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract State beginTransaction(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi, org.neo4j.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String,Object> txMetadata) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
			  internal abstract State beginTransaction( MutableTransactionState ctx, Org.Neo4j.Bolt.runtime.TransactionStateMachineSPI spi, Org.Neo4j.Bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, IDictionary<string, object> txMetadata );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract State run(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi, String statement, org.neo4j.values.virtual.MapValue params, org.neo4j.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String,Object> txMetadata) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
			  internal abstract State run( MutableTransactionState ctx, Org.Neo4j.Bolt.runtime.TransactionStateMachineSPI spi, string statement, Org.Neo4j.Values.@virtual.MapValue @params, Org.Neo4j.Bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, IDictionary<string, object> txMetadata );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract org.neo4j.bolt.v1.runtime.bookmarking.Bookmark streamResult(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi, org.neo4j.function.ThrowingConsumer<org.neo4j.bolt.runtime.BoltResult,Exception> resultConsumer) throws Exception;
			  internal abstract Org.Neo4j.Bolt.v1.runtime.bookmarking.Bookmark streamResult( MutableTransactionState ctx, Org.Neo4j.Bolt.runtime.TransactionStateMachineSPI spi, Org.Neo4j.Function.ThrowingConsumer<Org.Neo4j.Bolt.runtime.BoltResult, Exception> resultConsumer );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract State commitTransaction(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
			  internal abstract State commitTransaction( MutableTransactionState ctx, Org.Neo4j.Bolt.runtime.TransactionStateMachineSPI spi );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract State rollbackTransaction(MutableTransactionState ctx, org.neo4j.bolt.runtime.TransactionStateMachineSPI spi) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
			  internal abstract State rollbackTransaction( MutableTransactionState ctx, Org.Neo4j.Bolt.runtime.TransactionStateMachineSPI spi );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void terminateQueryAndRollbackTransaction(MutableTransactionState ctx) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
			  internal void TerminateQueryAndRollbackTransaction( MutableTransactionState ctx )
			  {
					if ( ctx.CurrentResultHandle != null )
					{
						 ctx.CurrentResultHandle.terminate();
						 ctx.CurrentResultHandle = null;
					}
					if ( ctx.CurrentResult != null )
					{
						 ctx.CurrentResult.close();
						 ctx.CurrentResult = null;
					}

				  CloseTransaction( ctx, false );
			  }

			  /*
			   * This is overly careful about always closing and nulling the transaction since
			   * reset can cause ctx.currentTransaction to be null we store in local variable.
			   */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void closeTransaction(MutableTransactionState ctx, boolean success) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
			  internal void CloseTransaction( MutableTransactionState ctx, bool success )
			  {
					KernelTransaction tx = ctx.CurrentTransaction;
					ctx.CurrentTransaction = null;
					if ( tx != null )
					{
						 try
						 {
							  if ( success )
							  {
									tx.Success();
							  }
							  else
							  {
									tx.Failure();
							  }
							  if ( tx.Open )
							  {
									tx.Close();
							  }
						 }
						 finally
						 {
							  ctx.CurrentTransaction = null;
						 }
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean consumeResult(MutableTransactionState ctx, org.neo4j.function.ThrowingConsumer<org.neo4j.bolt.runtime.BoltResult,Exception> resultConsumer) throws Exception
			  internal bool ConsumeResult( MutableTransactionState ctx, Org.Neo4j.Function.ThrowingConsumer<Org.Neo4j.Bolt.runtime.BoltResult, Exception> resultConsumer )
			  {
					bool success = false;
					try
					{
						 resultConsumer.Accept( ctx.CurrentResult );
						 success = true;
					}
					finally
					{
						 ctx.CurrentResult.close();
						 ctx.CurrentResult = null;

						 if ( ctx.CurrentResultHandle != null )
						 {
							  ctx.CurrentResultHandle.close( success );
							  ctx.CurrentResultHandle = null;
						 }
					}
					return success;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void startExecution(MutableTransactionState ctx, org.neo4j.bolt.runtime.BoltResultHandle resultHandle) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  internal void StartExecution( MutableTransactionState ctx, Org.Neo4j.Bolt.runtime.BoltResultHandle resultHandle )
			  {
					ctx.CurrentResultHandle = resultHandle;
					try
					{
						 ctx.CurrentResult = resultHandle.Start();
					}
					catch ( Exception t )
					{
						 ctx.CurrentResultHandle.close( false );
						 ctx.CurrentResultHandle = null;
						 throw t;
					}
			  }


			 public static IList<State> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static State valueOf( string name )
			 {
				 foreach ( State enumInstance in State.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void waitForBookmark(org.neo4j.bolt.runtime.TransactionStateMachineSPI spi, org.neo4j.bolt.v1.runtime.bookmarking.Bookmark bookmark) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private static void WaitForBookmark( TransactionStateMachineSPI spi, Bookmark bookmark )
		 {
			  if ( bookmark != null )
			  {
					spi.AwaitUpToDate( bookmark.TxId() );
			  }
		 }

		 private static Bookmark NewestBookmark( TransactionStateMachineSPI spi )
		 {
			  long txId = spi.NewestEncounteredTxId();
			  return new Bookmark( txId );
		 }

		 internal class MutableTransactionState
		 {
			  /// <summary>
			  /// The current session security context to be used for starting transactions </summary>
			  internal readonly LoginContext LoginContext;

			  /// <summary>
			  /// The current transaction, if present </summary>
			  internal KernelTransaction CurrentTransaction;

			  internal Status PendingTerminationNotice;

			  /// <summary>
			  /// Last Cypher statement executed </summary>
			  internal string LastStatement = "";

			  /// <summary>
			  /// The current pending result, if present </summary>
			  internal BoltResult CurrentResult;

			  internal BoltResultHandle CurrentResultHandle;

			  internal readonly Clock Clock;

			  /// <summary>
			  /// A re-usable statement metadata instance that always represents the currently running statement </summary>
			  internal readonly StatementMetadata currentStatementMetadata = new StatementMetadataAnonymousInnerClass();

			  private class StatementMetadataAnonymousInnerClass : StatementMetadata
			  {
				  public string[] fieldNames()
				  {
						return outerInstance.currentResult.fieldNames();
				  }
			  }

			  internal MutableTransactionState( AuthenticationResult authenticationResult, Clock clock )
			  {
					this.Clock = clock;
					this.LoginContext = authenticationResult.LoginContext;
			  }
		 }
	}

}