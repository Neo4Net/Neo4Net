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


	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using KernelTransactionHandle = Neo4Net.Kernel.Api.KernelTransactionHandle;
	using ExecutingQuery = Neo4Net.Kernel.Api.query.ExecutingQuery;
	using QuerySnapshot = Neo4Net.Kernel.Api.query.QuerySnapshot;
	using TestKernelTransactionHandle = Neo4Net.Kernel.Impl.Api.TestKernelTransactionHandle;
	using ActiveLock = Neo4Net.Kernel.impl.locking.ActiveLock;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using CpuClock = Neo4Net.Resources.CpuClock;
	using HeapAllocation = Neo4Net.Resources.HeapAllocation;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;
	using Clocks = Neo4Net.Time.Clocks;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isEmptyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class TransactionDependenciesResolverTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void detectIndependentTransactionsAsNotBlocked()
		 public virtual void DetectIndependentTransactionsAsNotBlocked()
		 {
			  Dictionary<KernelTransactionHandle, IList<QuerySnapshot>> map = new Dictionary<KernelTransactionHandle, IList<QuerySnapshot>>();
			  TestKernelTransactionHandle handle1 = new TestKernelTransactionHandleWithLocks( new StubKernelTransaction() );
			  TestKernelTransactionHandle handle2 = new TestKernelTransactionHandleWithLocks( new StubKernelTransaction() );

			  map[handle1] = singletonList( CreateQuerySnapshot( 1 ) );
			  map[handle2] = singletonList( CreateQuerySnapshot( 2 ) );
			  TransactionDependenciesResolver resolver = new TransactionDependenciesResolver( map );

			  assertFalse( resolver.IsBlocked( handle1 ) );
			  assertFalse( resolver.IsBlocked( handle2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void detectBlockedTransactionsByExclusiveLock()
		 public virtual void DetectBlockedTransactionsByExclusiveLock()
		 {
			  Dictionary<KernelTransactionHandle, IList<QuerySnapshot>> map = new Dictionary<KernelTransactionHandle, IList<QuerySnapshot>>();
			  TestKernelTransactionHandle handle1 = new TestKernelTransactionHandleWithLocks( new StubKernelTransaction(), 0, singletonList(ActiveLock.exclusiveLock(ResourceTypes.NODE, 1)) );
			  TestKernelTransactionHandle handle2 = new TestKernelTransactionHandleWithLocks( new StubKernelTransaction() );

			  map[handle1] = singletonList( CreateQuerySnapshot( 1 ) );
			  map[handle2] = singletonList( CreateQuerySnapshotWaitingForLock( 2, false, ResourceTypes.NODE, 1 ) );
			  TransactionDependenciesResolver resolver = new TransactionDependenciesResolver( map );

			  assertFalse( resolver.IsBlocked( handle1 ) );
			  assertTrue( resolver.IsBlocked( handle2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void detectBlockedTransactionsBySharedLock()
		 public virtual void DetectBlockedTransactionsBySharedLock()
		 {
			  Dictionary<KernelTransactionHandle, IList<QuerySnapshot>> map = new Dictionary<KernelTransactionHandle, IList<QuerySnapshot>>();
			  TestKernelTransactionHandle handle1 = new TestKernelTransactionHandleWithLocks( new StubKernelTransaction(), 0, singletonList(ActiveLock.sharedLock(ResourceTypes.NODE, 1)) );
			  TestKernelTransactionHandle handle2 = new TestKernelTransactionHandleWithLocks( new StubKernelTransaction() );

			  map[handle1] = singletonList( CreateQuerySnapshot( 1 ) );
			  map[handle2] = singletonList( CreateQuerySnapshotWaitingForLock( 2, true, ResourceTypes.NODE, 1 ) );
			  TransactionDependenciesResolver resolver = new TransactionDependenciesResolver( map );

			  assertFalse( resolver.IsBlocked( handle1 ) );
			  assertTrue( resolver.IsBlocked( handle2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void blockingChainDescriptionForIndependentTransactionsIsEmpty()
		 public virtual void BlockingChainDescriptionForIndependentTransactionsIsEmpty()
		 {
			  Dictionary<KernelTransactionHandle, IList<QuerySnapshot>> map = new Dictionary<KernelTransactionHandle, IList<QuerySnapshot>>();
			  TestKernelTransactionHandle handle1 = new TestKernelTransactionHandleWithLocks( new StubKernelTransaction() );
			  TestKernelTransactionHandle handle2 = new TestKernelTransactionHandleWithLocks( new StubKernelTransaction() );

			  map[handle1] = singletonList( CreateQuerySnapshot( 1 ) );
			  map[handle2] = singletonList( CreateQuerySnapshot( 2 ) );
			  TransactionDependenciesResolver resolver = new TransactionDependenciesResolver( map );

			  assertThat( resolver.DescribeBlockingTransactions( handle1 ), EmptyString );
			  assertThat( resolver.DescribeBlockingTransactions( handle2 ), EmptyString );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void blockingChainDescriptionForDirectlyBlockedTransaction()
		 public virtual void BlockingChainDescriptionForDirectlyBlockedTransaction()
		 {
			  Dictionary<KernelTransactionHandle, IList<QuerySnapshot>> map = new Dictionary<KernelTransactionHandle, IList<QuerySnapshot>>();
			  TestKernelTransactionHandle handle1 = new TestKernelTransactionHandleWithLocks( new StubKernelTransaction(), 3, singletonList(ActiveLock.exclusiveLock(ResourceTypes.NODE, 1)) );
			  TestKernelTransactionHandle handle2 = new TestKernelTransactionHandleWithLocks( new StubKernelTransaction() );

			  map[handle1] = singletonList( CreateQuerySnapshot( 1 ) );
			  map[handle2] = singletonList( CreateQuerySnapshotWaitingForLock( 2, false, ResourceTypes.NODE, 1 ) );
			  TransactionDependenciesResolver resolver = new TransactionDependenciesResolver( map );

			  assertThat( resolver.DescribeBlockingTransactions( handle1 ), EmptyString );
			  assertEquals( "[transaction-3]", resolver.DescribeBlockingTransactions( handle2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void blockingChainDescriptionForChainedBlockedTransaction()
		 public virtual void BlockingChainDescriptionForChainedBlockedTransaction()
		 {
			  Dictionary<KernelTransactionHandle, IList<QuerySnapshot>> map = new Dictionary<KernelTransactionHandle, IList<QuerySnapshot>>();
			  TestKernelTransactionHandle handle1 = new TestKernelTransactionHandleWithLocks( new StubKernelTransaction(), 4, singletonList(ActiveLock.exclusiveLock(ResourceTypes.NODE, 1)) );
			  TestKernelTransactionHandle handle2 = new TestKernelTransactionHandleWithLocks( new StubKernelTransaction(), 5, singletonList(ActiveLock.sharedLock(ResourceTypes.NODE, 2)) );
			  TestKernelTransactionHandle handle3 = new TestKernelTransactionHandleWithLocks( new StubKernelTransaction(), 6 );

			  map[handle1] = singletonList( CreateQuerySnapshot( 1 ) );
			  map[handle2] = singletonList( CreateQuerySnapshotWaitingForLock( 2, false, ResourceTypes.NODE, 1 ) );
			  map[handle3] = singletonList( CreateQuerySnapshotWaitingForLock( 3, true, ResourceTypes.NODE, 2 ) );
			  TransactionDependenciesResolver resolver = new TransactionDependenciesResolver( map );

			  assertThat( resolver.DescribeBlockingTransactions( handle1 ), EmptyString );
			  assertEquals( "[transaction-4]", resolver.DescribeBlockingTransactions( handle2 ) );
			  assertEquals( "[transaction-4, transaction-5]", resolver.DescribeBlockingTransactions( handle3 ) );
		 }

		 private QuerySnapshot CreateQuerySnapshot( long queryId )
		 {
			  return CreateExecutingQuery( queryId ).snapshot();
		 }

		 private QuerySnapshot CreateQuerySnapshotWaitingForLock( long queryId, bool exclusive, ResourceType resourceType, long id )
		 {
			  ExecutingQuery executingQuery = CreateExecutingQuery( queryId );
			  executingQuery.LockTracer().waitForLock(exclusive, resourceType, id);
			  return executingQuery.Snapshot();
		 }

		 private ExecutingQuery CreateExecutingQuery( long queryId )
		 {
			  return new ExecutingQuery( queryId, ClientConnectionInfo.EMBEDDED_CONNECTION, "test", "testQuey", VirtualValues.EMPTY_MAP, Collections.emptyMap(), () => 1L, PageCursorTracer.NULL, Thread.CurrentThread.Id, Thread.CurrentThread.Name, Clocks.nanoClock(), CpuClock.NOT_AVAILABLE, HeapAllocation.NOT_AVAILABLE );
		 }

		 private class TestKernelTransactionHandleWithLocks : TestKernelTransactionHandle
		 {

			  internal readonly long UserTxId;
			  internal readonly IList<ActiveLock> Locks;

			  internal TestKernelTransactionHandleWithLocks( KernelTransaction tx ) : this( tx, 0, Collections.emptyList() )
			  {
			  }

			  internal TestKernelTransactionHandleWithLocks( KernelTransaction tx, long userTxId ) : this( tx, userTxId, Collections.emptyList() )
			  {
			  }

			  internal TestKernelTransactionHandleWithLocks( KernelTransaction tx, long userTxId, IList<ActiveLock> locks ) : base( tx )
			  {
					this.UserTxId = userTxId;
					this.Locks = locks;
			  }

			  public override Stream<ActiveLock> ActiveLocks()
			  {
					return Locks.stream();
			  }

			  public override long UserTransactionId
			  {
				  get
				  {
						return UserTxId;
				  }
			  }
		 }
	}

}