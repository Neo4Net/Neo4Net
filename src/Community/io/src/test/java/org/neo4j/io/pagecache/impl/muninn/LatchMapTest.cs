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
namespace Neo4Net.Io.pagecache.impl.muninn
{
	using Test = org.junit.jupiter.api.Test;


	using ThreadTestUtils = Neo4Net.Test.ThreadTestUtils;
	using BinaryLatch = Neo4Net.Util.concurrent.BinaryLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.notNullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;

	internal class LatchMapTest
	{
		 private LatchMap _latches = new LatchMap();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void takeOrAwaitLatchMustReturnLatchIfAvailable()
		 internal virtual void TakeOrAwaitLatchMustReturnLatchIfAvailable()
		 {
			  BinaryLatch latch = _latches.takeOrAwaitLatch( 0 );
			  assertThat( latch, @is( notNullValue() ) );
			  latch.Release();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void takeOrAwaitLatchMustAwaitExistingLatchAndReturnNull() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TakeOrAwaitLatchMustAwaitExistingLatchAndReturnNull()
		 {
			  AtomicReference<Thread> threadRef = new AtomicReference<Thread>();
			  BinaryLatch latch = _latches.takeOrAwaitLatch( 42 );
			  assertThat( latch, @is( notNullValue() ) );
			  ExecutorService executor = Executors.newSingleThreadExecutor();
			  Future<BinaryLatch> future = executor.submit(() =>
			  {
				threadRef.set( Thread.CurrentThread );
				return _latches.takeOrAwaitLatch( 42 );
			  });
			  Thread th;
			  do
			  {
					th = threadRef.get();
			  } while ( th == null );
			  ThreadTestUtils.awaitThreadState( th, 10_000, Thread.State.WAITING );
			  latch.Release();
			  assertThat( future.get( 1, TimeUnit.SECONDS ), @is( nullValue() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void takeOrAwaitLatchMustNotLetUnrelatedLatchesConflictTooMuch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TakeOrAwaitLatchMustNotLetUnrelatedLatchesConflictTooMuch()
		 {
			  BinaryLatch latch = _latches.takeOrAwaitLatch( 42 );
			  assertThat( latch, @is( notNullValue() ) );
			  ExecutorService executor = Executors.newSingleThreadExecutor();
			  Future<BinaryLatch> future = executor.submit( () => _latches.takeOrAwaitLatch(33) );
			  assertThat( future.get( 1, TimeUnit.SECONDS ), @is( notNullValue() ) );
			  latch.Release();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void latchMustBeAvailableAfterRelease()
		 internal virtual void LatchMustBeAvailableAfterRelease()
		 {
			  _latches.takeOrAwaitLatch( 42 ).release();
			  _latches.takeOrAwaitLatch( 42 ).release();
		 }
	}

}