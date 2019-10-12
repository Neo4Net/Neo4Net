using System;
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
namespace Org.Neo4j.Kernel.impl.transaction
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using Test = org.junit.Test;


	using IdOrderingQueue = Org.Neo4j.Kernel.impl.util.IdOrderingQueue;
	using SynchronizedArrayIdOrderingQueue = Org.Neo4j.Kernel.impl.util.SynchronizedArrayIdOrderingQueue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.DoubleLatch.awaitLatch;

	public class SynchronizedArrayIdOrderingQueueStressTest
	{
		 private const int THRESHOLD = 100;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWithstandHighStressAndStillKeepOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWithstandHighStressAndStillKeepOrder()
		 {
			  // GIVEN an ordering queue w/ low initial size as to also exercise resize under stress
			  VerifyingIdOrderingQueue queue = new VerifyingIdOrderingQueue( new SynchronizedArrayIdOrderingQueue() );
			  Committer[] committers = new Committer[20];
			  System.Threading.CountdownEvent readySignal = new System.Threading.CountdownEvent( committers.Length );
			  AtomicBoolean end = new AtomicBoolean();
			  System.Threading.CountdownEvent startSignal = new System.Threading.CountdownEvent( 1 );
			  LongIterator idSource = NeverEndingIdStream();
			  for ( int i = 0; i < committers.Length; i++ )
			  {
					committers[i] = new Committer( queue, idSource, end, readySignal, startSignal );
			  }

			  // WHEN GO!
			  readySignal.await();
			  startSignal.Signal();
			  long startTime = currentTimeMillis();
			  long endTime = startTime + SECONDS.toMillis( 20 ); // worst-case
			  while ( currentTimeMillis() < endTime && queue.NumberOfOrderlyRemovedIds < THRESHOLD )
			  {
					Thread.Sleep( 100 );
			  }
			  end.set( true );
			  foreach ( Committer committer in committers )
			  {
					committer.AwaitFinish();
			  }

			  // THEN there should have been at least a few ids processed. The order of those
			  // are verified as they go, by the VerifyingIdOrderingQueue
			  assertTrue( "Would have wanted at least a few ids to be processed, but only saw " + queue.NumberOfOrderlyRemovedIds, queue.NumberOfOrderlyRemovedIds >= THRESHOLD );
		 }

		 private class VerifyingIdOrderingQueue : IdOrderingQueue
		 {
			  internal readonly IdOrderingQueue Delegate;
			  internal readonly AtomicInteger RemovedCount = new AtomicInteger();
			  internal volatile long PreviousId = -1;

			  internal VerifyingIdOrderingQueue( IdOrderingQueue @delegate )
			  {
					this.Delegate = @delegate;
			  }

			  public override void RemoveChecked( long expectedValue )
			  {
					if ( expectedValue < PreviousId )
					{ // Just to bypass the string creation every check
						 assertTrue( "Expected to remove head " + expectedValue + ", which should have been greater than previously seen id " + PreviousId, expectedValue > PreviousId );
					}
					PreviousId = expectedValue;
					Delegate.removeChecked( expectedValue );
					RemovedCount.incrementAndGet();
			  }

			  public override void Offer( long value )
			  {
					Delegate.offer( value );
			  }

			  public virtual bool Empty
			  {
				  get
				  {
						return Delegate.Empty;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void waitFor(long value) throws InterruptedException
			  public override void WaitFor( long value )
			  {
					Delegate.waitFor( value );
			  }

			  public virtual int NumberOfOrderlyRemovedIds
			  {
				  get
				  {
						return RemovedCount.get();
				  }
			  }
		 }

		 private LongIterator NeverEndingIdStream()
		 {
			  return new LongIteratorAnonymousInnerClass( this );
		 }

		 private class LongIteratorAnonymousInnerClass : LongIterator
		 {
			 private readonly SynchronizedArrayIdOrderingQueueStressTest _outerInstance;

			 public LongIteratorAnonymousInnerClass( SynchronizedArrayIdOrderingQueueStressTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 stride = new Stride();
			 }

			 private readonly Stride stride;
			 private long next;

			 public override bool hasNext()
			 {
				  return true;
			 }

			 public override long next()
			 {
				  try
				  {
						return next;
				  }
				  finally
				  {
						next += stride.next();
				  }
			 }
		 }

		 private class Committer : Thread
		 {
			  internal readonly Random Random = new Random();
			  internal readonly IdOrderingQueue Queue;
			  internal readonly AtomicBoolean End;
			  internal readonly System.Threading.CountdownEvent StartSignal;
			  internal readonly LongIterator IdSource;
			  internal readonly System.Threading.CountdownEvent ReadySignal;
			  internal volatile Exception Exception;

			  internal Committer( IdOrderingQueue queue, LongIterator idSource, AtomicBoolean end, System.Threading.CountdownEvent readySignal, System.Threading.CountdownEvent startSignal )
			  {
					this.Queue = queue;
					this.IdSource = idSource;
					this.End = end;
					this.ReadySignal = readySignal;
					this.StartSignal = startSignal;
					start();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitFinish() throws Exception
			  public virtual void AwaitFinish()
			  {
					join();
					if ( Exception != null )
					{
						 throw Exception;
					}
			  }

			  public override void Run()
			  {
					try
					{
						 ReadySignal.Signal();
						 awaitLatch( StartSignal );
						 while ( !End.get() )
						 {
							  long id;

							  // Ids must be offered in order
							  lock ( Queue )
							  {
									id = IdSource.next();
									Queue.offer( id );
							  }

							  Queue.waitFor( id );
							  for ( int i = 0, max = Random.Next( 10_000 ); i < max; i++ )
							  {
									// Jit - please don't take this loop away. Look busy... check queue for empty, or something!
									Queue.Empty;
							  }
							  Queue.removeChecked( id );
						 }
					}
					catch ( Exception e )
					{
						 this.Exception = e;
					}
			  }
		 }

		 /// <summary>
		 /// Strides predictably: 1, 2, 3, ..., MAX, 1, 2, 3, ... a.s.o
		 /// </summary>
		 private class Stride
		 {
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal int StrideConflict;
			  internal readonly int Max = 5;

			  public virtual int Next()
			  {
					return ( StrideConflict++ % Max ) + 1;
			  }
		 }
	}

}