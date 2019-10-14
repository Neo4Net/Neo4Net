using System;
using System.Collections.Generic;
using System.Threading;

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


	using Neo4Net.Collections.Pooling;
	using TransactionTerminatedException = Neo4Net.Graphdb.TransactionTerminatedException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AutoIndexing = Neo4Net.Kernel.api.explicitindex.AutoIndexing;
	using AuxiliaryTransactionStateManager = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateManager;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using ConstraintIndexCreator = Neo4Net.Kernel.Impl.Api.state.ConstraintIndexCreator;
	using StandardConstraintSemantics = Neo4Net.Kernel.impl.constraints.StandardConstraintSemantics;
	using CanWrite = Neo4Net.Kernel.impl.factory.CanWrite;
	using ExplicitIndexStore = Neo4Net.Kernel.impl.index.ExplicitIndexStore;
	using NoOpClient = Neo4Net.Kernel.impl.locking.NoOpClient;
	using SimpleStatementLocks = Neo4Net.Kernel.impl.locking.SimpleStatementLocks;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using TransactionHeaderInformationFactory = Neo4Net.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using TransactionMonitor = Neo4Net.Kernel.impl.transaction.TransactionMonitor;
	using TransactionTracer = Neo4Net.Kernel.impl.transaction.tracing.TransactionTracer;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using CpuClock = Neo4Net.Resources.CpuClock;
	using HeapAllocation = Neo4Net.Resources.HeapAllocation;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using Race = Neo4Net.Test.Race;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
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
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.security.SecurityContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.collection.CollectionsFactorySupplier_Fields.ON_HEAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.MockedNeoStores.mockedTokenHolders;

	public class KernelTransactionTerminationTest
	{
		 private const int TEST_RUN_TIME_MS = 5_000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_RUN_TIME_MS * 20) public void transactionCantBeTerminatedAfterItIsClosed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TransactionCantBeTerminatedAfterItIsClosed()
		 {
			  RunTwoThreads(tx => tx.markForTermination(Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionMarkedAsFailed), tx =>
			  {
						  Close( tx );
						  assertFalse( tx.ReasonIfTerminated.Present );
						  tx.initialize();
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_RUN_TIME_MS * 20) public void closeTransaction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseTransaction()
		 {
			  BlockingQueue<bool> committerToTerminator = new LinkedBlockingQueue<bool>( 1 );
			  BlockingQueue<TerminatorAction> terminatorToCommitter = new LinkedBlockingQueue<TerminatorAction>( 1 );

			  RunTwoThreads(tx =>
			  {
						  bool? terminatorShouldAct = committerToTerminator.poll();
						  if ( terminatorShouldAct != null && terminatorShouldAct )
						  {
								TerminatorAction action = TerminatorAction.random();
								action.executeOn( tx );
								assertTrue( terminatorToCommitter.add( action ) );
						  }
			  }, tx =>
			  {
						  tx.initialize();
						  CommitterAction committerAction = CommitterAction.random();
						  committerAction.executeOn( tx );
						  if ( committerToTerminator.offer( true ) )
						  {
								TerminatorAction terminatorAction;
								try
								{
									 terminatorAction = terminatorToCommitter.poll( 1, TimeUnit.SECONDS );
								}
								catch ( InterruptedException )
								{
									 Thread.CurrentThread.Interrupt();
									 return;
								}
								if ( terminatorAction != null )
								{
									 Close( tx, committerAction, terminatorAction );
								}
						  }
					 });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void runTwoThreads(System.Action<TestKernelTransaction> thread1Action, System.Action<TestKernelTransaction> thread2Action) throws Throwable
		 private void RunTwoThreads( System.Action<TestKernelTransaction> thread1Action, System.Action<TestKernelTransaction> thread2Action )
		 {
			  TestKernelTransaction tx = TestKernelTransaction.Create().initialize();
			  AtomicLong t1Count = new AtomicLong();
			  AtomicLong t2Count = new AtomicLong();
			  long endTime = currentTimeMillis() + TEST_RUN_TIME_MS;
			  int limit = 20_000;

			  Race race = new Race();
			  race.WithEndCondition( () => ((t1Count.get() >= limit) && (t2Count.get() >= limit)) || (currentTimeMillis() >= endTime) );
			  race.AddContestant(() =>
			  {
				thread1Action( tx );
				t1Count.incrementAndGet();
			  });
			  race.AddContestant(() =>
			  {
				thread2Action( tx );
				t2Count.incrementAndGet();
			  });
			  race.Go();
		 }

		 private static void Close( KernelTransaction tx )
		 {
			  try
			  {
					tx.Close();
			  }
			  catch ( TransactionFailureException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private static void Close( TestKernelTransaction tx, CommitterAction committer, TerminatorAction terminator )
		 {
			  try
			  {
					if ( terminator == TerminatorAction.None )
					{
						 committer.closeNotTerminated( tx );
					}
					else
					{
						 committer.closeTerminated( tx );
					}
			  }
			  catch ( TransactionFailureException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private abstract class TerminatorAction
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NONE { void executeOn(org.neo4j.kernel.api.KernelTransaction tx) { } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           TERMINATE { void executeOn(org.neo4j.kernel.api.KernelTransaction tx) { tx.markForTermination(org.neo4j.kernel.api.exceptions.Status_Transaction.TransactionMarkedAsFailed); } };

			  private static readonly IList<TerminatorAction> valueList = new List<TerminatorAction>();

			  static TerminatorAction()
			  {
				  valueList.Add( NONE );
				  valueList.Add( TERMINATE );
			  }

			  public enum InnerEnum
			  {
				  NONE,
				  TERMINATE
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private TerminatorAction( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract void executeOn( Neo4Net.Kernel.api.KernelTransaction tx );

			  internal static TerminatorAction Random()
			  {
					return ThreadLocalRandom.current().nextBoolean() ? TERMINATE : NONE;
			  }

			 public static IList<TerminatorAction> values()
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

			 public static TerminatorAction valueOf( string name )
			 {
				 foreach ( TerminatorAction enumInstance in TerminatorAction.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private abstract class CommitterAction
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NONE { void executeOn(org.neo4j.kernel.api.KernelTransaction tx) { } void closeTerminated(TestKernelTransaction tx) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException { tx.assertTerminated(); tx.close(); tx.assertRolledBack(); } void closeNotTerminated(TestKernelTransaction tx) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException { tx.assertNotTerminated(); tx.close(); tx.assertRolledBack(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           MARK_SUCCESS { void executeOn(org.neo4j.kernel.api.KernelTransaction tx) { tx.success(); } void closeTerminated(TestKernelTransaction tx) { tx.assertTerminated(); try { tx.close(); fail("Exception expected"); } catch(Exception e) { assertThat(e, instanceOf(org.neo4j.graphdb.TransactionTerminatedException.class)); } tx.assertRolledBack(); } void closeNotTerminated(TestKernelTransaction tx) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException { tx.assertNotTerminated(); tx.close(); tx.assertCommitted(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           MARK_FAILURE { void executeOn(org.neo4j.kernel.api.KernelTransaction tx) { tx.failure(); } void closeTerminated(TestKernelTransaction tx) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException { NONE.closeTerminated(tx); } void closeNotTerminated(TestKernelTransaction tx) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException { NONE.closeNotTerminated(tx); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           MARK_SUCCESS_AND_FAILURE { void executeOn(org.neo4j.kernel.api.KernelTransaction tx) { tx.success(); tx.failure(); } void closeTerminated(TestKernelTransaction tx) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException { MARK_SUCCESS.closeTerminated(tx); } void closeNotTerminated(TestKernelTransaction tx) { tx.assertNotTerminated(); try { tx.close(); fail("Exception expected"); } catch(Exception e) { assertThat(e, instanceOf(org.neo4j.internal.kernel.api.exceptions.TransactionFailureException.class)); } tx.assertRolledBack(); } };

			  private static readonly IList<CommitterAction> valueList = new List<CommitterAction>();

			  static CommitterAction()
			  {
				  valueList.Add( NONE );
				  valueList.Add( MARK_SUCCESS );
				  valueList.Add( MARK_FAILURE );
				  valueList.Add( MARK_SUCCESS_AND_FAILURE );
			  }

			  public enum InnerEnum
			  {
				  NONE,
				  MARK_SUCCESS,
				  MARK_FAILURE,
				  MARK_SUCCESS_AND_FAILURE
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private CommitterAction( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal static readonly CommitterAction[] VALUES = values();

			  internal abstract void executeOn( Neo4Net.Kernel.api.KernelTransaction tx );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void closeTerminated(TestKernelTransaction tx) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException;
			  internal abstract void closeTerminated( TestKernelTransaction tx );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void closeNotTerminated(TestKernelTransaction tx) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException;
			  internal abstract void closeNotTerminated( TestKernelTransaction tx );

			  internal static CommitterAction Random()
			  {
					return Values[ThreadLocalRandom.current().Next(Values.Length)];
			  }

			 public static IList<CommitterAction> values()
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

			 public static CommitterAction valueOf( string name )
			 {
				 foreach ( CommitterAction enumInstance in CommitterAction.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private class TestKernelTransaction : KernelTransactionImplementation
		 {
			  internal readonly CommitTrackingMonitor Monitor;

			  internal TestKernelTransaction( CommitTrackingMonitor monitor ) : base( Config.defaults(), mock(typeof(StatementOperationParts)), mock(typeof(SchemaWriteGuard)), new TransactionHooks(), mock(typeof(ConstraintIndexCreator)), new Procedures(), TransactionHeaderInformationFactory.DEFAULT, mock(typeof(TransactionCommitProcess)), monitor, mock(typeof(AuxiliaryTransactionStateManager)), mock(typeof(Pool)), Clocks.fakeClock(), new AtomicReference<CpuClock>(CpuClock.NOT_AVAILABLE), new AtomicReference<HeapAllocation>(HeapAllocation.NOT_AVAILABLE), org.neo4j.kernel.impl.transaction.tracing.TransactionTracer_Fields.Null, LockTracer.NONE, org.neo4j.io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, mock(typeof(StorageEngine), RETURNS_MOCKS), new CanWrite(), AutoIndexing.UNSUPPORTED, mock(typeof(ExplicitIndexStore)), EmptyVersionContextSupplier.EMPTY, ON_HEAP, new StandardConstraintSemantics(), mock(typeof(SchemaState)), mock(typeof(IndexingService)), mockedTokenHolders(), new Dependencies() )
			  {

					this.Monitor = monitor;
			  }

			  internal static TestKernelTransaction Create()
			  {
					return new TestKernelTransaction( new CommitTrackingMonitor() );
			  }

			  internal virtual TestKernelTransaction Initialize()
			  {
					Initialize( 42, 42, new SimpleStatementLocks( new NoOpClient() ), Neo4Net.Internal.Kernel.Api.Transaction_Type.Implicit, AUTH_DISABLED, 0L, 1L );
					Monitor.reset();
					return this;
			  }

			  internal virtual void AssertCommitted()
			  {
					assertTrue( Monitor.committed );
			  }

			  internal virtual void AssertRolledBack()
			  {
					assertTrue( Monitor.rolledBack );
			  }

			  internal virtual void AssertTerminated()
			  {
					assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionMarkedAsFailed, ReasonIfTerminated.get() );
					assertTrue( Monitor.terminated );
			  }

			  internal virtual void AssertNotTerminated()
			  {
					assertFalse( ReasonIfTerminated.Present );
					assertFalse( Monitor.terminated );
			  }
		 }

		 private class CommitTrackingMonitor : TransactionMonitor
		 {
			  internal volatile bool Committed;
			  internal volatile bool RolledBack;
			  internal volatile bool Terminated;

			  public override void TransactionStarted()
			  {
			  }

			  public override void TransactionFinished( bool successful, bool writeTx )
			  {
					if ( successful )
					{
						 Committed = true;
					}
					else
					{
						 RolledBack = true;
					}
			  }

			  public override void TransactionTerminated( bool writeTx )
			  {
					Terminated = true;
			  }

			  public override void UpgradeToWriteTransaction()
			  {
			  }

			  internal virtual void Reset()
			  {
					Committed = false;
					RolledBack = false;
					Terminated = false;
			  }
		 }
	}

}