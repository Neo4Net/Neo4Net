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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class LongBitsManipulatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldManageTwoSlots()
		 public virtual void ShouldManageTwoSlots()
		 {
			  // GIVEN
			  LongBitsManipulator manipulator = new LongBitsManipulator( 64 - 29, 29 );
			  long field = 0;

			  // WHEN
			  field = manipulator.Set( field, 0, 10 );
			  field = manipulator.Set( field, 1, 13 );

			  // THEN
			  assertEquals( 10, manipulator.Get( field, 0 ) );
			  assertEquals( 13, manipulator.Get( field, 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInterpretAllOnesAsMinusOne()
		 public virtual void ShouldInterpretAllOnesAsMinusOne()
		 {
			  // GIVEN
			  LongBitsManipulator manipulator = new LongBitsManipulator( 64 - 29, 29 );

			  // WHEN
			  long field = manipulator.Template( true, false );

			  // THEN
			  assertEquals( -1, manipulator.Get( field, 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMinusOneValues()
		 public virtual void ShouldHandleMinusOneValues()
		 {
			  // GIVEN
			  LongBitsManipulator manipulator = new LongBitsManipulator( 1, 5, 10, 16, 32 ); // = 64 bits

			  // WHEN/THEN
			  long field = 0;
			  for ( int i = 0; i < 5; i++ )
			  { // For every value, set all others to 0, the current to -1 and verify all
					for ( int j = 0; j < 5; j++ )
					{
						 if ( j == i )
						 { // The current one
							  long valueAfterClearWouldHaveChangedIt = manipulator.Clear( field, j, true );
							  field = manipulator.Set( field, j, -1 );
							  // We piggy pack testing of clear(true) vs. set -1 here
							  assertEquals( "Clear(true) and set -1 produced different results for i:" + i + ", j:" + j, field, valueAfterClearWouldHaveChangedIt );
						 }
						 else
						 { // The other ones
							  long valueAfterClearWouldHaveChangedIt = manipulator.Clear( field, j, false );
							  field = manipulator.Set( field, j, 0 );
							  // We piggy pack testing of clear(false) vs. set 0 here
							  assertEquals( "Clear(false) and set 0 produced different results for i:" + i + ", j:" + j, field, valueAfterClearWouldHaveChangedIt );
						 }
					}

					for ( int j = 0; j < 5; j++ )
					{
						 long value = manipulator.Get( field, j );
						 if ( j == i )
						 { // The current one
							  assertEquals( -1L, value );
						 }
						 else
						 { // The other ones
							  assertEquals( 0L, value );
						 }
					}
			  }
		 }
	}

}