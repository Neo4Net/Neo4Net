using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.util.collection
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ContinuableArrayCursorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException thrown = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException Thrown = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnAnyElementOnEmptySupplier()
		 public virtual void ShouldNotReturnAnyElementOnEmptySupplier()
		 {
			  // given
			  ContinuableArrayCursor cursor = new ContinuableArrayCursor<>( () => null );

			  // then
			  assertFalse( cursor.next() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnAnyElementOnSupplierWithOneEmptyArray()
		 public virtual void ShouldNotReturnAnyElementOnSupplierWithOneEmptyArray()
		 {
			  // given
			  ContinuableArrayCursor cursor = new ContinuableArrayCursor( Supply( new int?[0] ) );

			  // then
			  assertFalse( cursor.next() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMoveCursorOverSingleArray()
		 public virtual void ShouldMoveCursorOverSingleArray()
		 {
			  // given
			  int?[] array = new int?[]{ 1, 2, 3 };
			  ContinuableArrayCursor<int> cursor = new ContinuableArrayCursor<int>( Supply( array ) );

			  // then
			  AssertCursor( cursor, array );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMoveCursorOverMultipleArrays()
		 public virtual void ShouldMoveCursorOverMultipleArrays()
		 {
			  // given
			  int?[][] arrays = new int?[][]
			  {
				  new int?[]{ 1, 2, 3 },
				  new int?[]{ 4, 5, 6 },
				  new int?[]{ 7 }
			  };
			  ContinuableArrayCursor<int> cursor = new ContinuableArrayCursor<int>( Supply( arrays ) );

			  // then
			  AssertCursor( cursor, arrays );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void callGetBeforeNextShouldThrowIllegalStateException()
		 public virtual void CallGetBeforeNextShouldThrowIllegalStateException()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ContinuableArrayCursor<?> cursor = new ContinuableArrayCursor(supply(new System.Nullable<int>[0]));
			  ContinuableArrayCursor<object> cursor = new ContinuableArrayCursor( Supply( new int?[0] ) );

			  // then
			  Thrown.expect( typeof( System.InvalidOperationException ) );
			  cursor.Get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void callGetAfterNextReturnsFalseShouldThrowIllegalStateException()
		 public virtual void CallGetAfterNextReturnsFalseShouldThrowIllegalStateException()
		 {
			  // given
			  ContinuableArrayCursor<int> cursor = new ContinuableArrayCursor<int>( Supply( new int?[0] ) );

			  // when
			  assertFalse( cursor.Next() );

			  // then
			  Thrown.expect( typeof( System.InvalidOperationException ) );
			  cursor.Get();
		 }

		 private System.Func<int[]> Supply( int?[] array )
		 {
			  return Supply( new int?[][]{ array } );
		 }

		 private System.Func<int[]> Supply( int?[][] arrays )
		 {
			  IEnumerator<int[]> iterator = Arrays.asList( arrays ).GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return () => iterator.hasNext() ? iterator.next() : null;
		 }

		 private void AssertCursor<T1>( ContinuableArrayCursor<T1> cursor, params object[][] arrays )
		 {
			  foreach ( object[] array in arrays )
			  {
					foreach ( object obj in array )
					{
						 assertTrue( cursor.Next() );
						 assertEquals( obj, cursor.Get() );
					}
			  }
			  assertFalse( cursor.Next() );
		 }
	}

}