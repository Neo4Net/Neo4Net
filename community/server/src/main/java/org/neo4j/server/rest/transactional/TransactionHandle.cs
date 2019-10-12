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
namespace Org.Neo4j.Server.rest.transactional
{

	using CypherException = Org.Neo4j.Cypher.CypherException;
	using InvalidSemanticsException = Org.Neo4j.Cypher.InvalidSemanticsException;
	using Result = Org.Neo4j.Graphdb.Result;
	using AuthorizationViolationException = Org.Neo4j.Graphdb.security.AuthorizationViolationException;
	using WriteOperationsNotAllowedException = Org.Neo4j.Graphdb.security.WriteOperationsNotAllowedException;
	using Transaction_Type = Org.Neo4j.@internal.Kernel.Api.Transaction_Type;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using DeadlockDetectedException = Org.Neo4j.Kernel.DeadlockDetectedException;
	using GraphDatabaseQueryService = Org.Neo4j.Kernel.GraphDatabaseQueryService;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using QueryExecutionEngine = Org.Neo4j.Kernel.impl.query.QueryExecutionEngine;
	using QueryExecutionKernelException = Org.Neo4j.Kernel.impl.query.QueryExecutionKernelException;
	using TransactionalContext = Org.Neo4j.Kernel.impl.query.TransactionalContext;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using InternalBeginTransactionError = Org.Neo4j.Server.rest.transactional.error.InternalBeginTransactionError;
	using Neo4jError = Org.Neo4j.Server.rest.transactional.error.Neo4jError;
	using TransactionUriScheme = Org.Neo4j.Server.rest.web.TransactionUriScheme;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.addToCollection;

	/// <summary>
	/// Encapsulates executing statements in a transaction, committing the transaction, or rolling it back.
	/// 
	/// Constructing a <seealso cref="TransactionHandle"/> does not immediately ask the kernel to create a
	/// <seealso cref="org.neo4j.kernel.api.KernelTransaction"/>; instead a <seealso cref="org.neo4j.kernel.api.KernelTransaction"/> is
	/// only created when the first statements need to be executed.
	/// 
	/// At the end of each statement-executing method, the <seealso cref="org.neo4j.kernel.api.KernelTransaction"/> is either
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
			  IList<Neo4jError> errors = new LinkedList<Neo4jError>();
			  try
			  {
					output.TransactionCommitUri( _uriScheme.txCommitUri( _id ) );
					EnsureActiveTransaction();
					Execute( statements, output, errors, request );
			  }
			  catch ( InternalBeginTransactionError e )
			  {
					errors.Add( e.ToNeo4jError() );
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
			  IList<Neo4jError> errors = new LinkedList<Neo4jError>();
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
					errors.Add( e.ToNeo4jError() );
			  }
			  catch ( CypherException e )
			  {
					errors.Add( new Neo4jError( e.status(), e ) );
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
			  IList<Neo4jError> errors = new LinkedList<Neo4jError>();
			  try
			  {
					EnsureActiveTransaction();
					Rollback( errors );
			  }
			  catch ( InternalBeginTransactionError e )
			  {
					errors.Add( e.ToNeo4jError() );
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
//ORIGINAL LINE: private void ensureActiveTransaction() throws org.neo4j.server.rest.transactional.error.InternalBeginTransactionError
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

		 private void Execute( StatementDeserializer statements, ExecutionResultSerializer output, IList<Neo4jError> errors, HttpServletRequest request )
		 {
			  ExecuteStatements( statements, output, errors, request );

			  if ( Neo4jError.shouldRollBackOn( errors ) )
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

		 private void CloseContextAndCollectErrors( IList<Neo4jError> errors )
		 {
			  if ( errors.Count == 0 )
			  {
					try
					{
						 _context.commit();
					}
					catch ( Exception e )
					{
						 if ( e.InnerException is Org.Neo4j.Kernel.Api.Exceptions.Status_HasStatus )
						 {
							  errors.Add( new Neo4jError( ( ( Org.Neo4j.Kernel.Api.Exceptions.Status_HasStatus ) e.InnerException ).Status(), e ) );
						 }
						 else
						 {
							  _log.error( "Failed to commit transaction.", e );
							  errors.Add( new Neo4jError( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.TransactionCommitFailed, e ) );
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
						 errors.Add( new Neo4jError( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.TransactionRollbackFailed, e ) );
					}
			  }
		 }

		 private void Rollback( IList<Neo4jError> errors )
		 {
			  try
			  {
					_context.rollback();
			  }
			  catch ( Exception e )
			  {
					_log.error( "Failed to rollback transaction.", e );
					errors.Add( new Neo4jError( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.TransactionRollbackFailed, e ) );
			  }
			  finally
			  {
					_registry.forget( _id );
			  }
		 }

		 private void ExecuteStatements( StatementDeserializer statements, ExecutionResultSerializer output, IList<Neo4jError> errors, HttpServletRequest request )
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
							  errors.Add( new Neo4jError( e.status(), e ) );
							  break;
						 }
						 catch ( DeadlockDetectedException e )
						 {
							  errors.Add( new Neo4jError( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.DeadlockDetected, e ) );
						 }
						 catch ( IOException e )
						 {
							  errors.Add( new Neo4jError( Org.Neo4j.Kernel.Api.Exceptions.Status_Network.CommunicationError, e ) );
							  break;
						 }
						 catch ( Exception e )
						 {
							  Exception cause = e.InnerException;
							  if ( cause is Org.Neo4j.Kernel.Api.Exceptions.Status_HasStatus )
							  {
									errors.Add( new Neo4jError( ( ( Org.Neo4j.Kernel.Api.Exceptions.Status_HasStatus ) cause ).Status(), cause ) );
							  }
							  else
							  {
									errors.Add( new Neo4jError( Org.Neo4j.Kernel.Api.Exceptions.Status_Statement.ExecutionFailed, e ) );
							  }

							  break;
						 }
					}

					addToCollection( statements.Errors(), errors );
			  }
			  catch ( Exception e )
			  {
					errors.Add( new Neo4jError( Org.Neo4j.Kernel.Api.Exceptions.Status_General.UnknownError, e ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.graphdb.Result safelyExecute(Statement statement, boolean hasPeriodicCommit, org.neo4j.kernel.impl.query.TransactionalContext tc) throws org.neo4j.kernel.impl.query.QueryExecutionKernelException, java.io.IOException
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