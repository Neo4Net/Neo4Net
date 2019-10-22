using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core.consensus.log.cache
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;

	public class CircularBufferTest
	{
		 private readonly ThreadLocalRandom _tlr = ThreadLocalRandom.current();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeInitiallyEmpty()
		 public virtual void ShouldBeInitiallyEmpty()
		 {
			  // when
			  CircularBuffer<object> buffer = new CircularBuffer<object>( 3 );

			  // then
			  assertEquals( 0, buffer.Size() );
			  assertNull( buffer.Remove() );
			  assertNull( buffer.Read( 0 ) );

			  // again for idempotency check
			  assertEquals( 0, buffer.Size() );
			  assertNull( buffer.Remove() );
			  assertNull( buffer.Read( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeShouldReturnNullWhenEmpty()
		 public virtual void RemoveShouldReturnNullWhenEmpty()
		 {
			  // given
			  CircularBuffer<object> buffer = new CircularBuffer<object>( 3 );

			  buffer.Append( 1L );
			  buffer.Append( 2L );
			  buffer.Append( 3L );

			  // when
			  buffer.Remove();
			  buffer.Remove();
			  buffer.Remove();

			  // then
			  assertNull( buffer.Remove() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEvictElementsWhenClearing()
		 public virtual void ShouldEvictElementsWhenClearing()
		 {
			  // given
			  CircularBuffer<int> buffer = new CircularBuffer<int>( 3 );
			  int?[] evictions = new int?[3];
			  buffer.Append( 1 );
			  buffer.Append( 2 );

			  // when
			  buffer.Clear( evictions );

			  // then
			  assertEquals( 0, buffer.Size() );
			  assertArrayEquals( evictions, new int?[]{ 1, 2, null } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNullRemovedElements()
		 public virtual void ShouldNullRemovedElements()
		 {
			  // given
			  CircularBuffer<int> buffer = new CircularBuffer<int>( 3 );
			  int?[] evictions = new int?[3];
			  buffer.Append( 1 );
			  buffer.Append( 2 );
			  buffer.Append( 3 );

			  // when
			  buffer.Remove();
			  buffer.Remove();
			  buffer.Remove();

			  // then
			  assertNull( buffer.Read( 0 ) );
			  assertNull( buffer.Read( 1 ) );
			  assertNull( buffer.Read( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNullClearedElements()
		 public virtual void ShouldNullClearedElements()
		 {
			  // given
			  CircularBuffer<int> buffer = new CircularBuffer<int>( 3 );
			  int?[] evictions = new int?[3];
			  buffer.Append( 1 );
			  buffer.Append( 2 );
			  buffer.Append( 3 );

			  // when
			  buffer.Clear( evictions );

			  // then
			  assertNull( buffer.Read( 0 ) );
			  assertNull( buffer.Read( 1 ) );
			  assertNull( buffer.Read( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void comprehensivelyTestAppendRemove()
		 public virtual void ComprehensivelyTestAppendRemove()
		 {
			  for ( int capacity = 1; capacity <= 128; capacity++ )
			  {
					for ( int operations = 1; operations < capacity * 3; operations++ )
					{
						 ComprehensivelyTestAppendRemove( capacity, operations, new CircularBuffer<int>( capacity ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void comprehensivelyTestAppendRemoveHead()
		 public virtual void ComprehensivelyTestAppendRemoveHead()
		 {
			  for ( int capacity = 1; capacity <= 128; capacity++ )
			  {
					for ( int operations = 1; operations < capacity * 3; operations++ )
					{
						 ComprehensivelyTestAppendRemoveHead( capacity, operations, new CircularBuffer<int>( capacity ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void comprehensivelyTestAppendRemoveReusingBuffer()
		 public virtual void ComprehensivelyTestAppendRemoveReusingBuffer()
		 {
			  for ( int capacity = 1; capacity <= 128; capacity++ )
			  {
					CircularBuffer<int> buffer = new CircularBuffer<int>( capacity );
					for ( int operations = 1; operations <= capacity * 3; operations++ )
					{
						 ComprehensivelyTestAppendRemove( capacity, operations, buffer );
					}
			  }
		 }

		 private void ComprehensivelyTestAppendRemove( int capacity, int operations, CircularBuffer<int> buffer )
		 {
			  List<int> numbers = new List<int>( operations );

			  // when: adding a bunch of random numbers
			  for ( int i = 0; i < operations; i++ )
			  {
					int number = _tlr.Next();
					numbers.Add( number );
					buffer.Append( number );
			  }

			  // then: these should have been knocked out
			  for ( int i = 0; i < operations - capacity; i++ )
			  {
					numbers.RemoveAt( 0 );
			  }

			  // and these should remain
			  while ( numbers.Count > 0 )
			  {
					assertEquals( numbers.RemoveAt( 0 ), buffer.Remove() );
			  }

			  assertEquals( 0, buffer.Size() );
		 }

		 private void ComprehensivelyTestAppendRemoveHead( int capacity, int operations, CircularBuffer<int> buffer )
		 {
			  List<int> numbers = new List<int>( operations );

			  // when: adding a bunch of random numbers
			  for ( int i = 0; i < operations; i++ )
			  {
					int number = _tlr.Next();
					numbers.Add( number );
					buffer.Append( number );
			  }

			  // then: these should have been knocked out
			  for ( int i = 0; i < operations - capacity; i++ )
			  {
					numbers.RemoveAt( 0 );
			  }

			  // and these should remain
			  while ( numbers.Count > 0 )
			  {
					assertEquals( numbers.RemoveAt( numbers.Count - 1 ), buffer.RemoveHead() );
			  }

			  assertEquals( 0, buffer.Size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void comprehensivelyTestAppendRead()
		 public virtual void ComprehensivelyTestAppendRead()
		 {
			  for ( int capacity = 1; capacity <= 128; capacity++ )
			  {
					for ( int operations = 1; operations < capacity * 3; operations++ )
					{
						 ComprehensivelyTestAppendRead( capacity, operations );
					}
			  }
		 }

		 private void ComprehensivelyTestAppendRead( int capacity, int operations )
		 {
			  CircularBuffer<int> buffer = new CircularBuffer<int>( capacity );
			  List<int> numbers = new List<int>( operations );

			  // when: adding a bunch of random numbers
			  for ( int i = 0; i < operations; i++ )
			  {
					int number = _tlr.Next();
					numbers.Add( number );
					buffer.Append( number );
			  }

			  // then: these should have been knocked out
			  for ( int i = 0; i < operations - capacity; i++ )
			  {
					numbers.RemoveAt( 0 );
			  }

			  // and these should remain
			  int i = 0;
			  while ( numbers.Count > 0 )
			  {
					assertEquals( numbers.RemoveAt( 0 ), buffer.Read( i++ ) );
			  }
		 }
	}

}