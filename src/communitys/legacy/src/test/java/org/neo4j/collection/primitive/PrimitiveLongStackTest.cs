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
namespace Neo4Net.Collections.primitive
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class PrimitiveLongStackTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPushAndPollSomeEntities()
		 internal virtual void ShouldPushAndPollSomeEntities()
		 {
			  // GIVEN
			  PrimitiveLongStack stack = new PrimitiveLongStack( 6 );

			  // WHEN/THEN
			  assertTrue( stack.Empty );
			  assertEquals( -1, stack.Poll() );

			  stack.Push( 123 );
			  assertFalse( stack.Empty );

			  stack.Push( 456 );
			  assertFalse( stack.Empty );
			  assertEquals( 456, stack.Poll() );

			  assertFalse( stack.Empty );
			  assertEquals( 123, stack.Poll() );

			  assertTrue( stack.Empty );
			  assertEquals( -1, stack.Poll() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGrowArray()
		 internal virtual void ShouldGrowArray()
		 {
			  // GIVEN
			  PrimitiveLongStack stack = new PrimitiveLongStack( 5 );

			  // WHEN
			  for ( int i = 0; i <= 7; i++ )
			  {
					stack.Push( i );
			  }

			  // THEN
			  for ( int i = 7; i >= 0; i-- )
			  {
					assertFalse( stack.Empty );
					assertEquals( i, stack.Poll() );
			  }
			  assertTrue( stack.Empty );
			  assertEquals( -1, stack.Poll() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldStoreLongs()
		 internal virtual void ShouldStoreLongs()
		 {
			  // GIVEN
			  PrimitiveLongStack stack = new PrimitiveLongStack( 5 );
			  long value1 = 10L * int.MaxValue;
			  long value2 = 101L * int.MaxValue;
			  stack.Push( value1 );
			  stack.Push( value2 );

			  // WHEN
			  long firstPolledValue = stack.Poll();
			  long secondPolledValue = stack.Poll();

			  // THEN
			  assertEquals( value2, firstPolledValue );
			  assertEquals( value1, secondPolledValue );
			  assertTrue( stack.Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIterate()
		 internal virtual void ShouldIterate()
		 {
			  // GIVEN
			  PrimitiveLongStack stack = new PrimitiveLongStack();

			  // WHEN
			  for ( int i = 0; i < 7; i++ )
			  {
					stack.Push( i );
			  }

			  // THEN
			  PrimitiveLongIterator iterator = stack.GetEnumerator();
			  long i = 0;
			  while ( iterator.HasNext() )
			  {
					assertEquals( i++, iterator.Next() );
			  }
			  assertEquals( 7L, i );
		 }
	}

}