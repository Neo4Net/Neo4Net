using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.enterprise.builtinprocs
{
	using Test = org.junit.Test;


	using Neo4Net.Collections.Pooling;
	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using KernelTransactionHandle = Neo4Net.Kernel.api.KernelTransactionHandle;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using AutoIndexing = Neo4Net.Kernel.api.explicitindex.AutoIndexing;
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using QuerySnapshot = Neo4Net.Kernel.api.query.QuerySnapshot;
	using AuxiliaryTransactionStateManager = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateManager;
	using Config = Neo4Net.Kernel.configuration.Config;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using SchemaState = Neo4Net.Kernel.Impl.Api.SchemaState;
	using SchemaWriteGuard = Neo4Net.Kernel.Impl.Api.SchemaWriteGuard;
	using StatementOperationParts = Neo4Net.Kernel.Impl.Api.StatementOperationParts;
	using TestKernelTransactionHandle = Neo4Net.Kernel.Impl.Api.TestKernelTransactionHandle;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionExecutionStatistic = Neo4Net.Kernel.Impl.Api.TransactionExecutionStatistic;
	using TransactionHooks = Neo4Net.Kernel.Impl.Api.TransactionHooks;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using ConstraintIndexCreator = Neo4Net.Kernel.Impl.Api.state.ConstraintIndexCreator;
	using StandardConstraintSemantics = Neo4Net.Kernel.impl.constraints.StandardConstraintSemantics;
	using CanWrite = Neo4Net.Kernel.impl.factory.CanWrite;
	using ExplicitIndexStore = Neo4Net.Kernel.impl.index.ExplicitIndexStore;
	using ActiveLock = Neo4Net.Kernel.impl.locking.ActiveLock;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using HttpConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.HttpConnectionInfo;
	using TransactionHeaderInformationFactory = Neo4Net.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using DatabaseTransactionStats = Neo4Net.Kernel.impl.transaction.stats.DatabaseTransactionStats;
	using TransactionTracer = Neo4Net.Kernel.impl.transaction.tracing.TransactionTracer;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using CpuClock = Neo4Net.Resources.CpuClock;
	using HeapAllocation = Neo4Net.Resources.HeapAllocation;
	using StorageEngine = Neo4Net.Kernel.Api.StorageEngine.StorageEngine;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using Clocks = Neo4Net.Time.Clocks;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.StringUtils.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.util.collection.CollectionsFactorySupplier_Fields.ON_HEAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.MockedNeoStores.mockedTokenHolders;

	public class TransactionStatusResultTest
	{
		private bool InstanceFieldsInitialized = false;

		public TransactionStatusResultTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_blockerResolver = new TransactionDependenciesResolver( _snapshotsMap );
		}


		 private TestKernelTransactionHandle _transactionHandle = new TransactionHandleWithLocks( new StubKernelTransaction() );
		 private Dictionary<KernelTransactionHandle, IList<QuerySnapshot>> _snapshotsMap = new Dictionary<KernelTransactionHandle, IList<QuerySnapshot>>();
		 private TransactionDependenciesResolver _blockerResolver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void statusOfTransactionWithSingleQuery() throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StatusOfTransactionWithSingleQuery()
		 {
			  _snapshotsMap[_transactionHandle] = singletonList( CreateQuerySnapshot( 7L ) );
			  TransactionStatusResult statusResult = new TransactionStatusResult( _transactionHandle, _blockerResolver, _snapshotsMap, ZoneId.of( "UTC" ) );

			  CheckTransactionStatus( statusResult, "testQuery", "query-7", "1970-01-01T00:00:01.984Z" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void statusOfTransactionWithoutRunningQuery() throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StatusOfTransactionWithoutRunningQuery()
		 {
			  _snapshotsMap[_transactionHandle] = emptyList();
			  TransactionStatusResult statusResult = new TransactionStatusResult( _transactionHandle, _blockerResolver, _snapshotsMap, ZoneId.of( "UTC" ) );

			  CheckTransactionStatusWithoutQueries( statusResult );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void statusOfTransactionWithMultipleQueries() throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StatusOfTransactionWithMultipleQueries()
		 {
			  _snapshotsMap[_transactionHandle] = new IList<QuerySnapshot> { CreateQuerySnapshot( 7L ), CreateQuerySnapshot( 8L ) };
			  TransactionStatusResult statusResult = new TransactionStatusResult( _transactionHandle, _blockerResolver, _snapshotsMap, ZoneId.of( "UTC" ) );

			  CheckTransactionStatus( statusResult, "testQuery", "query-7", "1970-01-01T00:00:01.984Z" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void statusOfTransactionWithDifferentTimeZone() throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StatusOfTransactionWithDifferentTimeZone()
		 {
			  _snapshotsMap[_transactionHandle] = singletonList( CreateQuerySnapshot( 7L ) );
			  TransactionStatusResult statusResult = new TransactionStatusResult( _transactionHandle, _blockerResolver, _snapshotsMap, ZoneId.of( "UTC+1" ) );

			  CheckTransactionStatus( statusResult, "testQuery", "query-7", "1970-01-01T01:00:01.984+01:00" );
		 }

		 private void CheckTransactionStatusWithoutQueries( TransactionStatusResult statusResult )
		 {
			  assertEquals( "transaction-8", statusResult.TransactionId );
			  assertEquals( "testUser", statusResult.Username );
			  assertEquals( Collections.emptyMap(), statusResult.MetaData );
			  assertEquals( "1970-01-01T00:00:01.984Z", statusResult.StartTime );
			  assertEquals( EMPTY, statusResult.Protocol );
			  assertEquals( EMPTY, statusResult.ConnectionId );
			  assertEquals( EMPTY, statusResult.ClientAddress );
			  assertEquals( EMPTY, statusResult.RequestUri );
			  assertEquals( EMPTY, statusResult.CurrentQueryId );
			  assertEquals( EMPTY, statusResult.CurrentQuery );
			  assertEquals( 1, statusResult.ActiveLockCount );
			  assertEquals( "Running", statusResult.Status );
			  assertEquals( Collections.emptyMap(), statusResult.ResourceInformation );
			  assertEquals( 1810L, statusResult.ElapsedTimeMillis );
			  assertEquals( Convert.ToInt64( 1L ), statusResult.CpuTimeMillis );
			  assertEquals( 0L, statusResult.WaitTimeMillis );
			  assertEquals( Convert.ToInt64( 1809 ), statusResult.IdleTimeMillis );
			  assertEquals( Convert.ToInt64( 1 ), statusResult.AllocatedBytes );
			  assertEquals( Convert.ToInt64( 0 ), statusResult.AllocatedDirectBytes );
			  assertEquals( 0L, statusResult.PageHits );
			  assertEquals( 0L, statusResult.PageFaults );
		 }

		 private void CheckTransactionStatus( TransactionStatusResult statusResult, string currentQuery, string currentQueryId, string startTime )
		 {
			  assertEquals( "transaction-8", statusResult.TransactionId );
			  assertEquals( "testUser", statusResult.Username );
			  assertEquals( Collections.emptyMap(), statusResult.MetaData );
			  assertEquals( startTime, statusResult.StartTime );
			  assertEquals( "https", statusResult.Protocol );
			  assertEquals( "https-42", statusResult.ConnectionId );
			  assertEquals( "localhost:1000", statusResult.ClientAddress );
			  assertEquals( "https://localhost:1001/path", statusResult.RequestUri );
			  assertEquals( currentQueryId, statusResult.CurrentQueryId );
			  assertEquals( currentQuery, statusResult.CurrentQuery );
			  assertEquals( 1, statusResult.ActiveLockCount );
			  assertEquals( "Running", statusResult.Status );
			  assertEquals( Collections.emptyMap(), statusResult.ResourceInformation );
			  assertEquals( 1810, statusResult.ElapsedTimeMillis );
			  assertEquals( Convert.ToInt64( 1 ), statusResult.CpuTimeMillis );
			  assertEquals( 0L, statusResult.WaitTimeMillis );
			  assertEquals( Convert.ToInt64( 1809 ), statusResult.IdleTimeMillis );
			  assertEquals( Convert.ToInt64( 1 ), statusResult.AllocatedBytes );
			  assertEquals( Convert.ToInt64( 0 ), statusResult.AllocatedDirectBytes );
			  assertEquals( 0, statusResult.PageHits );
			  assertEquals( 0, statusResult.PageFaults );
		 }

		 private QuerySnapshot CreateQuerySnapshot( long queryId )
		 {
			  ExecutingQuery executingQuery = CreateExecutingQuery( queryId );
			  return executingQuery.Snapshot();
		 }

		 private ExecutingQuery CreateExecutingQuery( long queryId )
		 {
			  return new ExecutingQuery( queryId, TestConnectionInfo, "testUser", "testQuery", VirtualValues.EMPTY_MAP, Collections.emptyMap(), () => 1L, PageCursorTracer.NULL, Thread.CurrentThread.Id, Thread.CurrentThread.Name, new CountingNanoClock(), new CountingCpuClock(), new CountingHeapAllocation() );
		 }

		 private HttpConnectionInfo TestConnectionInfo
		 {
			 get
			 {
				  return new HttpConnectionInfo( "https-42", "https", new InetSocketAddress( "localhost", 1000 ), new InetSocketAddress( "localhost", 1001 ), "/path" );
			 }
		 }

		 private class TransactionHandleWithLocks : TestKernelTransactionHandle
		 {

			  internal TransactionHandleWithLocks( KernelTransaction tx ) : base( tx )
			  {
			  }

			  public override Stream<ActiveLock> ActiveLocks()
			  {
					return Stream.of( ActiveLock.sharedLock( ResourceTypes.NODE, 3 ) );
			  }

			  public override TransactionExecutionStatistic TransactionStatistic()
			  {
					KernelTransactionImplementation transaction = new KernelTransactionImplementationAnonymousInnerClass( this, Config.defaults(), mock(typeof(StatementOperationParts)), mock(typeof(SchemaWriteGuard)), mock(typeof(ConstraintIndexCreator)), TransactionHeaderInformationFactory.DEFAULT, mock(typeof(TransactionCommitProcess)), mock(typeof(AuxiliaryTransactionStateManager)), mock(typeof(Pool)), Clocks.fakeClock(), Neo4Net.Kernel.impl.transaction.tracing.TransactionTracer_Fields.Null, LockTracer.NONE, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, mock(typeof(StorageEngine), RETURNS_MOCKS), AutoIndexing.UNSUPPORTED, mock(typeof(ExplicitIndexStore)), EmptyVersionContextSupplier.EMPTY, ON_HEAP, mock(typeof(SchemaState)), mock(typeof(IndexingService)), mockedTokenHolders() );
					return new TransactionExecutionStatistic( transaction, Clocks.fakeClock().forward(2010, MILLISECONDS), 200 );
			  }

			  private class KernelTransactionImplementationAnonymousInnerClass : KernelTransactionImplementation
			  {
				  private readonly TransactionHandleWithLocks _outerInstance;

				  public KernelTransactionImplementationAnonymousInnerClass( TransactionHandleWithLocks outerInstance, Config defaults, UnknownType mock, UnknownType mock, UnknownType mock, UnknownType @default, UnknownType mock, UnknownType mock, UnknownType mock, Neo4Net.Time.FakeClock fakeClock, TransactionTracer @null, UnknownType none, PageCursorTracerSupplier @null, UnknownType mock, UnknownType unsupported, UnknownType mock, Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier empty, UnknownType onHeap, UnknownType mock, UnknownType mock, UnknownType mockedTokenHolders ) : base( defaults, mock, mock, new TransactionHooks(), mock, new Procedures(), @default, mock, new DatabaseTransactionStats(), mock, mock, fakeClock, new AtomicReference<CpuClock>(CpuClock.NOT_AVAILABLE), new AtomicReference<HeapAllocation>(HeapAllocation.NOT_AVAILABLE), @null, none, @null, mock, new CanWrite(), unsupported, mock, empty, onHeap, new StandardConstraintSemantics(), mock, mock, mockedTokenHolders, new Dependencies() )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public override Statistics Statistics
				  {
					  get
					  {
							TestStatistics statistics = new TestStatistics( this, new AtomicReference<CpuClock>( new CountingCpuClock() ), new AtomicReference<HeapAllocation>(new CountingHeapAllocation()) );
							statistics.Init( Thread.CurrentThread.Id, PageCursorTracer.NULL );
							return statistics;
					  }
				  }
			  }
		 }

		 private class TestStatistics : KernelTransactionImplementation.Statistics
		 {
			  internal TestStatistics( KernelTransactionImplementation transaction, AtomicReference<CpuClock> cpuClockRef, AtomicReference<HeapAllocation> heapAllocationRef ) : base( transaction, cpuClockRef, heapAllocationRef )
			  {
			  }

			  protected internal override void Init( long threadId, PageCursorTracer pageCursorTracer )
			  {
					base.Init( threadId, pageCursorTracer );
			  }
		 }

		 private class CountingNanoClock : SystemNanoClock
		 {
			  internal long Time;

			  public override long Nanos()
			  {
					Time += MILLISECONDS.toNanos( 1 );
					return Time;
			  }
		 }

		 private class CountingCpuClock : CpuClock
		 {
			  internal long CpuTime;

			  public override long CpuTimeNanos( long threadId )
			  {
					CpuTime += MILLISECONDS.toNanos( 1 );
					return CpuTime;
			  }
		 }

		 private class CountingHeapAllocation : HeapAllocation
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long AllocatedBytesConflict;

			  public override long AllocatedBytes( long threadId )
			  {
					return AllocatedBytesConflict++;
			  }
		 }
	}

}