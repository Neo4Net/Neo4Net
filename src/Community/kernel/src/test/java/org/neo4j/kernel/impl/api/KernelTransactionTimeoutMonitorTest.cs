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
namespace Neo4Net.Kernel.Impl.Api
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;


	using KernelTransactionHandle = Neo4Net.Kernel.api.KernelTransactionHandle;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using KernelTransactionMonitor = Neo4Net.Kernel.Impl.Api.transaciton.monitor.KernelTransactionMonitor;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using SimpleLogService = Neo4Net.Logging.@internal.SimpleLogService;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class KernelTransactionTimeoutMonitorTest
	{
		 private const int EXPECTED_REUSE_COUNT = 2;
		 private KernelTransactions _kernelTransactions;
		 private FakeClock _fakeClock;
		 private AssertableLogProvider _logProvider;
		 private LogService _logService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _kernelTransactions = mock( typeof( KernelTransactions ) );
			  _fakeClock = Clocks.fakeClock();
			  _logProvider = new AssertableLogProvider();
			  _logService = new SimpleLogService( _logProvider, _logProvider );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void terminateExpiredTransactions()
		 internal virtual void TerminateExpiredTransactions()
		 {
			  HashSet<KernelTransactionHandle> transactions = new HashSet<KernelTransactionHandle>();
			  KernelTransactionImplementation tx1 = PrepareTxMock( 3, 1, 3 );
			  KernelTransactionImplementation tx2 = PrepareTxMock( 4, 1, 8 );
			  KernelTransactionImplementationHandle handle1 = new KernelTransactionImplementationHandle( tx1, _fakeClock );
			  KernelTransactionImplementationHandle handle2 = new KernelTransactionImplementationHandle( tx2, _fakeClock );
			  transactions.Add( handle1 );
			  transactions.Add( handle2 );

			  when( _kernelTransactions.activeTransactions() ).thenReturn(transactions);

			  KernelTransactionMonitor transactionMonitor = BuildTransactionMonitor();

			  _fakeClock.forward( 3, TimeUnit.MILLISECONDS );
			  transactionMonitor.Run();

			  verify( tx1, never() ).markForTermination(Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionTimedOut);
			  verify( tx2, never() ).markForTermination(Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionTimedOut);
			  _logProvider.rawMessageMatcher().assertNotContains("timeout");

			  _fakeClock.forward( 2, TimeUnit.MILLISECONDS );
			  transactionMonitor.Run();

			  verify( tx1 ).markForTermination( EXPECTED_REUSE_COUNT, Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionTimedOut );
			  verify( tx2, never() ).markForTermination(Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionTimedOut);
			  _logProvider.rawMessageMatcher().assertContains("timeout");

			  _logProvider.clear();
			  _fakeClock.forward( 10, TimeUnit.MILLISECONDS );
			  transactionMonitor.Run();

			  verify( tx2 ).markForTermination( EXPECTED_REUSE_COUNT, Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionTimedOut );
			  _logProvider.rawMessageMatcher().assertContains("timeout");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void skipTransactionWithoutTimeout()
		 internal virtual void SkipTransactionWithoutTimeout()
		 {
			  HashSet<KernelTransactionHandle> transactions = new HashSet<KernelTransactionHandle>();
			  KernelTransactionImplementation tx1 = PrepareTxMock( 7, 3, 0 );
			  KernelTransactionImplementation tx2 = PrepareTxMock( 8, 4, 0 );
			  KernelTransactionImplementationHandle handle1 = new KernelTransactionImplementationHandle( tx1, _fakeClock );
			  KernelTransactionImplementationHandle handle2 = new KernelTransactionImplementationHandle( tx2, _fakeClock );
			  transactions.Add( handle1 );
			  transactions.Add( handle2 );

			  when( _kernelTransactions.activeTransactions() ).thenReturn(transactions);

			  KernelTransactionMonitor transactionMonitor = BuildTransactionMonitor();

			  _fakeClock.forward( 300, TimeUnit.MILLISECONDS );
			  transactionMonitor.Run();

			  verify( tx1, never() ).markForTermination(Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionTimedOut);
			  verify( tx2, never() ).markForTermination(Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionTimedOut);
			  _logProvider.rawMessageMatcher().assertNotContains("timeout");
		 }

		 private KernelTransactionMonitor BuildTransactionMonitor()
		 {
			  return new KernelTransactionMonitor( _kernelTransactions, _fakeClock, _logService );
		 }

		 private static KernelTransactionImplementation PrepareTxMock( long userTxId, long startMillis, long timeoutMillis )
		 {
			  KernelTransactionImplementation transaction = mock( typeof( KernelTransactionImplementation ) );
			  when( transaction.StartTime() ).thenReturn(startMillis);
			  when( transaction.UserTransactionId() ).thenReturn(userTxId);
			  when( transaction.ReuseCount ).thenReturn( EXPECTED_REUSE_COUNT );
			  when( transaction.Timeout() ).thenReturn(timeoutMillis);
			  when( transaction.MarkForTermination( EXPECTED_REUSE_COUNT, Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionTimedOut ) ).thenReturn( true );
			  return transaction;
		 }
	}

}