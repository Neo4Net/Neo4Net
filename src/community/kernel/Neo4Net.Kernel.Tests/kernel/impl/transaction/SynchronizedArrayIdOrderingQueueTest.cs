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
namespace Neo4Net.Kernel.impl.transaction
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using IdOrderingQueue = Neo4Net.Kernel.impl.util.IdOrderingQueue;
	using SynchronizedArrayIdOrderingQueue = Neo4Net.Kernel.impl.util.SynchronizedArrayIdOrderingQueue;
	using Neo4Net.Test;
	using Neo4Net.Test.OtherThreadExecutor;
	using CleanupRule = Neo4Net.Test.rule.CleanupRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class SynchronizedArrayIdOrderingQueueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.CleanupRule cleanup = new org.neo4j.test.rule.CleanupRule();
		 public readonly CleanupRule Cleanup = new CleanupRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOfferQueueABunchOfIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOfferQueueABunchOfIds()
		 {
			  // GIVEN
			  IdOrderingQueue queue = new SynchronizedArrayIdOrderingQueue( 5 );

			  // WHEN
			  for ( int i = 0; i < 7; i++ )
			  {
					queue.Offer( i );
			  }

			  // THEN
			  for ( int i = 0; i < 7; i++ )
			  {
					assertFalse( queue.Empty );
					queue.WaitFor( i );
					queue.RemoveChecked( i );
			  }
			  assertTrue( queue.Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOfferAwaitAndRemoveRoundAndRound() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOfferAwaitAndRemoveRoundAndRound()
		 {
			  // GIVEN
			  IdOrderingQueue queue = new SynchronizedArrayIdOrderingQueue( 5 );
			  long offeredId = 0;
			  long awaitedId = 0;
			  queue.Offer( offeredId++ );
			  queue.Offer( offeredId++ );

			  // WHEN
			  for ( int i = 0; i < 20; i++ )
			  {
					queue.WaitFor( awaitedId );
					queue.RemoveChecked( awaitedId++ );
					queue.Offer( offeredId++ );
					assertFalse( queue.Empty );
			  }

			  // THEN
			  queue.RemoveChecked( awaitedId++ );
			  queue.RemoveChecked( awaitedId );
			  assertTrue( queue.Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveOneThreadWaitForARemoval() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveOneThreadWaitForARemoval()
		 {
			  // GIVEN
			  IdOrderingQueue queue = new SynchronizedArrayIdOrderingQueue( 5 );
			  queue.Offer( 3 );
			  queue.Offer( 5 );

			  // WHEN another thread comes in and awaits 5
			  OtherThreadExecutor<Void> t2 = Cleanup.add( new OtherThreadExecutor<Void>( "T2", null ) );
			  Future<object> await5 = t2.ExecuteDontWait( AwaitHead( queue, 5 ) );
			  t2.WaitUntilWaiting();
			  // ... and head (3) gets removed
			  queue.RemoveChecked( 3 );

			  // THEN the other thread should be OK to continue
			  await5.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtendArrayWhenIdsAreWrappingAround()
		 public virtual void ShouldExtendArrayWhenIdsAreWrappingAround()
		 {
			  // GIVEN
			  IdOrderingQueue queue = new SynchronizedArrayIdOrderingQueue( 5 );
			  for ( int i = 0; i < 3; i++ )
			  {
					queue.Offer( i );
					queue.RemoveChecked( i );
			  }
			  // Now we're at [0,1,2,0,0]
			  //                     ^-- headIndex and offerIndex
			  for ( int i = 3; i < 8; i++ )
			  {
					queue.Offer( i );
			  }
			  // Now we're at [5,6,2,3,4]
			  //                     ^-- headIndex and offerIndex%length

			  // WHEN offering one more, so that the queue is forced to resize
			  queue.Offer( 8 );

			  // THEN it should have been offered as well as all the previous ids should be intact
			  for ( int i = 3; i <= 8; i++ )
			  {
					assertFalse( queue.Empty );
					queue.RemoveChecked( i );
			  }
			  assertTrue( queue.Empty );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.test.OtherThreadExecutor.WorkerCommand<Void, Object> awaitHead(final org.neo4j.kernel.impl.util.IdOrderingQueue queue, final long id)
		 private OtherThreadExecutor.WorkerCommand<Void, object> AwaitHead( IdOrderingQueue queue, long id )
		 {
			  return state =>
			  {
				queue.WaitFor( id );
				return null;
			  };
		 }
	}

}