﻿using System;

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
namespace Org.Neo4j.Kernel.impl.query
{
	using MutableObject = org.apache.commons.lang3.mutable.MutableObject;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;
	using Mockito = org.mockito.Mockito;
	using ReturnsDeepStubs = org.mockito.@internal.stubbing.defaultanswers.ReturnsDeepStubs;

	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using TransactionTerminatedException = Org.Neo4j.Graphdb.TransactionTerminatedException;
	using ExecutionStatistics = Org.Neo4j.@internal.Kernel.Api.ExecutionStatistics;
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using SecurityContext = Org.Neo4j.@internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using QueryRegistryOperations = Org.Neo4j.Kernel.api.QueryRegistryOperations;
	using Statement = Org.Neo4j.Kernel.api.Statement;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using ExecutingQuery = Org.Neo4j.Kernel.api.query.ExecutingQuery;
	using KernelStatement = Org.Neo4j.Kernel.Impl.Api.KernelStatement;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using InternalTransaction = Org.Neo4j.Kernel.impl.coreapi.InternalTransaction;
	using PropertyContainerLocker = Org.Neo4j.Kernel.impl.coreapi.PropertyContainerLocker;
	using StatisticProvider = Org.Neo4j.Kernel.impl.query.statistic.StatisticProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	public class Neo4jTransactionalContextTest
	{
		 private GraphDatabaseQueryService _queryService;
		 private KernelStatement _initialStatement;
		 private ThreadToStatementContextBridge _txBridge;
		 private ConfiguredExecutionStatistics _statistics;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  SetUpMocks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkKernelStatementOnCheck()
		 public virtual void CheckKernelStatementOnCheck()
		 {
			  InternalTransaction initialTransaction = mock( typeof( InternalTransaction ), new ReturnsDeepStubs() );
			  Kernel kernel = mock( typeof( Kernel ) );
			  ThreadToStatementContextBridge txBridge = mock( typeof( ThreadToStatementContextBridge ) );
			  KernelTransaction kernelTransaction = MockTransaction( _initialStatement );
			  when( txBridge.GetKernelTransactionBoundToThisThread( true ) ).thenReturn( kernelTransaction );

			  Neo4jTransactionalContext transactionalContext = new Neo4jTransactionalContext( null, txBridge, null, initialTransaction, _initialStatement, null, kernel );

			  transactionalContext.Check();

			  verify( kernelTransaction ).assertOpen();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("ConstantConditions") @Test public void neverStopsExecutingQueryDuringCommitAndRestartTx()
		 public virtual void NeverStopsExecutingQueryDuringCommitAndRestartTx()
		 {
			  // Given
			  KernelTransaction initialKTX = MockTransaction( _initialStatement );
			  InternalTransaction initialTransaction = mock( typeof( InternalTransaction ), new ReturnsDeepStubs() );
			  KernelTransaction.Type transactionType = KernelTransaction.Type.@implicit;
			  SecurityContext securityContext = SecurityContext.AUTH_DISABLED;
			  when( initialTransaction.TransactionType() ).thenReturn(transactionType);
			  when( initialTransaction.SecurityContext() ).thenReturn(securityContext);
			  when( initialTransaction.TerminationReason() ).thenReturn(null);
			  QueryRegistryOperations initialQueryRegistry = mock( typeof( QueryRegistryOperations ) );
			  ExecutingQuery executingQuery = mock( typeof( ExecutingQuery ) );
			  PropertyContainerLocker locker = null;
			  ThreadToStatementContextBridge txBridge = mock( typeof( ThreadToStatementContextBridge ) );

			  Statement secondStatement = mock( typeof( Statement ) );
			  KernelTransaction secondKTX = MockTransaction( secondStatement );
			  InternalTransaction secondTransaction = mock( typeof( InternalTransaction ) );
			  when( secondTransaction.TerminationReason() ).thenReturn(null);
			  QueryRegistryOperations secondQueryRegistry = mock( typeof( QueryRegistryOperations ) );

			  when( executingQuery.QueryText() ).thenReturn("X");
			  when( executingQuery.QueryParameters() ).thenReturn(EMPTY_MAP);
			  when( _initialStatement.queryRegistration() ).thenReturn(initialQueryRegistry);
			  when( _queryService.beginTransaction( transactionType, securityContext ) ).thenReturn( secondTransaction );
			  when( txBridge.GetKernelTransactionBoundToThisThread( true ) ).thenReturn( initialKTX, initialKTX, secondKTX );
			  when( secondStatement.QueryRegistration() ).thenReturn(secondQueryRegistry);

			  Kernel kernel = mock( typeof( Kernel ) );
			  Neo4jTransactionalContext context = new Neo4jTransactionalContext( _queryService, txBridge, locker, initialTransaction, _initialStatement, executingQuery, kernel );

			  // When
			  context.CommitAndRestartTx();

			  // Then
			  object[] mocks = new object[] { txBridge, initialTransaction, initialKTX, initialQueryRegistry, secondQueryRegistry, secondKTX };
			  InOrder order = Mockito.inOrder( mocks );

			  // (0) Constructor
			  order.verify( initialTransaction ).transactionType();
			  order.verify( initialTransaction ).securityContext();
			  order.verify( txBridge ).getKernelTransactionBoundToThisThread( true );
			  order.verify( initialTransaction ).terminationReason(); // not terminated check

			  // (1) Collect stats
			  order.verify( initialKTX ).executionStatistics();

			  // (2) Unbind old
			  order.verify( txBridge ).getKernelTransactionBoundToThisThread( true );
			  order.verify( txBridge ).unbindTransactionFromCurrentThread();

			  // (3) Register and unbind new
			  order.verify( txBridge ).getKernelTransactionBoundToThisThread( true );
			  order.verify( secondKTX ).acquireStatement();
			  order.verify( secondQueryRegistry ).registerExecutingQuery( executingQuery );
			  order.verify( txBridge ).unbindTransactionFromCurrentThread();

			  // (4) Rebind, unregister, and close old
			  order.verify( txBridge ).bindTransactionToCurrentThread( initialKTX );
			  order.verify( initialQueryRegistry ).unregisterExecutingQuery( executingQuery );
			  order.verify( initialTransaction ).success();
			  order.verify( initialTransaction ).close();
			  order.verify( txBridge ).unbindTransactionFromCurrentThread();

			  // (5) Rebind new
			  order.verify( txBridge ).bindTransactionToCurrentThread( secondKTX );
			  verifyNoMoreInteractions( mocks );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("ConstantConditions") @Test public void rollsBackNewlyCreatedTransactionIfTerminationDetectedOnCloseDuringPeriodicCommit()
		 public virtual void RollsBackNewlyCreatedTransactionIfTerminationDetectedOnCloseDuringPeriodicCommit()
		 {
			  // Given
			  InternalTransaction initialTransaction = mock( typeof( InternalTransaction ), new ReturnsDeepStubs() );
			  KernelTransaction.Type transactionType = KernelTransaction.Type.@implicit;
			  SecurityContext securityContext = SecurityContext.AUTH_DISABLED;
			  when( initialTransaction.TransactionType() ).thenReturn(transactionType);
			  when( initialTransaction.SecurityContext() ).thenReturn(securityContext);
			  when( initialTransaction.TerminationReason() ).thenReturn(null);

			  GraphDatabaseQueryService queryService = mock( typeof( GraphDatabaseQueryService ) );
			  Statement initialStatement = mock( typeof( Statement ) );
			  KernelTransaction initialKTX = MockTransaction( initialStatement );
			  QueryRegistryOperations initialQueryRegistry = mock( typeof( QueryRegistryOperations ) );
			  ExecutingQuery executingQuery = mock( typeof( ExecutingQuery ) );
			  PropertyContainerLocker locker = new PropertyContainerLocker();
			  ThreadToStatementContextBridge txBridge = mock( typeof( ThreadToStatementContextBridge ) );

			  Statement secondStatement = mock( typeof( Statement ) );
			  KernelTransaction secondKTX = MockTransaction( secondStatement );
			  InternalTransaction secondTransaction = mock( typeof( InternalTransaction ) );
			  when( secondTransaction.TerminationReason() ).thenReturn(null);
			  QueryRegistryOperations secondQueryRegistry = mock( typeof( QueryRegistryOperations ) );

			  when( executingQuery.QueryText() ).thenReturn("X");
			  when( executingQuery.QueryParameters() ).thenReturn(EMPTY_MAP);
			  Mockito.doThrow( typeof( Exception ) ).when( initialTransaction ).close();
			  when( initialStatement.QueryRegistration() ).thenReturn(initialQueryRegistry);
			  when( queryService.BeginTransaction( transactionType, securityContext ) ).thenReturn( secondTransaction );
			  when( txBridge.GetKernelTransactionBoundToThisThread( true ) ).thenReturn( initialKTX, initialKTX, secondKTX );
			  when( txBridge.Get() ).thenReturn(secondStatement);
			  when( secondStatement.QueryRegistration() ).thenReturn(secondQueryRegistry);

			  Kernel kernel = mock( typeof( Kernel ) );
			  Neo4jTransactionalContext context = new Neo4jTransactionalContext( queryService, txBridge, locker, initialTransaction, initialStatement, executingQuery, kernel );

			  // When
			  try
			  {
					context.CommitAndRestartTx();
					throw new AssertionError( "Expected RuntimeException to be thrown" );
			  }
			  catch ( Exception )
			  {
					// Then
					object[] mocks = new object[] { txBridge, initialTransaction, initialQueryRegistry, initialKTX, secondQueryRegistry, secondKTX, secondTransaction };
					InOrder order = Mockito.inOrder( mocks );

					// (0) Constructor
					order.verify( initialTransaction ).transactionType();
					order.verify( initialTransaction ).securityContext();
					order.verify( txBridge ).getKernelTransactionBoundToThisThread( true );
					order.verify( initialTransaction ).terminationReason(); // not terminated check

					// (1) Collect statistics
					order.verify( initialKTX ).executionStatistics();

					// (2) Unbind old
					order.verify( txBridge ).getKernelTransactionBoundToThisThread( true );
					order.verify( txBridge ).unbindTransactionFromCurrentThread();

					// (3) Register and unbind new
					order.verify( txBridge ).getKernelTransactionBoundToThisThread( true );
					order.verify( secondKTX ).acquireStatement();
					order.verify( secondQueryRegistry ).registerExecutingQuery( executingQuery );
					order.verify( txBridge ).unbindTransactionFromCurrentThread();

					// (4) Rebind, unregister, and close old
					order.verify( txBridge ).bindTransactionToCurrentThread( initialKTX );
					order.verify( initialQueryRegistry ).unregisterExecutingQuery( executingQuery );
					order.verify( initialTransaction ).success();
					order.verify( initialTransaction ).close();
					order.verify( txBridge ).bindTransactionToCurrentThread( secondKTX );
					order.verify( secondTransaction ).failure();
					order.verify( secondTransaction ).close();
					order.verify( txBridge ).unbindTransactionFromCurrentThread();

					verifyNoMoreInteractions( mocks );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void accumulateExecutionStatisticOverCommitAndRestart()
		 public virtual void AccumulateExecutionStatisticOverCommitAndRestart()
		 {
			  InternalTransaction initialTransaction = mock( typeof( InternalTransaction ), new ReturnsDeepStubs() );
			  when( initialTransaction.TerminationReason() ).thenReturn(null);
			  Kernel kernel = mock( typeof( Kernel ) );
			  Neo4jTransactionalContext transactionalContext = new Neo4jTransactionalContext( _queryService, _txBridge, null, initialTransaction, _initialStatement, null, kernel );

			  _statistics.Faults = 2;
			  _statistics.Hits = 5;

			  transactionalContext.CommitAndRestartTx();

			  _statistics.Faults = 2;
			  _statistics.Hits = 5;

			  transactionalContext.CommitAndRestartTx();

			  _statistics.Faults = 2;
			  _statistics.Hits = 5;

			  StatisticProvider statisticProvider = transactionalContext.KernelStatisticProvider();

			  assertEquals( "Expect to see accumulated number of page cache misses.", 6, statisticProvider.PageCacheMisses );
			  assertEquals( "Expected to see accumulated number of page cache hits.", 15, statisticProvider.PageCacheHits );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeOpenAfterCreation()
		 public virtual void ShouldBeOpenAfterCreation()
		 {
			  InternalTransaction tx = mock( typeof( InternalTransaction ) );

			  Neo4jTransactionalContext context = NewContext( tx );

			  assertTrue( context.Open );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeTopLevelWithImplicitTx()
		 public virtual void ShouldBeTopLevelWithImplicitTx()
		 {
			  InternalTransaction tx = mock( typeof( InternalTransaction ) );
			  when( tx.TransactionType() ).thenReturn(KernelTransaction.Type.@implicit);

			  Neo4jTransactionalContext context = NewContext( tx );

			  assertTrue( context.TopLevelTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeTopLevelWithExplicitTx()
		 public virtual void ShouldNotBeTopLevelWithExplicitTx()
		 {
			  InternalTransaction tx = mock( typeof( InternalTransaction ) );
			  when( tx.TransactionType() ).thenReturn(KernelTransaction.Type.@explicit);

			  Neo4jTransactionalContext context = NewContext( tx );

			  assertFalse( context.TopLevelTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCloseTransactionDuringTermination()
		 public virtual void ShouldNotCloseTransactionDuringTermination()
		 {
			  InternalTransaction tx = mock( typeof( InternalTransaction ) );
			  when( tx.TransactionType() ).thenReturn(KernelTransaction.Type.@implicit);

			  Neo4jTransactionalContext context = NewContext( tx );

			  context.Terminate();

			  verify( tx ).terminate();
			  verify( tx, never() ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBePossibleToCloseAfterTermination()
		 public virtual void ShouldBePossibleToCloseAfterTermination()
		 {
			  InternalTransaction tx = mock( typeof( InternalTransaction ) );
			  when( tx.TransactionType() ).thenReturn(KernelTransaction.Type.@implicit);

			  Neo4jTransactionalContext context = NewContext( tx );

			  context.Terminate();

			  verify( tx ).terminate();
			  verify( tx, never() ).close();

			  context.Close( false );
			  verify( tx ).failure();
			  verify( tx ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBePossibleToTerminateWithoutActiveTransaction()
		 public virtual void ShouldBePossibleToTerminateWithoutActiveTransaction()
		 {
			  InternalTransaction tx = mock( typeof( InternalTransaction ) );
			  Neo4jTransactionalContext context = NewContext( tx );

			  context.Close( true );
			  verify( tx ).success();
			  verify( tx ).close();

			  context.Terminate();
			  verify( tx, never() ).terminate();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenRestartedAfterTermination()
		 public virtual void ShouldThrowWhenRestartedAfterTermination()
		 {
			  MutableObject<Status> terminationReason = new MutableObject<Status>();
			  InternalTransaction tx = mock( typeof( InternalTransaction ) );
			  doAnswer(invocation =>
			  {
				terminationReason.Value = Status.Transaction.Terminated;
				return null;
			  }).when( tx ).terminate();
			  when( tx.TerminationReason() ).then(invocation => Optional.ofNullable(terminationReason.Value));

			  Neo4jTransactionalContext context = NewContext( tx );

			  context.Terminate();

			  try
			  {
					context.CommitAndRestartTx();
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( TransactionTerminatedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenGettingTxAfterTermination()
		 public virtual void ShouldThrowWhenGettingTxAfterTermination()
		 {
			  MutableObject<Status> terminationReason = new MutableObject<Status>();
			  InternalTransaction tx = mock( typeof( InternalTransaction ) );
			  doAnswer(invocation =>
			  {
				terminationReason.Value = Status.Transaction.Terminated;
				return null;
			  }).when( tx ).terminate();
			  when( tx.TerminationReason() ).then(invocation => Optional.ofNullable(terminationReason.Value));

			  Neo4jTransactionalContext context = NewContext( tx );

			  context.Terminate();

			  try
			  {
					context.OrBeginNewIfClosed;
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( TransactionTerminatedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBePossibleToCloseMultipleTimes()
		 public virtual void ShouldNotBePossibleToCloseMultipleTimes()
		 {
			  InternalTransaction tx = mock( typeof( InternalTransaction ) );
			  Neo4jTransactionalContext context = NewContext( tx );

			  context.Close( false );
			  context.Close( true );
			  context.Close( false );

			  verify( tx ).failure();
			  verify( tx, never() ).success();
			  verify( tx ).close();
		 }

		 private void SetUpMocks()
		 {
			  _queryService = mock( typeof( GraphDatabaseQueryService ) );
			  DependencyResolver resolver = mock( typeof( DependencyResolver ) );
			  _txBridge = mock( typeof( ThreadToStatementContextBridge ) );
			  _initialStatement = mock( typeof( KernelStatement ) );

			  _statistics = new ConfiguredExecutionStatistics( this );
			  QueryRegistryOperations queryRegistryOperations = mock( typeof( QueryRegistryOperations ) );
			  InternalTransaction internalTransaction = mock( typeof( InternalTransaction ) );
			  when( internalTransaction.TerminationReason() ).thenReturn(null);

			  when( _initialStatement.queryRegistration() ).thenReturn(queryRegistryOperations);
			  when( _queryService.DependencyResolver ).thenReturn( resolver );
			  when( resolver.ResolveDependency( typeof( ThreadToStatementContextBridge ) ) ).thenReturn( _txBridge );
			  when( _queryService.beginTransaction( any(), any() ) ).thenReturn(internalTransaction);

			  KernelTransaction mockTransaction = mockTransaction( _initialStatement );
			  when( _txBridge.get() ).thenReturn(_initialStatement);
			  when( _txBridge.getKernelTransactionBoundToThisThread( true ) ).thenReturn( mockTransaction );
		 }

		 private Neo4jTransactionalContext NewContext( InternalTransaction initialTx )
		 {
			  return new Neo4jTransactionalContext( _queryService, _txBridge, new PropertyContainerLocker(), initialTx, _initialStatement, null, null );
		 }

		 private KernelTransaction MockTransaction( Statement statement )
		 {
			  KernelTransaction kernelTransaction = mock( typeof( KernelTransaction ), new ReturnsDeepStubs() );
			  when( kernelTransaction.ExecutionStatistics() ).thenReturn(_statistics);
			  when( kernelTransaction.AcquireStatement() ).thenReturn(statement);
			  return kernelTransaction;
		 }

		 private class ConfiguredExecutionStatistics : ExecutionStatistics
		 {
			 private readonly Neo4jTransactionalContextTest _outerInstance;

			 public ConfiguredExecutionStatistics( Neo4jTransactionalContextTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long HitsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long FaultsConflict;

			  public override long PageHits()
			  {
					return HitsConflict;
			  }

			  public override long PageFaults()
			  {
					return FaultsConflict;
			  }

			  internal virtual long Hits
			  {
				  set
				  {
						this.HitsConflict = value;
				  }
			  }

			  internal virtual long Faults
			  {
				  set
				  {
						this.FaultsConflict = value;
				  }
			  }
		 }
	}

}