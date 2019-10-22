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
namespace Neo4Net.Graphalgo.impl.util
{
	using Test = org.junit.jupiter.api.Test;

	using Neo4Net.Graphalgo.impl.util.PriorityMap;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class TestPriorityMap
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testIt()
		 internal virtual void TestIt()
		 {
			  PriorityMap<int, int, double> map = PriorityMap.WithSelfKeyNaturalOrder();
			  map.Put( 0, 5d );
			  map.Put( 1, 4d );
			  map.Put( 1, 4d );
			  map.Put( 1, 3d );
			  AssertEntry( map.Pop(), 1, 3d );
			  AssertEntry( map.Pop(), 0, 5d );
			  assertNull( map.Pop() );

			  int start = 0;
			  int a = 1;
			  int b = 2;
			  int c = 3;
			  int d = 4;
			  int e = 6;
			  int f = 7;
			  int y = 8;
			  int x = 9;
			  map.Put( start, 0d );
			  map.Put( a, 1d );
			  // get start
			  // get a
			  map.Put( x, 10d );
			  map.Put( b, 2d );
			  // get b
			  map.Put( x, 9d );
			  map.Put( c, 3d );
			  // get c
			  map.Put( x, 8d );
			  map.Put( x, 6d );
			  map.Put( d, 4d );
			  // get d
			  map.Put( x, 7d );
			  map.Put( e, 5d );
			  // get e
			  map.Put( x, 6d );
			  map.Put( f, 7d );
			  // get x
			  map.Put( y, 8d );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReplaceIfBetter()
		 internal virtual void ShouldReplaceIfBetter()
		 {
			  // GIVEN
			  PriorityMap<int, int, double> map = PriorityMap.WithSelfKeyNaturalOrder();
			  map.Put( 1, 2d );

			  // WHEN
			  bool putResult = map.Put( 1, 1.5d );

			  // THEN
			  assertTrue( putResult );
			  Entry<int, double> top = map.Pop();
			  assertNull( map.Peek() );
			  assertEquals( 1, top.Entity );
			  assertEquals( 1.5d, top.Priority, 0.00001 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldKeepAllPrioritiesIfToldTo()
		 internal virtual void ShouldKeepAllPrioritiesIfToldTo()
		 {
			  // GIVEN
			  int IEntity = 5;
			  PriorityMap<int, int, double> map = PriorityMap.WithSelfKeyNaturalOrder( false, false );
			  assertTrue( map.Put( IEntity, 3d ) );
			  assertTrue( map.Put( IEntity, 2d ) );

			  // WHEN
			  assertTrue( map.Put( IEntity, 5d ) );
			  assertTrue( map.Put( IEntity, 4d ) );

			  // THEN
			  AssertEntry( map.Pop(), IEntity, 2d );
			  AssertEntry( map.Pop(), IEntity, 3d );
			  AssertEntry( map.Pop(), IEntity, 4d );
			  AssertEntry( map.Pop(), IEntity, 5d );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void inCaseSaveAllPrioritiesShouldHandleNewEntryWithWorsePrio()
		 internal virtual void InCaseSaveAllPrioritiesShouldHandleNewEntryWithWorsePrio()
		 {
			  // GIVEN
			  int first = 1;
			  int second = 2;
			  PriorityMap<int, int, double> map = PriorityMap.WithSelfKeyNaturalOrder( false, false );

			  // WHEN
			  assertTrue( map.Put( first, 1d ) );
			  assertTrue( map.Put( second, 2d ) );
			  assertTrue( map.Put( first, 3d ) );

			  // THEN
			  AssertEntry( map.Pop(), first, 1d );
			  AssertEntry( map.Pop(), second, 2d );
			  AssertEntry( map.Pop(), first, 3d );
			  assertNull( map.Peek() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void inCaseSaveAllPrioritiesShouldHandleNewEntryWithBetterPrio()
		 internal virtual void InCaseSaveAllPrioritiesShouldHandleNewEntryWithBetterPrio()
		 {
			  // GIVEN
			  int first = 1;
			  int second = 2;
			  PriorityMap<int, int, double> map = PriorityMap.WithSelfKeyNaturalOrder( false, false );

			  // WHEN
			  assertTrue( map.Put( first, 3d ) );
			  assertTrue( map.Put( second, 2d ) );
			  assertTrue( map.Put( first, 1d ) );

			  // THEN
			  AssertEntry( map.Pop(), first, 1d );
			  AssertEntry( map.Pop(), second, 2d );
			  AssertEntry( map.Pop(), first, 3d );
			  assertNull( map.Peek() );
		 }

		 private static void AssertEntry( Entry<int, double> entry, int? IEntity, double? priority )
		 {
			  assertNotNull( entry );
			  assertEquals( IEntity, entry.Entity );
			  assertEquals( priority, entry.Priority );
		 }
	}

}