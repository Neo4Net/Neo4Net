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
namespace Neo4Net.Kernel.impl.util
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Neo4Net.Test;
	using Neo4Net.Test.OtherThreadExecutor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.DoubleLatch.awaitLatch;

	public class LazySingleReferenceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyAllowSingleThreadToInitialize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOnlyAllowSingleThreadToInitialize()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger initCalls = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger initCalls = new AtomicInteger();
			  LazySingleReference<int> @ref = new LazySingleReferenceAnonymousInnerClass( this, latch, initCalls );
			  Future<int> t1Evaluate = _t1.executeDontWait( Evaluate( @ref ) );
			  _t1.waitUntilWaiting();

			  // WHEN
			  Future<int> t2Evaluate = _t2.executeDontWait( Evaluate( @ref ) );
			  _t2.waitUntilBlocked();
			  latch.Signal();
			  int e1 = t1Evaluate.get();
			  int e2 = t2Evaluate.get();

			  // THEN
			  assertEquals( "T1 evaluation", 1, e1 );
			  assertEquals( "T2 evaluation", 1, e2 );
		 }

		 private class LazySingleReferenceAnonymousInnerClass : LazySingleReference<int>
		 {
			 private readonly LazySingleReferenceTest _outerInstance;

			 private System.Threading.CountdownEvent _latch;
			 private AtomicInteger _initCalls;

			 public LazySingleReferenceAnonymousInnerClass( LazySingleReferenceTest outerInstance, System.Threading.CountdownEvent latch, AtomicInteger initCalls )
			 {
				 this.outerInstance = outerInstance;
				 this._latch = latch;
				 this._initCalls = initCalls;
			 }

			 protected internal override int? create()
			 {
				  awaitLatch( _latch );
				  return _initCalls.incrementAndGet();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMutexAccessBetweenInvalidateAndinstance() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMutexAccessBetweenInvalidateAndinstance()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger initCalls = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger initCalls = new AtomicInteger();
			  LazySingleReference<int> @ref = new LazySingleReferenceAnonymousInnerClass2( this, latch, initCalls );
			  Future<int> t1Evaluate = _t1.executeDontWait( Evaluate( @ref ) );
			  _t1.waitUntilWaiting();

			  // WHEN
			  Future<Void> t2Invalidate = _t2.executeDontWait( Invalidate( @ref ) );
			  _t2.waitUntilBlocked();
			  latch.Signal();
			  int e = t1Evaluate.get();
			  t2Invalidate.get();

			  // THEN
			  assertEquals( "Evaluation", 1, e );
		 }

		 private class LazySingleReferenceAnonymousInnerClass2 : LazySingleReference<int>
		 {
			 private readonly LazySingleReferenceTest _outerInstance;

			 private System.Threading.CountdownEvent _latch;
			 private AtomicInteger _initCalls;

			 public LazySingleReferenceAnonymousInnerClass2( LazySingleReferenceTest outerInstance, System.Threading.CountdownEvent latch, AtomicInteger initCalls )
			 {
				 this.outerInstance = outerInstance;
				 this._latch = latch;
				 this._initCalls = initCalls;
			 }

			 protected internal override int? create()
			 {
				  awaitLatch( _latch );
				  return _initCalls.incrementAndGet();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInitializeAgainAfterInvalidated()
		 public virtual void ShouldInitializeAgainAfterInvalidated()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger initCalls = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger initCalls = new AtomicInteger();
			  LazySingleReference<int> @ref = new LazySingleReferenceAnonymousInnerClass3( this, initCalls );
			  assertEquals( "First evaluation", 1, @ref.Get().intValue() );

			  // WHEN
			  @ref.Invalidate();
			  int e2 = @ref.Get();

			  // THEN
			  assertEquals( "Second evaluation", 2, e2 );
		 }

		 private class LazySingleReferenceAnonymousInnerClass3 : LazySingleReference<int>
		 {
			 private readonly LazySingleReferenceTest _outerInstance;

			 private AtomicInteger _initCalls;

			 public LazySingleReferenceAnonymousInnerClass3( LazySingleReferenceTest outerInstance, AtomicInteger initCalls )
			 {
				 this.outerInstance = outerInstance;
				 this._initCalls = initCalls;
			 }

			 protected internal override int? create()
			 {
				  return _initCalls.incrementAndGet();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondToIsInitialized()
		 public virtual void ShouldRespondToIsInitialized()
		 {
			  // GIVEN
			  LazySingleReference<int> @ref = new LazySingleReferenceAnonymousInnerClass4( this );

			  // WHEN
			  bool firstResult = @ref.Created;
			  @ref.Get();
			  bool secondResult = @ref.Created;
			  @ref.Invalidate();
			  bool thirdResult = @ref.Created;
			  @ref.Get();
			  bool fourthResult = @ref.Created;

			  // THEN
			  assertFalse( "Should not start off as initialized", firstResult );
			  assertTrue( "Should be initialized after an evaluation", secondResult );
			  assertFalse( "Should not be initialized after invalidated", thirdResult );
			  assertTrue( "Should be initialized after a re-evaluation", fourthResult );
		 }

		 private class LazySingleReferenceAnonymousInnerClass4 : LazySingleReference<int>
		 {
			 private readonly LazySingleReferenceTest _outerInstance;

			 public LazySingleReferenceAnonymousInnerClass4( LazySingleReferenceTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override int? create()
			 {
				  return 1;
			 }
		 }

		 private OtherThreadExecutor<Void> _t1;
		 private OtherThreadExecutor<Void> _t2;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _t1 = new OtherThreadExecutor<Void>( "T1", null );
			  _t2 = new OtherThreadExecutor<Void>( "T2", null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _t2.Dispose();
			  _t1.Dispose();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Neo4Net.test.OtherThreadExecutor.WorkerCommand<Void,int> Evaluate(final LazySingleReference<int> ref)
		 private OtherThreadExecutor.WorkerCommand<Void, int> Evaluate( LazySingleReference<int> @ref )
		 {
			  return state => @ref.Get();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Neo4Net.test.OtherThreadExecutor.WorkerCommand<Void,Void> invalidate(final LazySingleReference<int> ref)
		 private OtherThreadExecutor.WorkerCommand<Void, Void> Invalidate( LazySingleReference<int> @ref )
		 {
			  return state =>
			  {
				@ref.Invalidate();
				return null;
			  };
		 }
	}

}