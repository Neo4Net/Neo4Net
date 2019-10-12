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
namespace Org.Neo4j.Collection.primitive
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class PrimitiveLongListTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void newListIsEmpty()
		 internal virtual void NewListIsEmpty()
		 {
			  assertTrue( ( new PrimitiveLongList() ).Empty );
			  assertTrue( ( new PrimitiveLongList( 12 ) ).Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void newListHasZeroSize()
		 internal virtual void NewListHasZeroSize()
		 {
			  assertEquals( 0, ( new PrimitiveLongList() ).Size() );
			  assertEquals( 0, ( new PrimitiveLongList( 12 ) ).Size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addingElementsChangeSize()
		 internal virtual void AddingElementsChangeSize()
		 {
			  PrimitiveLongList longList = new PrimitiveLongList();
			  longList.Add( 1L );

			  assertFalse( longList.Empty );
			  assertEquals( 1, longList.Size() );

			  longList.Add( 2L );
			  assertFalse( longList.Empty );
			  assertEquals( 2, longList.Size() );

			  longList.Add( 3L );

			  assertFalse( longList.Empty );
			  assertEquals( 3, longList.Size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void accessAddedElements()
		 internal virtual void AccessAddedElements()
		 {
			  PrimitiveLongList longList = new PrimitiveLongList();
			  for ( long i = 1; i < 6L; i++ )
			  {
					longList.Add( i );
			  }

			  assertEquals( 5L, longList.Get( 4 ) );
			  assertEquals( 1L, longList.Get( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void throwExceptionOnAccessingNonExistentElement()
		 internal virtual void ThrowExceptionOnAccessingNonExistentElement()
		 {
			  PrimitiveLongList longList = new PrimitiveLongList();
			  assertThrows( typeof( System.IndexOutOfRangeException ), () => longList.Get(0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void iterateOverListElements()
		 internal virtual void IterateOverListElements()
		 {
			  PrimitiveLongList longList = new PrimitiveLongList();
			  for ( long i = 0; i < 10L; i++ )
			  {
					longList.Add( i );
			  }

			  int iteratorElements = 0;
			  long value = 0;
			  PrimitiveLongIterator iterator = longList.GetEnumerator();
			  while ( iterator.HasNext() )
			  {
					iteratorElements++;
					assertEquals( value++, iterator.Next() );
			  }

			  assertEquals( iteratorElements, longList.Size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void clearResetListSize()
		 internal virtual void ClearResetListSize()
		 {
			  PrimitiveLongList longList = new PrimitiveLongList();
			  long size = 10;
			  for ( long i = 0; i < 10L; i++ )
			  {
					longList.Add( i );
			  }
			  assertEquals( size, longList.Size() );

			  longList.Clear();

			  assertEquals( 0, longList.Size() );
			  assertTrue( longList.Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addAfterClear()
		 internal virtual void AddAfterClear()
		 {
			  PrimitiveLongList longList = new PrimitiveLongList();
			  longList.Clear();

			  longList.Add( 1 );
			  assertEquals( 1, longList.Get( 0 ) );
			  assertEquals( 1, longList.Size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void transformListToArray()
		 internal virtual void TransformListToArray()
		 {
			  PrimitiveLongList longList = new PrimitiveLongList();
			  long size = 24L;
			  for ( long i = 0; i < size; i++ )
			  {
					longList.Add( i );
			  }

			  long[] longs = longList.ToArray();
			  assertEquals( size, longs.Length );
			  for ( int i = 0; i < longs.Length; i++ )
			  {
					assertEquals( i, longs[i] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void holdLotsOfElements()
		 internal virtual void HoldLotsOfElements()
		 {
			  PrimitiveLongList longList = new PrimitiveLongList();
			  long size = 13077L;
			  for ( long i = 0; i < size; i++ )
			  {
					longList.Add( i );
			  }

			  assertEquals( size, longList.Size() );
			  for ( int i = 0; i < size; i++ )
			  {
					assertEquals( i, longList.Get( i ) );
			  }
		 }
	}

}