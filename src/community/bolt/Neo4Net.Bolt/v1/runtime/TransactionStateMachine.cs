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
namespace Neo4Net.Bolt.v1.runtime
{

	using BoltResult = Neo4Net.Bolt.runtime.BoltResult;
	using BoltResultHandle = Neo4Net.Bolt.runtime.BoltResultHandle;
	using StatementMetadata = Neo4Net.Bolt.runtime.StatementMetadata;
	using StatementProcessor = Neo4Net.Bolt.runtime.StatementProcessor;
	using TransactionStateMachineSPI = Neo4Net.Bolt.runtime.TransactionStateMachineSPI;
	using AuthenticationResult = Neo4Net.Bolt.security.auth.AuthenticationResult;
	using Bookmark = Neo4Net.Bolt.v1.runtime.bookmarking.Bookmark;
	using BookmarkResult = Neo4Net.Bolt.v1.runtime.spi.BookmarkResult;
	using InvalidSemanticsException = Neo4Net.Cypher.InvalidSemanticsException;
	using Neo4Net.Functions;
	using TransactionTerminatedException = Neo4Net.GraphDb.TransactionTerminatedException;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.Kernel.Api.Internal.security.LoginContext;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using QueryExecutionKernelException = Neo4Net.Kernel.impl.query.QueryExecutionKernelException;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.util.Preconditions.checkState;

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
//ORIGINAL LINE: public void BeginTransaction(Neo4Net.bolt.v1.runtime.bookmarking.Bookmark bookmark) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 public override void BeginTransaction( Bookmark bookmark )
		 {
			  BeginTransaction( bookmark, null, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void BeginTransaction(Neo4Net.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String,Object> txMetadata) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 public override void BeginTransaction( Bookmark bookmark, Duration txTimeout, IDictionary<string, object> txMetadata )
		 {
			  Before();
			  try
			  {
					EnsureNoPendingTerminationNotice();

					StateConflict = StateConflict.BeginTransaction( Ctx, Spi, bookmark, txTimeout, txMetadata );
			  }
			  finally
			  {
					After();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.bolt.runtime.StatementMetadata run(String statement, Neo4Net.values.virtual.MapValue params) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 public override StatementMetadata Run( string statement, MapValue @params )
		 {
			  return Run( statement, @params, null, null, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.bolt.runtime.StatementMetadata run(String statement, Neo4Net.values.virtual.MapValue params, Neo4Net.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String,Object> txMetaData) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
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
//ORIGINAL LINE: public Neo4Net.bolt.v1.runtime.bookmarking.Bookmark streamResult(Neo4Net.function.ThrowingConsumer<Neo4Net.bolt.runtime.BoltResult, Exception> resultConsumer) throws Exception
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
//ORIGINAL LINE: public Neo4Net.bolt.v1.runtime.bookmarking.Bookmark commitTransaction() throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
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
//ORIGINAL LINE: public void rollbackTransaction() throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
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
//ORIGINAL LINE: public void reset() throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
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
					tx.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validateTransaction() throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
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
//           AUTO_COMMIT { State BeginTransaction(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi, Neo4Net.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String, Object> txMetadata) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException { waitForBookmark(spi, bookmark); ctx.currentResult = Neo4Net.bolt.runtime.BoltResult.EMPTY; ctx.currentTransaction = spi.BeginTransaction(ctx.loginContext, txTimeout, txMetadata); return EXPLICIT_TRANSACTION; } State run(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi, String statement, Neo4Net.values.virtual.MapValue params, Neo4Net.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String, Object> txMetadata) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException { statement = parseStatement(ctx, statement); waitForBookmark(spi, bookmark); execute(ctx, spi, statement, params, spi.isPeriodicCommit(statement), txTimeout, txMetadata); return AUTO_COMMIT; } private String parseStatement(MutableTransactionState ctx, String statement) { if(statement.isEmpty()) { statement = ctx.lastStatement; } else { ctx.lastStatement = statement; } return statement; } void execute(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi, String statement, Neo4Net.values.virtual.MapValue params, boolean isPeriodicCommit, java.time.Duration txTimeout, java.util.Map<String, Object> txMetadata) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException { if(!isPeriodicCommit) { ctx.currentTransaction = spi.BeginTransaction(ctx.loginContext, txTimeout, txMetadata); } boolean failed = true; try { Neo4Net.bolt.runtime.BoltResultHandle resultHandle = spi.executeQuery(ctx.loginContext, statement, params, txTimeout, txMetadata); startExecution(ctx, resultHandle); failed = false; } finally { if(!isPeriodicCommit) { if(failed) { closeTransaction(ctx, false); } } else { ctx.currentTransaction = spi.BeginTransaction(ctx.loginContext, txTimeout, txMetadata); } } } Bookmark streamResult(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi, Neo4Net.function.ThrowingConsumer<Neo4Net.bolt.runtime.BoltResult, Exception> resultConsumer) throws Exception { assert ctx.currentResult != null; try { consumeResult(ctx, resultConsumer); closeTransaction(ctx, true); return newestBookmark(spi); } finally { closeTransaction(ctx, false); } } State commitTransaction(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException { throw new Neo4Net.kernel.impl.query.QueryExecutionKernelException(new Neo4Net.cypher.InvalidSemanticsException("No current transaction to commit.")); } State rollbackTransaction(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi) { ctx.currentResult = Neo4Net.bolt.runtime.BoltResult.EMPTY; return AUTO_COMMIT; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           EXPLICIT_TRANSACTION { State BeginTransaction(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi, Neo4Net.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String, Object> txMetadata) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException { throw new Neo4Net.kernel.impl.query.QueryExecutionKernelException(new Neo4Net.cypher.InvalidSemanticsException("Nested transactions are not supported.")); } State run(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi, String statement, Neo4Net.values.virtual.MapValue params, Neo4Net.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration ignored1, java.util.Map<String, Object> ignored2) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException { checkState(ignored1 == null, "Explicit Transaction should not run with tx_timeout"); checkState(ignored2 == null, "Explicit Transaction should not run with tx_metadata"); if(statement.isEmpty()) { statement = ctx.lastStatement; } else { ctx.lastStatement = statement; } if(spi.isPeriodicCommit(statement)) { throw new Neo4Net.kernel.impl.query.QueryExecutionKernelException(new Neo4Net.cypher.InvalidSemanticsException("Executing queries that use periodic commit in an " + "open transaction is not possible.")); } else { Neo4Net.bolt.runtime.BoltResultHandle resultHandle = spi.executeQuery(ctx.loginContext, statement, params, null, null); startExecution(ctx, resultHandle); return EXPLICIT_TRANSACTION; } } Bookmark streamResult(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi, Neo4Net.function.ThrowingConsumer<Neo4Net.bolt.runtime.BoltResult, Exception> resultConsumer) throws Exception { assert ctx.currentResult != null; consumeResult(ctx, resultConsumer); return null; } State commitTransaction(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException { closeTransaction(ctx, true); Neo4Net.bolt.v1.runtime.bookmarking.Bookmark bookmark = newestBookmark(spi); ctx.currentResult = new Neo4Net.bolt.v1.runtime.spi.BookmarkResult(bookmark); return AUTO_COMMIT; } State rollbackTransaction(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException { closeTransaction(ctx, false); ctx.currentResult = Neo4Net.bolt.runtime.BoltResult.EMPTY; return AUTO_COMMIT; } };

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
//ORIGINAL LINE: abstract State BeginTransaction(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi, Neo4Net.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String,Object> txMetadata) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
			  internal abstract State BeginTransaction( MutableTransactionState ctx, Neo4Net.Bolt.runtime.TransactionStateMachineSPI spi, Neo4Net.Bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, IDictionary<string, object> txMetadata );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract State run(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi, String statement, Neo4Net.values.virtual.MapValue params, Neo4Net.bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, java.util.Map<String,Object> txMetadata) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
			  internal abstract State run( MutableTransactionState ctx, Neo4Net.Bolt.runtime.TransactionStateMachineSPI spi, string statement, Neo4Net.Values.@virtual.MapValue @params, Neo4Net.Bolt.v1.runtime.bookmarking.Bookmark bookmark, java.time.Duration txTimeout, IDictionary<string, object> txMetadata );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract Neo4Net.bolt.v1.runtime.bookmarking.Bookmark streamResult(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi, Neo4Net.function.ThrowingConsumer<Neo4Net.bolt.runtime.BoltResult,Exception> resultConsumer) throws Exception;
			  internal abstract Neo4Net.Bolt.v1.runtime.bookmarking.Bookmark streamResult( MutableTransactionState ctx, Neo4Net.Bolt.runtime.TransactionStateMachineSPI spi, Neo4Net.Functions.ThrowingConsumer<Neo4Net.Bolt.runtime.BoltResult, Exception> resultConsumer );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract State commitTransaction(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
			  internal abstract State commitTransaction( MutableTransactionState ctx, Neo4Net.Bolt.runtime.TransactionStateMachineSPI spi );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract State rollbackTransaction(MutableTransactionState ctx, Neo4Net.bolt.runtime.TransactionStateMachineSPI spi) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
			  internal abstract State rollbackTransaction( MutableTransactionState ctx, Neo4Net.Bolt.runtime.TransactionStateMachineSPI spi );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void terminateQueryAndRollbackTransaction(MutableTransactionState ctx) throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
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
//ORIGINAL LINE: void closeTransaction(MutableTransactionState ctx, boolean success) throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
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
//ORIGINAL LINE: boolean consumeResult(MutableTransactionState ctx, Neo4Net.function.ThrowingConsumer<Neo4Net.bolt.runtime.BoltResult,Exception> resultConsumer) throws Exception
			  internal bool ConsumeResult( MutableTransactionState ctx, Neo4Net.Functions.ThrowingConsumer<Neo4Net.Bolt.runtime.BoltResult, Exception> resultConsumer )
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
//ORIGINAL LINE: void startExecution(MutableTransactionState ctx, Neo4Net.bolt.runtime.BoltResultHandle resultHandle) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
			  internal void StartExecution( MutableTransactionState ctx, Neo4Net.Bolt.runtime.BoltResultHandle resultHandle )
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

			 public static State ValueOf( string name )
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
//ORIGINAL LINE: private static void waitForBookmark(Neo4Net.bolt.runtime.TransactionStateMachineSPI spi, Neo4Net.bolt.v1.runtime.bookmarking.Bookmark bookmark) throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
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