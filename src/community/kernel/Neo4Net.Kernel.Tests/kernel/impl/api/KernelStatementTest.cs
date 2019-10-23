﻿/*
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
	using Test = org.junit.Test;

	using NotInTransactionException = Neo4Net.GraphDb.NotInTransactionException;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using TxStateHolder = Neo4Net.Kernel.api.txstate.TxStateHolder;
	using CpuClock = Neo4Net.Resources.CpuClock;
	using HeapAllocation = Neo4Net.Resources.HeapAllocation;
	using StorageReader = Neo4Net.Kernel.Api.StorageEngine.StorageReader;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class KernelStatementTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReleaseStorageReaderWhenForceClosed()
		 public virtual void ShouldReleaseStorageReaderWhenForceClosed()
		 {
			  // given
			  StorageReader storeStatement = mock( typeof( StorageReader ) );
			  KernelStatement statement = new KernelStatement( mock( typeof( KernelTransactionImplementation ) ), null, storeStatement, LockTracer.NONE, mock( typeof( StatementOperationParts ) ), new ClockContext(), EmptyVersionContextSupplier.EMPTY );
			  statement.Acquire();

			  // when
			  try
			  {
					statement.ForceClose();
			  }
			  catch ( KernelStatement.StatementNotClosedException )
			  {
					// ignore
			  }

			  // then
			  verify( storeStatement ).release();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.Neo4Net.graphdb.NotInTransactionException.class) public void assertStatementIsNotOpenWhileAcquireIsNotInvoked()
		 public virtual void AssertStatementIsNotOpenWhileAcquireIsNotInvoked()
		 {
			  KernelTransactionImplementation transaction = mock( typeof( KernelTransactionImplementation ) );
			  TxStateHolder txStateHolder = mock( typeof( TxStateHolder ) );
			  StorageReader storeStatement = mock( typeof( StorageReader ) );
			  KernelStatement statement = new KernelStatement( transaction, txStateHolder, storeStatement, LockTracer.NONE, mock( typeof( StatementOperationParts ) ), new ClockContext(), EmptyVersionContextSupplier.EMPTY );

			  statement.AssertOpen();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reportQueryWaitingTimeToTransactionStatisticWhenFinishQueryExecution()
		 public virtual void ReportQueryWaitingTimeToTransactionStatisticWhenFinishQueryExecution()
		 {
			  KernelTransactionImplementation transaction = mock( typeof( KernelTransactionImplementation ) );
			  TxStateHolder txStateHolder = mock( typeof( TxStateHolder ) );
			  StorageReader storeStatement = mock( typeof( StorageReader ) );

			  KernelTransactionImplementation.Statistics statistics = new KernelTransactionImplementation.Statistics( transaction, new AtomicReference<CpuClock>( CpuClock.NOT_AVAILABLE ), new AtomicReference<HeapAllocation>( HeapAllocation.NOT_AVAILABLE ) );
			  when( transaction.GetStatistics() ).thenReturn(statistics);
			  when( transaction.ExecutingQueries() ).thenReturn(ExecutingQueryList.EMPTY);

			  KernelStatement statement = new KernelStatement( transaction, txStateHolder, storeStatement, LockTracer.NONE, mock( typeof( StatementOperationParts ) ), new ClockContext(), EmptyVersionContextSupplier.EMPTY );
			  statement.Acquire();

			  ExecutingQuery query = QueryWithWaitingTime;
			  ExecutingQuery query2 = QueryWithWaitingTime;
			  ExecutingQuery query3 = QueryWithWaitingTime;

			  statement.StopQueryExecution( query );
			  statement.StopQueryExecution( query2 );
			  statement.StopQueryExecution( query3 );

			  assertEquals( 3, statistics.GetWaitingTimeNanos( 1 ) );
		 }

		 private ExecutingQuery QueryWithWaitingTime
		 {
			 get
			 {
				  ExecutingQuery executingQuery = mock( typeof( ExecutingQuery ) );
				  when( executingQuery.ReportedWaitingTimeNanos() ).thenReturn(1L);
				  return executingQuery;
			 }
		 }
	}

}