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
namespace Neo4Net.Server.rest.transactional
{

	using CypherException = Neo4Net.Cypher.CypherException;
	using InvalidSemanticsException = Neo4Net.Cypher.InvalidSemanticsException;
	using Result = Neo4Net.GraphDb.Result;
	using AuthorizationViolationException = Neo4Net.GraphDb.security.AuthorizationViolationException;
	using WriteOperationsNotAllowedException = Neo4Net.GraphDb.security.WriteOperationsNotAllowedException;
	using Transaction_Type = Neo4Net.Kernel.Api.Internal.Transaction_Type;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using LoginContext = Neo4Net.Kernel.Api.Internal.security.LoginContext;
	using DeadlockDetectedException = Neo4Net.Kernel.DeadlockDetectedException;
	using GraphDatabaseQueryService = Neo4Net.Kernel.GraphDatabaseQueryService;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using QueryExecutionEngine = Neo4Net.Kernel.impl.query.QueryExecutionEngine;
	using QueryExecutionKernelException = Neo4Net.Kernel.impl.query.QueryExecutionKernelException;
	using TransactionalContext = Neo4Net.Kernel.impl.query.TransactionalContext;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using InternalBeginTransactionError = Neo4Net.Server.rest.transactional.error.InternalBeginTransactionError;
	using Neo4NetError = Neo4Net.Server.rest.transactional.error.Neo4NetError;
	using TransactionUriScheme = Neo4Net.Server.rest.web.TransactionUriScheme;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.addToCollection;

	/// <summary>
	/// Encapsulates executing statements in a transaction, committing the transaction, or rolling it back.
	/// 
	/// Constructing a <seealso cref="TransactionHandle"/> does not immediately ask the kernel to create a
	/// <seealso cref="Neo4Net.kernel.api.KernelTransaction"/>; instead a <seealso cref="Neo4Net.kernel.api.KernelTransaction"/> is
	/// only created when the first statements need to be executed.
	/// 
	/// At the end of each statement-executing method, the <seealso cref="Neo4Net.kernel.api.KernelTransaction"/> is either
	/// suspended (ready to be resumed by a later operation), or committed, or rolled back.
	/// 
	/// If you acquire instances of this class from <seealso cref="TransactionHandleRegistry"/>, it will prevent concurrent access to
	/// the same instance. Therefore the implementation assumes that a single instance will only be accessed from
	/// a single thread.
	/// 
	/// All of the public methods on this class are "single-shot"; once you have called one method, the handle returns
	/// itself
	/// to the registry. If you want to use it again, you'll need to acquire it back from the registry to ensure exclusive
	/// use.
	/// </summary>
	public class TransactionHandle : TransactionTerminationHandle
	{
		 private readonly TransitionalPeriodTransactionMessContainer _txManagerFacade;
		 private readonly QueryExecutionEngine _engine;
		 private readonly TransactionRegistry _registry;
		 private readonly TransactionUriScheme _uriScheme;
		 private readonly Transaction_Type _type;
		 private readonly LoginContext _loginContext;
		 private long _customTransactionTimeout;
		 private readonly Log _log;
		 private readonly long _id;
		 private TransitionalTxManagementKernelTransaction _context;
		 private GraphDatabaseQueryService _queryService;

		 internal TransactionHandle( TransitionalPeriodTransactionMessContainer txManagerFacade, QueryExecutionEngine engine, GraphDatabaseQueryService queryService, TransactionRegistry registry, TransactionUriScheme uriScheme, bool implicitTransaction, LoginContext loginContext, long customTransactionTimeout, LogProvider logProvider )
		 {
			  this._txManagerFacade = txManagerFacade;
			  this._engine = engine;
			  this._queryService = queryService;
			  this._registry = registry;
			  this._uriScheme = uriScheme;
			  this._type = implicitTransaction ? Transaction_Type.@implicit : Transaction_Type.@explicit;
			  this._loginContext = loginContext;
			  this._customTransactionTimeout = customTransactionTimeout;
			  this._log = logProvider.getLog( this.GetType() );
			  this._id = registry.Begin( this );
		 }

		 public virtual URI Uri()
		 {
			  return _uriScheme.txUri( _id );
		 }

		 public virtual bool Implicit
		 {
			 get
			 {
				  return _type == Transaction_Type.@implicit;
			 }
		 }

		 public virtual void Execute( StatementDeserializer statements, ExecutionResultSerializer output, HttpServletRequest request )
		 {
			  IList<Neo4NetError> errors = new LinkedList<Neo4NetError>();
			  try
			  {
					output.TransactionCommitUri( _uriScheme.txCommitUri( _id ) );
					EnsureActiveTransaction();
					Execute( statements, output, errors, request );
			  }
			  catch ( InternalBeginTransactionError e )
			  {
					errors.Add( e.ToNeo4NetError() );
			  }
			  finally
			  {
					output.Errors( errors );
					output.Finish();
			  }
		 }

		 public override bool Terminate()
		 {
			  if ( _context != null )
			  {
					_context.terminate();
			  }
			  return true;
		 }

		 public virtual void Commit( StatementDeserializer statements, ExecutionResultSerializer output, HttpServletRequest request )
		 {
			  IList<Neo4NetError> errors = new LinkedList<Neo4NetError>();
			  try
			  {
					try
					{
						 Statement peek = statements.Peek();
						 if ( Implicit && peek == null ) // JSON parse error
						 {
							  addToCollection( statements.Errors(), errors );
						 }
						 else
						 {
							  EnsureActiveTransaction();
							  ExecuteStatements( statements, output, errors, request );
							  CloseContextAndCollectErrors( errors );
						 }
					}
					finally
					{
						 _registry.forget( _id );
					}
			  }
			  catch ( InternalBeginTransactionError e )
			  {
					errors.Add( e.ToNeo4NetError() );
			  }
			  catch ( CypherException e )
			  {
					errors.Add( new Neo4NetError( e.status(), e ) );
					throw e;
			  }
			  finally
			  {
					output.Errors( errors );
					output.Finish();
			  }
		 }

		 public virtual void Rollback( ExecutionResultSerializer output )
		 {
			  IList<Neo4NetError> errors = new LinkedList<Neo4NetError>();
			  try
			  {
					EnsureActiveTransaction();
					Rollback( errors );
			  }
			  catch ( InternalBeginTransactionError e )
			  {
					errors.Add( e.ToNeo4NetError() );
			  }
			  finally
			  {
					output.Errors( errors );
					output.Finish();
			  }
		 }

		 internal virtual void ForceRollback()
		 {
			  _context.resumeSinceTransactionsAreStillThreadBound();
			  _context.rollback();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureActiveTransaction() throws Neo4Net.server.rest.transactional.error.InternalBeginTransactionError
		 private void EnsureActiveTransaction()
		 {
			  if ( _context == null )
			  {
					try
					{
						 _context = _txManagerFacade.newTransaction( _type, _loginContext, _customTransactionTimeout );
					}
					catch ( Exception e )
					{
						 _log.error( "Failed to start transaction.", e );
						 throw new InternalBeginTransactionError( e );
					}
			  }
			  else
			  {
					_context.resumeSinceTransactionsAreStillThreadBound();
			  }
		 }

		 private void Execute( StatementDeserializer statements, ExecutionResultSerializer output, IList<Neo4NetError> errors, HttpServletRequest request )
		 {
			  ExecuteStatements( statements, output, errors, request );

			  if ( Neo4NetError.shouldRollBackOn( errors ) )
			  {
					Rollback( errors );
			  }
			  else
			  {
					_context.suspendSinceTransactionsAreStillThreadBound();
					long lastActiveTimestamp = _registry.release( _id, this );
					output.TransactionStatus( lastActiveTimestamp );
			  }
		 }

		 private void CloseContextAndCollectErrors( IList<Neo4NetError> errors )
		 {
			  if ( errors.Count == 0 )
			  {
					try
					{
						 _context.commit();
					}
					catch ( Exception e )
					{
						 if ( e.InnerException is Neo4Net.Kernel.Api.Exceptions.Status_HasStatus )
						 {
							  errors.Add( new Neo4NetError( ( ( Neo4Net.Kernel.Api.Exceptions.Status_HasStatus ) e.InnerException ).Status(), e ) );
						 }
						 else
						 {
							  _log.error( "Failed to commit transaction.", e );
							  errors.Add( new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionCommitFailed, e ) );
						 }
					}
			  }
			  else
			  {
					try
					{
						 _context.rollback();
					}
					catch ( Exception e )
					{
						 _log.error( "Failed to rollback transaction.", e );
						 errors.Add( new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionRollbackFailed, e ) );
					}
			  }
		 }

		 private void Rollback( IList<Neo4NetError> errors )
		 {
			  try
			  {
					_context.rollback();
			  }
			  catch ( Exception e )
			  {
					_log.error( "Failed to rollback transaction.", e );
					errors.Add( new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionRollbackFailed, e ) );
			  }
			  finally
			  {
					_registry.forget( _id );
			  }
		 }

		 private void ExecuteStatements( StatementDeserializer statements, ExecutionResultSerializer output, IList<Neo4NetError> errors, HttpServletRequest request )
		 {
			  try
			  {
					bool hasPrevious = false;
					while ( statements.MoveNext() )
					{
						 Statement statement = statements.Current;
						 try
						 {
							  bool hasPeriodicCommit = _engine.isPeriodicCommit( statement.StatementConflict() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  if ( ( statements.HasNext() || hasPrevious ) && hasPeriodicCommit )
							  {
									throw new QueryExecutionKernelException( new InvalidSemanticsException( "Cannot execute another statement after executing " + "PERIODIC COMMIT statement in the same transaction" ) );
							  }

							  if ( !hasPrevious && hasPeriodicCommit )
							  {
									_context.closeTransactionForPeriodicCommit();
							  }

							  hasPrevious = true;
							  TransactionalContext tc = _txManagerFacade.create( request, _queryService, _type, _loginContext, statement.StatementConflict(), statement.Parameters() );
							  Result result = SafelyExecute( statement, hasPeriodicCommit, tc );
							  output.StatementResult( result, statement.IncludeStats(), statement.ResultDataContents() );
							  output.Notifications( result.Notifications );
						 }
						 catch ( Exception e ) when ( e is KernelException || e is CypherException || e is AuthorizationViolationException || e is WriteOperationsNotAllowedException )
						 {
							  errors.Add( new Neo4NetError( e.status(), e ) );
							  break;
						 }
						 catch ( DeadlockDetectedException e )
						 {
							  errors.Add( new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.DeadlockDetected, e ) );
						 }
						 catch ( IOException e )
						 {
							  errors.Add( new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Network.CommunicationError, e ) );
							  break;
						 }
						 catch ( Exception e )
						 {
							  Exception cause = e.InnerException;
							  if ( cause is Neo4Net.Kernel.Api.Exceptions.Status_HasStatus )
							  {
									errors.Add( new Neo4NetError( ( ( Neo4Net.Kernel.Api.Exceptions.Status_HasStatus ) cause ).Status(), cause ) );
							  }
							  else
							  {
									errors.Add( new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Statement.ExecutionFailed, e ) );
							  }

							  break;
						 }
					}

					addToCollection( statements.Errors(), errors );
			  }
			  catch ( Exception e )
			  {
					errors.Add( new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError, e ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Neo4Net.graphdb.Result safelyExecute(Statement statement, boolean hasPeriodicCommit, Neo4Net.kernel.impl.query.TransactionalContext tc) throws Neo4Net.kernel.impl.query.QueryExecutionKernelException, java.io.IOException
		 private Result SafelyExecute( Statement statement, bool hasPeriodicCommit, TransactionalContext tc )
		 {
			  try
			  {
					return _engine.executeQuery( statement.StatementConflict(), ValueUtils.asMapValue(statement.Parameters()), tc );
			  }
			  finally
			  {
					if ( hasPeriodicCommit )
					{
						 _context.reopenAfterPeriodicCommit();
					}
			  }
		 }
	}

}