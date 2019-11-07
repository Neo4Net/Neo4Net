using System;
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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using Test = org.junit.Test;


	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;
	using ThreadTestUtils = Neo4Net.Test.ThreadTestUtils;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.api.index.SchemaIndexTestHelper.mockIndexProxy;

	public class ContractCheckingIndexProxyTest
	{
		 private const long TEST_TIMEOUT = 20_000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldNotCreateIndexTwice() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCreateIndexTwice()
		 {
			  // GIVEN
			  IndexProxy inner = mockIndexProxy();
			  IndexProxy outer = NewContractCheckingIndexProxy( inner );

			  // WHEN
			  outer.Start();
			  outer.Start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldNotCloseIndexTwice() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCloseIndexTwice()
		 {
			  // GIVEN
			  IndexProxy inner = mockIndexProxy();
			  IndexProxy outer = NewContractCheckingIndexProxy( inner );

			  // WHEN
			  outer.Close();
			  outer.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldNotDropIndexTwice() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDropIndexTwice()
		 {
			  // GIVEN
			  IndexProxy inner = mockIndexProxy();
			  IndexProxy outer = NewContractCheckingIndexProxy( inner );

			  // WHEN
			  outer.Drop();
			  outer.Drop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldNotDropAfterClose() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDropAfterClose()
		 {
			  // GIVEN
			  IndexProxy inner = mockIndexProxy();
			  IndexProxy outer = NewContractCheckingIndexProxy( inner );

			  // WHEN
			  outer.Close();
			  outer.Drop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropAfterCreate() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropAfterCreate()
		 {
			  // GIVEN
			  IndexProxy inner = mockIndexProxy();
			  IndexProxy outer = NewContractCheckingIndexProxy( inner );

			  // WHEN
			  outer.Start();

			  // PASS
			  outer.Drop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseAfterCreate() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseAfterCreate()
		 {
			  // GIVEN
			  IndexProxy inner = mockIndexProxy();
			  IndexProxy outer = NewContractCheckingIndexProxy( inner );

			  // WHEN
			  outer.Start();

			  // PASS
			  outer.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldNotUpdateBeforeCreate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotUpdateBeforeCreate()
		 {
			  // GIVEN
			  IndexProxy inner = mockIndexProxy();
			  IndexProxy outer = NewContractCheckingIndexProxy( inner );

			  // WHEN
			  using ( IndexUpdater updater = outer.NewUpdater( IndexUpdateMode.Online ) )
			  {
					updater.Process( null );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldNotUpdateAfterClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotUpdateAfterClose()
		 {
			  // GIVEN
			  IndexProxy inner = mockIndexProxy();
			  IndexProxy outer = NewContractCheckingIndexProxy( inner );

			  // WHEN
			  outer.Start();
			  outer.Close();
			  using ( IndexUpdater updater = outer.NewUpdater( IndexUpdateMode.Online ) )
			  {
					updater.Process( null );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldNotForceBeforeCreate() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotForceBeforeCreate()
		 {
			  // GIVEN
			  IndexProxy inner = mockIndexProxy();
			  IndexProxy outer = NewContractCheckingIndexProxy( inner );

			  // WHEN
			  outer.Force( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldNotForceAfterClose() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotForceAfterClose()
		 {
			  // GIVEN
			  IndexProxy inner = mockIndexProxy();
			  IndexProxy outer = NewContractCheckingIndexProxy( inner );

			  // WHEN
			  outer.Start();
			  outer.Close();
			  outer.Force( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldNotCloseWhileCreating() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCloseWhileCreating()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.test.DoubleLatch latch = new Neo4Net.test.DoubleLatch();
			  DoubleLatch latch = new DoubleLatch();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexProxy inner = new IndexProxyAdapter()
			  IndexProxy inner = new IndexProxyAdapterAnonymousInnerClass( this, latch );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexProxy outer = newContractCheckingIndexProxy(inner);
			  IndexProxy outer = NewContractCheckingIndexProxy( inner );

			  // WHEN
			  RunInSeparateThread( outer.start );

			  try
			  {
					latch.WaitForAllToStart();
					outer.Close();
			  }
			  finally
			  {
					latch.Finish();
			  }
		 }

		 private class IndexProxyAdapterAnonymousInnerClass : IndexProxyAdapter
		 {
			 private readonly ContractCheckingIndexProxyTest _outerInstance;

			 private DoubleLatch _latch;

			 public IndexProxyAdapterAnonymousInnerClass( ContractCheckingIndexProxyTest outerInstance, DoubleLatch latch )
			 {
				 this.outerInstance = outerInstance;
				 this._latch = latch;
			 }

			 public override void start()
			 {
				  _latch.startAndWaitForAllToStartAndFinish();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldNotDropWhileCreating() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDropWhileCreating()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.test.DoubleLatch latch = new Neo4Net.test.DoubleLatch();
			  DoubleLatch latch = new DoubleLatch();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexProxy inner = new IndexProxyAdapter()
			  IndexProxy inner = new IndexProxyAdapterAnonymousInnerClass2( this, latch );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexProxy outer = newContractCheckingIndexProxy(inner);
			  IndexProxy outer = NewContractCheckingIndexProxy( inner );

			  // WHEN
			  RunInSeparateThread( outer.start );

			  try
			  {
					latch.WaitForAllToStart();
					outer.Drop();
			  }
			  finally
			  {
					latch.Finish();
			  }
		 }

		 private class IndexProxyAdapterAnonymousInnerClass2 : IndexProxyAdapter
		 {
			 private readonly ContractCheckingIndexProxyTest _outerInstance;

			 private DoubleLatch _latch;

			 public IndexProxyAdapterAnonymousInnerClass2( ContractCheckingIndexProxyTest outerInstance, DoubleLatch latch )
			 {
				 this.outerInstance = outerInstance;
				 this._latch = latch;
			 }

			 public override void start()
			 {
				  _latch.startAndWaitForAllToStartAndFinish();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void closeWaitForUpdateToFinish() throws java.io.IOException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseWaitForUpdateToFinish()
		 {
			  // GIVEN
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexProxy inner = new IndexProxyAdapter()
			  IndexProxy inner = new IndexProxyAdapterAnonymousInnerClass3( this );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexProxy outer = newContractCheckingIndexProxy(inner);
			  IndexProxy outer = NewContractCheckingIndexProxy( inner );
			  Thread actionThread = CreateActionThread( outer.close );
			  outer.Start();

			  // WHEN
			  Thread updaterThread = RunInSeparateThread(() =>
			  {
				try
				{
					using ( IndexUpdater updater = outer.NewUpdater( IndexUpdateMode.Online ) )
					{
						 updater.process( null );
						 try
						 {
							  actionThread.Start();
							  latch.await();
						 }
						 catch ( InterruptedException e )
						 {
							  throw new Exception( e );
						 }
					}
				}
				catch ( IndexEntryConflictException e )
				{
					 throw new Exception( e );
				}
			  });

			  ThreadTestUtils.awaitThreadState( actionThread, TEST_TIMEOUT, Thread.State.TIMED_WAITING );
			  latch.Signal();
			  updaterThread.Join();
			  actionThread.Join();
		 }

		 private class IndexProxyAdapterAnonymousInnerClass3 : IndexProxyAdapter
		 {
			 private readonly ContractCheckingIndexProxyTest _outerInstance;

			 public IndexProxyAdapterAnonymousInnerClass3( ContractCheckingIndexProxyTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override IndexUpdater newUpdater( IndexUpdateMode mode )
			 {
				  return base.newUpdater( mode );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void closeWaitForForceToComplete() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseWaitForForceToComplete()
		 {
			  // GIVEN
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
			  AtomicReference<Thread> actionThreadReference = new AtomicReference<Thread>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexProxy inner = new IndexProxyAdapter()
			  IndexProxy inner = new IndexProxyAdapterAnonymousInnerClass4( this, latch, actionThreadReference );
			  IndexProxy outer = NewContractCheckingIndexProxy( inner );
			  Thread actionThread = CreateActionThread( outer.close );
			  actionThreadReference.set( actionThread );

			  outer.Start();
			  Thread thread = RunInSeparateThread( () => outer.force(Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited) );

			  ThreadTestUtils.awaitThreadState( actionThread, TEST_TIMEOUT, Thread.State.TIMED_WAITING );
			  latch.Signal();

			  thread.Join();
			  actionThread.Join();
		 }

		 private class IndexProxyAdapterAnonymousInnerClass4 : IndexProxyAdapter
		 {
			 private readonly ContractCheckingIndexProxyTest _outerInstance;

			 private System.Threading.CountdownEvent _latch;
			 private AtomicReference<Thread> _actionThreadReference;

			 public IndexProxyAdapterAnonymousInnerClass4( ContractCheckingIndexProxyTest outerInstance, System.Threading.CountdownEvent latch, AtomicReference<Thread> actionThreadReference )
			 {
				 this.outerInstance = outerInstance;
				 this._latch = latch;
				 this._actionThreadReference = actionThreadReference;
			 }

			 public override void force( IOLimiter ioLimiter )
			 {
				  try
				  {
						_actionThreadReference.get().start();
						_latch.await();
				  }
				  catch ( Exception e )
				  {
					 throw new Exception( e );
				  }
			 }
		 }

		 private interface ThrowingRunnable
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void run() throws java.io.IOException;
			  void Run();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Thread runInSeparateThread(final ThrowingRunnable action)
		 private Thread RunInSeparateThread( ThrowingRunnable action )
		 {
			  Thread thread = CreateActionThread( action );
			  thread.Start();
			  return thread;
		 }

		 private Thread CreateActionThread( ThrowingRunnable action )
		 {
			  return new Thread(() =>
			  {
				try
				{
					 action.Run();
				}
				catch ( IOException e )
				{
					 throw new Exception( e );
				}
			  });
		 }

		 private ContractCheckingIndexProxy NewContractCheckingIndexProxy( IndexProxy inner )
		 {
			  return new ContractCheckingIndexProxy( inner, false );
		 }
	}

}