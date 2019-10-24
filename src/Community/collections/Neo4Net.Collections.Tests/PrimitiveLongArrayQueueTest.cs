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
namespace Neo4Net.Collections
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class PrimitiveLongArrayQueueTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void newQueueIsEmpty()
		 internal virtual void NewQueueIsEmpty()
		 {
			  assertTrue( CreateQueue().Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void growQueueOnElementOffer()
		 internal virtual void GrowQueueOnElementOffer()
		 {
			  PrimitiveLongArrayQueue longArrayQueue = CreateQueue();
			  for ( int i = 1; i < 1000; i++ )
			  {
					longArrayQueue.Enqueue( i );
					assertEquals( i, longArrayQueue.Size() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addRemoveElementKeepQueueEmpty()
		 internal virtual void AddRemoveElementKeepQueueEmpty()
		 {
			  PrimitiveLongArrayQueue longArrayQueue = CreateQueue();
			  for ( int i = 0; i < 1000; i++ )
			  {
					longArrayQueue.Enqueue( i );
					assertEquals( i, longArrayQueue.Dequeue() );
					assertTrue( longArrayQueue.Empty );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void offerLessThenQueueCapacityElements()
		 internal virtual void OfferLessThenQueueCapacityElements()
		 {
			  PrimitiveLongArrayQueue arrayQueue = CreateQueue();
			  for ( int i = 1; i < 16; i++ )
			  {
					arrayQueue.Enqueue( i );
					assertEquals( i, arrayQueue.Size() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failToRemoveElementFromNewEmptyQueue()
		 internal virtual void FailToRemoveElementFromNewEmptyQueue()
		 {
			  assertThrows( typeof( System.InvalidOperationException ), () => CreateQueue().dequeue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void offerMoreThenQueueCapacityElements()
		 internal virtual void OfferMoreThenQueueCapacityElements()
		 {
			  PrimitiveLongArrayQueue arrayQueue = CreateQueue();
			  for ( int i = 1; i < 1234; i++ )
			  {
					arrayQueue.Enqueue( i );
			  }
			  int currentValue = 1;
			  while ( !arrayQueue.Empty )
			  {
					assertEquals( currentValue++, arrayQueue.Dequeue() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void emptyQueueAfterClear()
		 internal virtual void EmptyQueueAfterClear()
		 {
			  PrimitiveLongArrayQueue queue = CreateQueue();
			  queue.Enqueue( 2 );
			  queue.Enqueue( 3 );
			  assertFalse( queue.Empty );
			  assertEquals( 2, queue.Size() );

			  queue.Clear();

			  assertTrue( queue.Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void tailBeforeHeadCorrectSize()
		 internal virtual void TailBeforeHeadCorrectSize()
		 {
			  PrimitiveLongArrayQueue queue = CreateQueue();
			  for ( int i = 0; i < 14; i++ )
			  {
					queue.Enqueue( i );
			  }
			  for ( int i = 0; i < 10; i++ )
			  {
					assertEquals( i, queue.Dequeue() );
			  }
			  for ( int i = 14; i < 24 ; i++ )
			  {
					queue.Enqueue( i );
			  }

			  assertEquals( 14, queue.Size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void tailBeforeHeadCorrectResize()
		 internal virtual void TailBeforeHeadCorrectResize()
		 {
			  PrimitiveLongArrayQueue queue = CreateQueue();
			  for ( int i = 0; i < 14; i++ )
			  {
					queue.Enqueue( i );
			  }
			  for ( int i = 0; i < 10; i++ )
			  {
					assertEquals( i, queue.Dequeue() );
			  }
			  for ( int i = 14; i < 34 ; i++ )
			  {
					queue.Enqueue( i );
			  }

			  assertEquals( 24, queue.Size() );
			  for ( int j = 10; j < 34; j++ )
			  {
					assertEquals( j, queue.Dequeue() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void tailBeforeHeadCorrectIteration()
		 internal virtual void TailBeforeHeadCorrectIteration()
		 {
			  PrimitiveLongArrayQueue queue = CreateQueue();
			  for ( int i = 0; i < 14; i++ )
			  {
					queue.Enqueue( i );
			  }
			  for ( int i = 0; i < 10; i++ )
			  {
					assertEquals( i, queue.Dequeue() );
			  }
			  for ( int i = 14; i < 24 ; i++ )
			  {
					queue.Enqueue( i );
			  }

			  assertEquals( 14, queue.Size() );
			  LongIterator iterator = queue.LongIterator();
			  for ( int j = 10; j < 24; j++ )
			  {
					assertTrue( iterator.hasNext() );
					assertEquals( j, iterator.next() );
			  }
			  assertFalse( iterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failToGetNextOnEmptyQueueIterator()
		 internal virtual void FailToGetNextOnEmptyQueueIterator()
		 {
			  assertThrows( typeof( NoSuchElementException ), () => CreateQueue().longIterator().next() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addAllElementsFromOtherQueue()
		 internal virtual void AddAllElementsFromOtherQueue()
		 {
			  PrimitiveLongArrayQueue queue = CreateQueue();
			  queue.Enqueue( 1 );
			  queue.Enqueue( 2 );
			  PrimitiveLongArrayQueue otherQueue = CreateQueue();
			  otherQueue.Enqueue( 3 );
			  otherQueue.Enqueue( 4 );
			  queue.AddAll( otherQueue );

			  assertTrue( otherQueue.Empty );
			  assertEquals( 0, otherQueue.Size() );
			  assertEquals( 4, queue.Size() );
			  for ( int value = 1; value <= 4; value++ )
			  {
					assertEquals( value, queue.Dequeue() );
			  }
			  assertTrue( queue.Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void doNotAllowCreationOfQueueWithRandomCapacity()
		 internal virtual void DoNotAllowCreationOfQueueWithRandomCapacity()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => new PrimitiveLongArrayQueue(7) );
		 }

		 private PrimitiveLongArrayQueue CreateQueue()
		 {
			  return new PrimitiveLongArrayQueue();
		 }
	}

}