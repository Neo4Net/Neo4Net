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
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;


	using SyntaxException = Neo4Net.Cypher.SyntaxException;
	using Notification = Neo4Net.GraphDb.Notification;
	using Result = Neo4Net.GraphDb.Result;
	using Transaction_Type = Neo4Net.Internal.Kernel.Api.Transaction_Type;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using DeadlockDetectedException = Neo4Net.Kernel.DeadlockDetectedException;
	using GraphDatabaseQueryService = Neo4Net.Kernel.GraphDatabaseQueryService;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using QueryExecutionEngine = Neo4Net.Kernel.impl.query.QueryExecutionEngine;
	using QueryExecutionKernelException = Neo4Net.Kernel.impl.query.QueryExecutionKernelException;
	using TransactionalContext = Neo4Net.Kernel.impl.query.TransactionalContext;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Neo4NetError = Neo4Net.Server.rest.transactional.error.Neo4NetError;
	using TransactionUriScheme = Neo4Net.Server.rest.web.TransactionUriScheme;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyCollectionOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.isNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.hamcrest.MockitoHamcrest.argThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.Transaction_Type.@explicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.transactional.StubStatementDeserializer.statements;

	public class TransactionHandleTest
	{

		 internal static MapValue NoParams = VirtualValues.emptyMap();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExecuteStatements() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExecuteStatements()
		 {
			  // given
			  TransitionalPeriodTransactionMessContainer kernel = MockKernel();

			  QueryExecutionEngine executionEngine = mock( typeof( QueryExecutionEngine ) );
			  Result executionResult = mock( typeof( Result ) );
			  TransactionalContext transactionalContext = PrepareKernelWithQuerySession( kernel );
			  when( executionEngine.ExecuteQuery( "query", NoParams, transactionalContext ) ).thenReturn( executionResult );
			  TransactionRegistry registry = mock( typeof( TransactionRegistry ) );
			  when( registry.Begin( any( typeof( TransactionHandle ) ) ) ).thenReturn( 1337L );
			  TransactionHandle handle = GetTransactionHandle( kernel, executionEngine, registry );
			  ExecutionResultSerializer output = mock( typeof( ExecutionResultSerializer ) );

			  // when
			  handle.Execute( statements( new Statement( "query", map(), false, (ResultDataContent[]) null ) ), output, mock(typeof(HttpServletRequest)) );

			  // then
			  verify( executionEngine ).executeQuery( "query", NoParams, transactionalContext );

			  InOrder outputOrder = inOrder( output );
			  outputOrder.verify( output ).transactionCommitUri( uriScheme.txCommitUri( 1337 ) );
			  outputOrder.verify( output ).statementResult( executionResult, false, ( ResultDataContent[] )null );
			  outputOrder.verify( output ).notifications( anyCollection() );
			  outputOrder.verify( output ).transactionStatus( anyLong() );
			  outputOrder.verify( output ).errors( argThat( HasNoErrors() ) );
			  outputOrder.verify( output ).finish();
			  verifyNoMoreInteractions( output );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSuspendTransactionAndReleaseForOtherRequestsAfterExecutingStatements() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSuspendTransactionAndReleaseForOtherRequestsAfterExecutingStatements()
		 {
			  // given
			  TransitionalPeriodTransactionMessContainer kernel = MockKernel();
			  TransitionalTxManagementKernelTransaction transactionContext = kernel.NewTransaction( @explicit, AUTH_DISABLED, -1 );

			  TransactionRegistry registry = mock( typeof( TransactionRegistry ) );

			  QueryExecutionEngine executionEngine = mock( typeof( QueryExecutionEngine ) );
			  TransactionalContext transactionalContext = PrepareKernelWithQuerySession( kernel );
			  Result executionResult = mock( typeof( Result ) );
			  when( executionEngine.ExecuteQuery( "query", NoParams, transactionalContext ) ).thenReturn( executionResult );
			  when( registry.Begin( any( typeof( TransactionHandle ) ) ) ).thenReturn( 1337L );
			  TransactionHandle handle = GetTransactionHandle( kernel, executionEngine, registry );
			  ExecutionResultSerializer output = mock( typeof( ExecutionResultSerializer ) );

			  // when
			  handle.Execute( statements( new Statement( "query", map(), false, (ResultDataContent[]) null ) ), output, mock(typeof(HttpServletRequest)) );

			  // then
			  InOrder transactionOrder = inOrder( transactionContext, registry );
			  transactionOrder.verify( transactionContext ).suspendSinceTransactionsAreStillThreadBound();
			  transactionOrder.verify( registry ).release( 1337L, handle );

			  InOrder outputOrder = inOrder( output );
			  outputOrder.verify( output ).transactionCommitUri( uriScheme.txCommitUri( 1337 ) );
			  outputOrder.verify( output ).statementResult( executionResult, false, ( ResultDataContent[] )null );
			  outputOrder.verify( output ).notifications( anyCollection() );
			  outputOrder.verify( output ).transactionStatus( anyLong() );
			  outputOrder.verify( output ).errors( argThat( HasNoErrors() ) );
			  outputOrder.verify( output ).finish();
			  verifyNoMoreInteractions( output );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResumeTransactionWhenExecutingStatementsOnSecondRequest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResumeTransactionWhenExecutingStatementsOnSecondRequest()
		 {
			  // given
			  TransitionalPeriodTransactionMessContainer kernel = MockKernel();
			  TransitionalTxManagementKernelTransaction transactionContext = kernel.NewTransaction( @explicit, AUTH_DISABLED, -1 );

			  TransactionRegistry registry = mock( typeof( TransactionRegistry ) );
			  QueryExecutionEngine executionEngine = mock( typeof( QueryExecutionEngine ) );
			  TransactionalContext transactionalContext = PrepareKernelWithQuerySession( kernel );
			  when( registry.Begin( any( typeof( TransactionHandle ) ) ) ).thenReturn( 1337L );
			  TransactionHandle handle = GetTransactionHandle( kernel, executionEngine, registry );
			  ExecutionResultSerializer output = mock( typeof( ExecutionResultSerializer ) );

			  handle.Execute( statements( new Statement( "query", map(), false, (ResultDataContent[]) null ) ), output, mock(typeof(HttpServletRequest)) );
			  reset( transactionContext, registry, executionEngine, output );
			  Result executionResult = mock( typeof( Result ) );
			  when( executionEngine.ExecuteQuery( "query", NoParams, transactionalContext ) ).thenReturn( executionResult );

			  // when
			  handle.Execute( statements( new Statement( "query", map(), false, (ResultDataContent[]) null ) ), output, mock(typeof(HttpServletRequest)) );

			  // then
			  InOrder order = inOrder( transactionContext, registry, executionEngine );
			  order.verify( transactionContext ).resumeSinceTransactionsAreStillThreadBound();
			  order.verify( executionEngine ).executeQuery( "query", NoParams, transactionalContext );
			  order.verify( transactionContext ).suspendSinceTransactionsAreStillThreadBound();
			  order.verify( registry ).release( 1337L, handle );

			  InOrder outputOrder = inOrder( output );
			  outputOrder.verify( output ).transactionCommitUri( uriScheme.txCommitUri( 1337 ) );
			  outputOrder.verify( output ).statementResult( executionResult, false, ( ResultDataContent[] ) null );
			  outputOrder.verify( output ).notifications( anyCollectionOf( typeof( Notification ) ) );
			  outputOrder.verify( output ).transactionStatus( anyLong() );
			  outputOrder.verify( output ).errors( argThat( HasNoErrors() ) );
			  outputOrder.verify( output ).finish();
			  verifyNoMoreInteractions( output );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCommitSinglePeriodicCommitStatement() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCommitSinglePeriodicCommitStatement()
		 {
			  // given
			  string queryText = "USING PERIODIC COMMIT CREATE()";
			  TransitionalPeriodTransactionMessContainer kernel = MockKernel();

			  QueryExecutionEngine executionEngine = mock( typeof( QueryExecutionEngine ) );
			  Result executionResult = mock( typeof( Result ) );
			  TransactionalContext transactionalContext = PrepareKernelWithQuerySession( kernel );
			  when( executionEngine.IsPeriodicCommit( queryText ) ).thenReturn( true );
			  when( executionEngine.ExecuteQuery( eq( queryText ), eq( NoParams ), eq( transactionalContext ) ) ).thenReturn( executionResult );

			  TransactionRegistry registry = mock( typeof( TransactionRegistry ) );
			  when( registry.Begin( any( typeof( TransactionHandle ) ) ) ).thenReturn( 1337L );
			  TransactionHandle handle = GetTransactionHandle( kernel, executionEngine, registry );
			  ExecutionResultSerializer output = mock( typeof( ExecutionResultSerializer ) );
			  Statement statement = new Statement( queryText, map(), false, (ResultDataContent[]) null );

			  // when
			  handle.Commit( statements( statement ), output, mock( typeof( HttpServletRequest ) ) );

			  // then
			  verify( executionEngine ).executeQuery( queryText, NoParams, transactionalContext );

			  InOrder outputOrder = inOrder( output );
			  outputOrder.verify( output ).statementResult( executionResult, false, ( ResultDataContent[] ) null );
			  outputOrder.verify( output ).notifications( anyCollectionOf( typeof( Notification ) ) );
			  outputOrder.verify( output ).errors( argThat( HasNoErrors() ) );
			  outputOrder.verify( output ).finish();
			  verifyNoMoreInteractions( output );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCommitTransactionAndTellRegistryToForgetItsHandle() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCommitTransactionAndTellRegistryToForgetItsHandle()
		 {
			  // given
			  TransitionalPeriodTransactionMessContainer kernel = MockKernel();
			  TransitionalTxManagementKernelTransaction transactionContext = kernel.NewTransaction( @explicit, AUTH_DISABLED, -1 );

			  TransactionRegistry registry = mock( typeof( TransactionRegistry ) );

			  QueryExecutionEngine engine = mock( typeof( QueryExecutionEngine ) );
			  TransactionalContext transactionalContext = PrepareKernelWithQuerySession( kernel );
			  Result result = mock( typeof( Result ) );
			  when( engine.ExecuteQuery( "query", NoParams, transactionalContext ) ).thenReturn( result );
			  when( registry.Begin( any( typeof( TransactionHandle ) ) ) ).thenReturn( 1337L );
			  GraphDatabaseQueryService queryService = mock( typeof( GraphDatabaseQueryService ) );
			  TransactionHandle handle = new TransactionHandle( kernel, engine, queryService, registry, uriScheme, false, AUTH_DISABLED, anyLong(), NullLogProvider.Instance );
			  ExecutionResultSerializer output = mock( typeof( ExecutionResultSerializer ) );

			  // when
			  Statement statement = new Statement( "query", map(), false, (ResultDataContent[]) null );
			  handle.Commit( statements( statement ), output, mock( typeof( HttpServletRequest ) ) );

			  // then
			  InOrder transactionOrder = inOrder( transactionContext, registry );
			  transactionOrder.verify( transactionContext ).commit();
			  transactionOrder.verify( registry ).forget( 1337L );

			  InOrder outputOrder = inOrder( output );
			  outputOrder.verify( output ).statementResult( result, false, ( ResultDataContent[] )null );
			  outputOrder.verify( output ).notifications( anyCollectionOf( typeof( Notification ) ) );
			  outputOrder.verify( output ).errors( argThat( HasNoErrors() ) );
			  outputOrder.verify( output ).finish();
			  verifyNoMoreInteractions( output );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackTransactionAndTellRegistryToForgetItsHandle()
		 public virtual void ShouldRollbackTransactionAndTellRegistryToForgetItsHandle()
		 {
			  // given
			  TransitionalPeriodTransactionMessContainer kernel = MockKernel();
			  TransitionalTxManagementKernelTransaction transactionContext = kernel.NewTransaction( @explicit, AUTH_DISABLED, -1 );

			  TransactionRegistry registry = mock( typeof( TransactionRegistry ) );
			  when( registry.Begin( any( typeof( TransactionHandle ) ) ) ).thenReturn( 1337L );
			  GraphDatabaseQueryService queryService = mock( typeof( GraphDatabaseQueryService ) );
			  TransactionHandle handle = new TransactionHandle( kernel, mock( typeof( QueryExecutionEngine ) ), queryService, registry, uriScheme, true, AUTH_DISABLED, anyLong(), NullLogProvider.Instance );
			  ExecutionResultSerializer output = mock( typeof( ExecutionResultSerializer ) );

			  // when
			  handle.Rollback( output );

			  // then
			  InOrder transactionOrder = inOrder( transactionContext, registry );
			  transactionOrder.verify( transactionContext ).rollback();
			  transactionOrder.verify( registry ).forget( 1337L );

			  InOrder outputOrder = inOrder( output );
			  outputOrder.verify( output ).errors( argThat( HasNoErrors() ) );
			  outputOrder.verify( output ).finish();
			  verifyNoMoreInteractions( output );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateTransactionContextOnlyWhenFirstNeeded() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateTransactionContextOnlyWhenFirstNeeded()
		 {
			  // given
			  TransitionalPeriodTransactionMessContainer kernel = MockKernel();
			  ExecutionResultSerializer output = mock( typeof( ExecutionResultSerializer ) );
			  TransactionRegistry registry = mock( typeof( TransactionRegistry ) );

			  // when
			  QueryExecutionEngine engine = mock( typeof( QueryExecutionEngine ) );
			  Result executionResult = mock( typeof( Result ) );
			  TransactionalContext transactionalContext = PrepareKernelWithQuerySession( kernel );
			  when( engine.ExecuteQuery( "query", NoParams, transactionalContext ) ).thenReturn( executionResult );
			  when( registry.Begin( any( typeof( TransactionHandle ) ) ) ).thenReturn( 1337L );
			  GraphDatabaseQueryService queryService = mock( typeof( GraphDatabaseQueryService ) );
			  TransactionHandle handle = new TransactionHandle( kernel, engine, queryService, registry, uriScheme, true, AUTH_DISABLED, anyLong(), NullLogProvider.Instance );

			  // then
			  verifyZeroInteractions( kernel );

			  // when
			  handle.Execute( statements( new Statement( "query", map(), false, (ResultDataContent[]) null ) ), output, mock(typeof(HttpServletRequest)) );

			  // then
			  verify( kernel ).newTransaction( any( typeof( Transaction_Type ) ), any( typeof( LoginContext ) ), anyLong() );

			  InOrder outputOrder = inOrder( output );
			  outputOrder.verify( output ).transactionCommitUri( uriScheme.txCommitUri( 1337 ) );
			  outputOrder.verify( output ).statementResult( executionResult, false, ( ResultDataContent[] )null );
			  outputOrder.verify( output ).notifications( anyCollectionOf( typeof( Notification ) ) );
			  outputOrder.verify( output ).transactionStatus( anyLong() );
			  outputOrder.verify( output ).errors( argThat( HasNoErrors() ) );
			  outputOrder.verify( output ).finish();
			  verifyNoMoreInteractions( output );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackTransactionIfExecutionErrorOccurs() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackTransactionIfExecutionErrorOccurs()
		 {
			  // given
			  TransitionalPeriodTransactionMessContainer kernel = MockKernel();
			  TransitionalTxManagementKernelTransaction transactionContext = kernel.NewTransaction( @explicit, AUTH_DISABLED, -1 );

			  TransactionRegistry registry = mock( typeof( TransactionRegistry ) );

			  QueryExecutionEngine executionEngine = mock( typeof( QueryExecutionEngine ) );
			  TransactionalContext transactionalContext = PrepareKernelWithQuerySession( kernel );
			  when( executionEngine.ExecuteQuery( "query", NoParams, transactionalContext ) ).thenThrow( new System.NullReferenceException() );

			  when( registry.Begin( any( typeof( TransactionHandle ) ) ) ).thenReturn( 1337L );
			  TransactionHandle handle = GetTransactionHandle( kernel, executionEngine, registry );
			  ExecutionResultSerializer output = mock( typeof( ExecutionResultSerializer ) );

			  // when
			  handle.Execute( statements( new Statement( "query", map(), false, (ResultDataContent[]) null ) ), output, mock(typeof(HttpServletRequest)) );

			  // then
			  verify( transactionContext ).rollback();
			  verify( registry ).forget( 1337L );

			  InOrder outputOrder = inOrder( output );
			  outputOrder.verify( output ).transactionCommitUri( uriScheme.txCommitUri( 1337 ) );
			  outputOrder.verify( output ).errors( argThat( HasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Statement.ExecutionFailed ) ) );
			  outputOrder.verify( output ).finish();
			  verifyNoMoreInteractions( output );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogMessageIfCommitErrorOccurs() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogMessageIfCommitErrorOccurs()
		 {
			  // given
			  TransitionalPeriodTransactionMessContainer kernel = MockKernel();
			  TransitionalTxManagementKernelTransaction transactionContext = kernel.NewTransaction( @explicit, AUTH_DISABLED, -1 );
			  doThrow( new System.NullReferenceException() ).when(transactionContext).commit();

			  LogProvider logProvider = mock( typeof( LogProvider ) );
			  Log log = mock( typeof( Log ) );
			  when( logProvider.GetLog( typeof( TransactionHandle ) ) ).thenReturn( log );

			  TransactionRegistry registry = mock( typeof( TransactionRegistry ) );

			  QueryExecutionEngine engine = mock( typeof( QueryExecutionEngine ) );
			  Result executionResult = mock( typeof( Result ) );
			  TransactionalContext transactionalContext = PrepareKernelWithQuerySession( kernel );
			  when( engine.ExecuteQuery( "query", NoParams, transactionalContext ) ).thenReturn( executionResult );
			  when( registry.Begin( any( typeof( TransactionHandle ) ) ) ).thenReturn( 1337L );
			  GraphDatabaseQueryService queryService = mock( typeof( GraphDatabaseQueryService ) );
			  TransactionHandle handle = new TransactionHandle( kernel, engine, queryService, registry, uriScheme, false, AUTH_DISABLED, anyLong(), logProvider );
			  ExecutionResultSerializer output = mock( typeof( ExecutionResultSerializer ) );

			  // when
			  Statement statement = new Statement( "query", map(), false, (ResultDataContent[]) null );
			  handle.Commit( statements( statement ), output, mock( typeof( HttpServletRequest ) ) );

			  // then
			  verify( log ).error( eq( "Failed to commit transaction." ), any( typeof( System.NullReferenceException ) ) );
			  verify( registry ).forget( 1337L );

			  InOrder outputOrder = inOrder( output );
			  outputOrder.verify( output ).statementResult( executionResult, false, ( ResultDataContent[] )null );
			  outputOrder.verify( output ).notifications( anyCollectionOf( typeof( Notification ) ) );
			  outputOrder.verify( output ).errors( argThat( HasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionCommitFailed ) ) );
			  outputOrder.verify( output ).finish();
			  verifyNoMoreInteractions( output );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogMessageIfCypherSyntaxErrorOccurs() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogMessageIfCypherSyntaxErrorOccurs()
		 {
			  // given
			  TransitionalPeriodTransactionMessContainer kernel = MockKernel();

			  QueryExecutionEngine executionEngine = mock( typeof( QueryExecutionEngine ) );
			  TransactionalContext transactionalContext = PrepareKernelWithQuerySession( kernel );
			  when( executionEngine.ExecuteQuery( "matsch (n) return n", NoParams, transactionalContext ) ).thenThrow( new QueryExecutionKernelException( new SyntaxException( "did you mean MATCH?" ) ) );

			  TransactionRegistry registry = mock( typeof( TransactionRegistry ) );
			  when( registry.Begin( any( typeof( TransactionHandle ) ) ) ).thenReturn( 1337L );
			  GraphDatabaseQueryService queryService = mock( typeof( GraphDatabaseQueryService ) );
			  TransactionHandle handle = new TransactionHandle( kernel, executionEngine, queryService, registry, uriScheme, false, AUTH_DISABLED, anyLong(), NullLogProvider.Instance );
			  ExecutionResultSerializer output = mock( typeof( ExecutionResultSerializer ) );

			  // when
			  Statement statement = new Statement( "matsch (n) return n", map(), false, (ResultDataContent[]) null );
			  handle.Commit( statements( statement ), output, mock( typeof( HttpServletRequest ) ) );

			  // then
			  verify( registry ).forget( 1337L );

			  InOrder outputOrder = inOrder( output );
			  outputOrder.verify( output ).errors( argThat( HasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Statement.SyntaxError ) ) );
			  outputOrder.verify( output ).finish();
			  verifyNoMoreInteractions( output );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleExecutionEngineThrowingUndeclaredCheckedExceptions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleExecutionEngineThrowingUndeclaredCheckedExceptions()
		 {
			  // given
			  QueryExecutionEngine executionEngine = mock( typeof( QueryExecutionEngine ) );
			  when( executionEngine.ExecuteQuery( eq( "match (n) return n" ), eq( NoParams ), any( typeof( TransactionalContext ) ) ) ).thenAnswer(invocationOnMock =>
			  {
						  throw new Exception( "BOO" );
			  });

			  TransactionRegistry registry = mock( typeof( TransactionRegistry ) );
			  when( registry.Begin( any( typeof( TransactionHandle ) ) ) ).thenReturn( 1337L );
			  GraphDatabaseQueryService queryService = mock( typeof( GraphDatabaseQueryService ) );
			  TransactionHandle handle = new TransactionHandle( MockKernel(), executionEngine, queryService, registry, uriScheme, false, AUTH_DISABLED, anyLong(), NullLogProvider.Instance );
			  ExecutionResultSerializer output = mock( typeof( ExecutionResultSerializer ) );

			  // when
			  Statement statement = new Statement( "match (n) return n", map(), false, (ResultDataContent[]) null );
			  handle.Commit( statements( statement ), output, mock( typeof( HttpServletRequest ) ) );

			  // then
			  verify( registry ).forget( 1337L );

			  InOrder outputOrder = inOrder( output );
			  outputOrder.verify( output ).statementResult( Null, eq( false ), Null );
			  outputOrder.verify( output ).errors( argThat( HasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Statement.ExecutionFailed ) ) );
			  outputOrder.verify( output ).finish();
			  verifyNoMoreInteractions( output );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInterruptTransaction()
		 public virtual void ShouldInterruptTransaction()
		 {
			  // given
			  TransitionalPeriodTransactionMessContainer kernel = MockKernel();
			  TransitionalTxManagementKernelTransaction tx = mock( typeof( TransitionalTxManagementKernelTransaction ) );
			  when( kernel.NewTransaction( any( typeof( Transaction_Type ) ), any( typeof( LoginContext ) ), anyLong() ) ).thenReturn(tx);
			  TransactionRegistry registry = mock( typeof( TransactionRegistry ) );
			  when( registry.Begin( any( typeof( TransactionHandle ) ) ) ).thenReturn( 1337L );
			  QueryExecutionEngine executionEngine = mock( typeof( QueryExecutionEngine ) );
			  TransactionHandle handle = GetTransactionHandle( kernel, executionEngine, registry );

			  ExecutionResultSerializer output = mock( typeof( ExecutionResultSerializer ) );
			  Statement statement = new Statement( "MATCH (n) RETURN n", map(), false, (ResultDataContent[]) null );
			  handle.Execute( statements( statement ), output, mock( typeof( HttpServletRequest ) ) );

			  // when
			  handle.Terminate();

			  // then
			  verify( tx, times( 1 ) ).terminate();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void deadlockExceptionHasCorrectStatus() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DeadlockExceptionHasCorrectStatus()
		 {
			  // given
			  QueryExecutionEngine executionEngine = mock( typeof( QueryExecutionEngine ) );
			  when( executionEngine.ExecuteQuery( anyString(), any(typeof(MapValue)), Null ) ).thenThrow(new DeadlockDetectedException("deadlock"));

			  GraphDatabaseQueryService queryService = mock( typeof( GraphDatabaseQueryService ) );
			  TransactionHandle handle = new TransactionHandle( MockKernel(), executionEngine, queryService, mock(typeof(TransactionRegistry)), uriScheme, true, AUTH_DISABLED, anyLong(), NullLogProvider.Instance );

			  ExecutionResultSerializer output = mock( typeof( ExecutionResultSerializer ) );

			  // when
			  handle.Execute( statements( new Statement( "query", map(), false, (ResultDataContent[]) null ) ), output, mock(typeof(HttpServletRequest)) );

			  // then
			  verify( output ).errors( argThat( HasErrors( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.DeadlockDetected ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void startTransactionWithRequestedTimeout()
		 public virtual void StartTransactionWithRequestedTimeout()
		 {
			  QueryExecutionEngine executionEngine = mock( typeof( QueryExecutionEngine ) );
			  GraphDatabaseQueryService queryService = mock( typeof( GraphDatabaseQueryService ) );
			  ExecutionResultSerializer output = mock( typeof( ExecutionResultSerializer ) );

			  TransitionalPeriodTransactionMessContainer txManagerFacade = MockKernel();
			  TransactionHandle handle = new TransactionHandle( txManagerFacade, executionEngine, queryService, mock( typeof( TransactionRegistry ) ), uriScheme, true, AUTH_DISABLED, 100, NullLogProvider.Instance );

			  handle.Commit( statements( new Statement( "query", map(), false, (ResultDataContent[]) null ) ), output, mock(typeof(HttpServletRequest)) );

			  verify( txManagerFacade ).newTransaction( Transaction_Type.@implicit, AUTH_DISABLED, 100 );
		 }

		 private TransactionHandle GetTransactionHandle( TransitionalPeriodTransactionMessContainer kernel, QueryExecutionEngine executionEngine, TransactionRegistry registry )
		 {
			  GraphDatabaseQueryService queryService = mock( typeof( GraphDatabaseQueryService ) );
			  return new TransactionHandle( kernel, executionEngine, queryService, registry, uriScheme, true, AUTH_DISABLED, anyLong(), NullLogProvider.Instance );
		 }

		 private static readonly TransactionUriScheme uriScheme = new TransactionUriSchemeAnonymousInnerClass();

		 private class TransactionUriSchemeAnonymousInnerClass : TransactionUriScheme
		 {
			 public URI txUri( long id )
			 {
				  return URI.create( "transaction/" + id );
			 }

			 public URI txCommitUri( long id )
			 {
				  return URI.create( "transaction/" + id + "/commit" );
			 }
		 }

		 private TransitionalPeriodTransactionMessContainer MockKernel()
		 {
			  TransitionalTxManagementKernelTransaction context = mock( typeof( TransitionalTxManagementKernelTransaction ) );
			  TransitionalPeriodTransactionMessContainer kernel = mock( typeof( TransitionalPeriodTransactionMessContainer ) );
			  when( kernel.NewTransaction( any( typeof( Transaction_Type ) ), any( typeof( LoginContext ) ), anyLong() ) ).thenReturn(context);
			  return kernel;
		 }

		 private static Matcher<IEnumerable<Neo4NetError>> HasNoErrors()
		 {
			  return HasErrors();
		 }

		 private static Matcher<IEnumerable<Neo4NetError>> HasErrors( params Status[] codes )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<org.Neo4Net.kernel.api.exceptions.Status> expectedErrorsCodes = new java.util.HashSet<>(asList(codes));
			  ISet<Status> expectedErrorsCodes = new HashSet<Status>( asList( codes ) );

			  return new TypeSafeMatcherAnonymousInnerClass( expectedErrorsCodes );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<IEnumerable<Neo4NetError>>
		 {
			 private ISet<Status> _expectedErrorsCodes;

			 public TypeSafeMatcherAnonymousInnerClass( ISet<Status> expectedErrorsCodes )
			 {
				 this._expectedErrorsCodes = expectedErrorsCodes;
			 }

			 protected internal override bool matchesSafely( IEnumerable<Neo4NetError> item )
			 {
				  ISet<Status> actualErrorCodes = new HashSet<Status>();
				  foreach ( Neo4NetError Neo4NetError in item )
				  {
						actualErrorCodes.Add( Neo4NetError.Status() );
				  }
				  return _expectedErrorsCodes.SetEquals( actualErrorCodes );
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "Errors with set of codes" ).appendValue( _expectedErrorsCodes );
			 }
		 }

		 private TransactionalContext PrepareKernelWithQuerySession( TransitionalPeriodTransactionMessContainer kernel )
		 {
			  TransactionalContext tc = mock( typeof( TransactionalContext ) );
			  when( kernel.Create( any( typeof( HttpServletRequest ) ), any( typeof( GraphDatabaseQueryService ) ), any( typeof( Transaction_Type ) ), any( typeof( LoginContext ) ), any( typeof( string ) ), any( typeof( System.Collections.IDictionary ) ) ) ).thenReturn( tc );
			  return tc;
		 }
	}

}