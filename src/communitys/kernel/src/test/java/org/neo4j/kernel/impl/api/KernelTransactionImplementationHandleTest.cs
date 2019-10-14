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
	using Test = org.junit.Test;

	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Clocks = Neo4Net.Time.Clocks;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class KernelTransactionImplementationHandleTest
	{
		 private readonly SystemNanoClock _clock = Clocks.nanoClock();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void returnsCorrectLastTransactionTimestampWhenStarted()
		 public virtual void ReturnsCorrectLastTransactionTimestampWhenStarted()
		 {
			  long lastCommittedTxTimestamp = 42;

			  KernelTransactionImplementation tx = mock( typeof( KernelTransactionImplementation ) );
			  when( tx.LastTransactionTimestampWhenStarted() ).thenReturn(lastCommittedTxTimestamp);
			  when( tx.Open ).thenReturn( true );

			  KernelTransactionImplementationHandle handle = new KernelTransactionImplementationHandle( tx, _clock );

			  assertEquals( lastCommittedTxTimestamp, handle.LastTransactionTimestampWhenStarted() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void returnsCorrectLastTransactionTimestampWhenStartedForClosedTx()
		 public virtual void ReturnsCorrectLastTransactionTimestampWhenStartedForClosedTx()
		 {
			  long lastCommittedTxTimestamp = 4242;

			  KernelTransactionImplementation tx = mock( typeof( KernelTransactionImplementation ) );
			  when( tx.LastTransactionTimestampWhenStarted() ).thenReturn(lastCommittedTxTimestamp);
			  when( tx.Open ).thenReturn( false );

			  KernelTransactionImplementationHandle handle = new KernelTransactionImplementationHandle( tx, _clock );

			  assertEquals( lastCommittedTxTimestamp, handle.LastTransactionTimestampWhenStarted() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void isOpenForUnchangedKernelTransactionImplementation()
		 public virtual void isOpenForUnchangedKernelTransactionImplementation()
		 {
			  int reuseCount = 42;

			  KernelTransactionImplementation tx = mock( typeof( KernelTransactionImplementation ) );
			  when( tx.Open ).thenReturn( true );
			  when( tx.ReuseCount ).thenReturn( reuseCount );

			  KernelTransactionImplementationHandle handle = new KernelTransactionImplementationHandle( tx, _clock );

			  assertTrue( handle.Open );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void isOpenForReusedKernelTransactionImplementation()
		 public virtual void isOpenForReusedKernelTransactionImplementation()
		 {
			  int initialReuseCount = 42;
			  int nextReuseCount = 4242;

			  KernelTransactionImplementation tx = mock( typeof( KernelTransactionImplementation ) );
			  when( tx.Open ).thenReturn( true );
			  when( tx.ReuseCount ).thenReturn( initialReuseCount ).thenReturn( nextReuseCount );

			  KernelTransactionImplementationHandle handle = new KernelTransactionImplementationHandle( tx, _clock );

			  assertFalse( handle.Open );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void markForTerminationCallsKernelTransactionImplementation()
		 public virtual void MarkForTerminationCallsKernelTransactionImplementation()
		 {
			  int reuseCount = 42;
			  Neo4Net.Kernel.Api.Exceptions.Status_Transaction terminationReason = Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated;

			  KernelTransactionImplementation tx = mock( typeof( KernelTransactionImplementation ) );
			  when( tx.ReuseCount ).thenReturn( reuseCount );

			  KernelTransactionImplementationHandle handle = new KernelTransactionImplementationHandle( tx, _clock );
			  handle.MarkForTermination( terminationReason );

			  verify( tx ).markForTermination( reuseCount, terminationReason );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void markForTerminationReturnsTrueWhenSuccessful()
		 public virtual void MarkForTerminationReturnsTrueWhenSuccessful()
		 {
			  KernelTransactionImplementation tx = mock( typeof( KernelTransactionImplementation ) );
			  when( tx.ReuseCount ).thenReturn( 42 );
			  when( tx.MarkForTermination( anyLong(), any() ) ).thenReturn(true);

			  KernelTransactionImplementationHandle handle = new KernelTransactionImplementationHandle( tx, _clock );
			  assertTrue( handle.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void markForTerminationReturnsFalseWhenNotSuccessful()
		 public virtual void MarkForTerminationReturnsFalseWhenNotSuccessful()
		 {
			  KernelTransactionImplementation tx = mock( typeof( KernelTransactionImplementation ) );
			  when( tx.ReuseCount ).thenReturn( 42 );
			  when( tx.MarkForTermination( anyLong(), any() ) ).thenReturn(false);

			  KernelTransactionImplementationHandle handle = new KernelTransactionImplementationHandle( tx, _clock );
			  assertFalse( handle.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionStatisticForReusedTransactionIsNotAvailable()
		 public virtual void TransactionStatisticForReusedTransactionIsNotAvailable()
		 {
			  KernelTransactionImplementation tx = mock( typeof( KernelTransactionImplementation ) );
			  when( tx.Open ).thenReturn( true );
			  when( tx.ReuseCount ).thenReturn( 2 ).thenReturn( 3 );

			  KernelTransactionImplementationHandle handle = new KernelTransactionImplementationHandle( tx, _clock );
			  assertSame( TransactionExecutionStatistic.NotAvailable, handle.TransactionStatistic() );
		 }
	}

}