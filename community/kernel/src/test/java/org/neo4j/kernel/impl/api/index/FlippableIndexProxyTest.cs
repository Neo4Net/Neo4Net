using System.Threading;

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
namespace Org.Neo4j.Kernel.Impl.Api.index
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using IndexProxyAlreadyClosedKernelException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexProxyAlreadyClosedKernelException;
	using Org.Neo4j.Test;
	using CleanupRule = Org.Neo4j.Test.rule.CleanupRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.SchemaIndexTestHelper.awaitLatch;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.SchemaIndexTestHelper.mockIndexProxy;

	public class FlippableIndexProxyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.CleanupRule cleanup = new org.neo4j.test.rule.CleanupRule();
		 public readonly CleanupRule Cleanup = new CleanupRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSwitchDelegate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSwitchDelegate()
		 {
			  // GIVEN
			  IndexProxy actual = mockIndexProxy();
			  IndexProxy other = mockIndexProxy();
			  FlippableIndexProxy @delegate = new FlippableIndexProxy( actual );
			  @delegate.FlipTarget = SingleProxy( other );

			  // WHEN
			  @delegate.Flip( NoOp(), null );
			  @delegate.Drop();

			  // THEN
			  verify( other ).drop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToFlipAfterClosed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToFlipAfterClosed()
		 {
			  //GIVEN
			  IndexProxy actual = mockIndexProxy();
			  IndexProxyFactory indexContextFactory = mock( typeof( IndexProxyFactory ) );

			  FlippableIndexProxy @delegate = new FlippableIndexProxy( actual );

			  //WHEN
			  @delegate.Close();

			  @delegate.FlipTarget = indexContextFactory;

			  //THEN
			  ExpectedException.expect( typeof( IndexProxyAlreadyClosedKernelException ) );

			  @delegate.Flip( NoOp(), null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToFlipAfterDrop() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToFlipAfterDrop()
		 {
			  //GIVEN
			  IndexProxy actual = mockIndexProxy();
			  IndexProxy failed = mockIndexProxy();
			  IndexProxyFactory indexContextFactory = mock( typeof( IndexProxyFactory ) );

			  FlippableIndexProxy @delegate = new FlippableIndexProxy( actual );
			  @delegate.FlipTarget = indexContextFactory;

			  //WHEN
			  @delegate.Drop();

			  //THEN
			  ExpectedException.expect( typeof( IndexProxyAlreadyClosedKernelException ) );
			  @delegate.Flip( NoOp(), SingleFailedDelegate(failed) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBlockAccessDuringFlipAndThenDelegateToCorrectContext() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBlockAccessDuringFlipAndThenDelegateToCorrectContext()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexProxy contextBeforeFlip = mockIndexProxy();
			  IndexProxy contextBeforeFlip = mockIndexProxy();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexProxy contextAfterFlip = mockIndexProxy();
			  IndexProxy contextAfterFlip = mockIndexProxy();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final FlippableIndexProxy flippable = new FlippableIndexProxy(contextBeforeFlip);
			  FlippableIndexProxy flippable = new FlippableIndexProxy( contextBeforeFlip );
			  flippable.FlipTarget = SingleProxy( contextAfterFlip );

			  // And given complicated thread race condition tools
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch triggerFinishFlip = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent triggerFinishFlip = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch triggerExternalAccess = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent triggerExternalAccess = new System.Threading.CountdownEvent( 1 );

			  OtherThreadExecutor<Void> flippingThread = Cleanup.add( new OtherThreadExecutor<Void>( "Flipping thread", null ) );
			  OtherThreadExecutor<Void> dropIndexThread = Cleanup.add( new OtherThreadExecutor<Void>( "Drop index thread", null ) );

			  // WHEN one thread starts flipping to another context
			  Future<Void> flipContextFuture = flippingThread.ExecuteDontWait( StartFlipAndWaitForLatchBeforeFinishing( flippable, triggerFinishFlip, triggerExternalAccess ) );

			  // And I wait until the flipping thread is in the middle of "the flip"
			  assertTrue( triggerExternalAccess.await( 10, SECONDS ) );

			  // And another thread comes along and drops the index
			  Future<Void> dropIndexFuture = dropIndexThread.ExecuteDontWait( DropTheIndex( flippable ) );
			  dropIndexThread.WaitUntilWaiting();

			  // And the flipping thread finishes the flip
			  triggerFinishFlip.Signal();

			  // And both threads get to finish up and return
			  dropIndexFuture.get( 10, SECONDS );
			  flipContextFuture.get( 10, SECONDS );

			  // THEN the thread wanting to drop the index should not have interacted with the original context
			  // eg. it should have waited for the flip to finish
			  verifyNoMoreInteractions( contextBeforeFlip );

			  // But it should have gotten to drop the new index context, after the flip happened.
			  verify( contextAfterFlip ).drop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAbortStoreScanWaitOnDrop() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAbortStoreScanWaitOnDrop()
		 {
			  // given the proxy structure
			  FakePopulatingIndexProxy @delegate = new FakePopulatingIndexProxy();
			  FlippableIndexProxy flipper = new FlippableIndexProxy( @delegate );
			  OtherThreadExecutor<Void> waiter = Cleanup.add( new OtherThreadExecutor<Void>( "Waiter", null ) );

			  // and a thread stuck in the awaitStoreScanCompletion loop
			  Future<object> waiting = waiter.ExecuteDontWait( state => flipper.AwaitStoreScanCompleted( 0, MILLISECONDS ) );
			  while ( !@delegate.AwaitCalled )
			  {
					Thread.Sleep( 10 );
			  }

			  // when
			  flipper.Drop();

			  // then the waiting should quickly be over
			  waiting.get( 10, SECONDS );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.test.OtherThreadExecutor.WorkerCommand<Void, Void> dropTheIndex(final FlippableIndexProxy flippable)
		 private OtherThreadExecutor.WorkerCommand<Void, Void> DropTheIndex( FlippableIndexProxy flippable )
		 {
			  return state =>
			  {
				flippable.Drop();
				return null;
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.test.OtherThreadExecutor.WorkerCommand<Void, Void> startFlipAndWaitForLatchBeforeFinishing(final FlippableIndexProxy flippable, final java.util.concurrent.CountDownLatch triggerFinishFlip, final java.util.concurrent.CountDownLatch triggerExternalAccess)
		 private OtherThreadExecutor.WorkerCommand<Void, Void> StartFlipAndWaitForLatchBeforeFinishing( FlippableIndexProxy flippable, System.Threading.CountdownEvent triggerFinishFlip, System.Threading.CountdownEvent triggerExternalAccess )
		 {
			  return state =>
			  {
				flippable.Flip(() =>
				{
					 triggerExternalAccess.Signal();
					 assertTrue( awaitLatch( triggerFinishFlip ) );
					 return true;
				}, null);
				return null;
			  };
		 }

		 private Callable<bool> NoOp()
		 {
			  return () => true;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static IndexProxyFactory singleProxy(final IndexProxy proxy)
		 public static IndexProxyFactory SingleProxy( IndexProxy proxy )
		 {
			  return () => proxy;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private FailedIndexProxyFactory singleFailedDelegate(final IndexProxy failed)
		 private FailedIndexProxyFactory SingleFailedDelegate( IndexProxy failed )
		 {
			  return failure => failed;
		 }

		 private class FakePopulatingIndexProxy : IndexProxyAdapter
		 {
			  internal volatile bool AwaitCalled;

			  public override bool AwaitStoreScanCompleted( long time, TimeUnit unit )
			  {
					AwaitCalled = true;
					return true;
			  }
		 }
	}

}